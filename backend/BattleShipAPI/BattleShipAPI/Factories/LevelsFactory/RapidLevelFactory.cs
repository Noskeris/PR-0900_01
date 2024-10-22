using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.Models;

namespace BattleShipAPI.Factories.LevelsFactory
{
    public class RapidLevelFactory : AbstractLevelFactory
    {
        public override Ships CreateShipsConfig()
        {
            var shipConfigs = new List<ShipConfig>
            {
                this.BattleshipBuilder.StartRapid().AddShield().Build(),
                this.CarrierBuilder.StartRapid().AddShield().Build(),
                this.CruiserBuilder.StartRapid().AddShield().Build(),
                this.DestroyerBuilder.StartRapid().AddShield().Build(),
                this.SubmarineBuilder.StartRapid().AddShield().Build()
            };

            return new LevelShips(shipConfigs);
        }

        public override void SetTimer()
        {
            timerDuration = 5;
        }
    }
}
