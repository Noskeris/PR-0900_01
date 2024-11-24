using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.GameItems.SuperAttacks;
using BattleShipAPI.Models;
using BattleShipAPI.Enums;

namespace BattleShipAPI.Factories.LevelsFactory
{
    public class RapidLevelFactory : AbstractLevelFactory
    {
        public override Ships CreateShipsConfig()
        {
            var shipConfigs = new List<ShipConfig>
            {
                this.BattleshipBuilder.SetGameMode(GameMode.Rapid).SetGlowing(true).Build(),
                this.CarrierBuilder.SetGameMode(GameMode.Rapid).SetGlowing(true).Build(),
                this.CruiserBuilder.SetGameMode(GameMode.Rapid).SetFragile(true).Build(),
                this.DestroyerBuilder.SetGameMode(GameMode.Rapid).SetFragile(true).Build(),
                this.SubmarineBuilder.SetGameMode(GameMode.Rapid).SetShield(true).Build()
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
