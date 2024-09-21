using BattleShipAPI.Enums;

namespace BattleShipAPI.Models;

public class PlacedShip
{
    public ShipType ShipType { get; set; }
    public int StartX { get; set; }
    public int StartY { get; set; }
    public int EndX { get; set; }
    public int EndY { get; set; }
}