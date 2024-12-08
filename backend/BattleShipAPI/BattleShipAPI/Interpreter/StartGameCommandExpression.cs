using BattleShipAPI.Command;

namespace BattleShipAPI.Interpreter
{
    public class StartGameCommandExpression : ICommandExpression
    {
        public async Task InterpretAsync(CommandContext context, string[] commandParts)
        {
            var startGameCommand = new StartGameCommand();
            await startGameCommand.Execute(context, commandParts);
        }
    }
}
