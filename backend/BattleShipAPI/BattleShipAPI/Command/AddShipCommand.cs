﻿using BattleShipAPI.Enums;
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
            if (context.Db.Connections.TryGetValue(context.CallerContext.ConnectionId, out var connection)
                && context.Db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom))
            {
                var shipConfig = gameRoom.ShipsConfig.FirstOrDefault(config => config.ShipType == shipType);

                if (shipConfig == null)
                {
                    await context.NotificationService.NotifyClient(
                        context.Clients,
                        context.CallerContext.ConnectionId,
                        "FailedToAddShip",
                        "No available ships in config.");
                    return;
                }

                int shipSize = shipConfig.Size;
                int endX = startX;
                int endY = startY;

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

                await context.GameFacade.AddShip(context.CallerContext, context.Clients, placedShip);
            }
        }
        else
        {
            await context.NotificationService.NotifyClient(
                context.Clients,
                context.CallerContext.ConnectionId,
                "FailedToAddShip",
                "Invalid command format for adding a ship.");
        }
    }
}
