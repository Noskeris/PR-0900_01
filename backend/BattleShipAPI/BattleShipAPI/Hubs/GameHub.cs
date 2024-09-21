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
            if (_db.Connections.Values.Any(c => c.GameRoomName == connection.GameRoomName && c.Username == connection.Username))
            {
                await Clients.Caller.SendAsync("JoinFailed", "Username already taken in this room");
                return;
            }
            
            if (_db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom) && gameRoom.State != GameState.NotStarted)
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

            connection.PlayerId = Guid.NewGuid();

            await Groups.AddToGroupAsync(Context.ConnectionId, connection.GameRoomName);

            if (usersInRoom.Count == 0)
            {
                connection.IsModerator = true;
                _db.GameRooms[connection.GameRoomName] = new GameRoom();
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
            await Clients.Group(connection.GameRoomName).SendAsync("JoinSpecificGameRoom", "admin", $"{connection.Username} has joined the game room {connection.GameRoomName}");
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
                await Clients.Group(gameRoom.Name).SendAsync("GameStateChanged", (int)gameRoom.State);
                await Clients.Group(gameRoom.Name).SendAsync("BoardGenerated", gameRoom.Name, gameRoom.Board);
                await Clients.Group(gameRoom.Name).SendAsync("UpdatedShipsConfig", gameRoom.Settings.ShipsConfig);
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
                
                await Clients.Caller.SendAsync("UpdatedShipsConfig", player.GetAllowedShipsConfig(gameRoom.Settings.ShipsConfig));
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
                
                connection.IsReady = true;
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
                
                if (players.Any(x => !x.IsReady))
                {
                    await Clients.Caller.SendAsync("FailedToStartGame", "Not all players are ready.");
                    return;
                }
                
                gameRoom.State = GameState.InProgress;
                
                _db.GameRooms[connection.GameRoomName] = gameRoom;
                
                Console.WriteLine($"Game state changed to: {gameRoom.State}");
                await Clients.Group(gameRoom.Name).SendAsync("GameStateChanged", (int)gameRoom.State);
            }
        }
    }
}
