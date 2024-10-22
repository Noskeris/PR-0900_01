using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.Models;

namespace BattleShipAPI.Factories.LevelsFactory
{
    public class MobilityLevelFactory : AbstractLevelFactory
    {
        public override Ships CreateShipsConfig()
        {
            var shipConfigs = new List<ShipConfig>
            {
                this.BattleshipBuilder.StartMobility().AddMobility().Build(),
                this.CarrierBuilder.StartMobility().AddMobility().Build(),
                this.CruiserBuilder.StartMobility().AddMobility().Build(),
                this.DestroyerBuilder.StartMobility().AddMobility().Build(),
                this.SubmarineBuilder.StartMobility().AddMobility().Build()
            };

            return new LevelShips(shipConfigs);
        }

        public override void SetTimer()
        {
            timerDuration = -1;
        }
    }
}
