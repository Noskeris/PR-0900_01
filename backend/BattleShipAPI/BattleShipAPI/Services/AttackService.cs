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

    public async Task PerformAttack(AttackInformation request)
    {
        var players = _db.Connections.Values.Where(c => c.GameRoomName == request.Connection.GameRoomName).ToList();
        
        if (!request.Connection.TryUseSuperAttack(request.AttackRequest.AttackType, request.GameRoom.SuperAttacksConfig))
        {
            await _mediator.Notify("InformClient", new InformClientRequest()
            {
                Clients = request.Clients,
                ClientId = request.Context.ConnectionId,
                Key = "FailedToAttackCell",
                Values = ["You cannot use this super attack."]
            });

            return;
        }

        _db.Connections[request.Context.ConnectionId] = request.Connection;

        await _mediator.Notify("InformClient", new InformClientRequest()
        {
            Clients = request.Clients,
            ClientId = request.Context.ConnectionId,
            Key = "UpdatedSuperAttacksConfig",
            Values = [
                "You cannot use this super attack.",
                request.Connection.GetAllowedSuperAttacksConfig(request.GameRoom.SuperAttacksConfig)]
        });
        
        await _mediator.Notify("InformClient", new InformClientRequest()
        {
            Clients = request.Clients,
            ClientId = request.Context.ConnectionId,
            Key = "UpdatedSuperAttacksConfig",
            Values = [request.Connection.GetAllowedSuperAttacksConfig(request.GameRoom.SuperAttacksConfig)]
        });

        var strategy = GameHelper.GetAttackStrategy(request.AttackRequest.AttackType);
        var attackContext = new AttackContext(strategy);
        var attackCells = attackContext.ExecuteAttack(request.AttackRequest.X, request.AttackRequest.Y, request.GameRoom, request.Connection);

        foreach (var (xCell, yCell) in attackCells)
        {
            await AttackCellByOne(request, xCell, yCell, players, request.GameRoom, request.Connection);
        }
        
        await _mediator.Notify("InformGroup", new InformGroupRequest()
        {
            Clients = request.Clients,
            GroupName = request.GameRoom.Name,
            Key = "BoardUpdated",
            Values = [request.GameRoom.Name, request.GameRoom.Board]
        });
        
        if (request.GameRoom.State != GameState.Finished)
        {
            var startTime = DateTime.UtcNow;
            
            await _mediator.Notify("InformGroup", new InformGroupRequest()
            {
                Clients = request.Clients,
                GroupName = request.GameRoom.Name,
                Key = "PlayerTurn",
                Values = [request.GameRoom.GetNextTurnPlayerId(players),
                    startTime,
                    request.GameRoom.TimerDuration]
            });
        }

        _db.GameRooms[request.GameRoom.Name] = request.GameRoom;

        await _mediator.Notify("InformAboutPerformedCellAttackSuccess", request);
    }
    
    private async Task AttackCellByOne(
        AttackInformation request,
        int x,
        int y,
        List<UserConnection> players,
        GameRoom gameRoom,
        UserConnection connection)
    {
        var cell = gameRoom.Board.Cells[x][y];

        if (cell.State == CellState.HasShip || cell.State == CellState.SunkenShip)
        {
            var cellOwner = players.First(p => p.PlayerId == cell.OwnerId);

            if (!gameRoom.TryFullySinkShip(x, y, cellOwner))
            {
                await _mediator.Notify("InformGroup", new InformGroupRequest()
                {
                    Clients = request.Clients,
                    GroupName = request.GameRoom.Name,
                    Key = "AttackResult",
                    Values = [$"{connection.Username} hit the ship!"]
                });
            }
            else
            {
                await _mediator.Notify("InformGroup", new InformGroupRequest()
                {
                    Clients = request.Clients,
                    GroupName = request.GameRoom.Name,
                    Key = "AttackResult",
                    Values = [$"{connection.Username} sunk the ship!"]
                });

                if (!gameRoom.HasAliveShips(cellOwner))
                {
                    cellOwner.CanPlay = false;
                    _db.Connections[cellOwner.PlayerId] = cellOwner;
                    
                    await _mediator.Notify("InformGroup", new InformGroupRequest()
                    {
                        Clients = request.Clients,
                        GroupName = request.GameRoom.Name,
                        Key = "GameLostResult",
                        Values = [$"{cellOwner.Username}", "lost the game!"]
                    });
                }

                if (players
                    .Where(p => p.PlayerId != connection.PlayerId)
                    .All(p => !p.CanPlay))
                {
                    gameRoom.State = GameState.Finished;

                    await _mediator.Notify("InformGroup", new InformGroupRequest()
                    {
                        Clients = request.Clients,
                        GroupName = request.GameRoom.Name,
                        Key = "WinnerResult",
                        Values = [$"{connection.Username}", "won the game!"]
                    }); 
                    
                    await _mediator.Notify("InformGroup", new InformGroupRequest()
                    {
                        Clients = request.Clients,
                        GroupName = request.GameRoom.Name,
                        Key = "GameStateChanged",
                        Values = [(int)gameRoom.State]
                    });
                }
            }
        }
        else
        {
            cell.State = CellState.Missed;

            await _mediator.Notify("InformGroup", new InformGroupRequest()
            {
                Clients = request.Clients,
                GroupName = request.GameRoom.Name,
                Key = "AttackResult",
                Values = [$"{connection.Username} missed!"]
            });
        }
    }
}