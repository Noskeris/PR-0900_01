using BattleShipAPI.Composite;
using BattleShipAPI.Models;

namespace BattleShipAPI.Visitor
{
    public interface IVisitor
    {
        void VisitFleet(Fleet fleet);
        void VisitSubFleet(SubFleet subFleet);
        void VisitPlacedShip(PlacedShip placedShip);
    }
}
