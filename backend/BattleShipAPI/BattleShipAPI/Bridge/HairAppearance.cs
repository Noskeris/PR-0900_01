using BattleShipAPI.Enums.Avatar;

namespace BattleShipAPI.Bridge;

public class HairAppearance : Appearance
{
    private static readonly Random _random = new Random();
    
    public string Shape => _random.Next(0, 2) switch
    {
        0 => HairShape.Short.ToString(),
        _ => HairShape.Puff.ToString()
    };
    
    public string Color => "Red";
}