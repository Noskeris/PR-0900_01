using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.Boards;

public class TwoPlayersBoard : Board
{
    protected override List<Section> BoardSections { get; }

    public TwoPlayersBoard() : base(20, 10)
    {
        BoardSections =
        [
            new Section(0, 0, 9, 9),
            new Section(10, 0, 19, 9)
        ];
    }
}