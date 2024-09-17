using BattleShipAPI.Models;
using System.Collections.Concurrent;

namespace BattleShipAPI.Repository
{
    public class InMemoryDB
    {
        private readonly ConcurrentDictionary<string, UserConnection> _connections = new ConcurrentDictionary<string, UserConnection>();

        public ConcurrentDictionary<string, UserConnection> connections => _connections;
    }
}
