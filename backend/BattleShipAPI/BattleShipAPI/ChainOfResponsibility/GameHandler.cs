using BattleShipAPI.Mediator;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.ChainOfResponsibility;

public abstract class GameHandler : BaseComponent, IGameHandler
{
    protected IGameHandler? NextHandler;

    public IGameHandler SetNext(IGameHandler nextHandler)
    {
        NextHandler = nextHandler;

        return nextHandler;
    }

    public virtual async Task HandleRequest(string action, HubCallerContext context, IHubCallerClients clients, object? data)
    {
        if (NextHandler != null)
        {
            await NextHandler.HandleRequest(action, context, clients, data);
        }
    }
}
