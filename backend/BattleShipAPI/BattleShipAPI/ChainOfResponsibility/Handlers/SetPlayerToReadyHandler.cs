using BattleShipAPI.Adapter.Logs;
using BattleShipAPI.Enums;
using BattleShipAPI.Notifications;
using BattleShipAPI.Proxy;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.ChainOfResponsibility.Handlers;

public class SetPlayerToReadyHandler : GameHandler
{
    private readonly InMemoryDB _db;
    private readonly INotificationService _notificationService;
    private readonly ILoggerOnReceive _loggerOnReceive;
    private readonly ILoggerOnSend _loggerOnSend;

    public SetPlayerToReadyHandler(INotificationService notificationService, ILoggerOnReceive loggerOnReceive,
        ILoggerOnSend loggerOnSend)
    {
        _notificationService = notificationService;
        _loggerOnReceive = loggerOnReceive;
        _loggerOnSend = loggerOnSend;
        _db = InMemoryDB.Instance;
    }

    public override async Task HandleRequest(string action, HubCallerContext context, IHubCallerClients clients,
        object? data)
    {
        if (action == "SetPlayerToReady")
        {
            _loggerOnReceive.WriteLog(new LogEntry
            {
                Message = $"Received SetPlayerToReady request from {context.ConnectionId}",
            });

            if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.PlacingShips)
            {
                if (connection.GetAllowedShipsConfig(gameRoom.ShipsConfig).Any(x => x.Count != 0))
                {
                    await _notificationService.NotifyClient(
                        clients,
                        context.ConnectionId,
                        "PlayerNotReady",
                        "You have not placed all your ships.");

                    _loggerOnSend.WriteLog(new LogEntry
                    {
                        Message = $"Sent PlayerNotReady to {connection.Username}",
                    });

                    return;
                }

                connection.CanPlay = true;
                _db.Connections[context.ConnectionId] = connection;

                await _notificationService.NotifyClient(
                    clients,
                    context.ConnectionId,
                    "PlayerIsReady",
                    "You are ready to start the game.");

                _loggerOnSend.WriteLog(new LogEntry
                {
                    Message = $"Sent PlayerIsReady to {connection.Username}",
                });
            }
        }
        else
        {
            await base.HandleRequest(action, context, clients, data);
        }
    }
}