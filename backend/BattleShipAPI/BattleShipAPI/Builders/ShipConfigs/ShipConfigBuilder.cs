using BattleShipAPI.Models;

namespace BattleShipAPI.Builders.ShipConfigs;

public abstract class ShipConfigBuilder
{
    protected ShipConfig shipConfig = new();

    public ShipConfigBuilder StartNew(ShipConfig newShipConfig)
    {
        shipConfig = newShipConfig;
        return this;
    }

    public abstract ShipConfigBuilder StartNormal();
    public abstract ShipConfigBuilder StartRapid();
    public abstract ShipConfigBuilder StartMobility();
    public abstract ShipConfigBuilder AddShield();
    public abstract ShipConfigBuilder AddMobility();

    public ShipConfig Build()
    {
        return shipConfig;
    }
}
