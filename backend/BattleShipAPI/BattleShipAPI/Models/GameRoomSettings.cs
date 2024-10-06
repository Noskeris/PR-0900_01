using BattleShipAPI.Enums;

namespace BattleShipAPI.Models;

public class GameRoomSettings
{
    public List<ShipConfig> ShipsConfig { get; set; } =
    [
        new ShipConfig() { ShipType = ShipType.Battleship, Count = 1 },
        new ShipConfig() { ShipType = ShipType.Destroyer, Count = 1 },
        new ShipConfig() { ShipType = ShipType.Submarine, Count = 1 },
        new ShipConfig() { ShipType = ShipType.Carrier, Count = 1 },
        new ShipConfig() { ShipType = ShipType.Cruiser, Count = 1 }
    ];
    
    public List<SuperAttackConfig> SuperAttacksConfig { get; set; } =
    [
        new SuperAttackConfig() { AttackType = AttackType.Plus, Count = 1 },
        new SuperAttackConfig() { AttackType = AttackType.Cross, Count = 0 },
        new SuperAttackConfig() { AttackType = AttackType.Boom, Count = 0 }
    ];
}