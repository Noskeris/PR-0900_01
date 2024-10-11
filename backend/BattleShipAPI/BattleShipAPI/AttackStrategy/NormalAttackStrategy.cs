using BattleShipAPI.Models;

namespace BattleShipAPI.AttackStrategy
{
    public class NormalAttackStrategy : IAttackStrategy
    {
        public List<Tuple<int, int>> GetAttackCells(int x, int y, GameRoom gameRoom, UserConnection connection)
        {
            return new List<Tuple<int, int>> { new Tuple<int, int>(x, y) };
        }
    }
}
