using BattleShipAPI.Command;

namespace BattleShipAPI.Interpreter
{
    public class RedoPlacementCommandExpression : ICommandExpression
    {
        public async Task InterpretAsync(CommandContext context, string[] commandParts)
        {
            var redoPlacementCommand = new RedoPlacementCommand();
            await redoPlacementCommand.Execute(context, commandParts);
        }
    }
}
