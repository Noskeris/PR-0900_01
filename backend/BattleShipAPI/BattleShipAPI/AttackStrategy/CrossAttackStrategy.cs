using BattleShipAPI.Helpers;
using BattleShipAPI.Models;

namespace BattleShipAPI.AttackStrategy
{
    public class CrossAttackStrategy : IAttackStrategy
    {
        public List<Tuple<int, int>> GetAttackCells(int x, int y, GameRoom gameRoom, UserConnection connection)
        {
            var attackCells = new List<Tuple<int, int>> { new Tuple<int, int>(x, y) };

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
