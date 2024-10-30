using BattleShipAPI.Enums;

public class ConfirmGameModeCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        if (args.Length > 1 && Enum.TryParse<GameMode>(args[1], true, out var gameMode))
        {
            if (context.Db.Connections.TryGetValue(context.ConnectionId, out var connection)
                && context.Db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.NotStarted)
            {
                var players = context.Db.Connections.Values
                    .Where(c => c.GameRoomName == connection.GameRoomName)
                    .ToList();

                gameRoom.Mode = gameMode;
                gameRoom.State = GameState.GameModeConfirmed;

                await context.Hub.NotifyGroup(
                    gameRoom.Name,
                    "GameStateChanged",
                    (int)gameRoom.State);
            }
        }
        else
        {
            await context.Hub.NotifyClient(
                "InvalidGameMode",
                "Invalid game mode specified.");
        }
    }
}
