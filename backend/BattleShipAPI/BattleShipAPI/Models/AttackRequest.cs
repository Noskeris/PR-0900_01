using BattleShipAPI.Enums;

namespace BattleShipAPI.Models;

public class AttackRequest
{
    public int X { get; set; }
    public int Y { get; set; }
    public AttackType AttackType { get; set; }
}