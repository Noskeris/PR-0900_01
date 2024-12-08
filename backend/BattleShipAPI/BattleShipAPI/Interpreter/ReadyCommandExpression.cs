using BattleShipAPI.Command;

namespace BattleShipAPI.Interpreter
{
    public class ReadyCommandExpression : ICommandExpression
    {
        private readonly ReadyCommand _readyCommand;

        public ReadyCommandExpression()
        {
            _readyCommand = new ReadyCommand();
        }

        public async Task InterpretAsync(CommandContext context, string[] commandParts)
        {
            await _readyCommand.Execute(context, commandParts);
        }
    }
}
