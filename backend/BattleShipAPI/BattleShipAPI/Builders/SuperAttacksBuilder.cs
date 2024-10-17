using BattleShipAPI.Models;

namespace BattleShipAPI.Builders;

public class SuperAttacksBuilder : IConfigBuilder<SuperAttackConfig>
{
    private readonly List<SuperAttackConfig> _superAttacksConfig = new();

    public IConfigBuilder<SuperAttackConfig> AddConfig(SuperAttackConfig config)
    {
        _superAttacksConfig.Add(config);
        return this;
    }

    public List<SuperAttackConfig> Build()
    {
        return _superAttacksConfig;
    }
}
