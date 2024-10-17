using BattleShipAPI.Models;

namespace BattleShipAPI.Builders;

public class ShipsBuilder : IConfigBuilder<ShipConfig>
{
    private readonly List<ShipConfig> _shipsConfig = new();

    public IConfigBuilder<ShipConfig> AddConfig(ShipConfig config)
    {
        _shipsConfig.Add(config);
        return this;
    }

    public List<ShipConfig> Build()
    {
        return _shipsConfig;
    }
}
