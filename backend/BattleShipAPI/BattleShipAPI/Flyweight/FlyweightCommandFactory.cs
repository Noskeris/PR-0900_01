using BattleShipAPI.Command;

namespace BattleShipAPI.Flyweight
{
    public class FlyweightCommandFactory
    {
        private readonly Dictionary<string, IPlayerCommand> _commands = new();

        public IPlayerCommand? GetCommand(string commandName, Func<IPlayerCommand> createCommand)
        {
            if (_commands.ContainsKey(commandName))
            {
                return _commands[commandName];
            }

            if (createCommand != null)
            {
                var command = createCommand();
                _commands[commandName] = command;
                return command;
            }

            return null;
        }

        public IEnumerable<string> GetAvailableCommands()
        {
            return _commands.Keys;
        }
    }
}
