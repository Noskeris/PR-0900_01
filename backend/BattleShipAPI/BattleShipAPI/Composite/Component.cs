using BattleShipAPI.Visitor;

namespace BattleShipAPI.Composite
{
    //Component of Composite pattern
    public abstract class Component : IVisitable
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

        public virtual IEnumerable<Component> GetChildren()
        {
            throw new NotImplementedException("This component does not support iterating over children.");
        }

        public virtual void Accept(IVisitor visitor)
        {
            throw new NotImplementedException($"This component does not implement Accept for visitors.");
        }
    }
}
