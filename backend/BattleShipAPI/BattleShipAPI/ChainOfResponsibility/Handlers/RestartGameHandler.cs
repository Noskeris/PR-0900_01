using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using BattleShipAPI.State;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.ChainOfResponsibility.Handlers;

public class RestartGameHandler : GameHandler
{
    private readonly InMemoryDB _db;
    private readonly INotificationService _notificationService;
    
    public RestartGameHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
        _db = InMemoryDB.Instance;
    }
    
    public override async Task HandleRequest(string action, HubCallerContext context, IHubCallerClients clients, object? data)
    {
        if (action == "RestartGame")
        {
            if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom))
            {
                var gameContext = new GameContext(_db, _notificationService)
                {
                    Clients = clients
                };

                var state = GameStateFactory.GetHandler(gameRoom.State);

                gameContext.SetState(state);
                await gameContext.RestartGame(connection);
            }
        }
        else
        {
            await base.HandleRequest(action, context, clients, data);
        }
    }
}
