using BattleShipAPI.Models;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Hubs
{
    public class GameHub : Hub
    {
        private readonly InMemoryDB _db;

        public GameHub(InMemoryDB db) => _db = db;

        public async Task JoinRoom (UserConnection connection)
        {
            await Clients.All.SendAsync("RecieveMessage", "admin", $"{connection.Username} has joined");
        }

        public async Task JoinSpecificGameRoom(UserConnection connection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, connection.GameRoom);

            _db.connections[Context.ConnectionId] = connection;

            await Clients.Group(connection.GameRoom).SendAsync("JoinSpecificGameRoom", "admin", $"{connection.Username} has joined the chat room {connection.GameRoom}");
        }

        public async Task SendMessage(string message)
        {
            if(_db.connections.TryGetValue(Context.ConnectionId, out UserConnection connection))
            {
                await Clients.Group(connection.GameRoom)
                    .SendAsync("RecieveSpecificMessage", connection.Username, message);

            }
        }
    }
}
