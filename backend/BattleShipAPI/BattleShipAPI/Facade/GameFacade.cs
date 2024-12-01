using BattleShipAPI.Adapter.Logs;
using BattleShipAPI.AttackStrategy;
using BattleShipAPI.Bridge;
using BattleShipAPI.Enums;
using BattleShipAPI.Enums.Avatar;
using BattleShipAPI.Factories;
using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.Helpers;
using BattleShipAPI.Models;
using BattleShipAPI.Notifications;
using BattleShipAPI.Proxy;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Facade;

public class GameFacade
{
    private readonly InMemoryDB _db;
    private readonly INotificationService _notificationService;
    private readonly ILoggerOnReceive _loggerOnReceive;
    private readonly ILoggerOnSend _loggerOnSend;

    public GameFacade(INotificationService notificationService, ILoggerOnReceive loggerOnReceive, ILoggerOnSend loggerOnSend)
    {
        _notificationService = notificationService;
        _loggerOnReceive = loggerOnReceive;
        _loggerOnSend = loggerOnSend;
        _db = InMemoryDB.Instance;
    }

    public async Task JoinSpecificGameRoom(
        HubCallerContext context,
        IHubCallerClients clients,
        UserConnection connection)
    {
        _loggerOnReceive.WriteLog(new LogEntry
        {
            Message = $"Received JoinSpecificGameRoom request from {connection.Username}",
        });

        if (_db.Connections.Values.Any(c =>
                c.GameRoomName == connection.GameRoomName && c.Username == connection.Username))
        {
            await _notificationService.NotifyClient(
                clients,
                context.ConnectionId,
                "JoinFailed",
                "Username already taken in this room");

            _loggerOnSend.WriteLog(new LogEntry
            {
                Message = $"Sent JoinFailed to {connection.Username}",
            });

            return;
        }

        if (_db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom) &&
            (gameRoom.State != GameState.NotStarted && gameRoom.State != GameState.GameModeConfirmed))
        {
            await _notificationService.NotifyClient(
                clients,
                context.ConnectionId,
                "JoinFailed",
                "Game has already started");

            _loggerOnSend.WriteLog(new LogEntry
            {
                Message = $"Sent JoinFailed to {connection.Username}",
            });

            return;
        }

        var usersInRoom = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();

        if (usersInRoom.Count == 4)
        {
            await _notificationService.NotifyClient(
                clients,
                context.ConnectionId,
                "JoinFailed",
                "Game room is full.");

            _loggerOnSend.WriteLog(new LogEntry
            {
                Message = $"Sent JoinFailed to {connection.Username}",
            });

            return;
        }

        connection.PlayerId = context.ConnectionId;

        _notificationService.Subscribe(new Listener
        {
            ClientId = context.ConnectionId,
            GroupName = connection.GameRoomName
        });

        if (usersInRoom.Count == 0)
        {
            connection.IsModerator = true;
            _db.GameRooms[connection.GameRoomName] = new GameRoom() { Name = connection.GameRoomName };
        }

        _db.Connections[context.ConnectionId] = connection;

        await _notificationService.NotifyClient(
            clients,
            context.ConnectionId,
            "SetModerator",
            connection.IsModerator);

        _loggerOnSend.WriteLog(new LogEntry
        {
            Message = $"Sent SetModerator to {connection.Username}",
        });

        await _notificationService.NotifyClient(
            clients,
            context.ConnectionId,
            "ReceivePlayerId",
            connection.PlayerId);

        _loggerOnSend.WriteLog(new LogEntry
        {
            Message = $"Sent ReceivePlayerId to {connection.Username}",
        });

        await _notificationService.NotifyGroup(
            clients,
            connection.GameRoomName,
            "JoinSpecificGameRoom",
            "admin",
            $"{connection.Username} has joined the game room {connection.GameRoomName}");

