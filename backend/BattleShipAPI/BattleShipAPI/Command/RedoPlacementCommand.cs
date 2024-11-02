using BattleShipAPI.Enums;

public class RedoPlacementCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        if (context.Db.Connections.TryGetValue(context.CallerContext.ConnectionId, out var connection)
            && context.Db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.State == GameState.PlacingShips)
        {
            var player = context.Db.Connections[context.CallerContext.ConnectionId];
            var nextState = player.PlacingActionHistory.Redo();
            if (nextState == null)
            {
                await context.NotificationService.NotifyClient(
                    context.Clients,
                    context.CallerContext.ConnectionId,
                    "FailedToRedo",
                    "No actions to redo.");
                return;
            }

            // Restore the board and placed ships
            player.PlacedShips = nextState.Value.PlacedShips;
            gameRoom.SetBoard(nextState.Value.BoardState);

            context.Db.GameRooms[gameRoom.Name] = gameRoom;
            context.Db.Connections[context.CallerContext.ConnectionId] = player;

            await context.NotificationService.NotifyGroup(
                context.Clients,
                gameRoom.Name,
                "BoardUpdated",
                gameRoom.Name,
                gameRoom.Board);

            await context.NotificationService.NotifyClient(
                context.Clients,
                context.CallerContext.ConnectionId,
                "UpdatedShipsConfig",
                player.GetAllowedShipsConfig(gameRoom.ShipsConfig));
        }
    }
}
