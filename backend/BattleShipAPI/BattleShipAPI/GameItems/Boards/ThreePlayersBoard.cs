using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.Boards;

public class ThreePlayersBoard : Board
{
    protected override List<Section> BoardSections { get; }

    public ThreePlayersBoard() : base(10, 30)
    {
        BoardSections =
        [
            new Section(0, 0, 9, 9),
            new Section(0, 10, 9, 19),
            new Section(0, 20, 9, 29)
        ];
    }
}