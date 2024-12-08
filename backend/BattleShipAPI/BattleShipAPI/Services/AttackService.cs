using BattleShipAPI.AttackStrategy;
using BattleShipAPI.Enums;
using BattleShipAPI.Helpers;
using BattleShipAPI.Mediator;
using BattleShipAPI.Models;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Services;

public class AttackService : BaseComponent
{
    private readonly InMemoryDB _db;

    public AttackService()
    {
        _db = InMemoryDB.Instance;
    }

    public async Task PerformAttack(PerformAttackRequest request)
    {
        if (!request.Connection.TryUseSuperAttack(request.AttackType, request.GameRoom.SuperAttacksConfig))
        {
            await _mediator.Notify("FailedToAttackCell", new InformClientRequest()
            {
                Clients = request.Clients,
                ClientId = request,
                Key = "FailedToAttackCell",
                Values = new object[] { "You cannot use this super attack." }
            });
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
        
        // Call mediator InformAboutAttackCellCompletion
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
}