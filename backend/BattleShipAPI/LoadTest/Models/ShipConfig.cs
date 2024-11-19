using BattleShipAPI.Enums;

namespace BattleShipAPI.Models;

public class ShipConfig
{
    public ShipType ShipType { get; set; }
    
    public int Count { get; set; }
}