using System;
using BattleShipAPI.Enums;
using BattleShipAPI.GameItems.Boards;

public class ShieldedShipDecorator : PlacedShipDecorator
{
    private int _shieldStrength;

    public ShieldedShipDecorator(IPlacedShip placedShip, int shieldStrength)
        : base(placedShip)
    {
        _shieldStrength = shieldStrength;
    }

    public override void Hit(int x, int y, Board board)
    {
        if (_shieldStrength > 0)
        {
            _shieldStrength--;
            Console.WriteLine($"Shield absorbed the hit at ({x}, {y}). Remaining shield strength: {_shieldStrength}");
            // Optionally, you can update the board or notify players that the shield absorbed the hit
        }
        else
        {
            base.Hit(x, y, board);
        }
    }
}
