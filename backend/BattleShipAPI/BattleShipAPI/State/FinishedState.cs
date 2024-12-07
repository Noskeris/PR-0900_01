using BattleShipAPI.Models;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.State;

public class FinishedState : IGameState
{
    public async Task HandleDisconnection(GameContext context, UserConnection connection, HubCallerContext callerContext,
        IHubCallerClients clients)
    {
        connection.HasDisconnected = true;
        context.Db.Connections[callerContext.ConnectionId] = connection;

        await context.NotificationService.NotifyGroup(
            clients,
            connection.GameRoomName,
            "PlayerDisconnected",
            $"Player {connection.Username} has disconnected.");
    }
    
    public async Task RestartGame(GameContext context, UserConnection connection, IHubCallerClients clients)
    {
        var gameRoom = context.Db.GameRooms[connection.GameRoomName];

        if (connection.IsModerator)
        {
            context.Db.Connections.Values
                .Where(c => c.GameRoomName == connection.GameRoomName && c.HasDisconnected)
                .ToList()
                .ForEach(x => context.Db.Connections.Remove(x.PlayerId, out _));
            
            gameRoom = new GameRoom { Name = gameRoom.Name };
            context.Db.GameRooms[gameRoom.Name] = gameRoom;

            context.Db.Connections.Values
                .Where(c => c.GameRoomName == gameRoom.Name)
                .ToList()
                .ForEach(x =>
                {
                    x.CanPlay = false;
                    x.HasDisconnected = false;
                    x.PlacedShips.Clear();
                    x.UsedSuperAttacks.Clear();
                    x.PlacingActionHistory = new();
                });

            await context.NotificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "GameStateChanged",
                (int)gameRoom.State);

            await context.NotificationService.NotifyClient(
                clients,
                connection.PlayerId,
                "CurrentGameConfiguration",
                gameRoom.ShipsConfig);
        }
    }
}