using BattleShipAPI.Models;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.State;

public class NotStartedState : IGameState
{
    public async Task HandleDisconnection(GameContext context, UserConnection connection, HubCallerContext callerContext, IHubCallerClients clients)
    {
        context.Db.Connections.Remove(callerContext.ConnectionId, out _);
        await context.NotificationService.NotifyGroup(
            clients,
            connection.GameRoomName,
            "PlayerDisconnected",
            $"Player {connection.Username} has disconnected.");
    }

    public async Task RestartGame(GameContext context, UserConnection connection, IHubCallerClients clients)
    {

    }
}
