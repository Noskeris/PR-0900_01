using BattleShipAPI.Builders.ShipConfigs;
using BattleShipAPI.Builders.SuperAttackConfigs;
using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.GameItems.SuperAttacks;
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
        protected SuperAttackConfigBuilder PlusAttackBuilder;
        protected SuperAttackConfigBuilder CrossAttackBuilder;
        protected SuperAttackConfigBuilder BoomAttackBuilder;

        protected int timerDuration;

        public AbstractLevelFactory()
        {
            CarrierBuilder = new CarrierConfigBuilder();
            BattleshipBuilder = new BattleshipConfigBuilder();
            CruiserBuilder = new CruiserConfigBuilder();
            SubmarineBuilder = new SubmarineConfigBuilder();
            DestroyerBuilder = new DestroyerConfigBuilder();
            PlusAttackBuilder = new PlusAttackConfigBuilder();
            CrossAttackBuilder = new CrossAttackConfigBuilder();
            BoomAttackBuilder = new BoomAttackConfigBuilder();
        }

        public abstract Ships CreateShipsConfig();
        public abstract SuperAttacks CreateSuperAttacksConfig();
        public abstract void SetTimer();
        public GameRoomSettings CreateGameRoomSettings(Ships ships, SuperAttacks superAttacks, Board board)
        {
            return new GameRoomSettings(ships, superAttacks, board, timerDuration);
        }
    }
}
