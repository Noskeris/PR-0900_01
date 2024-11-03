using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.GameItems.SuperAttacks;
using BattleShipAPI.Models;

namespace BattleShipAPI.Factories.LevelsFactory
{
    public class RapidLevelFactory : AbstractLevelFactory
    {
        public override Ships CreateShipsConfig()
        {
            var shipConfigs = new List<ShipConfig>
            {
                this.BattleshipBuilder.StartRapid().AddFragile().Build(),
                this.CarrierBuilder.StartRapid().AddFragile().Build(),
                this.CruiserBuilder.StartRapid().AddFragile().Build(),
                this.DestroyerBuilder.StartRapid().AddFragile().Build(),
                this.SubmarineBuilder.StartRapid().AddFragile().Build()
            };

            return new LevelShips(shipConfigs);
        }

        public override SuperAttacks CreateSuperAttacksConfig()
        {
            var superAttacksConfigs = new List<SuperAttackConfig>
            {
                this.PlusAttackBuilder.StartRapid().Build(),
                this.CrossAttackBuilder.StartRapid().Build(),
                this.BoomAttackBuilder.StartRapid().Build(),
            };

            return new LevelSuperAttacks(superAttacksConfigs);
        }

        public override void SetTimer()
        {
            timerDuration = 5;
        }
    }
}
