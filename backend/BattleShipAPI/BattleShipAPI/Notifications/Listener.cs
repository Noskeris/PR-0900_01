namespace BattleShipAPI.Notifications;

public record Listener
{
    public string GroupName { get; set; }

    public string ClientId { get; set; }
}
