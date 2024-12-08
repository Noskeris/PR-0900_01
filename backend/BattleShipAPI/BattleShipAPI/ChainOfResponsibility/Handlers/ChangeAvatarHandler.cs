using BattleShipAPI.Bridge;
using BattleShipAPI.Enums;
using BattleShipAPI.Enums.Avatar;
using BattleShipAPI.Models;
using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.ChainOfResponsibility.Handlers;

public class ChangeAvatarHandler : GameHandler
{
    private readonly InMemoryDB _db;
    private readonly INotificationService _notificationService;

    public ChangeAvatarHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
        _db = InMemoryDB.Instance;
    }

    public override async Task HandleRequest(string action, HubCallerContext context, IHubCallerClients clients,
        object? data)
    {
        if (action == "ChangeAvatar" && data is AvatarRequest avatarRequest)
        {
            if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.PlacingShips)
            {
                var avatar = AvatarFactory.CreateAvatar(avatarRequest.HeadType, avatarRequest.AppearanceType);
                _db.Connections[context.ConnectionId].Avatar = avatar;

                await _notificationService.NotifyClient(
                    clients,
                    connection.PlayerId,
                    "SetPlayerAvatarConfigs",
                    avatar.GetAvatarParameters());
            }
        }
        else
        {
            await base.HandleRequest(action, context, clients, data);
        }
    }
}