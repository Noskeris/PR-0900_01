using BattleShipAPI.Models;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.State;

public interface IGameState
{
    Task HandleDisconnection(GameContext context, UserConnection connection, HubCallerContext callerContext, IHubCallerClients clients);
    Task RestartGame(GameContext context, UserConnection connection, IHubCallerClients clients);

}