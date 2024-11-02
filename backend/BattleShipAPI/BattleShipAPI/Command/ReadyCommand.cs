public class ReadyCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        await context.GameFacade.SetPlayerToReady(context.CallerContext, context.Clients);
    }
}
