using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.Boards;

public class TwoPlayersBoard : Board
{
    public TwoPlayersBoard(List<UserConnection> players) : base(20, 10)
    {
        var player1Of2 = players[0];
        var player2Of2 = players[1];

        AssignBoardSection(0, 0, 9, 9, player1Of2.PlayerId);
        AssignBoardSection(10, 0, 19, 9, player2Of2.PlayerId);
    }
}