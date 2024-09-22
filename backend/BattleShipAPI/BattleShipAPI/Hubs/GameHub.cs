using BattleShipAPI.Enums;
using BattleShipAPI.Models;
using BattleShipAPI.Repository;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Hubs
{
    public class GameHub : Hub
    {
        private readonly InMemoryDB _db;

        public GameHub(InMemoryDB db) => _db = db;

        public async Task JoinSpecificGameRoom(UserConnection connection)
        {
            if (_db.Connections.Values.Any(c =>
                    c.GameRoomName == connection.GameRoomName && c.Username == connection.Username))
            {
                await Clients.Caller.SendAsync("JoinFailed", "Username already taken in this room");
                return;
            }

            if (_db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom) &&
                gameRoom.State != GameState.NotStarted)
            {
                await Clients.Caller.SendAsync("JoinFailed", "Game has already started.");
                return;
            }

            var usersInRoom = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();

            if (usersInRoom.Count == 4)
            {
                await Clients.Caller.SendAsync("JoinFailed", "Game room is full.");
                return;
            }

            connection.PlayerId = Context.ConnectionId;

            await Groups.AddToGroupAsync(Context.ConnectionId, connection.GameRoomName);

            if (usersInRoom.Count == 0)
            {
                connection.IsModerator = true;
                _db.GameRooms[connection.GameRoomName] = new GameRoom() {Name = connection.GameRoomName};
                await Clients.Caller.SendAsync("AvailableShipsForConfiguration", new List<Ship>()
                {
                    new() { ShipType = ShipType.Carrier, Size = 5 },
                    new() { ShipType = ShipType.Battleship, Size = 4 },
                    new() { ShipType = ShipType.Cruiser, Size = 3 },
                    new() { ShipType = ShipType.Submarine, Size = 2 },
                    new() { ShipType = ShipType.Destroyer, Size = 1 }
                });
            }

            _db.Connections[Context.ConnectionId] = connection;

            await Clients.Caller.SendAsync("SetModerator", connection.IsModerator);
            await Clients.Caller.SendAsync("ReceivePlayerId", connection.PlayerId);
            await Clients.Group(connection.GameRoomName).SendAsync("JoinSpecificGameRoom", "admin",
                $"{connection.Username} has joined the game room {connection.GameRoomName}");
        }

        public async Task GenerateBoard()
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.NotStarted
                && connection.IsModerator)
            {
                Board gameBoard;

                var players = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();

                if (players.Count == 0)
                    return;

                switch (players.Count)
                {
                    case 2:
                        gameBoard = new Board(20, 10);
                        var player1Of2 = players[0];
                        var player2Of2 = players[1];

                        gameBoard.AssignBoardSection(0, 0, 9, 9, player1Of2.PlayerId);
                        gameBoard.AssignBoardSection(10, 0, 19, 9, player2Of2.PlayerId);

                        gameRoom.Board = gameBoard;
                        break;

                    case 3:
                        gameBoard = new Board(30, 10);
                        var player1Of3 = players[0];
                        var player2Of3 = players[1];
                        var player3Of3 = players[2];

                        gameBoard.AssignBoardSection(0, 0, 9, 9, player1Of3.PlayerId);
                        gameBoard.AssignBoardSection(10, 0, 19, 9, player2Of3.PlayerId);
                        gameBoard.AssignBoardSection(20, 0, 29, 9, player3Of3.PlayerId);

                        gameRoom.Board = gameBoard;
                        break;

                    case 4:
                        gameBoard = new Board(20, 20);
                        var player1Of4 = players[0];
                        var player2Of4 = players[1];
                        var player3Of4 = players[2];
                        var player4Of4 = players[3];

                        gameBoard.AssignBoardSection(0, 0, 9, 9, player1Of4.PlayerId);
                        gameBoard.AssignBoardSection(0, 10, 9, 19, player2Of4.PlayerId);
                        gameBoard.AssignBoardSection(10, 0, 19, 9, player3Of4.PlayerId);
                        gameBoard.AssignBoardSection(10, 10, 19, 19, player4Of4.PlayerId);

                        gameRoom.Board = gameBoard;
                        break;

                    default:
                        return;
                }

                gameRoom.State = GameState.PlacingShips;
                _db.GameRooms[gameRoom.Name] = gameRoom;

                Console.WriteLine($"Game state changed to: {gameRoom.State}");
                await Clients.Group(gameRoom.Name).SendAsync("UpdatedShipsConfig", gameRoom.Settings.ShipsConfig);
                await Clients.Group(gameRoom.Name).SendAsync("GameStateChanged", (int)gameRoom.State);
                await Clients.Group(gameRoom.Name).SendAsync("BoardGenerated", gameRoom.Name, gameRoom.Board);
            }
        }

        public async Task SetGameRoomSettings(GameRoomSettings roomSettings)
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.NotStarted
                && connection.IsModerator)
            {
                gameRoom.Settings = roomSettings;
                _db.GameRooms[connection.GameRoomName] = gameRoom;

                await Clients.Caller.SendAsync("GameSettingsSaved", "Game settings saved successfully.");
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
                var shipConfig = gameRoom.Settings.ShipsConfig
                    .FirstOrDefault(x => x.ShipType == placedShip.ShipType);

                if (shipConfig == null)
                {
                    await Clients.Caller.SendAsync("FailedToAddShip", "This ship is not part of the game.");
                    return;
                }

                if (player.PlacedShips.Count(x => x.ShipType == placedShip.ShipType) == shipConfig.Count)
                {
                    await Clients.Caller.SendAsync("FailedToAddShip", "No ships of this type left.");
                    return;
                }

                if (!board.TryPutShipOnBoard(
                        placedShip.StartX,
                        placedShip.StartY,
                        placedShip.EndX,
                        placedShip.EndY,
                        player.PlayerId))
                {
                    await Clients.Caller.SendAsync("FailedToAddShip", "Failed to add ship to board. Please try again.");
                    return;
                }

                player.PlacedShips.Add(placedShip);
                gameRoom.Board = board;
                _db.GameRooms[gameRoom.Name] = gameRoom;
                _db.Connections[Context.ConnectionId] = player;

                await Clients.Caller.SendAsync("UpdatedShipsConfig",
                    player.GetAllowedShipsConfig(gameRoom.Settings.ShipsConfig));
                await Clients.Group(gameRoom.Name).SendAsync("BoardUpdated", gameRoom.Name, gameRoom.Board);
            }
        }

        public async Task SetPlayerToReady()
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection)
                && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
                && gameRoom.State == GameState.PlacingShips)
            {
                if (connection.GetAllowedShipsConfig(gameRoom.Settings.ShipsConfig).Any(x => x.Count != 0))
                {
                    await Clients.Caller.SendAsync("PlayerNotReady", "You have not placed all your ships.");
                    return;
                }

                connection.CanPlay = true;
                _db.Connections[Context.ConnectionId] = connection;

                await Clients.Caller.SendAsync("PlayerReady", "You are ready to start the game.");
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
                    await Clients.Caller.SendAsync("FailedToStartGame", "Not enough players to start the game.");
                    return;
                }
                
                if (players.Any(x => !x.CanPlay))
                {
                    await Clients.Caller.SendAsync("FailedToStartGame", "Not all players are ready.");
                    return;
                }

                gameRoom.State = GameState.InProgress;

                _db.GameRooms[connection.GameRoomName] = gameRoom;

                Console.WriteLine($"Game state changed to: {gameRoom.State}");
                await Clients.Group(gameRoom.Name).SendAsync("GameStateChanged", (int)gameRoom.State);
                await Clients.Group(gameRoom.Name).SendAsync("PlayerTurn", gameRoom.GetNextTurnPlayerId(players));
            }
        }

        public async Task AttackCell(int x, int y)
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
                    await Clients.Caller.SendAsync("FailedToAttackCell", "You cannot attack your own territory.");
                    return;
                }

                if (cell.State == CellState.DamagedShip || cell.State == CellState.SunkenShip || cell.State == CellState.Missed)
                {
                    await Clients.Caller.SendAsync("FailedToAttackCell", "This territory has already been attacked.");
                    return;
                }

                if (cell.State == CellState.HasShip)
                {
                    var cellOwner = players.First(p => p.PlayerId == cell.OwnerId);
                    
                    if (!gameRoom.TryFullySinkShip(x, y, cellOwner))
                    {
                        await Clients.Group(gameRoom.Name).SendAsync("AttackResult", $"{connection.Username} hit the ship!");
                    }
                    else
                    {
                        await Clients.Group(gameRoom.Name).SendAsync("AttackResult", $"{connection.Username} sunk the ship!");

                        if (!gameRoom.HasAliveShips(cellOwner))
                        {
                            cellOwner.CanPlay = false;
                            _db.Connections[cellOwner.PlayerId] = cellOwner;
                            
                            await Clients.Group(gameRoom.Name).SendAsync("AttackResult", $"{cellOwner.Username} lost the game!");
                        }

                        if (players
                            .Where(p => p.PlayerId != connection.PlayerId)
                            .All(p => !p.CanPlay))
                        {
                            gameRoom.State = GameState.Finished;
                            await Clients.Group(gameRoom.Name).SendAsync("AttackResult", $"{connection.Username} won the game!");
                            await Clients.Group(gameRoom.Name).SendAsync("GameStateChanged", (int)gameRoom.State);
                        }
                    }

                    await Clients.Group(gameRoom.Name).SendAsync("BoardUpdated", gameRoom.Name, gameRoom.Board);
                }
                else
                {
                    cell.State = CellState.Missed;

                    await Clients.Group(gameRoom.Name).SendAsync("BoardUpdated", gameRoom.Name, gameRoom.Board);
                    await Clients.Group(gameRoom.Name).SendAsync("AttackResult", $"{connection.Username} missed!");
                }

                if (gameRoom.State != GameState.Finished)
                {
                    await Clients.Group(gameRoom.Name).SendAsync("PlayerTurn", gameRoom.GetNextTurnPlayerId(players));
                }
                
                _db.GameRooms[gameRoom.Name] = gameRoom;
            }
        }
        
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_db.Connections.TryGetValue(Context.ConnectionId, out var connection))
            {
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
                    
                    await Clients.Caller.SendAsync("SetModerator", connection.IsModerator);
                    await Clients.Client(newModerator.PlayerId).SendAsync("SetModerator", connection.IsModerator);
                }
                
                if (_db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom))
                {
                    switch (gameRoom.State)
                    {
                        case GameState.NotStarted:
                            _db.Connections.Remove(Context.ConnectionId, out _);
                            await Clients
                                .Group(connection.GameRoomName)
                                .SendAsync("PlayerDisconnected",
                                    $"Player {connection.Username} has disconnected.");
                            break;
                        
                        case GameState.PlacingShips:
                            connection.HasDisconnected = true;
                            _db.Connections[Context.ConnectionId] = connection;
                            await Clients
                                .Group(connection.GameRoomName)
                                .SendAsync("PlayerDisconnected",
                                    $"Player {connection.Username} has disconnected. Game need to be restarted");
                            
                            var moderator = _db.Connections.Values
                                .First(x => x.GameRoomName == gameRoom.Name && x.IsModerator);
                            await RestartGame(moderator);
                            break;
                        
                        case GameState.InProgress:
                            connection.HasDisconnected = true;
                            connection.CanPlay = false;
                            gameRoom.SinkAllShips(connection);
                            await Clients.Group(gameRoom.Name).SendAsync("BoardUpdated", gameRoom.Name, gameRoom.Board);
                            if (gameRoom.TurnPlayerId == connection.PlayerId)
                            {
                                await Clients.Group(gameRoom.Name).SendAsync("PlayerTurn", gameRoom.GetNextTurnPlayerId(players));
                            }
                            _db.Connections[Context.ConnectionId] = connection;
                            await Clients
                                .Group(connection.GameRoomName)
                                .SendAsync("PlayerDisconnected",
                                    $"Player {connection.Username} has disconnected.");
                            
                            players = _db.Connections.Values.Where(c => c.GameRoomName == connection.GameRoomName).ToList();
                            
                            if (players
                                .Where(p => p.PlayerId != gameRoom.TurnPlayerId)
                                .All(p => !p.CanPlay))
                            {
                                gameRoom.State = GameState.Finished;
                                await Clients.Group(gameRoom.Name).SendAsync("AttackResult", $"{connection.Username} won the game!");
                                await Clients.Group(gameRoom.Name).SendAsync("GameStateChanged", (int)gameRoom.State);
                            }
                            break;
                        
                        case GameState.Finished:
                            connection.HasDisconnected = true;
                            _db.Connections[Context.ConnectionId] = connection;
                            await Clients
                                .Group(connection.GameRoomName)
                                .SendAsync("PlayerDisconnected",
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

                gameRoom = new GameRoom() { Name = gameRoom.Name, Settings = gameRoom.Settings};
                _db.GameRooms[gameRoom.Name] = gameRoom;
                
                
                _db.Connections.Values
                    .Where(c => c.GameRoomName == gameRoom.Name)
                    .ToList()
                    .ForEach(x =>
                    {
                        _db.Connections[x.PlayerId].CanPlay = false;
                        _db.Connections[x.PlayerId].HasDisconnected = false;
                        _db.Connections[x.PlayerId].PlacedShips.Clear();
                    });
                
                await Clients.Group(gameRoom.Name).SendAsync("GameStateChanged", (int)gameRoom.State);
                
                await Clients.Client(connection.PlayerId).SendAsync("AvailableShipsForConfiguration", new List<Ship>()
                {
                    new() { ShipType = ShipType.Carrier, Size = 5 },
                    new() { ShipType = ShipType.Battleship, Size = 4 },
                    new() { ShipType = ShipType.Cruiser, Size = 3 },
                    new() { ShipType = ShipType.Submarine, Size = 2 },
                    new() { ShipType = ShipType.Destroyer, Size = 1 }
                });
                
                await Clients.Client(connection.PlayerId).SendAsync("CurrentGameConfiguration", gameRoom.Settings);
            }
        }
    }
}
