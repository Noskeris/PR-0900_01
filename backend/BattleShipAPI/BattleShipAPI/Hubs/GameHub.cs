using BattleShipAPI.Adapter.Logs;
using BattleShipAPI.AttackStrategy;
using BattleShipAPI.Bridge;
using BattleShipAPI.Enums;
using BattleShipAPI.Enums.Avatar;
using BattleShipAPI.Factories;
using BattleShipAPI.Helpers;
using BattleShipAPI.Models;
using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

//TODO add ship shield logic
//TODO add ship mobility logic
//TODO DESIGN PATTERN decorator
//TODO DESIGN PATTERN facade
//TODO DESIGN PATTERN bridge

namespace BattleShipAPI.Hubs
{
    public class GameHub : Hub
    {
        private readonly InMemoryDB _db;
        private readonly INotificationService _notificationService;
        private readonly ILoggerInterface _loggerOnReceive;
        private readonly ILoggerInterface _loggerOnSend;

        public GameHub(INotificationService notificationService, ConsoleLoggerAdapter loggerOnReceive, FileLoggerAdapter loggerOnSend)
        {
            _db = InMemoryDB.Instance;
            _notificationService = notificationService;
            _loggerOnReceive = loggerOnReceive;
            _loggerOnSend = loggerOnSend;
        }

        public async Task JoinSpecificGameRoom(UserConnection connection)
        {
            _loggerOnReceive.WriteLog(new LogEntry
            {
                Message = $"Received JoinSpecificGameRoom request from {connection.Username}",
            });

            if (_db.Connections.Values.Any(c =>
                    c.GameRoomName == connection.GameRoomName && c.Username == connection.Username))
            {
                await _notificationService.NotifyClient(
                    Clients,
                    Context.ConnectionId,
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
                    Clients,
                    Context.ConnectionId,
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
                    Clients,
                    Context.ConnectionId,
                    "JoinFailed",
                    "Game room is full.");

                _loggerOnSend.WriteLog(new LogEntry
                {
                    Message = $"Sent JoinFailed to {connection.Username}",
                });

                return;
            }

            connection.PlayerId = Context.ConnectionId;

            _notificationService.Subscribe(new Listener
            {
                ClientId = Context.ConnectionId,
                GroupName = connection.GameRoomName
            });

            if (usersInRoom.Count == 0)
            {
                connection.IsModerator = true;
                _db.GameRooms[connection.GameRoomName] = new GameRoom() { Name = connection.GameRoomName };
            }

            _db.Connections[Context.ConnectionId] = connection;

            await _notificationService.NotifyClient(
                Clients,
                Context.ConnectionId,
                "SetModerator",
                connection.IsModerator);

            _loggerOnSend.WriteLog(new LogEntry
            {
                Message = $"Sent SetModerator to {connection.Username}",
            });

            await _notificationService.NotifyClient(
                Clients,
                Context.ConnectionId,
                "ReceivePlayerId",
                connection.PlayerId);

            _loggerOnSend.WriteLog(new LogEntry
            {
                Message = $"Sent ReceivePlayerId to {connection.Username}",
            });

            await _notificationService.NotifyGroup(
                Clients,
                connection.GameRoomName,
                "JoinSpecificGameRoom",
                "admin",
                $"{connection.Username} has joined the game room {connection.GameRoomName}");

            _loggerOnSend.WriteLog(new LogEntry
            {
                Message = $"Sent JoinSpecificGameRoom to {connection.Username}",
            });
        }

        public async Task ConfirmGameMode(GameMode gameMode)
        {
            _loggerOnReceive.WriteLog(new LogEntry
            {
                Message = $"Received ConfirmGameMode request from {Context.ConnectionId}",
            });
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.NotStarted)
            {
                var players = _db.Connections.Values
                    .Where(c => c.GameRoomName == connection.GameRoomName)
                    .ToList();

                gameRoom.Mode = gameMode;
                gameRoom.State = GameState.GameModeConfirmed;

                await _notificationService.NotifyGroup(
                    Clients,
                    gameRoom.Name,
                    "GameStateChanged",
                    (int)gameRoom.State);

                _loggerOnSend.WriteLog(new LogEntry
                {
                    Message = $"Sent GameStateChanged to {gameRoom.Name}",
                });
            }
        }

