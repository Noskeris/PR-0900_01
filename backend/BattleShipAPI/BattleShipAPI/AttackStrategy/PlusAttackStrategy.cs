using BattleShipAPI.Enums;
using BattleShipAPI.Models;

namespace BattleShipAPI.AttackStrategy
{
    public class PlusAttackStrategy : IAttackStrategy
    {
        public List<Tuple<int, int>> GetAttackCells(int x, int y, GameRoom gameRoom, UserConnection connection)
        {
            var attackCells = new List<Tuple<int, int>> { new Tuple<int, int>(x, y) };

            if (x - 1 >= 0 && CanCellBeAttacked(x - 1, y, gameRoom, connection))
                attackCells.Add(new Tuple<int, int>(x - 1, y));

            if (x + 1 < gameRoom.Board.XLength && CanCellBeAttacked(x + 1, y, gameRoom, connection))
                attackCells.Add(new Tuple<int, int>(x + 1, y));

            if (y - 1 >= 0 && CanCellBeAttacked(x, y - 1, gameRoom, connection))
                attackCells.Add(new Tuple<int, int>(x, y - 1));

            if (y + 1 < gameRoom.Board.YLength && CanCellBeAttacked(x, y + 1, gameRoom, connection))
                attackCells.Add(new Tuple<int, int>(x, y + 1));

            return attackCells;
        }
        private bool CanCellBeAttacked(int x, int y, GameRoom gameRoom, UserConnection connection)
        {
            return gameRoom.Board.Cells[x][y].OwnerId != connection.PlayerId &&
                   gameRoom.Board.Cells[x][y].State != CellState.DamagedShip &&
                   gameRoom.Board.Cells[x][y].State != CellState.SunkenShip &&
                   gameRoom.Board.Cells[x][y].State != CellState.Missed;
        }
    }

}
