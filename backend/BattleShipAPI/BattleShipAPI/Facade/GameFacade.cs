using BattleShipAPI.ChainOfResponsibility;
using BattleShipAPI.ChainOfResponsibility.Handlers;
using BattleShipAPI.Notifications;
using BattleShipAPI.Proxy;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Facade;

public class GameFacade
{
    private readonly IGameHandler _handlerChain;

    public GameFacade(INotificationService notificationService, ILoggerOnReceive loggerOnReceive, ILoggerOnSend loggerOnSend)
    {
        var joinRoomHandler = new JoinSpecificGameRoomHandler(notificationService, loggerOnReceive, loggerOnSend);
        var confirmGameModeHandler = new ConfirmGameModeHandler(notificationService, loggerOnReceive, loggerOnSend);
        var generateBoardHandler = new GenerateBoardHandler(notificationService);
        var changeAvatarHandler = new ChangeAvatarHandler(notificationService);
        var addShipHandler = new AddShipHandler(notificationService);
        var setPlayerToReadyHandler = new SetPlayerToReadyHandler(notificationService, loggerOnReceive, loggerOnSend);
        var startGameHandler = new StartGameHandler(notificationService);
        var playerTurnEndedHandler = new PlayerTurnTimeEndedHandler(notificationService);
        var attackCellHandler = new AttackCellHandler(notificationService);
        var restartGameHandler = new RestartGameHandler(notificationService);
        var disconnectHandler = new DisconnectionHandler(notificationService);
        
        joinRoomHandler
            .SetNext(confirmGameModeHandler)
            .SetNext(generateBoardHandler)
            .SetNext(changeAvatarHandler)
            .SetNext(addShipHandler)
            .SetNext(setPlayerToReadyHandler)
            .SetNext(startGameHandler)
            .SetNext(playerTurnEndedHandler)
            .SetNext(attackCellHandler)
            .SetNext(restartGameHandler)
            .SetNext(disconnectHandler);

        _handlerChain = joinRoomHandler;
    }

    public async Task HandleAction(string action, HubCallerContext context, IHubCallerClients clients, object? data = null)
    {
        await _handlerChain.HandleRequest(action, context, clients, data);
    }
}