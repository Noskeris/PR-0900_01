using BattleShipAPI.Enums;

namespace BattleShipAPI.Models
{
    public class GameRoom
    {
        public string GameRoomString { get; set; } = string.Empty;
        public GameState GameState { get; set; } = GameState.NotStarted;

    }
}
