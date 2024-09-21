using BattleShipAPI.Enums;

namespace BattleShipAPI.Models
{
    public class GameRoom
    {
        public string Name { get; set; } = string.Empty;
        
        public GameState State { get; set; } = GameState.NotStarted;
        
        public GameRoomSettings Settings { get; set; } = new();
        
        public Board Board { get; set; } = new();
        
        private Guid TurnPlayerId { get; set; } = Guid.Empty;
        
        public Guid GetNextTurnPlayerId(List<UserConnection> players)
        {
            var sortedUsers = players.OrderBy(x => x.Username).ToList();

            if (TurnPlayerId == Guid.Empty)
            {
                TurnPlayerId = sortedUsers[0].PlayerId;
                return TurnPlayerId;
            }
            
            var currentPlayerIndex = sortedUsers.FindIndex(x => x.PlayerId == TurnPlayerId);
            var nextPlayerIndex = currentPlayerIndex + 1;
            if (nextPlayerIndex >= sortedUsers.Count)
            {
                nextPlayerIndex = 0;
            }
            
            TurnPlayerId = sortedUsers[nextPlayerIndex].PlayerId;
            
            return TurnPlayerId;
        }
    }
}
