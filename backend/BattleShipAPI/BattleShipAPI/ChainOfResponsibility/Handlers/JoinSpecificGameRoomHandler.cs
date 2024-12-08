using BattleShipAPI.Adapter.Logs;
using BattleShipAPI.Enums;
using BattleShipAPI.Models;
using BattleShipAPI.Notifications;
using BattleShipAPI.Proxy;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.ChainOfResponsibility.Handlers;

public class JoinSpecificGameRoomHandler : GameHandler
{
    private readonly InMemoryDB _db;
    private readonly INotificationService _notificationService;
    private readonly ILoggerOnReceive _loggerOnReceive;
    private readonly ILoggerOnSend _loggerOnSend;

    public JoinSpecificGameRoomHandler(INotificationService notificationService, ILoggerOnReceive loggerOnReceive,
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
        if (action == "JoinSpecificGameRoom" && data is UserConnection connection)
        {
            _loggerOnReceive.WriteLog(new LogEntry
            {
                Message = $"Received JoinSpecificGameRoom request from {connection.Username}",
            });

            if (_db.Connections.Values.Any(c =>
                    c.GameRoomName == connection.GameRoomName && c.Username == connection.Username))
            {
                await _notificationService.NotifyClient(
                    clients,
                    context.ConnectionId,
                    "JoinFailed",
                    "Username already taken in this room");

                _loggerOnSend.WriteLog(new LogEntry
                {
                    Message = $"Sent JoinFailed to {connection.Username}",
                });

                return;
            }

            if (_db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom) &&
                (gameRoom.State != GameState.NotStarted && gameRoom.State != GameState.GameModeConfirmed))
            {
                await _notificationService.NotifyClient(
                    clients,
                    context.ConnectionId,
                    "JoinFailed",
                    "Game has already started");

                _loggerOnSend.WriteLog(new LogEntry
                {
                    Message = $"Sent JoinFailed to {connection.Username}",
                });

                return;
            }

            var usersInRoom = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();

            if (usersInRoom.Count == 4)
            {
                await _notificationService.NotifyClient(
                    clients,
                    context.ConnectionId,
                    "JoinFailed",
                    "Game room is full.");

                _loggerOnSend.WriteLog(new LogEntry
                {
                    Message = $"Sent JoinFailed to {connection.Username}",
                });

                return;
            }

            connection.PlayerId = context.ConnectionId;

            _notificationService.Subscribe(new Listener
            {
                ClientId = context.ConnectionId,
                GroupName = connection.GameRoomName
            });

            if (usersInRoom.Count == 0)
            {
                connection.IsModerator = true;
                _db.GameRooms[connection.GameRoomName] = new GameRoom() { Name = connection.GameRoomName };
            }

            _db.Connections[context.ConnectionId] = connection;

            await _notificationService.NotifyClient(
                clients,
                context.ConnectionId,
                "SetModerator",
                connection.IsModerator);

            _loggerOnSend.WriteLog(new LogEntry
            {
                Message = $"Sent SetModerator to {connection.Username}",
            });

            await _notificationService.NotifyClient(
                clients,
                context.ConnectionId,
                "ReceivePlayerId",
                connection.PlayerId);

            _loggerOnSend.WriteLog(new LogEntry
            {
                Message = $"Sent ReceivePlayerId to {connection.Username}",
            });

            await _notificationService.NotifyGroup(
                clients,
                connection.GameRoomName,
                "JoinSpecificGameRoom",
                "admin",
                $"{connection.Username} has joined the game room {connection.GameRoomName}");

            _loggerOnSend.WriteLog(new LogEntry
            {
                Message = $"Sent JoinSpecificGameRoom to {connection.Username}",
            });
        }
        else
        {
            await base.HandleRequest(action, context, clients, data);
        }
    }
}