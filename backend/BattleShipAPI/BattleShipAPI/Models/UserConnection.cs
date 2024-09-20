namespace BattleShipAPI.Models
{
    public class UserConnection
    {
        public int PlayerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public GameRoom Room { get; set; }
        public bool IsModerator { get; set; } = false;
    }
}
