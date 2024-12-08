using BattleShipAPI.Composite;
using BattleShipAPI.Models;

namespace BattleShipAPI.Visitor
{
    public class ShipPlacementVisitor : IVisitor
    {
        private readonly List<PlacedShip> _validShips = new();
        private readonly List<string> _validationErrors = new();

        public void VisitFleet(Fleet fleet)
        {
            if(fleet.GetChildren().Count() <= 0)
            {
                _validationErrors.Add($"Invalid placement for fleet. It has no subfleets.");
            }
        }

        public void VisitSubFleet(SubFleet subFleet)
        {
            if (!subFleet.GetChildren().Any())
            {
                _validationErrors.Add($"Subfleet of type {subFleet.GetFleetType()} has no ships.");
            }
        }

        public void VisitPlacedShip(PlacedShip placedShip)
        {
            // Validate the placement of the individual ship
            if (placedShip.ValidatePlacement())
            {
                _validShips.Add(placedShip);
            }
            else
            {
                _validationErrors.Add($"Invalid placement for ship of type {placedShip.ShipType.ToString()}.");
            }
        }

        public IReadOnlyList<PlacedShip> GetValidShips() => _validShips;
        public IReadOnlyList<string> GetValidationErrors() => _validationErrors;
    }
}
