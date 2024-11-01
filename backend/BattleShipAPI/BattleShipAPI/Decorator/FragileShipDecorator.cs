using BattleShipAPI.Enums;


public class FragileShipDecorator : PlacedShipDecorator
{
    public FragileShipDecorator(IPlacedShip placedShip) : base(placedShip)
    {
    }

    public override void Hit(int x, int y)
    {
        // When hit, mark all coordinates as hit
        foreach (var coord in GetCoordinates())
        {
            _placedShip.Hit(coord.x, coord.y);
        }
    }
}

