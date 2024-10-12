using BattleShipAPI.AttackStrategy;
using BattleShipAPI.Enums;
using BattleShipAPI.Models;

namespace BattleShipAPI.Helpers
{
    public static class GameHelper
    {
        public static bool CanCellBeAttacked(int x, int y, GameRoom gameRoom, UserConnection connection)
        {
            if (x < 0 || y < 0 || x >= gameRoom.Board.XLength || y >= gameRoom.Board.YLength)
                return false;

            var cell = gameRoom.Board.Cells[x][y];

            return cell.OwnerId != connection.PlayerId &&
                   cell.State != CellState.DamagedShip &&
                   cell.State != CellState.SunkenShip &&
                   cell.State != CellState.Missed;
        }

        public static IAttackStrategy GetAttackStrategy(AttackType attackType)
        {
            return attackType switch
            {
                AttackType.Normal => new NormalAttackStrategy(),
                AttackType.Plus => new PlusAttackStrategy(),
                AttackType.Cross => new CrossAttackStrategy(),
                AttackType.Boom => new BoomAttackStrategy(),
                _ => throw new ArgumentOutOfRangeException(nameof(attackType), "Invalid attack type")
            };
        }
    }
}
