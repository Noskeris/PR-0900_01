using BattleShipAPI.Enums;
using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.GameItems.SuperAttacks;

namespace BattleShipAPI.Models
{
    public class GameRoom
    {
        public GameRoom()
        {
            
        }

        public string Name { get; set; } = string.Empty;

        public GameState State { get; set; } = GameState.NotStarted;

        public Board Board { get; private set; }
        
        public List<ShipConfig> ShipsConfig { get; private set; }
    
        public List<SuperAttackConfig> SuperAttacksConfig { get; private set; }

        public string TurnPlayerId { get; private set; } = string.Empty;

        private bool AreSettingsSet = false;
        
        public void SetSettings(GameRoomSettings settings)
        {
            Board = settings.Board;
            ShipsConfig = settings.Ships.ShipsConfig;
            SuperAttacksConfig = settings.SuperAttacks.SuperAttacksConfig;
            
            AreSettingsSet = true;
        }

        public string GetNextTurnPlayerId(List<UserConnection> players)
        {
            var filteredPlayers = players
                .Where(x => x.CanPlay)
                .OrderBy(x => x.Username)
                .ToList(); 

            if (!filteredPlayers.Any())
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(TurnPlayerId))
            {
                TurnPlayerId = filteredPlayers.First().PlayerId;
                return TurnPlayerId;
            }

            var turnPlayer = filteredPlayers.FirstOrDefault(x => x.PlayerId == TurnPlayerId);

            if (turnPlayer == null)
            {
                TurnPlayerId = filteredPlayers.First().PlayerId;
                return TurnPlayerId;
            }

            var nextPlayer = filteredPlayers
                .SkipWhile(x => x.Username != turnPlayer.Username)
                .Skip(1) 
                .FirstOrDefault(); 

            TurnPlayerId = nextPlayer?.PlayerId ?? filteredPlayers.First().PlayerId;

            return TurnPlayerId;
        }

        public bool TryFullySinkShip(int x, int y, UserConnection player)
        {
            var attackedCell = Board.Cells[x][y];
            if (attackedCell.State != CellState.HasShip)
            {
                return false;
            }
            
            attackedCell.State = CellState.DamagedShip;

            var ship = player.PlacedShips
                .First(s => s.StartX <= x && s.EndX >= x && s.StartY <= y && s.EndY >= y);

            var isShipFullyDamaged = true;

            if (ship != null) // Ensure the ship is found
            {
                for (int i = ship.StartX; i <= ship.EndX; i++)
                {
                    for (int j = ship.StartY; j <= ship.EndY; j++)
                    {
                        if (Board.Cells[i][j].OwnerId == player.PlayerId &&
                            Board.Cells[i][j].State != CellState.DamagedShip)
                        {
                            isShipFullyDamaged = false;
                            break;
                        }
                    }
                    if (!isShipFullyDamaged) break;
                }
            }

            if (isShipFullyDamaged)
            {
                Board.SinkShip(ship);
            }

            return isShipFullyDamaged;
        }

        public bool HasAliveShips(UserConnection cellOwner)
        {
            return cellOwner.PlacedShips
                .Any(s => Board.Cells[s.StartX][s.StartY].State != CellState.SunkenShip);
        }

        public void SinkAllShips(UserConnection player)
        {
            player.PlacedShips.ForEach(ship => Board.SinkShip(ship));
        }
    }
}