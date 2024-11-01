using System.Text.Json;
using BattleShipAPI.Enums.Avatar;

namespace BattleShipAPI.Bridge;

public abstract class Avatar
{
    protected IAppearance Appearance;
    public MoodType Mood { get; protected set; }
    public bool HasPimples { get; protected set; }

    protected Avatar(IAppearance appearance)
    {
        Appearance = appearance;
    }

    public abstract void SetMood();

    public AvatarDto GetAvatarParameters()
    {
        var avatarDto = new AvatarDto
        {
            HeadType = GetType().Name.Replace("Avatar", ""),
            Mood = Mood,
            HasPimples = HasPimples,
            Appearance = new AppearanceDto
            {
                Type = Appearance.GetType().Name.Replace("Appearance", ""),
                Shape = Appearance.Shape,
                Color = Appearance.Color
            }
        };
        
        Console.WriteLine(JsonSerializer.Serialize(avatarDto));
        return avatarDto;
    }
}