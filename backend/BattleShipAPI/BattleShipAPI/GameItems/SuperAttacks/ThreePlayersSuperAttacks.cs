using BattleShipAPI.Enums;
using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.SuperAttacks;

public class ThreePlayersSuperAttacks : SuperAttacks
{
    public override List<SuperAttackConfig> SuperAttacksConfig { get; }

    public ThreePlayersSuperAttacks()
    {
        SuperAttacksConfig = new List<SuperAttackConfig>()
        {
            new SuperAttackConfig() { AttackType = AttackType.Plus, Count = 2 },
            new SuperAttackConfig() { AttackType = AttackType.Cross, Count = 2 },
            new SuperAttackConfig() { AttackType = AttackType.Boom, Count = 2 }
        };
    }
}