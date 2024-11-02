public class StartGameCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        await context.GameFacade.StartGame(context.CallerContext, context.Clients);
    }
}
