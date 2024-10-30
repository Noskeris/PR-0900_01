using BattleShipAPI.Enums;
using BattleShipAPI.Models;

public class AddShipCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        if (args.Length >= 5 &&
            Enum.TryParse<ShipType>(args[1], true, out var shipType) &&
            int.TryParse(args[2], out var startX) &&
            int.TryParse(args[3], out var startY) &&
            Enum.TryParse<ShipOrientation>(args[4], true, out var shipOrientation))
        {
            if (context.Db.Connections.TryGetValue(context.ConnectionId, out var connection)
                && context.Db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.PlacingShips)
            {
                int endX = startX;
                int endY = startY;

                var shipConfig = gameRoom.ShipsConfig.FirstOrDefault(config => config.ShipType == shipType);

                if (shipConfig == null)
                {
                    await context.Hub.NotifyClient(
                        "FailedToAddShip",
                        "No available ships in config.");
                    return;
                }

                int shipSize = shipConfig.Size;

                if (shipOrientation == ShipOrientation.Horizontal)
                {
                    endX = startX + shipSize - 1;
                }
                else if (shipOrientation == ShipOrientation.Vertical)
                {
                    endY = startY + shipSize - 1;
                }

                var placedShip = new PlacedShip
                {
                    ShipType = shipType,
                    StartX = startX,
                    StartY = startY,
                    EndX = endX,
                    EndY = endY
                };

                // Implement the logic of adding a ship
                var board = gameRoom.Board;
                var player = context.Db.Connections[context.ConnectionId];

                if (player.PlacedShips.Count(x => x.ShipType == placedShip.ShipType) == shipConfig.Count)
                {
                    await context.Hub.NotifyClient(
                        "FailedToAddShip",
                        "No ships of this type left.");

                    return;
                }

                player.PlacingActionHistory.AddInitialState(player.PlacedShips, gameRoom.Board);

                if (!board.TryPutShipOnBoard(
                        placedShip.StartX,
                        placedShip.StartY,
                        placedShip.EndX,
                        placedShip.EndY,
                        player.PlayerId))
                {
                    await context.Hub.NotifyClient(
                        "FailedToAddShip",
                        "Failed to add ship to board. Please try again.");

                    return;
                }

                player.PlacedShips.Add(placedShip);
                player.PlacingActionHistory.AddAction(player.PlacedShips, gameRoom.Board);

                context.Db.GameRooms[gameRoom.Name] = gameRoom;
                context.Db.Connections[context.ConnectionId] = player;

                await context.Hub.NotifyClient(
                    "UpdatedShipsConfig",
                    player.GetAllowedShipsConfig(gameRoom.ShipsConfig));

                await context.Hub.NotifyGroup(
                    gameRoom.Name,
                    "BoardUpdated",
                    gameRoom.Name,
                    gameRoom.Board);
            }
        }
        else
        {
            await context.Hub.NotifyClient(
                "FailedToAddShip",
                "Invalid command format for adding a ship.");
        }
    }
}
