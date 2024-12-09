using BattleShipAPI.ChainOfResponsibility.Handlers;
using BattleShipAPI.Models;
using BattleShipAPI.Notifications;
using BattleShipAPI.Services;

namespace BattleShipAPI.Mediator;

public class ConcreteMediator : IMediator
{
    private readonly AttackCellHandler _attackCellHandler;
    private readonly NotificationService _notificationService;
    private readonly GameValidationService _gameValidationService;
    private readonly AttackService _attackService;
    
    public ConcreteMediator(
        AttackCellHandler attackCellHandler,
        INotificationService notificationService,
        GameValidationService gameValidationService,
        AttackService attackService)
    {
        _attackCellHandler = attackCellHandler;
        attackCellHandler.SetMediator(this);
        _notificationService = (notificationService as NotificationService)!;
        _notificationService.SetMediator(this);
        _gameValidationService = gameValidationService;
        _gameValidationService.SetMediator(this);
        _attackService = attackService;
        _attackService.SetMediator(this);
    }

    public async Task Notify(string eventName, object? data = null)
    {
        switch (eventName)
        {
            case "ValidateAttackCellRequest":
                await _gameValidationService.ValidateAttackRequest((data as AttackInformation)!);
                break;
            case "InformClient":
                var clientRequest = (data as InformClientRequest)!;
                await _notificationService.NotifyClient(clientRequest.Clients, clientRequest.ClientId, clientRequest.Key, clientRequest.Values);
                break;
            case "InformGroup":
                var groupRequest = (data as InformGroupRequest)!;
                await _notificationService.NotifyGroup(groupRequest.Clients, groupRequest.GroupName, groupRequest.Key, groupRequest.Values);
                break;
            case "InformAboutAttackCellRequestValidationSuccess":
                await _attackCellHandler.ContinueAttackHandling((data as AttackInformation)!);
                break;
            case "PerformCellAttack":
                await _attackService.PerformAttack((data as AttackInformation)!);
                break;
            case "InformAboutPerformedCellAttackSuccess":
                await _attackCellHandler.FinishAttackHandling((data as AttackInformation)!);
                break;
            default:
                Console.WriteLine("Unknown event.");
                break;
        }
    }
}