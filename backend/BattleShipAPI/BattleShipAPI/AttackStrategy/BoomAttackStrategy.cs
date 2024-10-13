using BattleShipAPI.Models;
using BattleShipAPI.Helpers;

namespace BattleShipAPI.AttackStrategy
{
    public class BoomAttackStrategy : IAttackStrategy
    {
        public List<Tuple<int, int>> GetAttackCells(int x, int y, GameRoom gameRoom, UserConnection connection)
        {
            var attackCells = new List<Tuple<int, int>> { new Tuple<int, int>(x, y) };

            if (GameHelper.CanCellBeAttacked(x - 1, y, gameRoom, connection))
                attackCells.Add(new Tuple<int, int>(x - 1, y));

            if (GameHelper.CanCellBeAttacked(x + 1, y, gameRoom, connection))
                attackCells.Add(new Tuple<int, int>(x + 1, y));

            if (GameHelper.CanCellBeAttacked(x, y - 1, gameRoom, connection))
                attackCells.Add(new Tuple<int, int>(x, y - 1));

            if (GameHelper.CanCellBeAttacked(x, y + 1, gameRoom, connection))
                attackCells.Add(new Tuple<int, int>(x, y + 1));

            if (GameHelper.CanCellBeAttacked(x - 1, y - 1, gameRoom, connection))
                attackCells.Add(new Tuple<int, int>(x - 1, y - 1));

            if (GameHelper.CanCellBeAttacked(x + 1, y + 1, gameRoom, connection))
                attackCells.Add(new Tuple<int, int>(x + 1, y + 1));

            if (GameHelper.CanCellBeAttacked(x - 1, y + 1, gameRoom, connection))
                attackCells.Add(new Tuple<int, int>(x - 1, y + 1));

            if (GameHelper.CanCellBeAttacked(x + 1, y - 1, gameRoom, connection))
                attackCells.Add(new Tuple<int, int>(x + 1, y - 1));

            return attackCells;
        }
    }
}
