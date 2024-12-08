using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.ChainOfResponsibility;

public interface IGameHandler
{
    IGameHandler SetNext(IGameHandler nextHandler);
    Task HandleRequest(string action, HubCallerContext context, IHubCallerClients clients, object? data);
}
