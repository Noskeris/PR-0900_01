namespace BattleShipAPI.Builders.ShipConfigs
{
    public class CarrierConfigBuilder : ShipConfigBuilder
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
            shipConfig.ShipType = Enums.ShipType.Carrier;
            shipConfig.Size = 5;
            return this;
        }

        public override ShipConfigBuilder StartNormal()
        {
            shipConfig.ShipType = Enums.ShipType.Carrier;
            shipConfig.Size = 6;
            return this;
        }

        public override ShipConfigBuilder StartRapid()
        {
            shipConfig.ShipType = Enums.ShipType.Carrier;
            shipConfig.Size = 7;
            return this;
        }
    }
}
