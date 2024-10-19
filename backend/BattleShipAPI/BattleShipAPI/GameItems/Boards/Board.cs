using BattleShipAPI.Enums;
using BattleShipAPI.Models;
using BattleShipAPI.Prototype;

namespace BattleShipAPI.GameItems.Boards
{
    public abstract class Board : IPrototype<Board>
    {
        public int XLength { get; set; }
        
        public int YLength { get; set; }
        
        public Cell[][] Cells { get; set; }
        
        protected abstract IReadOnlyList<Section> BoardSections { get; }

        public Board(int xLength, int yLength)
        {
            XLength = xLength;
            YLength = yLength;

            Cells = new Cell[xLength][];
            InitializeBoard();
        }
        
        public void AssignBoardSections(List<UserConnection> players)
        {
            if (players.Count != BoardSections.Count)
            {
                throw new Exception("Invalid number of players");
            }

            for (var i = 0; i < BoardSections.Count; i++)
            {
                var section = BoardSections[i];
                AssignBoardSection(
                    section.StartX,
                    section.StartY,
                    section.EndX,
                    section.EndY,
                    players[i].PlayerId);
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

        protected void AssignBoardSection(int xStart, int yStart, int xEnd, int yEnd, string ownerId)
        {
            for (int x = xStart; x <= xEnd; x++)
            {
                for (int y = yStart; y <= yEnd; y++)
                {
                    Cells[x][y].OwnerId = ownerId;
                }
            }
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

        public Board Clone()
        {
            var clonedBoard = (Board)MemberwiseClone();
            clonedBoard.Cells = new Cell[XLength][];
            for (var x = 0; x < XLength; x++)
            {
                clonedBoard.Cells[x] = new Cell[YLength];
                for (var y = 0; y < YLength; y++)
                {
                    clonedBoard.Cells[x][y] = new Cell
                    {
                        OwnerId = Cells[x][y].OwnerId,
                        State = Cells[x][y].State
                    };
                }
            }
            return clonedBoard;
        }
    }
}
