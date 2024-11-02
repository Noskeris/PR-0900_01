using BattleShipAPI.Enums.Avatar;

namespace BattleShipAPI.Bridge;

public class AvatarDto
{
    public string HeadType { get; set; }
    public MoodType Mood { get; set; }
    public bool HasPimples { get; set; }
    public AppearanceDto Appearance { get; set; }
}
