using BattleShipAPI.Builders;
using BattleShipAPI.Enums;
using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.SuperAttacks;

public class TwoPlayersSuperAttacks : SuperAttacks
{
    public override List<SuperAttackConfig> SuperAttacksConfig { get; }

    public TwoPlayersSuperAttacks(IConfigBuilder<SuperAttackConfig> builder)
    {
        SuperAttacksConfig = builder
            .AddConfig(new SuperAttackConfig { AttackType = AttackType.Plus, Count = 1 })
            .AddConfig(new SuperAttackConfig { AttackType = AttackType.Cross, Count = 1 })
            .AddConfig(new SuperAttackConfig { AttackType = AttackType.Boom, Count = 1 })
            .Build();
    }
}