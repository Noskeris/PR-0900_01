using BattleShipAPI.Enums;

namespace BattleShipAPI.Models
{
    public class GameRoom
    {
        public string GameRoomName { get; set; } = string.Empty;
        public GameState GameState { get; set; } = GameState.NotStarted;

    }
}
