using BattleShipAPI.Enums;
using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.SuperAttacks;

public class FourPlayersSuperAttacks : SuperAttacks
{
    public override List<SuperAttackConfig> SuperAttacksConfig { get; }
    
    public FourPlayersSuperAttacks()
    {
        SuperAttacksConfig = new List<SuperAttackConfig>()
        {
            new SuperAttackConfig() { AttackType = AttackType.Plus, Count = 3 },
            new SuperAttackConfig() { AttackType = AttackType.Cross, Count = 3 },
            new SuperAttackConfig() { AttackType = AttackType.Boom, Count = 3 }
        };
    }
}