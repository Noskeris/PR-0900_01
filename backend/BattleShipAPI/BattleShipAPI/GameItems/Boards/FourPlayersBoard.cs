using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.Boards;

public class FourPlayersBoard : Board
{
    public FourPlayersBoard(List<UserConnection> players) : base(20, 20)
    {
        var player1Of4 = players[0];
        var player2Of4 = players[1];
        var player3Of4 = players[2];
        var player4Of4 = players[3];

        AssignBoardSection(0, 0, 9, 9, player1Of4.PlayerId);
        AssignBoardSection(0, 10, 9, 19, player2Of4.PlayerId);
        AssignBoardSection(10, 0, 19, 9, player3Of4.PlayerId);
        AssignBoardSection(10, 10, 19, 19, player4Of4.PlayerId);
    }
}