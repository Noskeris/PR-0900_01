namespace BattleShipAPI.Command;

public class GenerateBoardCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        await context.GameFacade.HandleAction("GenerateBoard", context.CallerContext, context.Clients);
    }
}