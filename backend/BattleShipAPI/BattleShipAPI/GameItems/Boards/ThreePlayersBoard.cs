using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.Boards;

public class ThreePlayersBoard : Board
{
    public ThreePlayersBoard(List<UserConnection> players) : base(10, 30)
    {
        var player1Of3 = players[0];
        var player2Of3 = players[1];
        var player3Of3 = players[2];

        AssignBoardSection(0, 0, 9, 9, player1Of3.PlayerId);
        AssignBoardSection(0, 10, 9, 19, player2Of3.PlayerId);
        AssignBoardSection(0, 20, 9, 29, player3Of3.PlayerId);
    }
}