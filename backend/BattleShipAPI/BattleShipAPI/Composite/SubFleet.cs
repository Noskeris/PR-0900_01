using BattleShipAPI.Visitor;

namespace BattleShipAPI.Composite
{
    public class SubFleet : Fleet
    {
        private readonly SubFleetType _type;

        public SubFleet(SubFleetType type)
        {
            _type = type;
        }

        public SubFleetType GetFleetType()
        {
            return _type;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.VisitSubFleet(this);
            base.Accept(visitor); 
        }
    }
}
