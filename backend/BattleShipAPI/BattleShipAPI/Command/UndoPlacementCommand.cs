using BattleShipAPI.Enums;

public class UndoPlacementCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        if (context.Db.Connections.TryGetValue(context.ConnectionId, out var connection)
            && context.Db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.State == GameState.PlacingShips)
        {
            var player = context.Db.Connections[context.ConnectionId];
            var previousState = player.PlacingActionHistory.Undo();
            if (previousState == null)
            {
                await context.Hub.NotifyClient(
                    "FailedToUndo",
                    "No actions to undo.");
                return;
            }

            // Restore the board and placed ships
            player.PlacedShips = previousState.Value.PlacedShips;

            gameRoom.SetBoard(previousState.Value.BoardState);
            context.Db.GameRooms[gameRoom.Name] = gameRoom;
            context.Db.Connections[context.ConnectionId] = player;

            await context.Hub.NotifyGroup(
                gameRoom.Name,
                "BoardUpdated",
                gameRoom.Name,
                previousState.Value.BoardState);

            await context.Hub.NotifyClient(
                "UpdatedShipsConfig",
                player.GetAllowedShipsConfig(gameRoom.ShipsConfig));
        }
    }
}
