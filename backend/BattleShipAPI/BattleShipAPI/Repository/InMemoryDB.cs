using BattleShipAPI.Models;
using System.Collections.Concurrent;

namespace BattleShipAPI.Repository
{
    public class InMemoryDB
    {
        public ConcurrentDictionary<string, UserConnection> Connections { get; } = new();
        
        public ConcurrentDictionary<string, GameRoom> GameRooms { get; } = new();
    }
}
