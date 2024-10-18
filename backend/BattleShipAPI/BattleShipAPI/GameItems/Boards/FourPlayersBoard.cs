using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.Boards;

public class FourPlayersBoard : Board
{
    protected override List<Section> BoardSections { get; }

    public FourPlayersBoard() : base(20, 20)
    {
        BoardSections =
        [
            new Section(0, 0, 9, 9),
            new Section(0, 10, 9, 19),
            new Section(10, 0, 19, 9),
            new Section(10, 10, 19, 19)
        ];
    }
}