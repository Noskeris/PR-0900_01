namespace BattleShipAPI.Interpreter
{
    public class AddShipCommandExpression : ICommandExpression
    {
        public async Task InterpretAsync(CommandContext context, string[] commandParts)
        {
            var addShipCommand = new AddShipCommand();
            await addShipCommand.Execute(context, commandParts);
        }
    }
}
