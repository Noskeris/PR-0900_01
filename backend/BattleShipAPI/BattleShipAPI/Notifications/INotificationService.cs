using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Notifications;

public interface INotificationService
{
    void Subscribe(Listener listener);
    void Unsubscribe(string clientId);
    Task NotifyGroup(IHubCallerClients clients, string groupName, string key, params object?[] values);
    Task NotifyClient(IHubCallerClients clients, string clientId, string key, params object?[] values);
}
