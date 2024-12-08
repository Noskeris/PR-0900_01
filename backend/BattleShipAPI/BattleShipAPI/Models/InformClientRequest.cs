using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Models;

public class InformClientRequest
{
    public IHubCallerClients Clients { get; set; }
    public string ClientId { get; set; }
    public string Key { get; set; }
    public object?[] Values { get; set; }
}