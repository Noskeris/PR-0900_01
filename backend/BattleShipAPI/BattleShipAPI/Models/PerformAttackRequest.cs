using BattleShipAPI.Enums;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Models;

public class PerformAttackRequest
{
    public IHubCallerClients Clients { get; set; }
    public string ClientId { get; set; }
    public UserConnection Connection { get; set; }
    public AttackType AttackType { get; set; }
    public GameRoom GameRoom { get; set; }
}