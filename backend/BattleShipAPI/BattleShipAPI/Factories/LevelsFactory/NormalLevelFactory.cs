using BattleShipAPI.Enums;
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
                //visus sutvarkyt
                this.BattleshipBuilder.SetGameMode(GameMode.Normal).Build(),
                this.CarrierBuilder.SetGameMode(GameMode.Normal).Build(),
                this.CruiserBuilder.SetGameMode(GameMode.Normal).Build(),
                this.DestroyerBuilder.SetGameMode(GameMode.Normal).Build(),
                this.SubmarineBuilder.SetGameMode(GameMode.Normal).Build()
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
