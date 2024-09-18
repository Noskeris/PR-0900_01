namespace BattleShipAPI.Models
{
    public class UserConnection
    {
        public int PlayerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string GameRoom { get; set; } = string.Empty;
        public bool IsModerator { get; set; } = false;
    }
}
