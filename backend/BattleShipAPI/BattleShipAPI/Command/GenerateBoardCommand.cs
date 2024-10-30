using BattleShipAPI.Enums;
using BattleShipAPI.Factories;

public class GenerateBoardCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        if (context.Db.Connections.TryGetValue(context.ConnectionId, out var connection)
            && context.Db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.State == GameState.GameModeConfirmed
            && connection.IsModerator)
        {
            var players = context.Db.Connections.Values
                .Where(c => c.GameRoomName == connection.GameRoomName)
                .ToList();

            if (players.Count < 2)
            {
                await context.Hub.NotifyClient(
                    "CannotGenerateBoard",
                    "Not enough players to generate the board.");
                return;
            }

            var gameRoomSettings = GameRoomSettingsCreator.GetGameRoomSettings(players, gameRoom.Mode);
            gameRoom.SetSettings(gameRoomSettings);

            gameRoom.State = GameState.PlacingShips;
            context.Db.GameRooms[gameRoom.Name] = gameRoom;

            await context.Hub.NotifyGroup(
                gameRoom.Name,
                "UpdatedShipsConfig",
                gameRoom.ShipsConfig);

            await context.Hub.NotifyGroup(
                gameRoom.Name,
                "GameStateChanged",
                (int)gameRoom.State);

            await context.Hub.NotifyGroup(
                gameRoom.Name,
                "BoardGenerated",
                gameRoom.Name,
                gameRoom.Board);
        }
    }
}
