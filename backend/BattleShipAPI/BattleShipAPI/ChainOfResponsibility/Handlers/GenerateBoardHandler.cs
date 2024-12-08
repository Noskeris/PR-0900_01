using BattleShipAPI.Enums;
using BattleShipAPI.Factories;
using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.ChainOfResponsibility.Handlers;

public class GenerateBoardHandler : GameHandler
{
    private readonly InMemoryDB _db;
    private readonly INotificationService _notificationService;
    
    public GenerateBoardHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
        _db = InMemoryDB.Instance;
    }
    
    public override async Task HandleRequest(string action, HubCallerContext context, IHubCallerClients clients, object? data)
    {
        if (action == "GenerateBoard")
        {
            if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.GameModeConfirmed
                && connection.IsModerator)
            {
                var players = _db.Connections.Values
                    .Where(c => c.GameRoomName == connection.GameRoomName)
                    .ToList();

                if (players.Count < 2)
                    return;

                var gameRoomSettings = GameRoomSettingsCreator.GetGameRoomSettings(players, gameRoom.Mode);
                gameRoom.SetSettings(gameRoomSettings);

                gameRoom.State = GameState.PlacingShips;
                _db.GameRooms[gameRoom.Name] = gameRoom;

                await _notificationService.NotifyGroup(
                    clients,
                    gameRoom.Name,
                    "UpdatedShipsConfig",
                    gameRoom.ShipsConfig);

                await _notificationService.NotifyGroup(
                    clients,
                    gameRoom.Name,
                    "GameStateChanged",
                    (int)gameRoom.State);

                await _notificationService.NotifyGroup(
                    clients,
                    gameRoom.Name,
                    "BoardGenerated",
                    gameRoom.Name,
                    gameRoom.Board);

                foreach (var player in _db.Connections.Values.Where(x => x.GameRoomName == gameRoom.Name))
                {
                    await _notificationService.NotifyClient(
                        clients,
                        player.PlayerId,
                        "SetPlayerAvatarConfigs",
                        player.Avatar.GetAvatarParameters());
                }
            }
        }
        else
        {
            await base.HandleRequest(action, context, clients, data);
        }
    }
}