        public async Task GenerateBoard(GameMode gameMode)
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.GameModeConfirmed
                && connection.IsModerator)
            {
                var players = _db.Connections.Values
                    .Where(c => c.GameRoomName == connection.GameRoomName)
                    .ToList();

                if (players.Count < 2)
                    return;

                var gameRoomSettings = GameRoomSettingsCreator.GetGameRoomSettings(players, gameMode);
                gameRoom.SetSettings(gameRoomSettings);

                gameRoom.State = GameState.PlacingShips;
                _db.GameRooms[gameRoom.Name] = gameRoom;
                
                await _notificationService.NotifyGroup(
                    Clients,
                    gameRoom.Name,
                    "UpdatedShipsConfig",
                    gameRoom.ShipsConfig);
                
                await _notificationService.NotifyGroup(
                    Clients,
                    gameRoom.Name,
                    "GameStateChanged",
                    (int)gameRoom.State);
                
                await _notificationService.NotifyGroup(
                    Clients,
                    gameRoom.Name,
                    "BoardGenerated",
                    gameRoom.Name,
                    gameRoom.Board);

                foreach (var player in _db.Connections.Values.Where(x => x.GameRoomName == gameRoom.Name))
                {
                    await _notificationService.NotifyClient(
                        Clients,
                        player.PlayerId,
                        "SetPlayerAvatarConfigs",
                        player.Avatar.GetAvatarParameters());
                }
            }
        }

        public async Task ChangeAvatar(HeadType headType, AppearanceType appearanceType)
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.PlacingShips)
            {
                var avatar = AvatarFactory.CreateAvatar(headType, appearanceType);
                _db.Connections[Context.ConnectionId].Avatar = avatar;

                await _notificationService.NotifyClient(
                    Clients,
                    connection.PlayerId,
                    "SetPlayerAvatarConfigs",
                    avatar.GetAvatarParameters());
            }
        }

        public async Task AddShip(PlacedShip placedShip)
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.PlacingShips)
            {

                var board = gameRoom.Board;
                var player = _db.Connections[Context.ConnectionId];
                var shipConfig = gameRoom.ShipsConfig
                    .FirstOrDefault(x => x.ShipType == placedShip.ShipType);

                if (shipConfig == null)
                {
                    await _notificationService.NotifyClient(
                        Clients,
                        Context.ConnectionId,
                        "FailedToAddShip",
                        "This ship is not part of the game.");
                    
                    return;
                }

                if (player.PlacedShips.Count(x => x.ShipType == placedShip.ShipType) == shipConfig.Count)
                {
                    await _notificationService.NotifyClient(
                        Clients,
                        Context.ConnectionId,
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
                    await _notificationService.NotifyClient(
                        Clients,
                        Context.ConnectionId,
                        "FailedToAddShip",
                        "Failed to add ship to board. Please try again.");
                    
                    return;
                }

                player.PlacedShips.Add(placedShip);
                player.PlacingActionHistory.AddAction(player.PlacedShips, gameRoom.Board);

                _db.GameRooms[gameRoom.Name] = gameRoom;
                _db.Connections[Context.ConnectionId] = player;

                await _notificationService.NotifyClient(
                    Clients,
                    Context.ConnectionId,
                    "UpdatedShipsConfig",
                    player.GetAllowedShipsConfig(gameRoom.ShipsConfig));
                
                await _notificationService.NotifyGroup(
                    Clients,
                    gameRoom.Name,
                    "BoardUpdated",
                    gameRoom.Name,
                    gameRoom.Board);
            }
        }

        public async Task SetPlayerToReady()
        {
            _loggerOnReceive.WriteLog(new LogEntry
            {
                Message = $"Received SetPlayerToReady request from {Context.ConnectionId}",
            });
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.PlacingShips)
            {
                if (connection.GetAllowedShipsConfig(gameRoom.ShipsConfig).Any(x => x.Count != 0))
                {
                    await _notificationService.NotifyClient(
                        Clients,
                        Context.ConnectionId,
                        "PlayerNotReady",
                        "You have not placed all your ships.");
                    
                    _loggerOnSend.WriteLog(new LogEntry
                    {
                        Message = $"Sent PlayerNotReady to {connection.Username}",
                    });

                    return;
                }

                connection.CanPlay = true;
                _db.Connections[Context.ConnectionId] = connection;

                await _notificationService.NotifyClient(
                    Clients,
                    Context.ConnectionId,
                    "PlayerIsReady",
                    "You are ready to start the game.");

                _loggerOnSend.WriteLog(new LogEntry
                {
                    Message = $"Sent PlayerIsReady to {connection.Username}",
                });
            }
        }

        public async Task StartGame()
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.PlacingShips
                && connection.IsModerator)
            {
                var players = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();

                if (players.Count < 2)
                {
                    await _notificationService.NotifyClient(
                        Clients,
                        Context.ConnectionId,
                        "FailedToStartGame",
                        "Not enough players to start the game.");
                    
                    return;
                }

                if (players.Any(x => !x.CanPlay))
                {
                    await _notificationService.NotifyClient(
                        Clients,
                        Context.ConnectionId,
                        "FailedToStartGame",
                        "Not all players are ready.");

                    return;
                }

                gameRoom.State = GameState.InProgress;

                _db.GameRooms[connection.GameRoomName] = gameRoom;

                await _notificationService.NotifyGroup(
                    Clients,
                    gameRoom.Name,
                    "UpdatedSuperAttacksConfig",
                    gameRoom.SuperAttacksConfig);

                await _notificationService.NotifyGroup(
                    Clients,
                    gameRoom.Name,
                    "GameStateChanged",
                    (int)gameRoom.State);

                var startTime = DateTime.UtcNow;
                await _notificationService.NotifyGroup(
                    Clients,
                    gameRoom.Name,
                    "PlayerTurn",
                    gameRoom.GetNextTurnPlayerId(players),
                    startTime,
                    gameRoom.TimerDuration);
            }
        }

        public async Task PlayerTurnTimeEnded()
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.TurnPlayerId.Equals(connection.PlayerId)
                && gameRoom.State == GameState.InProgress)
            {
                _db.GameRooms[connection.GameRoomName] = gameRoom;
                var players = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();

                var startTime = DateTime.UtcNow;
                await _notificationService.NotifyGroup(
                    Clients,
                    gameRoom.Name,
                    "PlayerTurn",
                    gameRoom.GetNextTurnPlayerId(players),
                    startTime,
                    gameRoom.TimerDuration);
            }
        }

        public async Task AttackCell(int x, int y, AttackType attackType = AttackType.Normal)
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.InProgress
                && gameRoom.TurnPlayerId == connection.PlayerId)
            {
                var players = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();
                var cell = gameRoom.Board.Cells[x][y];

                if (cell.OwnerId == connection.PlayerId)
                {
                    await _notificationService.NotifyClient(
                        Clients,
                        Context.ConnectionId,
                        "FailedToAttackCell",
                        "You cannot attack your own territory.");
                    
                    return;
                }

                if (cell.State == CellState.DamagedShip || cell.State == CellState.SunkenShip || cell.State == CellState.Missed)
                {
                    await _notificationService.NotifyClient(
                        Clients,
                        Context.ConnectionId,
                        "FailedToAttackCell",
                        "This territory has already been attacked.");

                    return;
                }
                
                if (!connection.TryUseSuperAttack(attackType, gameRoom.SuperAttacksConfig))
                {
                    await _notificationService.NotifyClient(
                        Clients,
                        Context.ConnectionId,
                        "FailedToAttackCell",
                        "You cannot use this super attack.");

                    return;
                }
                
                _db.Connections[Context.ConnectionId] = connection;

                await _notificationService.NotifyClient(
                    Clients,
                    Context.ConnectionId,
                    "UpdatedSuperAttacksConfig",
                    connection.GetAllowedSuperAttacksConfig(gameRoom.SuperAttacksConfig));

                // 4. DESIGN PATTERN: Strategy
                var strategy = GameHelper.GetAttackStrategy(attackType);
                var context = new AttackContext(strategy);
                var attackCells = context.ExecuteAttack(x, y, gameRoom, connection);

                foreach (var (xCell, yCell) in attackCells)
                {
                    await AttackCellByOne(xCell, yCell, players, gameRoom, connection);
                }
                
                await _notificationService.NotifyGroup(
                    Clients,
                    gameRoom.Name,
                    "BoardUpdated",
                    gameRoom.Name,
                    gameRoom.Board);

                if (gameRoom.State != GameState.Finished)
                {
                    var startTime = DateTime.UtcNow;
                    await _notificationService.NotifyGroup(
                        Clients,
                        gameRoom.Name,
                        "PlayerTurn",
                        gameRoom.GetNextTurnPlayerId(players),
                        startTime,
                        gameRoom.TimerDuration);
                }

                _db.GameRooms[gameRoom.Name] = gameRoom;
            }
        }

        private async Task AttackCellByOne(int x, int y, List<UserConnection> players, GameRoom gameRoom, UserConnection connection)
        {
            var cell = gameRoom.Board.Cells[x][y];

            if (cell.State == CellState.HasShip)
            {
                var cellOwner = players.First(p => p.PlayerId == cell.OwnerId);

                if (!gameRoom.TryFullySinkShip(x, y, cellOwner))
                {
                    await _notificationService.NotifyGroup(
                        Clients,
                        gameRoom.Name,
                        "AttackResult",
                        $"{connection.Username} hit the ship!");
                }
                else
                {
                    await _notificationService.NotifyGroup(
                        Clients,
                        gameRoom.Name,
                        "AttackResult",
                        $"{connection.Username} sunk the ship!");

                    if (!gameRoom.HasAliveShips(cellOwner))
                    {
                        cellOwner.CanPlay = false;
                        _db.Connections[cellOwner.PlayerId] = cellOwner;

                        await _notificationService.NotifyGroup(
                            Clients,
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
                            Clients,
                            gameRoom.Name,
                            "WinnerResult",
                            $"{connection.Username}",
                            "won the game!");
                        
                        await _notificationService.NotifyGroup(
                            Clients,
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
                    Clients,
                    gameRoom.Name,
                    "AttackResult",
                    $"{connection.Username} missed!");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection))
            {
                _notificationService.Unsubscribe(Context.ConnectionId);
                
                // check if last connection or all others HasDisconnected (clean up everything)
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

                    await base.OnDisconnectedAsync(exception);
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
                        Clients,
                        newModerator.PlayerId,
                        "SetModerator",
                        _db.Connections[newModerator.PlayerId].IsModerator);
                }

                if (_db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom))
                {
                    switch (gameRoom.State)
                    {
                        case GameState.NotStarted:
                            _db.Connections.Remove(Context.ConnectionId, out _);
                            await _notificationService.NotifyGroup(
                                Clients,
                                connection.GameRoomName,
                                "PlayerDisconnected",
                                $"Player {connection.Username} has disconnected.");

                            break;

                        case GameState.GameModeConfirmed:
                        case GameState.PlacingShips:
                            connection.HasDisconnected = true;
                            _db.Connections[Context.ConnectionId] = connection;
                            await _notificationService.NotifyGroup(
                                Clients,
                                connection.GameRoomName,
                                "PlayerDisconnected",
                                $"Player {connection.Username} has disconnected. Game need to be restarted");

                            var moderator = _db.Connections.Values
                                .First(x => x.GameRoomName == gameRoom.Name && x.IsModerator);
                            await RestartGame(moderator);
                            break;

                        case GameState.InProgress:
                            connection.HasDisconnected = true;
                            connection.CanPlay = false;
                            gameRoom.SinkAllShips(connection);

                            await _notificationService.NotifyGroup(
                                Clients,
                                gameRoom.Name,
                                "BoardUpdated", 
                                gameRoom.Name, 
                                gameRoom.Board);

                            if (gameRoom.TurnPlayerId == connection.PlayerId)
                            {
                                var startTime = DateTime.UtcNow;
                                await _notificationService.NotifyGroup(
                                    Clients,
                                    gameRoom.Name,
                                    "PlayerTurn", 
                                    gameRoom.GetNextTurnPlayerId(players), 
                                    startTime,
                                    gameRoom.TimerDuration);
                            }

                            _db.Connections[Context.ConnectionId] = connection;
                            await _notificationService.NotifyGroup(
                                Clients,
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
                                    Clients,
                                    gameRoom.Name,
                                    "AttackResult",
                                    $"{connection.Username} won the game!");

                                await _notificationService.NotifyGroup(
                                    Clients,
                                    gameRoom.Name,
                                    "GameStateChanged",
                                    (int)gameRoom.State);
                            }
                            break;

                        case GameState.Finished:
                            connection.HasDisconnected = true;
                            _db.Connections[Context.ConnectionId] = connection;

                            await _notificationService.NotifyGroup(
                                Clients,
                                connection.GameRoomName,
                                "PlayerDisconnected",
                                $"Player {connection.Username} has disconnected.");

                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task RestartGame()
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection))
            {
                await RestartGame(connection);
            }
        }

        public async Task UndoShipPlacement()
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.PlacingShips)
            {
                var player = _db.Connections[Context.ConnectionId];
                var previousState = player.PlacingActionHistory.Undo();
                if (previousState == null)
                {
                    await _notificationService.NotifyClient(
                        Clients,
                        Context.ConnectionId,
                        "FailedToUndo",
                        "No actions to undo.");
                    return;
                }

                // Restore the board and placed ships
                player.PlacedShips = previousState.Value.PlacedShips;

                gameRoom.SetBoard(previousState.Value.BoardState);
                _db.GameRooms[gameRoom.Name] = gameRoom;
                _db.Connections[Context.ConnectionId] = player;

                await _notificationService.NotifyGroup(
                    Clients,
                    gameRoom.Name,
                    "BoardUpdated",
                    gameRoom.Name,
                    previousState.Value.BoardState);

                await _notificationService.NotifyClient(
                    Clients,
                    Context.ConnectionId,
                    "UpdatedShipsConfig",
                    player.GetAllowedShipsConfig(gameRoom.ShipsConfig));
            }
        }

        public async Task RedoShipPlacement()
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.PlacingShips)
            {
                var player = _db.Connections[Context.ConnectionId];
                var nextState = player.PlacingActionHistory.Redo();
                if (nextState == null)
                {
                    await _notificationService.NotifyClient(
                        Clients,
                        Context.ConnectionId,
                        "FailedToRedo",
                        "No actions to redo.");
                    return;
                }

                // Restore the board and placed ships
                player.PlacedShips = nextState.Value.PlacedShips;
                gameRoom.SetBoard(nextState.Value.BoardState);

                _db.GameRooms[gameRoom.Name] = gameRoom;
                _db.Connections[Context.ConnectionId] = player;

                await _notificationService.NotifyGroup(
                    Clients,
                    gameRoom.Name,
                    "BoardUpdated",
                    gameRoom.Name,
                    gameRoom.Board);

                await _notificationService.NotifyClient(
                    Clients,
                    Context.ConnectionId,
                    "UpdatedShipsConfig",
                    player.GetAllowedShipsConfig(gameRoom.ShipsConfig));
            }
        }


        public async Task HandlePlayerCommand(string command)
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && (gameRoom.State == GameState.NotStarted || gameRoom.State == GameState.GameModeConfirmed 
                || gameRoom.State == GameState.PlacingShips))
            {
                var commandParts = command.Split(' ');
                var action = commandParts[0].ToLower();
                switch (action)
                {
                    case "ready":
                        await SetPlayerToReady();
                        break;
                    case "start":
                        await StartGame();
                        break;
                    case "gamemode":
                        if (Enum.TryParse<GameMode>(commandParts[1], true, out var gameMode))
                        {
                            await ConfirmGameMode(gameMode);
                        }
                        break;
                    case "generateboard":
                        await GenerateBoard(gameRoom.Mode);
                        break;
                    case "addship":
                        if (commandParts.Length >= 5 &&
                            Enum.TryParse<ShipType>(commandParts[1], true, out var shipType) &&
                            int.TryParse(commandParts[2], out var startX) &&
                            int.TryParse(commandParts[3], out var startY) &&
                            Enum.TryParse<ShipOrientation>(commandParts[4], true, out var shipOrientation))
                        {
                            int endX = startX;
                            int endY = startY;

                            var shipConfig = gameRoom.ShipsConfig.FirstOrDefault(config => config.ShipType == shipType);

                            if (shipConfig == null)
                            {
                                await _notificationService.NotifyClient(
                                Clients,
                                Context.ConnectionId,
                                "FailedToAddShip",
                                "No available ships in config.");
                            }

                            int shipSize = shipConfig!.Size;

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

                            await AddShip(placedShip);
                        }
                        else
                        {
                            await _notificationService.NotifyClient(
                                Clients,
                                Context.ConnectionId,
                                "FailedToAddShip",
                                "Invalid command format for adding a ship.");
                        }
                        break;
                    case "undoplacement":
                        await UndoShipPlacement();
                        break;
                    case "redoplacement":
                        await RedoShipPlacement();
                        break;

                    default:
                        await _notificationService.NotifyClient(
                            Clients,
                            Context.ConnectionId,
                            "UnknownCommand",
                            $"Unknown command: {command}");
                        break;
                }
            }
        }

        private async Task RestartGame(UserConnection connection)
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
                    Clients, 
                    gameRoom.Name, 
                    "GameStateChanged", 
                    (int)gameRoom.State);

                await _notificationService.NotifyClient(
                    Clients,
                    connection.PlayerId,
                    "CurrentGameConfiguration", 
                    gameRoom.ShipsConfig);
            }
        }
    }
}