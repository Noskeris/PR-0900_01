using BattleShipAPI.Builders;
using BattleShipAPI.Enums;
using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.SuperAttacks;

public class FourPlayersSuperAttacks : SuperAttacks
{
    public override List<SuperAttackConfig> SuperAttacksConfig { get; }
    
    public FourPlayersSuperAttacks(IConfigBuilder<SuperAttackConfig> builder)
    {
        SuperAttacksConfig = builder
            .AddConfig(new SuperAttackConfig { AttackType = AttackType.Plus, Count = 3 })
            .AddConfig(new SuperAttackConfig { AttackType = AttackType.Cross, Count = 3 })
            .AddConfig(new SuperAttackConfig { AttackType = AttackType.Boom, Count = 3 })
            .Build();
    }
}