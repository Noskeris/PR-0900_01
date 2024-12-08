using BattleShipAPI.Composite;

namespace BattleShipAPI.Iterator
{
    public class FleetIterator : IIterator<Component>
    {
        private readonly List<Component> _components = new List<Component>();
        private int _currentIndex = 0;

        public FleetIterator(Fleet fleet)
        {
            var rootComponents = fleet.GetChildren().ToList();
            if (rootComponents != null && rootComponents.Count > 0)
            {
                foreach (var component in rootComponents)
                {
                    AddComponentAndChildren(component);
                }
            }
        }

        public bool HasNext()
        {
            return _currentIndex < _components.Count;
        }

        public Component Next()
        {
            if (!HasNext())
            {
                throw new InvalidOperationException("No more elements.");
            }

            var current = _components[_currentIndex];
            _currentIndex++;
            return current;
        }

        public void Reset()
        {
            _currentIndex = 0;
        }

        private void AddComponentAndChildren(Component component)
        {
            _components.Add(component);

            if (component is Fleet fleet)
            {
                foreach (var child in fleet.GetChildren())
                {
                    AddComponentAndChildren(child);
                }
            }
        }
    }
}
