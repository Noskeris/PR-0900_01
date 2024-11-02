using BattleShipAPI.Enums.Avatar;

namespace BattleShipAPI.Bridge;

public class TriangleHeadedAvatar : Avatar
{
    public TriangleHeadedAvatar(IAppearance appearance) : base(appearance) 
    { 
        SetMood(); 
        HasPimples = new Random().Next(0, 2) == 1;
    }

    public sealed override void SetMood()
    {
        Mood = new Random().Next(0, 2) == 0 ? MoodType.Angry : MoodType.Neutral;
    }
}