namespace BattleShipAPI.Builders.ShipConfigs
{
    public class DestroyerConfigBuilder : ShipConfigBuilder
    {
        protected override ShipConfigBuilder AddMobility()
        {
            shipConfig.HasMobility = true;
            return this;
        }

        protected override ShipConfigBuilder AddShield()
        {
            shipConfig.HasShield = true;
            return this;
        }

        protected override ShipConfigBuilder AddGlowing()
        {
            shipConfig.IsGlowing = true;
            return this;
        }

        protected override ShipConfigBuilder AddFragile()
        {
            shipConfig.IsFragile = true;
            return this;
        }

        public override ShipConfigBuilder StartMobility()
        {
            shipConfig.ShipType = Enums.ShipType.Destroyer;
            shipConfig.Size = 1;
            return this;
        }

        public override ShipConfigBuilder StartNormal()
        {
            shipConfig.ShipType = Enums.ShipType.Destroyer;
            shipConfig.Size = 2;
            return this;
        }

        public override ShipConfigBuilder StartRapid()
        {
            shipConfig.ShipType = Enums.ShipType.Destroyer;
            shipConfig.Size = 3;
            return this;
        }
    }
}
