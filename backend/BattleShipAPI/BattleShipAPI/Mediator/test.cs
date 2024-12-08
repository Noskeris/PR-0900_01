using BattleShipAPI.AttackStrategy;
using BattleShipAPI.Enums;
using BattleShipAPI.Helpers;
using BattleShipAPI.Models;
using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

public class AttackCellHandlerh : GameHandler
{
    private readonly InMemoryDB _db;
    private readonly INotificationService _notificationService;

    public AttackCellHandlerh(INotificationService notificationService)
    {
        _notificationService = notificationService;
        _db = InMemoryDB.Instance;
    }

    public override async Task HandleRequest(string action, HubCallerContext context, IHubCallerClients clients,
        object? data)
    {
        if (action == "AttackCell" && data is AttackRequest attackRequest)
        {
            if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.InProgress
                && gameRoom.TurnPlayerId == connection.PlayerId)
            {
                var players = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();
                var cell = gameRoom.Board.Cells[attackRequest.X][attackRequest.Y];

                // Call mediator PerformAttackRequestValidation with parameters cell and connection
            }
        }
        else
        {
            await base.HandleRequest(action, context, clients, data);
        }
    }

    public void ContinueAttackHandling()
    {
        // Call mediator PerformAttack
    }

    public void FinishAttackHandling()
    {
        var playerAvatars = players
            .Select(x => new AvatarResponse(
                x.Username,
                x.Avatar.GetAvatarParameters(),
                x is { CanPlay: true, HasDisconnected: false }))
            .ToList();
        
        // Call mediator InformGroupAboutAllAvatars with parameters clients, gameRoomName, playerAvatars
        //await _notificationService.NotifyGroup(clients, gameRoomName, "AllAvatars", playerAvatars);
    }
}