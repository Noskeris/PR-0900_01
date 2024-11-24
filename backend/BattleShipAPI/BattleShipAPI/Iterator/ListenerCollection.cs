using BattleShipAPI.Notifications;
using System.Collections.Concurrent;

namespace BattleShipAPI.Iterator
{
    public class ListenerCollection : IAggregate<Listener>
    {
        private readonly ConcurrentDictionary<Listener, bool> _listeners;

        public ListenerCollection(ConcurrentDictionary<Listener, bool> listeners)
        {
            _listeners = listeners;
        }

        public IIterator<Listener> CreateIterator()
        {
            return new ListenerIterator(_listeners.Keys);
        }
    }
}
