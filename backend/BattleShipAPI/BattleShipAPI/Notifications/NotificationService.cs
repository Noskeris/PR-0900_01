using BattleShipAPI.Iterator;
using BattleShipAPI.Mediator;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Notifications;

// 5. DESIGN PATTERN: Observer
public class NotificationService : BaseComponent, INotificationService
{
    private readonly InMemoryDB _db;

    public NotificationService()
    {
        _db = InMemoryDB.Instance;
    }

    public void Subscribe(Listener listener)
    {
        _db.Listeners[listener] = true;
    }

    public void Unsubscribe(string clientId)
    {
        _db.Listeners.Keys.Where(x => x.ClientId == clientId)
            .ToList()
            .ForEach(listener => _db.Listeners.TryRemove(listener, out _));
    }

    public async Task NotifyGroup(IHubCallerClients clients, string groupName, string key, params object?[] values)
    {
        var listenerCollection = new ListenerCollection(_db.Listeners);
        var iterator = listenerCollection.CreateIterator();

        while (iterator.HasNext())
        {
            var listener = iterator.Next();
            if (listener.GroupName == groupName)
            {
                await clients.Client(listener.ClientId).SendCoreAsync(key, values);
            }
        }
    }

    public async Task NotifyClient(IHubCallerClients clients, string clientId, string key, params object?[] values)
    {
        await clients.Client(clientId).SendCoreAsync(key, values);
    }
}
