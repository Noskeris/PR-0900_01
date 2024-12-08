namespace BattleShipAPI.Interpreter
{
    public interface ICommandExpression
    {
        Task InterpretAsync(CommandContext context, string[] commandParts);
    }
}
