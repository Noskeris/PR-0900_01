using BattleShipAPI.Models;
using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.State;

public class GameContext
{
    private IGameState _currentState;

    public InMemoryDB Db { get; }
    public INotificationService NotificationService { get; }
    public HubCallerContext CallerContext { get; set; }
    public IHubCallerClients Clients { get; set; }

    public GameContext(InMemoryDB db, INotificationService notificationService)
    {
        Db = db;
        NotificationService = notificationService;
    }

    public void SetState(IGameState state)
    {
        _currentState = state;
    }

    public Task HandleDisconnection(UserConnection connection)
    {
        return _currentState.HandleDisconnection(this, connection, CallerContext, Clients);
    }
    
    public Task RestartGame(UserConnection connection)
    {
        return _currentState.RestartGame(this, connection, Clients);
    }
}
