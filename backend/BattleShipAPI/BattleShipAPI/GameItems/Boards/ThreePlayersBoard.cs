using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.Boards;

public class ThreePlayersBoard : Board
{
    public ThreePlayersBoard(List<UserConnection> players) : base(30, 10)
    {
        var player1Of3 = players[0];
        var player2Of3 = players[1];
        var player3Of3 = players[2];

        AssignBoardSection(0, 0, 9, 9, player1Of3.PlayerId);
        AssignBoardSection(10, 0, 19, 9, player2Of3.PlayerId);
        AssignBoardSection(20, 0, 29, 9, player3Of3.PlayerId);
    }
}