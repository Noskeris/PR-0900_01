using BattleShipAPI.Enums;

namespace BattleShipAPI.Models
{
    public class GameRoom
    {
        public string Name { get; set; } = string.Empty;

        public GameState State { get; set; } = GameState.NotStarted;

        public GameRoomSettings Settings { get; set; } = new();

        public Board Board { get; set; } = new();

        public Guid TurnPlayerId { get; private set; } = Guid.Empty;

        public Guid GetNextTurnPlayerId(List<UserConnection> players)
        {
            var filteredPlayers = players
                .Where(x => x.CanPlay)
                .OrderBy(x => x.Username);

            if (TurnPlayerId == Guid.Empty)
            {
                TurnPlayerId = filteredPlayers.First().PlayerId;
                return TurnPlayerId;
            }

            var turnPlayer = players.First(x => x.PlayerId == TurnPlayerId);
            TurnPlayerId = players
                .Where(x => x.CanPlay)
                .FirstOrDefault(x =>
                    string.Compare(x.Username, turnPlayer.Username, StringComparison.Ordinal) > 0)?
                .PlayerId ?? filteredPlayers.First().PlayerId;

            return TurnPlayerId;
        }

        public bool TryFullySinkShip(int x, int y, UserConnection player)
        {
            var attackedCell = Board.Cells[x][y];
            if (attackedCell.State != CellState.HasShip)
            {
                return false;
            }

            var ship = player.PlacedShips
                .First(s => s.StartX <= x && s.EndX >= x && s.StartY <= y && s.EndY >= y);

            Board.SinkShip(ship);

            return true;
        }

        public bool HasAliveShips(UserConnection cellOwner)
        {
            return cellOwner.PlacedShips
                .Any(s => Board.Cells[s.StartX][s.StartY].State != CellState.SunkenShip);
        }
    }
}