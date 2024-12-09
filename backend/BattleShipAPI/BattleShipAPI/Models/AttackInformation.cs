using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Models;

public class AttackInformation
{
    public GameRoom GameRoom { get; set; }
    public UserConnection Connection { get; set; }
    public HubCallerContext Context { get; set; }
    public IHubCallerClients Clients { get; set; }
    public AttackRequest AttackRequest { get; set; }
}