﻿using BattleShipAPI.GameItems.Boards;

namespace BattleShipAPI.Decorator;

public class FragileShipDecorator : PlacedShipDecorator
{
    public FragileShipDecorator(IPlacedShip placedShip) : base(placedShip)
    {
    }

    public override void Hit(int x, int y, Board board)
    {
        foreach (var coord in GetCoordinates())
        {
            _placedShip.Hit(coord.x, coord.y, board);
        }
    }

}