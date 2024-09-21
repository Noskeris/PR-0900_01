namespace BattleShipAPI.Models
{
    public class UserConnection
    {
        public Guid PlayerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public GameRoom GameRoom { get; set; } = null!;
        public bool IsModerator { get; set; } = false;
    }
}
