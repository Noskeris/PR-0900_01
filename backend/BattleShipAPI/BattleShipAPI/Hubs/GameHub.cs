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
            if (_db.connections.Values.Any(c => c.GameRoom.GameRoomName == connection.GameRoom.GameRoomName && c.Username == connection.Username))
            {
                await Clients.Caller.SendAsync("JoinFailed", "Username already taken in this room");
                return;
            }

            var usersInRoom = _db.connections.Values.Where(c => c.GameRoom.GameRoomName == connection.GameRoom.GameRoomName).ToList();

            connection.PlayerId = usersInRoom.Count + 1;

            if (connection.PlayerId > 4)
            {
                await Clients.Caller.SendAsync("JoinFailed", "Game room is full.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, connection.GameRoom.GameRoomName);

            var isFirstUser = !_db.connections.Values.Any(c => c.GameRoom.GameRoomName == connection.GameRoom.GameRoomName);

            if (isFirstUser)
            {
                connection.IsModerator = true;
            }

            _db.connections[Context.ConnectionId] = connection;

            await Clients.Caller.SendAsync("SetModerator", connection.IsModerator);

            await Clients.Caller.SendAsync("RecievePlayerId", connection.PlayerId);

            await Clients.Group(connection.GameRoom.GameRoomName).SendAsync("JoinSpecificGameRoom", "admin", $"{connection.Username} has joined the game room {connection.GameRoom}");
        }

        public async Task GenerateBoard()
        {
            if (_db.connections.TryGetValue(Context.ConnectionId, out UserConnection connection))
            {
                var gameRoomName = connection.GameRoom.GameRoomName;

                var userCountInSameGameRoom = _db.connections.Values.Count(c => c.GameRoom.GameRoomName == gameRoomName);

                Board? gameBoard = null;

                var players = _db.connections.Values.Where(c => c.GameRoom.GameRoomName == gameRoomName).ToList();

                if (players.Count == 0)
                    return;

                switch (userCountInSameGameRoom)
                {
                    case 2:
                        gameBoard = new Board(20, 10);
                        var player1of2 = players[0];
                        var player2of2 = players[1];

                        gameBoard.AssignBoardSection(0, 0, 9, 9, player1of2.PlayerId);
                        gameBoard.AssignBoardSection(10, 0, 19, 9, player2of2.PlayerId);

                        break;

                    case 3:
                        gameBoard = new Board(30, 10);
                        var player1of3 = players[0];
                        var player2of3 = players[1];
                        var player3of3 = players[2];

                        gameBoard.AssignBoardSection(0, 0, 9, 9, player1of3.PlayerId);
                        gameBoard.AssignBoardSection(10, 0, 19, 9, player2of3.PlayerId);
                        gameBoard.AssignBoardSection(20, 0, 29, 9, player3of3.PlayerId);

                        break;

                    case 4:
                        gameBoard = new Board(20, 20);
                        var player1of4 = players[0];
                        var player2of4 = players[1];
                        var player3of4 = players[2];
                        var player4of4 = players[3];

                        gameBoard.AssignBoardSection(0, 0, 9, 9, player1of4.PlayerId);
                        gameBoard.AssignBoardSection(0, 10, 9, 19, player2of4.PlayerId);
                        gameBoard.AssignBoardSection(10, 0, 19, 9, player3of4.PlayerId);
                        gameBoard.AssignBoardSection(10, 10, 19, 19, player4of4.PlayerId);

                        break;

                    default:
                        break;
                }

                connection.GameRoom.GameState = Enums.GameState.PlacingShips;
                Console.WriteLine($"Game state changed to: {connection.GameRoom.GameState}");
                await Clients.Group(gameRoomName).SendAsync("GameStateChanged", (int)connection.GameRoom.GameState);
                await Clients.Group(gameRoomName).SendAsync("BoardGenerated", gameRoomName, gameBoard);
            }
        }

        public async Task StartGame()
        {
            if (_db.connections.TryGetValue(Context.ConnectionId, out UserConnection connection))
            {
                connection.GameRoom.GameState = Enums.GameState.InProgress;
                Console.WriteLine($"Game state changed to: {connection.GameRoom.GameState}");
                await Clients.Group(connection.GameRoom.GameRoomName).SendAsync("GameStateChanged", (int)connection.GameRoom.GameState);
            }
        }
    }
}
