using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Models;

public class AttackRequestValidationRequest
{
    public Cell Cell { get; set; }
    public UserConnection Connection { get; set; }
    public HubCallerContext Context { get; set; }
    public IHubCallerClients Clients { get; set; }
}