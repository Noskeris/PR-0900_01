using BattleShipAPI.Models;
using BattleShipAPI.Prototype;

namespace BattleShipAPI.GameItems.SuperAttacks;

public class LevelSuperAttacks : SuperAttacks
{
    public override List<SuperAttackConfig> SuperAttacksConfig { get; }

    public LevelSuperAttacks(List<SuperAttackConfig> superAttacksConfig)
    {
        SuperAttacksConfig = superAttacksConfig;
    }
}