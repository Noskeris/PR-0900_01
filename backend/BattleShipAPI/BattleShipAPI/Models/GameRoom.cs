﻿using BattleShipAPI.Decorator;
using BattleShipAPI.Enums;
using BattleShipAPI.GameItems.Boards;

namespace BattleShipAPI.Models
{
    public class GameRoom
    {
        public GameRoom()
        {
            
        }

        public string Name { get; set; } = string.Empty;

        public GameState State { get; set; } = GameState.NotStarted;

        public GameMode Mode { get; set; } = GameMode.Normal;

        public Board Board { get; private set; }
        
        public List<ShipConfig> ShipsConfig { get; private set; }
    
        public List<SuperAttackConfig> SuperAttacksConfig { get; private set; }

        public int TimerDuration { get; set; }

        public string TurnPlayerId { get; private set; } = string.Empty;

        public void SetBoard(Board board)
        {
            Board = board;
        }
        
        public void SetSettings(GameRoomSettings settings)
        {
            Board = settings.Board;
            ShipsConfig = settings.Ships.ShipsConfig;
            SuperAttacksConfig = settings.SuperAttacks.SuperAttacksConfig;
            TimerDuration = settings.TimerDuration;
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
            IPlacedShip ship = null;
            var iterator = player.PlacedShips.CreateIterator();
            while (iterator.HasNext())
            {
                var currentShip = iterator.Next();
                if (currentShip.GetCoordinates().Contains((x, y)))
                {
                    ship = currentShip;
                    break;
                }
            }

            if (ship == null)
            {
                return false;
            }

            ship.Hit(x, y, Board);

            var isShipFullyDamaged = ship.IsSunk;

            return isShipFullyDamaged;
        }


        public bool HasAliveShips(UserConnection cellOwner)
        {
            var iterator = cellOwner.PlacedShips.CreateIterator();
            while (iterator.HasNext())
            {
                var ship = iterator.Next();
                if (Board.Cells[ship.StartX][ship.StartY].State != CellState.SunkenShip)
                {
                    return true;
                }
            }
            return false;

        }

        public void SinkAllShips(UserConnection player)
        {
            var iterator = player.PlacedShips.CreateIterator();
            while (iterator.HasNext())
            {
                var ship = iterator.Next();
                Board.SinkShip(ship);
            }
        }
    }
}