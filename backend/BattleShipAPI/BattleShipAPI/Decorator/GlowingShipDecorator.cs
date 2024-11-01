using System;
using BattleShipAPI.Enums;

public class GlowingShipDecorator : PlacedShipDecorator
{
    private readonly Action<IPlacedShip> _revealShipAction;

    public GlowingShipDecorator(IPlacedShip placedShip, Action<IPlacedShip> revealShipAction) : base(placedShip)
    {
        _revealShipAction = revealShipAction;
    }

    public override void Hit(int x, int y)
    {
        base.Hit(x, y);
        RevealShip();
    }

    private void RevealShip()
    {
        _revealShipAction?.Invoke(this);
    }
}
