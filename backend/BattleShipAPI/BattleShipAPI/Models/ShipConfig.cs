using BattleShipAPI.Enums;

namespace BattleShipAPI.Models;

public class ShipConfig
{
    public int Count { get; set; } = 1;

    public ShipType ShipType { get; set; }

    public int Size { get; set; }

    public bool HasShield { get; set; }

    public bool HasMobility { get; set; }
}