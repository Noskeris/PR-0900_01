namespace BattleShipAPI.Builders;

public interface IConfigBuilder<TConfig>
{
    IConfigBuilder<TConfig> AddConfig(TConfig config);
    List<TConfig> Build();
}