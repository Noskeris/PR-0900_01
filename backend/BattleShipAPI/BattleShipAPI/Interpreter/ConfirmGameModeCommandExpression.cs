namespace BattleShipAPI.Interpreter
{
    public class ConfirmGameModeCommandExpression : ICommandExpression
    {
        public async Task InterpretAsync(CommandContext context, string[] commandParts)
        {
            var confirmGameModeCommand = new ConfirmGameModeCommand();
            await confirmGameModeCommand.Execute(context, commandParts);
        }
    }
}
