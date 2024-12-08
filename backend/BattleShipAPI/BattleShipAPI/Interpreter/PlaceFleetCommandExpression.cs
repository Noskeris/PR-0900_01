using BattleShipAPI.Command;

namespace BattleShipAPI.Interpreter
{
    public class PlaceFleetCommandExpression : ICommandExpression
    {
        public async Task InterpretAsync(CommandContext context, string[] commandParts)
        {
            var placeFleetCommand = new PlaceFleetCommand();
            await placeFleetCommand.Execute(context, commandParts);
        }
    }
}
