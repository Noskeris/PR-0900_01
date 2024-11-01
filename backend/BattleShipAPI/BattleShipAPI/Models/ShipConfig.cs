// File: Models/ShipConfig.cs
using System;
using BattleShipAPI.Enums;

namespace BattleShipAPI.Models
{
    public class ShipConfig
    {
        public int Count { get; set; } = 1;
        public ShipType ShipType { get; set; }
        public int Size { get; set; }
        public bool HasShield { get; set; }
        public bool HasMobility { get; set; }
        public bool IsFragile { get; set; }
        public bool IsGlowing { get; set; }

        public IPlacedShip CreateShip(int startX, int startY, int endX, int endY, Action<IPlacedShip> revealShipAction = null)
        {
            IPlacedShip ship = new PlacedShip
            {
                ShipType = this.ShipType,
                StartX = startX,
                StartY = startY,
                EndX = endX,
                EndY = endY
            };

            if (HasShield)
            {
                ship = new ShieldedShipDecorator(ship, shieldStrength: 2); 
            }

            if (IsFragile)
            {
                ship = new FragileShipDecorator(ship);
            }

            if (IsGlowing)
            {
                ship = new GlowingShipDecorator(ship, revealShipAction);
            }

            return ship;
        }
    }
}
