using BattleShipAPI.Enums.Avatar;

namespace BattleShipAPI.Bridge;

public class RoundHeadedAvatar : Avatar
{
    public RoundHeadedAvatar(Appearance appearance) : base(appearance) 
    { 
        SetMood(); 
        HasPimples = new Random().Next(0, 2) == 1;
    }

    public sealed override void SetMood()
    {
        Mood = new Random().Next(0, 2) == 0 ? MoodType.Happy : MoodType.Sad;
    }
}