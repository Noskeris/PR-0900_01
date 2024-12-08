using BattleShipAPI.Command;

namespace BattleShipAPI.Interpreter
{
    public class CommandInterpreter
    {
        private readonly Dictionary<string, Func<ICommandExpression>> _expressions;

        public CommandInterpreter()
        {
            _expressions = new Dictionary<string, Func<ICommandExpression>>
            {
                { "ready", () => new ReadyCommandExpression() },
                { "start", () => new StartGameCommandExpression() },
                { "gamemode", () => new ConfirmGameModeCommandExpression() },
                { "addship", () => new AddShipCommandExpression() },
                { "fleet", () => new PlaceFleetCommandExpression() },
                { "generateboard", () => new GenerateBoardCommandExpression() },
                { "undoplacement", () => new UndoPlacementCommandExpression() },
                { "redoplacement", () => new RedoPlacementCommandExpression() }
            };
        }

        public async Task InterpretAsync(CommandContext context, string command)
        {
            var commandParts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (commandParts.Length == 0)
            {
                await context.NotificationService.NotifyClient(
                    context.Clients, context.CallerContext.ConnectionId, "UnknownCommand", "No command provided.");
                return;
            }

            var action = commandParts[0].ToLower();
            if (_expressions.TryGetValue(action, out var expressionFactory))
            {
                var expression = expressionFactory();
                await expression.InterpretAsync(context, commandParts);
            }
            else
            {
                await context.NotificationService.NotifyClient(
                    context.Clients, context.CallerContext.ConnectionId, "UnknownCommand", $"Unknown command: {command}");
            }
        }
    }

}
