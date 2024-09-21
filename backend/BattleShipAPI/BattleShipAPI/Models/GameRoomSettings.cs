using BattleShipAPI.Enums;

namespace BattleShipAPI.Models;

public class GameRoomSettings
{
    public List<ShipConfig> ShipConfigs { get; set; } =
    [
        new ShipConfig() { ShipType = ShipType.Battleship, Count = 1 },
        new ShipConfig() { ShipType = ShipType.Destroyer, Count = 1 },
        new ShipConfig() { ShipType = ShipType.Submarine, Count = 1 },
        new ShipConfig() { ShipType = ShipType.Carrier, Count = 1 },
        new ShipConfig() { ShipType = ShipType.Cruiser, Count = 1 }
    ];
}