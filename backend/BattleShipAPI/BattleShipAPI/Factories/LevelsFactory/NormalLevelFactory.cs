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

        public override SuperAttacks CreateSuperAttacksConfig()
        {
            var superAttacksConfigs = new List<SuperAttackConfig>
            {
                this.PlusAttackBuilder.StartNormal().Build(),
                this.CrossAttackBuilder.StartNormal().Build(),
                this.BoomAttackBuilder.StartNormal().Build(),
            };

            return new LevelSuperAttacks(superAttacksConfigs);
        }

        public override void SetTimer()
        {
            timerDuration = 30;
        }
    }
}
