using BattleShipAPI.Enums;
using BattleShipAPI.Models;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.ChainOfResponsibility.Handlers;

public class AttackCellHandler : GameHandler
{
    private readonly InMemoryDB _db;
    public AttackCellHandler()
    {
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
                await _mediator.Notify("ValidateAttackCellRequest", new AttackInformation()
                {
                    Connection = connection,
                    Clients = clients,
                    Context = context,
                    GameRoom = gameRoom,
                    AttackRequest = attackRequest
                });
            }
        }
        else
        {
            await base.HandleRequest(action, context, clients, data);
        }
    }

    public async Task ContinueAttackHandling(AttackInformation request)
    {
        await _mediator.Notify("PerformCellAttack", request);
    }

    public async Task FinishAttackHandling(AttackInformation request)
    {
        var players = _db.Connections.Values
            .Where(c => c.GameRoomName == request.Connection.GameRoomName)
            .ToList();
        
        var playerAvatars = players
            .Select(x => new AvatarResponse(
                x.Username,
                x.Avatar.GetAvatarParameters(),
                x is { CanPlay: true, HasDisconnected: false }))
            .ToList();
        
        await _mediator.Notify("InformGroup", new InformGroupRequest()
        {
            Clients = request.Clients,
            GroupName = request.GameRoom.Name,
            Key = "AllAvatars",
            Values = [playerAvatars]
        });
    }
}