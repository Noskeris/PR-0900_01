using BattleShipAPI.Enums;

namespace BattleShipAPI.Models
{
    public class Board
    {
        public int xLength { get; set; }
        public int yLength { get; set; }
        public Cell[][] Cells { get; set; }

        public Board(int xLength, int yLength)
        {
            this.xLength = xLength;
            this.yLength = yLength;

            Cells = new Cell[xLength][];
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            for (int x = 0; x < xLength; x++)
            {
                Cells[x] = new Cell[yLength];
                for (int y = 0; y < yLength; y++)
                {
                    Cells[x][y] = new Cell();
                    //Cells[x][y].State = CellState.Empty;
                }
            }
        }

        public void AssignBoardSection(int xStart, int yStart, int xEnd, int yEnd, int ownerId)
        {
            for (int x = xStart; x <= xEnd; x++)
            {
                for (int y = yStart; y <= yEnd; y++)
                {
                    Cells[x][y].OwnerId = ownerId;
                }
            }
        }
    }
}
