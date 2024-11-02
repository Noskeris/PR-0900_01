public class GenerateBoardCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        await context.GameFacade.GenerateBoard(context.CallerContext, context.Clients);
    }
}
