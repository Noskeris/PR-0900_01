namespace BattleShipAPI.Mediator;

public interface IMediator
{
    Task Notify(string eventName, object? data = null);
}