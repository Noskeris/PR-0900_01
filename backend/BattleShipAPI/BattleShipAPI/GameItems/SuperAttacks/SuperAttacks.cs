using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.SuperAttacks;

public abstract class SuperAttacks
{
    public abstract List<SuperAttackConfig> SuperAttacksConfig { get; }
}