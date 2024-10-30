using BattleShipAPI.Enums;

public class StartGameCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        if (context.Db.Connections.TryGetValue(context.ConnectionId, out var connection)
            && context.Db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.State == GameState.PlacingShips
            && connection.IsModerator)
        {
            var players = context.Db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();

            if (players.Count < 2)
            {
                await context.Hub.NotifyClient(
                    "FailedToStartGame",
                    "Not enough players to start the game.");

                return;
            }

            if (players.Any(x => !x.CanPlay))
            {
                await context.Hub.NotifyClient(
                    "FailedToStartGame",
                    "Not all players are ready.");

                return;
            }

            gameRoom.State = GameState.InProgress;

            context.Db.GameRooms[connection.GameRoomName] = gameRoom;

            await context.Hub.NotifyGroup(
                gameRoom.Name,
                "UpdatedSuperAttacksConfig",
                gameRoom.SuperAttacksConfig);

            await context.Hub.NotifyGroup(
                gameRoom.Name,
                "GameStateChanged",
                (int)gameRoom.State);

            var startTime = DateTime.UtcNow;
            await context.Hub.NotifyGroup(
                gameRoom.Name,
                "PlayerTurn",
                gameRoom.GetNextTurnPlayerId(players),
                startTime,
                gameRoom.TimerDuration);
        }
    }
}
