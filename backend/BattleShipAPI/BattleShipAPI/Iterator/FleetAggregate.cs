using BattleShipAPI.Composite;

namespace BattleShipAPI.Iterator
{
    public class FleetAggregate : IAggregate<Component>
    {
        private readonly Fleet _fleet = new();

        public FleetAggregate(Fleet fleet)
        {
            _fleet = fleet;
        }

        public IIterator<Component> CreateIterator()
        {
            return new FleetIterator(_fleet);
        }
    }
}
