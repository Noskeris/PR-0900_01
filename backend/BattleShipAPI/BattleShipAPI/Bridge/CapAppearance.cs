using BattleShipAPI.Enums.Avatar;

namespace BattleShipAPI.Bridge;

public class CapAppearance : Appearance
{
    private static readonly Random _random = new Random();

    public string Shape => _random.Next(0, 2) == 0 ? CapShape.FullCap.ToString() : CapShape.Hat.ToString();
    public string Color => "Grey";
}