using BattleShipAPI.Enums;

public class ReadyCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        if (context.Db.Connections.TryGetValue(context.ConnectionId, out var connection)
            && context.Db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.State == GameState.PlacingShips)
        {
            if (connection.GetAllowedShipsConfig(gameRoom.ShipsConfig).Any(x => x.Count != 0))
            {
                await context.Hub.NotifyClient(
                    "PlayerNotReady",
                    "You have not placed all your ships.");

                return;
            }

            connection.CanPlay = true;
            context.Db.Connections[context.ConnectionId] = connection;

            await context.Hub.NotifyClient(
                "PlayerIsReady",
                "You are ready to start the game.");
        }
    }
}
