using BattleShipAPI.Enums;
using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.Ships;

public class ThreePlayersShips : Ships
{
    public override List<ShipConfig> ShipsConfig { get; }
    
    public ThreePlayersShips()
    {
        ShipsConfig = new List<ShipConfig>()
        {
            new ShipConfig() { ShipType = ShipType.Destroyer, Count = 1 },
            new ShipConfig() { ShipType = ShipType.Submarine, Count = 1 },
            new ShipConfig() { ShipType = ShipType.Carrier, Count = 1 },
            new ShipConfig() { ShipType = ShipType.Cruiser, Count = 1 }
        };
    }
}