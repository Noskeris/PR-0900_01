using BattleShipAPI.Command;

namespace BattleShipAPI.Interpreter
{
    public class GenerateBoardCommandExpression : ICommandExpression
    {
        public async Task InterpretAsync(CommandContext context, string[] commandParts)
        {
            var generateBoardCommand = new GenerateBoardCommand();
            await generateBoardCommand.Execute(context, commandParts);
        }
    }
}
