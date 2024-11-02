using BattleShipAPI.Enums;
using BattleShipAPI.GameItems.Boards;

public interface IPlacedShip
{
    ShipType ShipType { get; set; }
    int StartX { get; set; }
    int StartY { get; set; }
    int EndX { get; set; }
    int EndY { get; set; }
    bool IsSunk { get; }
    void Hit(int x, int y, Board board);
    List<(int x, int y)> GetCoordinates();
}
