namespace BattleShipAPI.Composite
{
    //Composite class of Composite pattern
    public class Fleet : Component
    {
        private readonly List<Component> _components = new List<Component>();

        public override void Add(Component component)
        {
            _components.Add(component);
        }

        public override void Remove(Component component)
        {
            _components.Remove(component);
        }

        public override Component GetChild(int index)
        {
            return _components[index];
        }

        public override bool ValidatePlacement()
        {
            foreach (var component in _components)
            {
                if (!component.ValidatePlacement())
                {
                    return false;
                }
            }
            return true;
        }
        public override IEnumerable<Component> GetChildren()
        {
            return _components;
        }
    }
}
