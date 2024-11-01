using BattleShipAPI.Enums.Avatar;

namespace BattleShipAPI.Bridge;

public class AvatarFactory
{
    public static Avatar CreateAvatar(HeadType headType, AppearanceType appearanceType)
    {
        IAppearance appearance = appearanceType switch
        {
            AppearanceType.Hair => new HairAppearance(),
            AppearanceType.Cap => new CapAppearance(),
            _ => throw new ArgumentException("Invalid appearance type")
        };

        return headType switch
        {
            HeadType.RoundHeaded => new RoundHeadedAvatar(appearance),
            HeadType.TriangleHeaded => new TriangleHeadedAvatar(appearance),
            _ => throw new ArgumentException("Invalid head type")
        };
    }
}