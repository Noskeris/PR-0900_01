using BattleShipAPI.Composite;
using BattleShipAPI.Enums;
using BattleShipAPI.Iterator;
using BattleShipAPI.Models;

namespace BattleShipAPI.Command
{
    public class PlaceFleetCommand : IPlayerCommand
    {
        public async Task Execute(CommandContext context, string[] args)
        {
            if (args.Length < 2)
            {
                await context.NotificationService.NotifyClient(
                    context.Clients,
                    context.CallerContext.ConnectionId,
                    "FailedToPlaceFleet",
                    "Invalid command. Use 'place fleet x' or 'place fleet corner'.");
                return;
            }

            if (context.Db.Connections.TryGetValue(context.CallerContext.ConnectionId, out var connection)
                && context.Db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.PlacingShips)
            {
                var fleetPattern = args[1].ToLower();

                (int x, int y) = GetSectionStartByOwner(connection.PlayerId, gameRoom.Board.Cells);

                var fleet = CreateFleet(fleetPattern, gameRoom, x, y);

                if (fleet == null)
                {
                    await context.NotificationService.NotifyClient(
                        context.Clients,
                        context.CallerContext.ConnectionId,
                        "FailedToPlaceFleet",
                        "Invalid fleet pattern. Use 'x' or 'corner'.");
                    return;
                }

                var fleetAggregate = new FleetAggregate(fleet);
                var iterator = fleetAggregate.CreateIterator();

                while (iterator.HasNext())
                {
                    var component = iterator.Next();
                    if (component is PlacedShip ship)
                    {
                        if (component.ValidatePlacement())
                        {
                            await context.GameFacade.AddShip(context.CallerContext, context.Clients, ship);
                        }
                    }
                }

                await context.NotificationService.NotifyClient(
                    context.Clients,
                    context.CallerContext.ConnectionId,
                    "FleetPlaced",
                    "Fleet successfully placed.");
            }
            else
            {
                await context.NotificationService.NotifyClient(
                    context.Clients,
                    context.CallerContext.ConnectionId,
                    "FailedToPlaceFleet",
                    "Game is not in a state to place fleets or you are not in a game room.");
            }
        }

        private Fleet? CreateFleet(string pattern, GameRoom gameRoom, int startX, int startY)
        {
            var fleet = new Fleet();
            var longShipSubFleet = new SubFleet(SubFleetType.LongShips);
            var shortShipSubFleet = new SubFleet(SubFleetType.ShortShips);

            var shipConfigs = gameRoom.ShipsConfig;

            var battleshipConfig = shipConfigs.FirstOrDefault(c => c.ShipType == ShipType.Battleship);
            var carrierConfig = shipConfigs.FirstOrDefault(c => c.ShipType == ShipType.Carrier);
            var destroyerConfig = shipConfigs.FirstOrDefault(c => c.ShipType == ShipType.Destroyer);
            var cruiserConfig = shipConfigs.FirstOrDefault(c => c.ShipType == ShipType.Cruiser);
            var submarineConfig = shipConfigs.FirstOrDefault(c => c.ShipType == ShipType.Submarine);

            if (pattern == "v")
            {
                if (carrierConfig != null) //5
                    //fleet.Add(CreatePlacedShip(ShipType.Carrier, ShipOrientation.Vertical, carrierConfig.Size, startX + 4, startY + 0));
                    longShipSubFleet.Add(CreatePlacedShip(ShipType.Carrier, ShipOrientation.Vertical, carrierConfig.Size, startX + 4, startY + 0)); 
                if (battleshipConfig != null) // 4
                    //fleet.Add(CreatePlacedShip(ShipType.Battleship, ShipOrientation.Vertical, battleshipConfig.Size, startX + 6, startY + 2));
                    longShipSubFleet.Add(CreatePlacedShip(ShipType.Battleship, ShipOrientation.Vertical, battleshipConfig.Size, startX + 6, startY + 2));
                if (cruiserConfig != null) // 3
                    //fleet.Add(CreatePlacedShip(ShipType.Cruiser, ShipOrientation.Vertical, cruiserConfig.Size, startX + 2, startY + 2));
                    longShipSubFleet.Add(CreatePlacedShip(ShipType.Cruiser, ShipOrientation.Vertical, cruiserConfig.Size, startX + 2, startY + 2));
                if (submarineConfig != null) //2
                    //fleet.Add(CreatePlacedShip(ShipType.Submarine, ShipOrientation.Vertical, submarineConfig.Size, startX + 8, startY + 4));
                    shortShipSubFleet.Add(CreatePlacedShip(ShipType.Submarine, ShipOrientation.Vertical, submarineConfig.Size, startX + 8, startY + 4));
                if (destroyerConfig != null) // 1
                    //fleet.Add(CreatePlacedShip(ShipType.Destroyer, ShipOrientation.Vertical, destroyerConfig.Size, startX + 0, startY + 4));
                    shortShipSubFleet.Add(CreatePlacedShip(ShipType.Destroyer, ShipOrientation.Vertical, destroyerConfig.Size, startX + 0, startY + 4));
            }
            else if (pattern == "corner")
            {
                if (carrierConfig != null)
                    //fleet.Add(CreatePlacedShip(ShipType.Carrier, ShipOrientation.Horizontal, carrierConfig.Size, startX + 0, startY + 0));
                    longShipSubFleet.Add(CreatePlacedShip(ShipType.Carrier, ShipOrientation.Horizontal, carrierConfig.Size, startX + 0, startY + 0));
                if (battleshipConfig != null)
                    //fleet.Add(CreatePlacedShip(ShipType.Battleship, ShipOrientation.Horizontal, battleshipConfig.Size, startX + 0, startY + 1));
                    longShipSubFleet.Add(CreatePlacedShip(ShipType.Battleship, ShipOrientation.Horizontal, battleshipConfig.Size, startX + 0, startY + 1));
                if (cruiserConfig != null)
                    //fleet.Add(CreatePlacedShip(ShipType.Cruiser, ShipOrientation.Horizontal, cruiserConfig.Size, startX + 0, startY + 2));
                    longShipSubFleet.Add(CreatePlacedShip(ShipType.Cruiser, ShipOrientation.Horizontal, cruiserConfig.Size, startX + 0, startY + 2));
                if (submarineConfig != null)
                    //fleet.Add(CreatePlacedShip(ShipType.Submarine, ShipOrientation.Horizontal, submarineConfig.Size, startX + 0, startY + 3));
                    shortShipSubFleet.Add(CreatePlacedShip(ShipType.Submarine, ShipOrientation.Horizontal, submarineConfig.Size, startX + 0, startY + 3));
                if (destroyerConfig != null)
                    //fleet.Add(CreatePlacedShip(ShipType.Destroyer, ShipOrientation.Horizontal, destroyerConfig.Size, startX + 0, startY + 4));
                    shortShipSubFleet.Add(CreatePlacedShip(ShipType.Destroyer, ShipOrientation.Horizontal, destroyerConfig.Size, startX + 0, startY + 4));
            }
            else
            {
                return null; 
            }

            fleet.Add(longShipSubFleet);
            fleet.Add(shortShipSubFleet);

            return fleet;
        }

        private PlacedShip  CreatePlacedShip (ShipType shipType, ShipOrientation shipOrientation, int size, int startX, int startY)
        {

            int shipSize = size;
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

            var placedShipData = new PlacedShip
            {
                ShipType = shipType,
                StartX = startX,
                StartY = startY,
                EndX = endX,
                EndY = endY
            };

            return placedShipData;
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
}