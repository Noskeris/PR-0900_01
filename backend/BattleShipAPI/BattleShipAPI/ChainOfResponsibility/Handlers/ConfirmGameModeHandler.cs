using BattleShipAPI.Adapter.Logs;
using BattleShipAPI.Enums;
using BattleShipAPI.Notifications;
using BattleShipAPI.Proxy;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.ChainOfResponsibility.Handlers;

public class ConfirmGameModeHandler : GameHandler
{
    private readonly InMemoryDB _db;
    private readonly INotificationService _notificationService;
    private readonly ILoggerOnReceive _loggerOnReceive;
    private readonly ILoggerOnSend _loggerOnSend;
    
    public ConfirmGameModeHandler(INotificationService notificationService, ILoggerOnReceive loggerOnReceive,
        ILoggerOnSend loggerOnSend)
    {
        _notificationService = notificationService;
        _loggerOnReceive = loggerOnReceive;
        _loggerOnSend = loggerOnSend;
        _db = InMemoryDB.Instance;
    }
    
    public override async Task HandleRequest(string action, HubCallerContext context, IHubCallerClients clients, object? data)
    {
        if (action == "ConfirmGameMode" && data is GameMode gameMode)
        {
            _loggerOnReceive.WriteLog(new LogEntry
            {
                Message = $"Received ConfirmGameMode request from {context.ConnectionId}",
            });

            if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.NotStarted)
            {
                gameRoom.Mode = gameMode;
                gameRoom.State = GameState.GameModeConfirmed;

                await _notificationService.NotifyGroup(
                    clients,
                    gameRoom.Name,
                    "GameStateChanged",
                    (int)gameRoom.State);

                _loggerOnSend.WriteLog(new LogEntry
                {
                    Message = $"Sent GameStateChanged to {gameRoom.Name}",
                });
            }
        }
        else
        {
            await base.HandleRequest(action, context, clients, data);
        }
    }
}
