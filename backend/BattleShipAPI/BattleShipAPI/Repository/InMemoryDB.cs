using BattleShipAPI.Models;
using System.Collections.Concurrent;

//1. DESIGN PATTERN: Singleton
namespace BattleShipAPI.Repository
{
    public class InMemoryDB
    {
        private static InMemoryDB _instance;
        private static readonly object _lock = new object();

        public ConcurrentDictionary<string, GameRoom> GameRooms { get; set; }
        public ConcurrentDictionary<string, UserConnection> Connections { get; set; }

        private InMemoryDB()
        {
            GameRooms = new ConcurrentDictionary<string, GameRoom>();
            Connections = new ConcurrentDictionary<string, UserConnection>();
        }

        public static InMemoryDB Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new InMemoryDB();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
