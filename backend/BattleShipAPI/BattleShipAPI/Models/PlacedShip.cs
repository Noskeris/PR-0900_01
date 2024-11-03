// File: Models/PlacedShip.cs
using System;
using System.Collections.Generic;
using System.Linq;
using BattleShipAPI.Enums;
using BattleShipAPI.GameItems.Boards;

namespace BattleShipAPI.Models
{
    public class PlacedShip : IPlacedShip
    {
        public ShipType ShipType { get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int EndX { get; set; }
        public int EndY { get; set; }

        private HashSet<(int x, int y)> _hitCoordinates = new HashSet<(int x, int y)>();

        public bool IsSunk => GetCoordinates().All(c => _hitCoordinates.Contains(c));

        public void Hit(int x, int y, Board board)
        {
            if (GetCoordinates().Contains((x, y)))
            {
                _hitCoordinates.Add((x, y));

                board.Cells[x][y].State = CellState.DamagedShip;

                if (IsSunk)
                {
                    foreach (var (cx, cy) in GetCoordinates())
                    {
                        board.Cells[cx][cy].State = CellState.SunkenShip;
                    }
                }
            }
            Console.WriteLine($"Ship hit at PlacedShip.cs {x}, {y}");
        }


        public List<(int x, int y)> GetCoordinates()
        {
            var coordinates = new List<(int x, int y)>();

            int deltaX = Math.Sign(EndX - StartX);
            int deltaY = Math.Sign(EndY - StartY);

            int length = Math.Max(Math.Abs(EndX - StartX), Math.Abs(EndY - StartY)) + 1;

            for (int i = 0; i < length; i++)
            {
                int x = StartX + i * deltaX;
                int y = StartY + i * deltaY;
                coordinates.Add((x, y));
            }

            return coordinates;
        }
    }
}
