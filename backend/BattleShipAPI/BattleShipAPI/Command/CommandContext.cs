using BattleShipAPI.Facade;
using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

public class CommandContext
{
    public IHubCallerClients Clients { get; }
    public HubCallerContext CallerContext { get; }
    public InMemoryDB Db { get; }
    public GameFacade GameFacade { get; }
    public INotificationService NotificationService { get; }

    public CommandContext(
        GameFacade gameFacade,
        IHubCallerClients clients,
        HubCallerContext callerContext,
        INotificationService notificationService)
    {
        GameFacade = gameFacade;
        Clients = clients;
        CallerContext = callerContext;
        NotificationService = notificationService;
        Db = InMemoryDB.Instance;
    }
}
