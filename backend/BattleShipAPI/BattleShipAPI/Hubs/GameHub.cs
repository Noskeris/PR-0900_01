﻿using BattleShipAPI.AttackStrategy;
using BattleShipAPI.Enums;
using BattleShipAPI.Factories;
using BattleShipAPI.Helpers;
using BattleShipAPI.Models;
using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Hubs
{
    public class GameHub : Hub
    {
        private readonly InMemoryDB _db;
        private readonly INotificationService _notificationService;
        private readonly int timeForTurn = 30;

        public GameHub(INotificationService notificationService)
        {
            _db = InMemoryDB.Instance;
            _notificationService = notificationService;
        }

        public async Task JoinSpecificGameRoom(UserConnection connection)
        {
            if (_db.Connections.Values.Any(c =>
                    c.GameRoomName == connection.GameRoomName && c.Username == connection.Username))
            {
                await _notificationService.NotifyClient(
                    Clients,
                    Context.ConnectionId,
                    "JoinFailed",
                    "Username already taken in this room");
                
                return;
            }

            if (_db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom) &&
                gameRoom.State != GameState.NotStarted)
            {
                await _notificationService.NotifyClient(
                    Clients,
                    Context.ConnectionId,
                    "JoinFailed",
                    "Game has already started");
                
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
                
                await _notificationService.NotifyClient(
                    Clients,
                    Context.ConnectionId,
                    "AvailableShipsForConfiguration",
                    new List<Ship>()
                    {
                        new() { ShipType = ShipType.Carrier, Size = 5 },
                        new() { ShipType = ShipType.Battleship, Size = 4 },
                        new() { ShipType = ShipType.Cruiser, Size = 3 },
                        new() { ShipType = ShipType.Submarine, Size = 2 },
                        new() { ShipType = ShipType.Destroyer, Size = 1 }
                    });
            }

            _db.Connections[Context.ConnectionId] = connection;

            await _notificationService.NotifyClient(
                Clients,
                Context.ConnectionId,
                "SetModerator",
                connection.IsModerator);
            
            await _notificationService.NotifyClient(
                Clients,
                Context.ConnectionId,
                "ReceivePlayerId",
                connection.PlayerId);

            await _notificationService.NotifyGroup(
                Clients,
                connection.GameRoomName,
                "JoinSpecificGameRoom",
                "admin",
                $"{connection.Username} has joined the game room {connection.GameRoomName}");
        }

        public async Task GenerateBoard()
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.NotStarted
                && connection.IsModerator)
            {
                var players = _db.Connections.Values
                    .Where(c => c.GameRoomName == connection.GameRoomName)
                    .ToList();

                if (players.Count == 0)
                    return;
                
                var gameRoomSettings = GameRoomSettingsCreator.GetGameRoomSettings(players);
                
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
                    
                    return;
                }

                connection.CanPlay = true;
                _db.Connections[Context.ConnectionId] = connection;

                await _notificationService.NotifyClient(
                    Clients,
                    Context.ConnectionId,
                    "PlayerNotReady",
                    "You are ready to start the game."); 
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
                    timeForTurn);
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
                    timeForTurn);
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

                foreach (var (xCell, yCell)  in attackCells)
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
                        timeForTurn);
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
                                    timeForTurn);
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
                    });

                await _notificationService.NotifyGroup(
                    Clients, 
                    gameRoom.Name, 
                    "GameStateChanged", 
                    (int)gameRoom.State);

                await _notificationService.NotifyClient(
                    Clients,
                    connection.PlayerId,
                    "AvailableShipsForConfiguration",
                    new List<Ship>()
                    {
                        new() { ShipType = ShipType.Carrier, Size = 5 },
                        new() { ShipType = ShipType.Battleship, Size = 4 },
                        new() { ShipType = ShipType.Cruiser, Size = 3 },
                        new() { ShipType = ShipType.Submarine, Size = 2 },
                        new() { ShipType = ShipType.Destroyer, Size = 1 }
                    });

                await _notificationService.NotifyClient(
                    Clients,
                    connection.PlayerId,
                    "CurrentGameConfiguration", 
                    gameRoom.ShipsConfig);
            }
        }
    }
}