using BattleShipAPI.Bridge;

namespace BattleShipAPI.Models;

public class AvatarResponse
{
    public string Nickname { get; set; }
    public AvatarDto Avatar { get; set; }
    
    public AvatarResponse(string nickname, AvatarDto avatar)
    {
        Nickname = nickname;
        Avatar = avatar;
    }
}