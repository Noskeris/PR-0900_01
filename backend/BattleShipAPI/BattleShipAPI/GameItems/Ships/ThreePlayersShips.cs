using BattleShipAPI.Builders;
using BattleShipAPI.Enums;
using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.Ships;

public class ThreePlayersShips : Ships
{
    public override List<ShipConfig> ShipsConfig { get; }
    
    public ThreePlayersShips(IConfigBuilder<ShipConfig> builder)
    {
        ShipsConfig = builder
            .AddConfig(new ShipConfig { ShipType = ShipType.Destroyer, Count = 1 })
            .AddConfig(new ShipConfig { ShipType = ShipType.Submarine, Count = 1 })
            .AddConfig(new ShipConfig { ShipType = ShipType.Carrier, Count = 1 })
            .AddConfig(new ShipConfig { ShipType = ShipType.Cruiser, Count = 1 })
            .Build();
    }
}