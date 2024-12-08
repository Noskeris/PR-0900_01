using BattleShipAPI.Decorator;
using BattleShipAPI.Enums;
using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.Models;
using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.ChainOfResponsibility.Handlers;

public class AddShipHandler : GameHandler
{
    private readonly InMemoryDB _db;
    private readonly INotificationService _notificationService;

    public AddShipHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
        _db = InMemoryDB.Instance;
    }

    public override async Task HandleRequest(string action, HubCallerContext context, IHubCallerClients clients,
        object? data)
    {
        if (action == "AddShip" && data is PlacedShip placedShipData)
        {
            if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.PlacingShips)
            {
                var player = _db.Connections[context.ConnectionId];
                var shipConfig = gameRoom.ShipsConfig
                    .FirstOrDefault(x => x.ShipType == placedShipData.ShipType);

                if (shipConfig == null)
                {
                    await _notificationService.NotifyClient(
                        clients,
                        context.ConnectionId,
                        "FailedToAddShip",
                        "This ship is not part of the game.");

                    return;
                }

                if (player.PlacedShips.CountShipsOfType(placedShipData.ShipType) >= shipConfig.Count)
                {
                    await _notificationService.NotifyClient(
                        clients,
                        context.ConnectionId,
                        "FailedToAddShip",
                        "No ships of this type left.");

                    return;
                }

                // Create a memento of the current state before adding the ship
                var initialStateMemento = player.CreateMemento(gameRoom.Board);
                player.PlacingActionHistory.AddInitialState(initialStateMemento);

                // Create the ship using ShipConfig.CreateShip
                IPlacedShip newShip = shipConfig.CreateShip(
                    placedShipData.StartX,
                    placedShipData.StartY,
                    placedShipData.EndX,
                    placedShipData.EndY,
                    revealShipAction: async (ship, board) =>
                        await RevealShipAsync(ship, board, gameRoom.Name, clients));

                if (!gameRoom.Board.TryPutShipOnBoard(newShip, player.PlayerId))
                {
                    await _notificationService.NotifyClient(
                        clients,
                        context.ConnectionId,
                        "FailedToAddShip",
                        "Failed to add ship to board. Please try again.");

                    return;
                }

                player.PlacedShips.Add(newShip);

                // Create another memento after adding the ship
                var actionMemento = player.CreateMemento(gameRoom.Board);
                player.PlacingActionHistory.AddAction(actionMemento);

                _db.GameRooms[gameRoom.Name] = gameRoom;
                _db.Connections[context.ConnectionId] = player;

                await _notificationService.NotifyClient(
                    clients,
                    context.ConnectionId,
                    "UpdatedShipsConfig",
                    player.GetAllowedShipsConfig(gameRoom.ShipsConfig));

                await _notificationService.NotifyGroup(
                    clients,
                    gameRoom.Name,
                    "BoardUpdated",
                    gameRoom.Name,
                    gameRoom.Board);
            }
        }
        else
        {
            await base.HandleRequest(action, context, clients, data);
        }
    }

    private async Task RevealShipAsync(IPlacedShip ship, Board board, string gameRoomName, IHubCallerClients clients)
    {
        var coordinates = ship.GetCoordinates();
        foreach (var (x, y) in coordinates)
        {
            board.Cells[x][y].IsRevealed = true;
        }

        // Notify clients about the update
        await _notificationService.NotifyGroup(
            clients,
            gameRoomName,
            "BoardUpdated",
            gameRoomName,
            board);
    }
}