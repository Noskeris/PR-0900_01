using BattleShipAPI.Decorator;

namespace BattleShipAPI.Iterator
{
    public class PlacedShipIterator : IIterator<IPlacedShip>
    {
        private readonly List<IPlacedShip> _placedShips;
        private int _position = 0;

        public PlacedShipIterator(List<IPlacedShip> placedShips)
        {
            _placedShips = placedShips;
        }

        public bool HasNext()
        {
            return _position < _placedShips.Count;
        }

        public IPlacedShip Next()
        {
            if (!HasNext())
                throw new InvalidOperationException("No more elements.");

            return _placedShips[_position++];
        }

        public void Reset()
        {
            _position = 0;
        }
    }

}
