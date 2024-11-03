using System.Collections.Generic;
using BattleShipAPI.Enums;
using BattleShipAPI.GameItems.Boards;

public abstract class PlacedShipDecorator : IPlacedShip
{
    protected IPlacedShip _placedShip;

    public PlacedShipDecorator(IPlacedShip placedShip)
    {
        _placedShip = placedShip;
    }

    public virtual ShipType ShipType
    {
        get => _placedShip.ShipType;
        set => _placedShip.ShipType = value;
    }

    public virtual int StartX
    {
        get => _placedShip.StartX;
        set => _placedShip.StartX = value;
    }

    public virtual int StartY
    {
        get => _placedShip.StartY;
        set => _placedShip.StartY = value;
    }

    public virtual int EndX
    {
        get => _placedShip.EndX;
        set => _placedShip.EndX = value;
    }

    public virtual int EndY
    {
        get => _placedShip.EndY;
        set => _placedShip.EndY = value;
    }

    public virtual bool IsSunk => _placedShip.IsSunk;

    public virtual void Hit(int x, int y, Board board)
    {
        _placedShip.Hit(x, y, board);
    }


    public virtual List<(int x, int y)> GetCoordinates()
    {
        return _placedShip.GetCoordinates();
    }
}

