namespace BattleShipAPI.Builders.SuperAttackConfigs
{
    public class PlusAttackConfigBuilder : SuperAttackConfigBuilder
    {
        public override SuperAttackConfigBuilder StartMobility()
        {
            superAttackConfig.AttackType = Enums.AttackType.Plus;
            superAttackConfig.Count = 1;
            return this;
        }

        public override SuperAttackConfigBuilder StartNormal()
        {
            superAttackConfig.AttackType = Enums.AttackType.Plus;
            superAttackConfig.Count = 2;
            return this;
        }

        public override SuperAttackConfigBuilder StartRapid()
        {
            superAttackConfig.AttackType = Enums.AttackType.Plus;
            superAttackConfig.Count = 2;
            return this;
        }
    }
}
