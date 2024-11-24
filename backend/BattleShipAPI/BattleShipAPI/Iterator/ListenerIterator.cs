using BattleShipAPI.Notifications;

namespace BattleShipAPI.Iterator
{
    public class ListenerIterator : IIterator<Listener>
    {
        private readonly List<Listener> _listeners;
        private int _position = 0;

        public ListenerIterator(IEnumerable<Listener> listeners)
        {
            _listeners = listeners.ToList();
        }

        public bool HasNext()
        {
            return _position < _listeners.Count;
        }

        public Listener Next()
        {
            return _listeners[_position++];
        }

        public void Reset()
        {
            _position = 0;
        }
    }
}
