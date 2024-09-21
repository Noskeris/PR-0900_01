using BattleShipAPI.Enums;

namespace BattleShipAPI.Models
{
    public class GameRoom
    {
        public string Name { get; set; } = string.Empty;
        public GameState State { get; set; } = GameState.NotStarted;
        public GameRoomSettings Settings { get; set; } = new();
        
        public Board Board { get; set; } = new();
    }
}
