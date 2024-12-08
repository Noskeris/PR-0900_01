using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Models;

public class InformGroupRequest
{
    public IHubCallerClients Clients { get; set; }
    public string GroupName { get; set; }
    public string Key { get; set; }
    public object?[] Values { get; set; }
}