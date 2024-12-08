using BattleShipAPI.AttackStrategy;
using BattleShipAPI.Enums;
using BattleShipAPI.Helpers;
using BattleShipAPI.Models;
using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.ChainOfResponsibility.Handlers;

public class AttackCellHandler : GameHandler
{
    private readonly InMemoryDB _db;
    private readonly INotificationService _notificationService;

    public AttackCellHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
        _db = InMemoryDB.Instance;
    }

    public override async Task HandleRequest(string action, HubCallerContext context, IHubCallerClients clients,
        object? data)
    {
        if (action == "AttackCell" && data is AttackRequest attackRequest)
        {
            if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.InProgress
                && gameRoom.TurnPlayerId == connection.PlayerId)
            {
                var players = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();
                var cell = gameRoom.Board.Cells[attackRequest.X][attackRequest.Y];

                if (cell.OwnerId == connection.PlayerId)
                {
                    await _notificationService.NotifyClient(
                        clients,
                        context.ConnectionId,
                        "FailedToAttackCell",
                        "You cannot attack your own territory.");

                    return;
                }

                if (cell.State == CellState.DamagedShip || cell.State == CellState.SunkenShip ||
                    cell.State == CellState.Missed)
                {
                    await _notificationService.NotifyClient(
                        clients,
                        context.ConnectionId,
                        "FailedToAttackCell",
                        "This territory has already been attacked.");

                    return;
                }

                if (!connection.TryUseSuperAttack(attackRequest.AttackType, gameRoom.SuperAttacksConfig))
                {
                    await _notificationService.NotifyClient(
                        clients,
                        context.ConnectionId,
                        "FailedToAttackCell",
                        "You cannot use this super attack.");

                    return;
                }

                _db.Connections[context.ConnectionId] = connection;

                await _notificationService.NotifyClient(
                    clients,
                    context.ConnectionId,
                    "UpdatedSuperAttacksConfig",
                    connection.GetAllowedSuperAttacksConfig(gameRoom.SuperAttacksConfig));

                var strategy = GameHelper.GetAttackStrategy(attackRequest.AttackType);
                var attackContext = new AttackContext(strategy);
                var attackCells = attackContext.ExecuteAttack(attackRequest.X, attackRequest.Y, gameRoom, connection);

                foreach (var (xCell, yCell) in attackCells)
                {
                    await AttackCellByOne(xCell, yCell, players, gameRoom, connection, clients);
                }

                await _notificationService.NotifyGroup(
                    clients,
                    gameRoom.Name,
                    "BoardUpdated",
                    gameRoom.Name,
                    gameRoom.Board);

                if (gameRoom.State != GameState.Finished)
                {
                    var startTime = DateTime.UtcNow;
                    await _notificationService.NotifyGroup(
                        clients,
                        gameRoom.Name,
                        "PlayerTurn",
                        gameRoom.GetNextTurnPlayerId(players),
                        startTime,
                        gameRoom.TimerDuration);
                }

                _db.GameRooms[gameRoom.Name] = gameRoom;

                await SendAvatarsToPlayers(clients, players, gameRoom.Name);
            }
        }
        else
        {
            await base.HandleRequest(action, context, clients, data);
        }
    }

    private async Task AttackCellByOne(
        int x,
        int y,
        List<UserConnection> players,
        GameRoom gameRoom,
        UserConnection connection,
        IHubCallerClients clients)
    {
        var cell = gameRoom.Board.Cells[x][y];

        if (cell.State == CellState.HasShip || cell.State == CellState.SunkenShip)
        {
            var cellOwner = players.First(p => p.PlayerId == cell.OwnerId);

            if (!gameRoom.TryFullySinkShip(x, y, cellOwner))
            {
                await _notificationService.NotifyGroup(
                    clients,
                    gameRoom.Name,
                    "AttackResult",
                    $"{connection.Username} hit the ship!");
            }
            else
            {
                await _notificationService.NotifyGroup(
                    clients,
                    gameRoom.Name,
                    "AttackResult",
                    $"{connection.Username} sunk the ship!");

                if (!gameRoom.HasAliveShips(cellOwner))
                {
                    cellOwner.CanPlay = false;
                    _db.Connections[cellOwner.PlayerId] = cellOwner;

                    await _notificationService.NotifyGroup(
                        clients,
                        gameRoom.Name,
                        "GameLostResult",
                        $"{cellOwner.Username}",
                        "lost the game!");
                }

                if (players
                    .Where(p => p.PlayerId != connection.PlayerId)
                    .All(p => !p.CanPlay))
                {
                    gameRoom.State = GameState.Finished;

                    await _notificationService.NotifyGroup(
                        clients,
                        gameRoom.Name,
                        "WinnerResult",
                        $"{connection.Username}",
                        "won the game!");

                    await _notificationService.NotifyGroup(
                        clients,
                        gameRoom.Name,
                        "GameStateChanged",
                        (int)gameRoom.State);
                }
            }
        }
        else
        {
            cell.State = CellState.Missed;

            await _notificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "AttackResult",
                $"{connection.Username} missed!");
        }
    }

    private async Task SendAvatarsToPlayers(IHubCallerClients clients, List<UserConnection> players,
        string gameRoomName)
    {
        var playerAvatars = players
            .Select(x => new AvatarResponse(
                x.Username,
                x.Avatar.GetAvatarParameters(),
                x is { CanPlay: true, HasDisconnected: false }))
            .ToList();

        await _notificationService.NotifyGroup(clients, gameRoomName, "AllAvatars", playerAvatars);
    }
}