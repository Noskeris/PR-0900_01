namespace BattleShipAPI.Builders.ShipConfigs
{
    public class BattleshipConfigBuilder : ShipConfigBuilder
    {
        public override ShipConfigBuilder AddMobility()
        {
            shipConfig.HasMobility = true;
            return this;
        }

        public override ShipConfigBuilder AddShield()
        {
            shipConfig.HasShield = true;
            return this;
        }

        public override ShipConfigBuilder StartMobility()
        {
            shipConfig.ShipType = Enums.ShipType.Battleship;
            shipConfig.Size = 4;
            return this;
        }

        public override ShipConfigBuilder StartNormal()
        {
            shipConfig.ShipType = Enums.ShipType.Battleship;
            shipConfig.Size = 5;
            return this;
        }

        public override ShipConfigBuilder StartRapid()
        {
            shipConfig.ShipType = Enums.ShipType.Battleship;
            shipConfig.Size = 6;
            return this;
        }
    }
}
