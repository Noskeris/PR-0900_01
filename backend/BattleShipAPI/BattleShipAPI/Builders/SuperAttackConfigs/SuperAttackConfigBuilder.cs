using BattleShipAPI.Models;

namespace BattleShipAPI.Builders.SuperAttackConfigs
{
    public abstract class SuperAttackConfigBuilder 
    {
        protected SuperAttackConfig superAttackConfig = new();

        public SuperAttackConfigBuilder StartNew(SuperAttackConfig newSuperAttackConfig)
        {
            superAttackConfig = newSuperAttackConfig;
            return this;
        }

        public abstract SuperAttackConfigBuilder StartNormal();
        public abstract SuperAttackConfigBuilder StartRapid();
        public abstract SuperAttackConfigBuilder StartMobility();

        public SuperAttackConfig Build()
        {
            return superAttackConfig;
        }
    }
}
