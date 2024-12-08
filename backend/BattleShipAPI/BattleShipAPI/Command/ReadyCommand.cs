namespace BattleShipAPI.Command;

public class ReadyCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        await context.GameFacade.HandleAction("SetPlayerToReady", context.CallerContext, context.Clients);
    }
}