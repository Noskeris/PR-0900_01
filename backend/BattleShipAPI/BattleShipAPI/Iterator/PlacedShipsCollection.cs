using BattleShipAPI.Decorator;
using BattleShipAPI.Enums;

namespace BattleShipAPI.Iterator
{
    public class PlacedShipsCollection : IAggregate<IPlacedShip>
    {
        private readonly List<IPlacedShip> _placedShips = new();

        public IIterator<IPlacedShip> CreateIterator()
        {
            return new PlacedShipIterator(_placedShips);
        }

        public void Add(IPlacedShip ship)
        {
            _placedShips.Add(ship);
        }

        public bool Remove(IPlacedShip ship)
        {
            return _placedShips.Remove(ship);
        }

        public void Clear()
        {
            _placedShips.Clear();
        }

        public int Count => _placedShips.Count;

        public List<IPlacedShip> ToList()
        {
            return new List<IPlacedShip>(_placedShips);
        }

        public int CountShipsOfType(ShipType shipType)
        {
            var count = 0;
            var iterator = CreateIterator();
            while (iterator.HasNext())
            {
                var ship = iterator.Next();
                if (ship.ShipType == shipType)
                {
                    count++;
                }
            }
            return count;
        }

        public bool ContainsCoordinate(int x, int y)
        {
            var iterator = CreateIterator();
            while (iterator.HasNext())
            {
                var ship = iterator.Next();
                if (ship.GetCoordinates().Contains((x, y)))
                {
                    return true;
                }
            }
            return false;
        }
    }

}
