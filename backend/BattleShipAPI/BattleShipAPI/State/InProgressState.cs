using BattleShipAPI.Enums;
using BattleShipAPI.Models;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.State;

public class InProgressState : IGameState
{
    public async Task HandleDisconnection(GameContext context, UserConnection connection, HubCallerContext callerContext,
        IHubCallerClients clients)
    {
        connection.HasDisconnected = true;
        connection.CanPlay = false;

        var gameRoom = context.Db.GameRooms[connection.GameRoomName];
        gameRoom.SinkAllShips(connection);

        var players = context.Db.Connections.Values
            .Where(c => c.GameRoomName == connection.GameRoomName)
            .ToList();

        await context.NotificationService.NotifyGroup(
            clients,
            gameRoom.Name,
            "BoardUpdated",
            gameRoom.Name,
            gameRoom.Board);

        if (gameRoom.TurnPlayerId == connection.PlayerId)
        {
            var startTime = DateTime.UtcNow;
            await context.NotificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "PlayerTurn",
                gameRoom.GetNextTurnPlayerId(players),
                startTime,
                gameRoom.TimerDuration);
        }

        context.Db.Connections[callerContext.ConnectionId] = connection;
        await context.NotificationService.NotifyGroup(
            clients,
            connection.GameRoomName,
            "PlayerDisconnected",
            $"Player {connection.Username} has disconnected.");

        if (players.Where(p => p.PlayerId != gameRoom.TurnPlayerId).All(p => !p.CanPlay))
        {
            gameRoom.State = GameState.Finished;
            await context.NotificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "AttackResult",
                $"{connection.Username} won the game!");
            await context.NotificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "GameStateChanged",
                (int)gameRoom.State);
        }
    }

    public async Task RestartGame(GameContext context, UserConnection connection, IHubCallerClients clients)
    {
        
    }
}