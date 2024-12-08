using BattleShipAPI.Enums;
using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.Models;

namespace BattleShipAPI.Command;

public class UndoPlacementCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        if (context.Db.Connections.TryGetValue(context.CallerContext.ConnectionId, out var connection)
            && context.Db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.State == GameState.PlacingShips)
        {
            var player = context.Db.Connections[context.CallerContext.ConnectionId];
            var previousMemento = player.PlacingActionHistory.Undo();
            if (previousMemento == null)
            {
                await context.NotificationService.NotifyClient(
                    context.Clients,
                    context.CallerContext.ConnectionId,
                    "FailedToUndo",
                    "No actions to undo.");
                return;
            }

            // Restore the player's placed ships
            player.RestoreFromMemento(previousMemento);

            // Restore ONLY the player's section of the board
            RestorePlayerSection(context, gameRoom, player, previousMemento.BoardState);

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

    private void RestorePlayerSection(CommandContext context, GameRoom gameRoom, UserConnection player, Board boardToRestoreFrom)
    {
        var (startX, startY) = GetSectionStartByOwner(player.PlayerId, gameRoom.Board.Cells);

        // Assuming each section is 10x10 (adjust if needed)
        int width = 10;
        int height = 10;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gameRoom.Board.Cells[startX + x][startY + y].OwnerId = boardToRestoreFrom.Cells[startX + x][startY + y].OwnerId;
                gameRoom.Board.Cells[startX + x][startY + y].State = boardToRestoreFrom.Cells[startX + x][startY + y].State;
                gameRoom.Board.Cells[startX + x][startY + y].IsRevealed = boardToRestoreFrom.Cells[startX + x][startY + y].IsRevealed;
            }
        }
    }

    private (int startX, int startY) GetSectionStartByOwner(string ownerId, Cell[][] cells)
    {
        if (cells.Length > 0 && cells[0].Length > 0 && cells[0][0]?.OwnerId == ownerId)
            return (0, 0);

        if (cells.Length > 0 && cells[0].Length > 10 && cells[0][10]?.OwnerId == ownerId)
            return (0, 10);

        if (cells.Length > 10 && cells[10].Length > 0 && cells[10][0]?.OwnerId == ownerId)
            return (10, 0);

        if (cells.Length > 10 && cells[10].Length > 10 && cells[10][10]?.OwnerId == ownerId)
            return (10, 10);

        return (-1, -1);
    }
}