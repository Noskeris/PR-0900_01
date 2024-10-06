using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.Ships;

public abstract class Ships
{
    public abstract List<ShipConfig> ShipsConfig { get; }
}