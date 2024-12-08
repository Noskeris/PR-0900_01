using BattleShipAPI.Enums;
using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.ChainOfResponsibility.Handlers;

public class PlayerTurnTimeEndedHandler : GameHandler
{
    private readonly InMemoryDB _db;
    private readonly INotificationService _notificationService;
    
    public PlayerTurnTimeEndedHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
        _db = InMemoryDB.Instance;
    }
    
    public override async Task HandleRequest(string action, HubCallerContext context, IHubCallerClients clients, object? data)
    {
        if (action == "PlayerTurnTimeEnded")
        {
            if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.TurnPlayerId.Equals(connection.PlayerId)
                && gameRoom.State == GameState.InProgress)
            {
                _db.GameRooms[connection.GameRoomName] = gameRoom;
                var players = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();

                var startTime = DateTime.UtcNow;
                await _notificationService.NotifyGroup(
                    clients,
                    gameRoom.Name,
                    "PlayerTurn",
                    gameRoom.GetNextTurnPlayerId(players),
                    startTime,
                    gameRoom.TimerDuration);
            }
        }
        else
        {
            await base.HandleRequest(action, context, clients, data);
        }
    }
}
