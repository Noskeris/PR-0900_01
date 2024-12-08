using BattleShipAPI.Enums;

namespace BattleShipAPI.Command;

public class ConfirmGameModeCommand : IPlayerCommand
{
    public async Task Execute(CommandContext context, string[] args)
    {
        if (args.Length > 1 && Enum.TryParse<GameMode>(args[1], true, out var gameMode))
        {
            await context.GameFacade.HandleAction("ConfirmGameMode", context.CallerContext, context.Clients, gameMode);
        }
        else
        {
            await context.NotificationService.NotifyClient(
                context.Clients,
                context.CallerContext.ConnectionId,
                "InvalidGameMode",
                "Invalid game mode specified.");
        }
    }
}