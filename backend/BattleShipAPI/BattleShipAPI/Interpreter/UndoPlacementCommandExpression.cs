namespace BattleShipAPI.Interpreter
{
    public class UndoPlacementCommandExpression : ICommandExpression
    {
        public async Task InterpretAsync(CommandContext context, string[] commandParts)
        {
            var undoPlacementCommand = new UndoPlacementCommand();
            await undoPlacementCommand.Execute(context, commandParts);
        }
    }
}
