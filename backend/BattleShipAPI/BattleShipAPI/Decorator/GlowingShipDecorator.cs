using BattleShipAPI.GameItems.Boards;

namespace BattleShipAPI.Decorator;

public class GlowingShipDecorator : PlacedShipDecorator
{
    private readonly Action<IPlacedShip, Board> _revealShipAction;

    public GlowingShipDecorator(IPlacedShip placedShip, Action<IPlacedShip, Board> revealShipAction)
        : base(placedShip)
    {
        _revealShipAction = revealShipAction;
    }

    public override void Hit(int x, int y, Board board)
    {
        base.Hit(x, y, board);
        RevealShip(board);
    }

    private void RevealShip(Board board)
    {
        _revealShipAction?.Invoke(this, board);
    }
}