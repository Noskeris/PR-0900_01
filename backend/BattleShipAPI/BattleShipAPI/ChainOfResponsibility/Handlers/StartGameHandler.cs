using BattleShipAPI.Enums;
using BattleShipAPI.Models;
using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.ChainOfResponsibility.Handlers;

public class StartGameHandler : GameHandler
{
    private readonly InMemoryDB _db;
    private readonly INotificationService _notificationService;
    
    public StartGameHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
        _db = InMemoryDB.Instance;
    }
    
    public override async Task HandleRequest(string action, HubCallerContext context, IHubCallerClients clients, object? data)
    {
        if (action == "StartGame")
        {
            if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
                        && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                        && gameRoom.State == GameState.PlacingShips
                        && connection.IsModerator)
                    {
                        var players = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();
            
                        if (players.Count < 2)
                        {
                            await _notificationService.NotifyClient(
                                clients,
                                context.ConnectionId,
                                "FailedToStartGame",
                                "Not enough players to start the game.");
            
                            return;
                        }
            
                        if (players.Any(x => !x.CanPlay))
                        {
                            await _notificationService.NotifyClient(
                                clients,
                                context.ConnectionId,
                                "FailedToStartGame",
                                "Not all players are ready.");
            
                            return;
                        }
            
                        gameRoom.State = GameState.InProgress;
            
                        _db.GameRooms[connection.GameRoomName] = gameRoom;
            
                        await _notificationService.NotifyGroup(
                            clients,
                            gameRoom.Name,
                            "UpdatedSuperAttacksConfig",
                            gameRoom.SuperAttacksConfig);
            
                        await _notificationService.NotifyGroup(
                            clients,
                            gameRoom.Name,
                            "GameStateChanged",
                            (int)gameRoom.State);

                        await SendAvatarsToPlayers(clients, players, gameRoom.Name);
                        
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
    
    private async Task SendAvatarsToPlayers(IHubCallerClients clients, List<UserConnection> players, string gameRoomName)
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