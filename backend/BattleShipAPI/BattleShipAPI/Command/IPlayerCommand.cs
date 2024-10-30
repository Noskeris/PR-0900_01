public interface IPlayerCommand
{
    Task Execute(CommandContext context, string[] args);
}
