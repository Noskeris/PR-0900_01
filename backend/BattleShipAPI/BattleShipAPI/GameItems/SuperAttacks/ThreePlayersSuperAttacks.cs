using BattleShipAPI.Builders;
using BattleShipAPI.Enums;
using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.SuperAttacks;

public class ThreePlayersSuperAttacks : SuperAttacks
{
    public override List<SuperAttackConfig> SuperAttacksConfig { get; }

    public ThreePlayersSuperAttacks(IConfigBuilder<SuperAttackConfig> builder)
    {
        SuperAttacksConfig = builder
            .AddConfig(new SuperAttackConfig { AttackType = AttackType.Plus, Count = 2 })
            .AddConfig(new SuperAttackConfig { AttackType = AttackType.Cross, Count = 2 })
            .AddConfig(new SuperAttackConfig { AttackType = AttackType.Boom, Count = 2 })
            .Build();
    }
}