using BattleShipAPI.Builders.ShipConfigs;
using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.Models;

namespace BattleShipAPI.Factories.LevelsFactory
{
    public abstract class AbstractLevelFactory
    {
        protected ShipConfigBuilder CarrierBuilder;
        protected ShipConfigBuilder BattleshipBuilder;
        protected ShipConfigBuilder CruiserBuilder;
        protected ShipConfigBuilder SubmarineBuilder;
        protected ShipConfigBuilder DestroyerBuilder;

        protected int timerDuration;

        public AbstractLevelFactory()
        {
            CarrierBuilder = new CarrierConfigBuilder();
            BattleshipBuilder = new BattleshipConfigBuilder();
            CruiserBuilder = new CruiserConfigBuilder();
            SubmarineBuilder = new SubmarineConfigBuilder();
            DestroyerBuilder = new DestroyerConfigBuilder();
        }

        public abstract Ships CreateShipsConfig();
        public abstract void SetTimer();
        public int GetTimerDuration()
        {
            return timerDuration;
        }
    }
}
