using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.Iterator;

namespace BattleShipAPI.Memento;

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