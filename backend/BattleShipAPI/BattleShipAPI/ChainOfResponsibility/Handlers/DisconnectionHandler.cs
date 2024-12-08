using BattleShipAPI.Models;
using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using BattleShipAPI.State;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.ChainOfResponsibility.Handlers;

public class DisconnectionHandler : GameHandler
{
    private readonly InMemoryDB _db;
    private readonly INotificationService _notificationService;

    public DisconnectionHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
        _db = InMemoryDB.Instance;
    }

    public override async Task HandleRequest(string action, HubCallerContext context, IHubCallerClients clients,
        object? data)
    {
        if (action == "OnDisconnected")
        {
            if (_db.Connections.TryGetValue(context.ConnectionId, out var connection))
            {
                _notificationService.Unsubscribe(context.ConnectionId);

                var players = _db.Connections.Values
                    .Where(c => c.GameRoomName == connection.GameRoomName)
                    .ToList();

                var haveAllPlayersDisconnected = players
                    .Where(x => x.PlayerId != connection.PlayerId)
                    .All(p => p.HasDisconnected);

                if (haveAllPlayersDisconnected)
                {
                    _db.GameRooms.Remove(connection.GameRoomName, out _);
                    _db.Connections.Values
                        .Where(c => c.GameRoomName == connection.GameRoomName)
                        .ToList()
                        .ForEach(x => _db.Connections.Remove(x.PlayerId, out _));

                    return;
                }

                if (connection.IsModerator)
                {
                    var newModerator = players
                        .First(x => x.PlayerId != connection.PlayerId);

                    newModerator.IsModerator = true;
                    _db.Connections[newModerator.PlayerId] = newModerator;
                    _db.Connections[connection.PlayerId].IsModerator = false;

                    await _notificationService.NotifyClient(
                        clients,
                        newModerator.PlayerId,
                        "SetModerator",
                        _db.Connections[newModerator.PlayerId].IsModerator);
                }

                if (_db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom))
                {
                    var gameContext = new GameContext(_db, _notificationService)
                    {
                        CallerContext = context,
                        Clients = clients
                    };

                    var state = GameStateFactory.GetHandler(gameRoom.State);

                    gameContext.SetState(state);
                    await gameContext.HandleDisconnection(connection);
                }

                await SendAvatarsToPlayers(clients, players, gameRoom.Name);
            }
        }
        else
        {
            await base.HandleRequest(action, context, clients, data);
        }
    }
    
    private async Task SendAvatarsToPlayers(IHubCallerClients clients, List<UserConnection> players,
        string gameRoomName)
    {
        var playerAvatars = players
            .Select(x => new AvatarResponse(
                x.Username,
                x.Avatar.GetAvatarParameters(),
                x is { CanPlay: true, HasDisconnected: false }))
            .ToList();

        await _notificationService.NotifyGroup(clients, gameRoomName, "AllAvatars", playerAvatars);
    }
}