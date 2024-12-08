using BattleShipAPI.Enums.Avatar;

namespace BattleShipAPI.Models;

public class AvatarRequest
{
    public HeadType HeadType { get; set; }
    public AppearanceType AppearanceType { get; set; }
}