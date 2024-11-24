using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.GameItems.SuperAttacks;
using BattleShipAPI.Models;
using BattleShipAPI.Enums;

namespace BattleShipAPI.Factories.LevelsFactory
{
    public class MobilityLevelFactory : AbstractLevelFactory
    {
        public override Ships CreateShipsConfig()
        {
            var shipConfigs = new List<ShipConfig>
            {
                this.BattleshipBuilder.SetGameMode(GameMode.Mobility).SetMobility(true).Build(),
                this.CarrierBuilder.SetGameMode(GameMode.Mobility).SetMobility(true).Build(),
                this.CruiserBuilder.SetGameMode(GameMode.Mobility).SetMobility(true).Build(),
                this.DestroyerBuilder.SetGameMode(GameMode.Mobility).SetMobility(true).Build(),
                this.SubmarineBuilder.SetGameMode(GameMode.Mobility).SetMobility(true).Build()
            };

            return new LevelShips(shipConfigs);
        }

        public override SuperAttacks CreateSuperAttacksConfig()
        {
            var superAttacksConfigs = new List<SuperAttackConfig>
            {
                this.PlusAttackBuilder.StartMobility().Build(),
                this.CrossAttackBuilder.StartMobility().Build(),
                this.BoomAttackBuilder.StartMobility().Build(),
            };

            return new LevelSuperAttacks(superAttacksConfigs);
        }

        public override void SetTimer()
        {
            timerDuration = -1;
        }
    }
}
