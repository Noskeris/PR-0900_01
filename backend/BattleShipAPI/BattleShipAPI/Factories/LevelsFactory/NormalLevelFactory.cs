using BattleShipAPI.Builders;
using BattleShipAPI.Builders.ShipConfigs;
using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.GameItems.SuperAttacks;
using BattleShipAPI.Models;

namespace BattleShipAPI.Factories.LevelsFactory
{
    public class NormalLevelFactory : AbstractLevelFactory
    {
        public override Ships CreateShipsConfig()
        {
            var shipConfigs = new List<ShipConfig>
            {
                this.BattleshipBuilder.StartNormal().Build(),
                this.CarrierBuilder.StartNormal().Build(),
                this.CruiserBuilder.StartNormal().Build(),
                this.DestroyerBuilder.StartNormal().Build(),
                this.SubmarineBuilder.StartNormal().Build()
            };

            return new LevelShips(shipConfigs);
        }

        //TODO SUPPER ATTACKS
        //var superAttacksBuilder = new SuperAttacksBuilder();
        //SuperAttacksConfig = new FourPlayersSuperAttacks(superAttacksBuilder);

        public override void SetTimer()
        {
            timerDuration = 30;
        }
    }
}
