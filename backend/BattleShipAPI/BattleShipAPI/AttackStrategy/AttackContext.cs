using BattleShipAPI.Models;

namespace BattleShipAPI.AttackStrategy
{
    public class AttackContext
    {
        private readonly IAttackStrategy _attackStrategy;

        public AttackContext(IAttackStrategy attackStrategy)
        {
            _attackStrategy = attackStrategy;
        }

        public List<Tuple<int, int>> ExecuteAttack(int x, int y, GameRoom gameRoom, UserConnection connection)
        {
            return _attackStrategy.GetAttackCells(x, y, gameRoom, connection);
        }
    }
}
