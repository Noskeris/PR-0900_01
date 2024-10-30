using BattleShipAPI.Hubs;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

public class CommandContext
{
    public GameHub Hub { get; }
    public IHubCallerClients Clients { get; }
    public HubCallerContext CallerContext { get; }
    public string ConnectionId { get; }
    public InMemoryDB Db { get; }

    public CommandContext(GameHub hub)
    {
        Hub = hub;
        Clients = hub.Clients;
        CallerContext = hub.Context;
        ConnectionId = hub.Context.ConnectionId;
        Db = InMemoryDB.Instance;
    }
}
