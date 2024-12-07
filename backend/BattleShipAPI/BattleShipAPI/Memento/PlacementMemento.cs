using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.Iterator;

public class PlacementMemento
{
    public PlacedShipsCollection PlacedShips { get; }
    public Board BoardState { get; }

    public PlacementMemento(PlacedShipsCollection placedShips, Board boardState)
    {
        PlacedShips = placedShips;
        BoardState = boardState;
    }
}
