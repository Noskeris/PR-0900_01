using BattleShipAPI.Models;
using BattleShipAPI.Prototype;

namespace BattleShipAPI.GameItems.SuperAttacks;

public abstract class SuperAttacks : IPrototype<SuperAttacks>
{
    public abstract List<SuperAttackConfig> SuperAttacksConfig { get; }
    
    public SuperAttacks Clone()
    {
        return (SuperAttacks)MemberwiseClone();
    }
}