        _loggerOnSend.WriteLog(new LogEntry
        {
            Message = $"Sent JoinSpecificGameRoom to {connection.Username}",
        });
    }

    public async Task ConfirmGameMode(
        HubCallerContext context,
        IHubCallerClients clients,
        GameMode gameMode)
    {
        _loggerOnReceive.WriteLog(new LogEntry
        {
            Message = $"Received ConfirmGameMode request from {context.ConnectionId}",
        });

        if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
            && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.State == GameState.NotStarted)
        {
            gameRoom.Mode = gameMode;
            gameRoom.State = GameState.GameModeConfirmed;

            await _notificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "GameStateChanged",
                (int)gameRoom.State);

            _loggerOnSend.WriteLog(new LogEntry
            {
                Message = $"Sent GameStateChanged to {gameRoom.Name}",
            });
        }
    }

    public async Task GenerateBoard(
        HubCallerContext context,
        IHubCallerClients clients)
    {
        if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
            && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.State == GameState.GameModeConfirmed
            && connection.IsModerator)
        {
            var players = _db.Connections.Values
                .Where(c => c.GameRoomName == connection.GameRoomName)
                .ToList();

            if (players.Count < 2)
                return;

            var gameRoomSettings = GameRoomSettingsCreator.GetGameRoomSettings(players, gameRoom.Mode);
            gameRoom.SetSettings(gameRoomSettings);

            gameRoom.State = GameState.PlacingShips;
            _db.GameRooms[gameRoom.Name] = gameRoom;

            await _notificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "UpdatedShipsConfig",
                gameRoom.ShipsConfig);

            await _notificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "GameStateChanged",
                (int)gameRoom.State);

            await _notificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "BoardGenerated",
                gameRoom.Name,
                gameRoom.Board);

            foreach (var player in _db.Connections.Values.Where(x => x.GameRoomName == gameRoom.Name))
            {
                await _notificationService.NotifyClient(
                    clients,
                    player.PlayerId,
                    "SetPlayerAvatarConfigs",
                    player.Avatar.GetAvatarParameters());
            }
        }
    }

    public async Task ChangeAvatar(
        HubCallerContext context,
        IHubCallerClients clients,
        HeadType headType,
        AppearanceType appearanceType)
    {
        if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
            && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.State == GameState.PlacingShips)
        {
            var avatar = AvatarFactory.CreateAvatar(headType, appearanceType);
            _db.Connections[context.ConnectionId].Avatar = avatar;

            await _notificationService.NotifyClient(
                clients,
                connection.PlayerId,
                "SetPlayerAvatarConfigs",
                avatar.GetAvatarParameters());
        }
    }

    public async Task AddShip(
    HubCallerContext context,
    IHubCallerClients clients,
    PlacedShip placedShipData)
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

            player.PlacingActionHistory.AddInitialState(player.PlacedShips, gameRoom.Board);

            // Create the ship using ShipConfig.CreateShip
            IPlacedShip newShip = shipConfig.CreateShip(
                placedShipData.StartX,
                placedShipData.StartY,
                placedShipData.EndX,
                placedShipData.EndY,
                revealShipAction: async (ship, board) => await RevealShipAsync(ship, board, gameRoom.Name, clients));

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
            player.PlacingActionHistory.AddAction(player.PlacedShips, gameRoom.Board);

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




    public async Task SetPlayerToReady(
        HubCallerContext context,
        IHubCallerClients clients)
    {
        _loggerOnReceive.WriteLog(new LogEntry
        {
            Message = $"Received SetPlayerToReady request from {context.ConnectionId}",
        });

        if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
            && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.State == GameState.PlacingShips)
        {
            if (connection.GetAllowedShipsConfig(gameRoom.ShipsConfig).Any(x => x.Count != 0))
            {
                await _notificationService.NotifyClient(
                    clients,
                    context.ConnectionId,
                    "PlayerNotReady",
                    "You have not placed all your ships.");

                _loggerOnSend.WriteLog(new LogEntry
                {
                    Message = $"Sent PlayerNotReady to {connection.Username}",
                });

                return;
            }

            connection.CanPlay = true;
            _db.Connections[context.ConnectionId] = connection;

            await _notificationService.NotifyClient(
                clients,
                context.ConnectionId,
                "PlayerIsReady",
                "You are ready to start the game.");

            _loggerOnSend.WriteLog(new LogEntry
            {
                Message = $"Sent PlayerIsReady to {connection.Username}",
            });
        }
    }

    public async Task StartGame(
        HubCallerContext context,
        IHubCallerClients clients)
    {
        if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
            && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.State == GameState.PlacingShips
            && connection.IsModerator)
        {
            var players = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();

            if (players.Count < 2)
            {
                await _notificationService.NotifyClient(
                    clients,
                    context.ConnectionId,
                    "FailedToStartGame",
                    "Not enough players to start the game.");

                return;
            }

            if (players.Any(x => !x.CanPlay))
            {
                await _notificationService.NotifyClient(
                    clients,
                    context.ConnectionId,
                    "FailedToStartGame",
                    "Not all players are ready.");

                return;
            }

            gameRoom.State = GameState.InProgress;

            _db.GameRooms[connection.GameRoomName] = gameRoom;

            await _notificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "UpdatedSuperAttacksConfig",
                gameRoom.SuperAttacksConfig);

            await _notificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "GameStateChanged",
                (int)gameRoom.State);

            await SendAvatarsToPlayers(clients, players, gameRoom.Name);

            var startTime = DateTime.UtcNow;
            await _notificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "PlayerTurn",
                gameRoom.GetNextTurnPlayerId(players),
                startTime,
                gameRoom.TimerDuration);
        }
    }

    public async Task PlayerTurnTimeEnded(
        HubCallerContext context,
        IHubCallerClients clients)
    {
        if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
            && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.TurnPlayerId.Equals(connection.PlayerId)
            && gameRoom.State == GameState.InProgress)
        {
            _db.GameRooms[connection.GameRoomName] = gameRoom;
            var players = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();

            var startTime = DateTime.UtcNow;
            await _notificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "PlayerTurn",
                gameRoom.GetNextTurnPlayerId(players),
                startTime,
                gameRoom.TimerDuration);
        }
    }

    public async Task AttackCell(
        HubCallerContext context,
        IHubCallerClients clients,
        int x,
        int y,
        AttackType attackType)
    {
        if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
            && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.State == GameState.InProgress
            && gameRoom.TurnPlayerId == connection.PlayerId)
        {
            var players = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();
            var cell = gameRoom.Board.Cells[x][y];

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

            if (!connection.TryUseSuperAttack(attackType, gameRoom.SuperAttacksConfig))
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

            var strategy = GameHelper.GetAttackStrategy(attackType);
            var attackContext = new AttackContext(strategy);
            var attackCells = attackContext.ExecuteAttack(x, y, gameRoom, connection);

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
    
    public async Task RestartGame(
        HubCallerContext context,
        IHubCallerClients clients)
    {
        if (_db.Connections.TryGetValue(context.ConnectionId, out var connection))
        {
            if (_db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State is GameState.Finished or GameState.PlacingShips
                && connection.IsModerator)
            {
                _db.Connections.Values
                    .Where(c => c.GameRoomName == connection.GameRoomName && c.HasDisconnected)
                    .ToList()
                    .ForEach(x => _db.Connections.Remove(x.PlayerId, out _));

                gameRoom = new GameRoom() { Name = gameRoom.Name };
                _db.GameRooms[gameRoom.Name] = gameRoom;

                _db.Connections.Values
                    .Where(c => c.GameRoomName == gameRoom.Name)
                    .ToList()
                    .ForEach(x =>
                    {
                        _db.Connections[x.PlayerId].CanPlay = false;
                        _db.Connections[x.PlayerId].HasDisconnected = false;
                        _db.Connections[x.PlayerId].PlacedShips.Clear();
                        _db.Connections[x.PlayerId].UsedSuperAttacks.Clear();
                        _db.Connections[x.PlayerId].PlacingActionHistory = new();
                    });

                await _notificationService.NotifyGroup(
                    clients,
                    gameRoom.Name,
                    "GameStateChanged",
                    (int)gameRoom.State);

                await _notificationService.NotifyClient(
                    clients,
                    connection.PlayerId,
                    "CurrentGameConfiguration",
                    gameRoom.ShipsConfig);
            }
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
    
    public async Task HandleDisconnection(
    HubCallerContext context,
    IHubCallerClients clients)
{
    if (_db.Connections.TryGetValue(context.ConnectionId, out var connection))
    {
        _notificationService.Unsubscribe(context.ConnectionId);

        var players = _db.Connections.Values
            .Where(c => c.GameRoomName == connection.GameRoomName)
            .ToList();

        var haveAllPlayersDisconnected = players
            .Where(x => x.PlayerId != connection.PlayerId)
            .All(p => p.HasDisconnected);

        if (haveAllPlayersDisconnected)
        {
            _db.GameRooms.Remove(connection.GameRoomName, out _);
            _db.Connections.Values
                .Where(c => c.GameRoomName == connection.GameRoomName)
                .ToList()
                .ForEach(x => _db.Connections.Remove(x.PlayerId, out _));

            return;
        }

        if (connection.IsModerator)
        {
            var newModerator = players
                .First(x => x.PlayerId != connection.PlayerId);

            newModerator.IsModerator = true;
            _db.Connections[newModerator.PlayerId] = newModerator;
            _db.Connections[connection.PlayerId].IsModerator = false;

            await _notificationService.NotifyClient(
                clients,
                newModerator.PlayerId,
                "SetModerator",
                _db.Connections[newModerator.PlayerId].IsModerator);
        }

        if (_db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom))
        {
            switch (gameRoom.State)
            {
                case GameState.NotStarted:
                    _db.Connections.Remove(context.ConnectionId, out _);
                    await _notificationService.NotifyGroup(
                        clients,
                        connection.GameRoomName,
                        "PlayerDisconnected",
                        $"Player {connection.Username} has disconnected.");
                    break;

                case GameState.GameModeConfirmed:
                case GameState.PlacingShips:
                    connection.HasDisconnected = true;
                    _db.Connections[context.ConnectionId] = connection;
                    await _notificationService.NotifyGroup(
                        clients,
                        connection.GameRoomName,
                        "PlayerDisconnected",
                        $"Player {connection.Username} has disconnected. Game need to be restarted");

                    var moderator = _db.Connections.Values
                        .First(x => x.GameRoomName == gameRoom.Name && x.IsModerator);
                    await RestartGame(context, clients);
                    break;

                case GameState.InProgress:
                    connection.HasDisconnected = true;
                    connection.CanPlay = false;
                    gameRoom.SinkAllShips(connection);

                    await _notificationService.NotifyGroup(
                        clients,
                        gameRoom.Name,
                        "BoardUpdated",
                        gameRoom.Name,
                        gameRoom.Board);

                    if (gameRoom.TurnPlayerId == connection.PlayerId)
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

                    _db.Connections[context.ConnectionId] = connection;
                    await _notificationService.NotifyGroup(
                        clients,
                        connection.GameRoomName,
                        "PlayerDisconnected",
                        $"Player {connection.Username} has disconnected.");

                    players = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();

                    if (players
                        .Where(p => p.PlayerId != gameRoom.TurnPlayerId)
                        .All(p => !p.CanPlay))
                    {
                        gameRoom.State = GameState.Finished;
                        await _notificationService.NotifyGroup(
                            clients,
                            gameRoom.Name,
                            "AttackResult",
                            $"{connection.Username} won the game!");

                        await _notificationService.NotifyGroup(
                            clients,
                            gameRoom.Name,
                            "GameStateChanged",
                            (int)gameRoom.State);
                    }
                    break;

                case GameState.Finished:
                    connection.HasDisconnected = true;
                    _db.Connections[context.ConnectionId] = connection;

                    await _notificationService.NotifyGroup(
                        clients,
                        connection.GameRoomName,
                        "PlayerDisconnected",
                        $"Player {connection.Username} has disconnected.");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        await SendAvatarsToPlayers(clients, players, gameRoom.Name);
    }
}

    private async Task SendAvatarsToPlayers(IHubCallerClients clients, List<UserConnection> players, string gameRoomName)
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