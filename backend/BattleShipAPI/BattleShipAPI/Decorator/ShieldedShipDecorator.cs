using BattleShipAPI.Enums;


public class ShieldedShipDecorator : PlacedShipDecorator
{
    private int _shieldStrength;

    public ShieldedShipDecorator(IPlacedShip placedShip, int shieldStrength) : base(placedShip)
    {
        _shieldStrength = shieldStrength;
    }

    public override void Hit(int x, int y)
    {
        if (_shieldStrength > 0)
        {
            _shieldStrength--;
            // Optionally notify that the shield absorbed the hit
        }
        else
        {
            base.Hit(x, y);
        }
    }
}
