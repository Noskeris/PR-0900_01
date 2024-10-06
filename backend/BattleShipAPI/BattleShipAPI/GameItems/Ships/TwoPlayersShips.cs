using BattleShipAPI.Enums;
using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.Ships;

public class TwoPlayersShips : Ships
{
    public override List<ShipConfig> ShipsConfig { get; }
    
    public TwoPlayersShips()
    {
        ShipsConfig = new List<ShipConfig>()
        {
            new ShipConfig() { ShipType = ShipType.Submarine, Count = 1 },
            new ShipConfig() { ShipType = ShipType.Carrier, Count = 1 },
            new ShipConfig() { ShipType = ShipType.Cruiser, Count = 1 }
        };
    }
}