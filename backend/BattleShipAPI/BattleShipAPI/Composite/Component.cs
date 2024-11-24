namespace BattleShipAPI.Composite
{
    //Component of Composite pattern
    public abstract class Component
    {
        public virtual bool ValidatePlacement()
        {
            throw new NotImplementedException("This component does not implement validation logic.");
        }

        public virtual void Add(Component component)
        {
            throw new NotImplementedException("This component does not support adding children.");
        }

        public virtual void Remove(Component component)
        {
            throw new NotImplementedException("This component does not support removing children.");
        }

        public virtual Component GetChild(int index)
        {
            throw new NotImplementedException("This component does not support child access.");
        }
    }
}
