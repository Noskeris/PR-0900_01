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
            var nextMemento = player.PlacingActionHistory.Redo();
            if (nextMemento == null)
            {
                await context.NotificationService.NotifyClient(
                    context.Clients,
                    context.CallerContext.ConnectionId,
                    "FailedToRedo",
                    "No actions to redo.");
                return;
            }

            // Restore the player's state from the memento
            player.RestoreFromMemento(nextMemento);

            // Restore the board state from the memento
            gameRoom.SetBoard(nextMemento.BoardState);

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
