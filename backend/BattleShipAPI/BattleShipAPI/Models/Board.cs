using BattleShipAPI.Enums;

namespace BattleShipAPI.Models
{
    public class Board
    {
        public int XLength { get; set; }
        
        public int YLength { get; set; }
        
        public Cell[][] Cells { get; set; }
        

        public Board()
        {
            
        }

        public Board(int xLength, int yLength)
        {
            XLength = xLength;
            YLength = yLength;

            Cells = new Cell[xLength][];
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            for (int x = 0; x < XLength; x++)
            {
                Cells[x] = new Cell[YLength];
                for (int y = 0; y < YLength; y++)
                {
                    Cells[x][y] = new Cell();
                }
            }
        }

        public void AssignBoardSection(int xStart, int yStart, int xEnd, int yEnd, string ownerId)
        {
            for (int x = xStart; x <= xEnd; x++)
            {
                for (int y = yStart; y <= yEnd; y++)
                {
                    Cells[x][y].OwnerId = ownerId;
                }
            }
        }
        
        public bool TryPutShipOnBoard(int xStart, int yStart, int xEnd, int yEnd, string ownerId)
        {
            for (int x = xStart; x <= xEnd; x++)
            {
                for (int y = yStart; y <= yEnd; y++)
                {
                    if (Cells[x][y].OwnerId == ownerId)
                    {
                        Cells[x][y].State = CellState.HasShip;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        
        public void SinkShip(PlacedShip ship)
        {
            for (int x = ship.StartX; x <= ship.EndX; x++)
            {
                for (int y = ship.StartY; y <= ship.EndY; y++)
                {
                  Cells[x][y].State = CellState.SunkenShip;
                }
            }
        }
    }
}
