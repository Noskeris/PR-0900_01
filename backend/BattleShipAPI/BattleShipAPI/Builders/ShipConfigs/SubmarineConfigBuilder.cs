﻿using BattleShipAPI.Models;

namespace BattleShipAPI.Builders.ShipConfigs
{
    public class SubmarineConfigBuilder : ShipConfigBuilder
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
            shipConfig.ShipType = Enums.ShipType.Submarine;
            shipConfig.Size = 2;
            return this;
        }

        public override ShipConfigBuilder StartNormal()
        {
            shipConfig.ShipType = Enums.ShipType.Submarine;
            shipConfig.Size = 3;
            return this;
        }

        public override ShipConfigBuilder StartRapid()
        {
            shipConfig.ShipType = Enums.ShipType.Submarine;
            shipConfig.Size = 4;
            return this;
        }
    }
}
