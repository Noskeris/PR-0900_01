using BattleShipAPI.Models;
using BattleShipAPI.Prototype;

namespace BattleShipAPI.GameItems.Ships;

public abstract class Ships : IPrototype<Ships>
{
    public abstract List<ShipConfig> ShipsConfig { get; }
    public Ships Clone()
    {
        return (Ships)MemberwiseClone();
    }
}