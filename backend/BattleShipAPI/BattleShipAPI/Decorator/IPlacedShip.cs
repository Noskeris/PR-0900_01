using BattleShipAPI.Enums;
using System.Collections.Generic;

public interface IPlacedShip
{
    ShipType ShipType { get; set; }
    int StartX { get; set; }
    int StartY { get; set; }
    int EndX { get; set; }
    int EndY { get; set; }
    bool IsSunk { get; }
    void Hit(int x, int y);
    List<(int x, int y)> GetCoordinates();
}

