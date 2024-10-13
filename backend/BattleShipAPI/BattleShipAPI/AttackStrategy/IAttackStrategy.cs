using BattleShipAPI.Models;

namespace BattleShipAPI.AttackStrategy
{
    public interface IAttackStrategy
    {
        List<Tuple<int, int>> GetAttackCells(int x, int y, GameRoom gameRoom, UserConnection connection);
    }

}
