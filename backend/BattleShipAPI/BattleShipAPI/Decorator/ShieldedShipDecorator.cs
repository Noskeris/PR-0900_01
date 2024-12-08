using BattleShipAPI.GameItems.Boards;

namespace BattleShipAPI.Decorator;

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
        }
        else
        {
            base.Hit(x, y, board);
        }
    }
}