using BattleShipAPI.Enums;
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
    public abstract ShipConfigBuilder AddGlowing();
    public abstract ShipConfigBuilder AddFragile();


    public GameMode gameMode;
    public bool glowing;
    public bool shielded;
    public bool mobile;
    public bool fragile;

    public ShipConfig Build()
    {
        if (gameMode == GameMode.Normal)
        {
            StartNormal();
        }
        else if (gameMode == GameMode.Rapid)
        {
            StartRapid();
        }
        else if (gameMode == GameMode.Mobility)
        {
            StartMobility();
        }
        if (shielded)
        {
            AddShield();
        }
        if (mobile)
        {
            AddMobility();
        }
        if (fragile)
        {
            AddFragile();
        }
        if (glowing)
        {
            AddGlowing();
        }


        return shipConfig;
    }

    public ShipConfigBuilder SetGameMode(GameMode gameMode)
    {
        this.gameMode = gameMode;
        return this;
    }
    public ShipConfigBuilder SetShield(bool shield)
    {
        this.shielded = shield;
        return this;
    }
    public ShipConfigBuilder SetMobility(bool mobile)
    {
        this.mobile = mobile;
        return this;
    }
    public ShipConfigBuilder SetGlowing(bool glow)
    {
        this.glowing = glow;
        return this; 
    }
    public ShipConfigBuilder SetFragile(bool fragile)
    {
        this.fragile = fragile;
        return this;
    }
}
