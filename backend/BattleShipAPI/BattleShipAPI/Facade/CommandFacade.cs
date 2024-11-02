using BattleShipAPI.Enums;
using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Facade;

public class CommandFacade
{
    private readonly INotificationService _notificationService;
    private readonly GameFacade _gameFacade;
    private readonly InMemoryDB _db;
    private readonly Dictionary<string, IPlayerCommand> _commands = new();
    
    
    public CommandFacade(
        INotificationService notificationService,
        GameFacade gameFacade)
    {
        _db = InMemoryDB.Instance;
        _notificationService = notificationService;
        _gameFacade = gameFacade;
        InitializeCommands();
    }
    
    private void InitializeCommands()
    {
        _commands["ready"] = new ReadyCommand();
        _commands["start"] = new StartGameCommand();
        _commands["gamemode"] = new ConfirmGameModeCommand();
        _commands["generateboard"] = new GenerateBoardCommand();
        _commands["addship"] = new AddShipCommand();
        _commands["undoplacement"] = new UndoPlacementCommand();
        _commands["redoplacement"] = new RedoPlacementCommand();
    }
    
    public async Task UndoShipPlacement(
        HubCallerContext context,
        IHubCallerClients clients)
    {
        if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
            && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.State == GameState.PlacingShips)
        {
            var player = _db.Connections[context.ConnectionId];
            var previousState = player.PlacingActionHistory.Undo();
            if (previousState == null)
            {
                await _notificationService.NotifyClient(
                    clients,
                    context.ConnectionId,
                    "FailedToUndo",
                    "No actions to undo.");
                return;
            }

            // Restore the board and placed ships
            player.PlacedShips = previousState.Value.PlacedShips;

            gameRoom.SetBoard(previousState.Value.BoardState);
            _db.GameRooms[gameRoom.Name] = gameRoom;
            _db.Connections[context.ConnectionId] = player;

            await _notificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "BoardUpdated",
                gameRoom.Name,
                previousState.Value.BoardState);

            await _notificationService.NotifyClient(
                clients,
                context.ConnectionId,
                "UpdatedShipsConfig",
                player.GetAllowedShipsConfig(gameRoom.ShipsConfig));
        }
    }
    
    public async Task RedoShipPlacement(
        HubCallerContext context,
        IHubCallerClients clients)
    {
        if (_db.Connections.TryGetValue(context.ConnectionId, out var connection)
            && _db.GameRooms.TryGetValue(connection.GameRoomName, out var gameRoom)
            && gameRoom.State == GameState.PlacingShips)
        {
            var player = _db.Connections[context.ConnectionId];
            var nextState = player.PlacingActionHistory.Redo();
            if (nextState == null)
            {
                await _notificationService.NotifyClient(
                    clients,
                    context.ConnectionId,
                    "FailedToRedo",
                    "No actions to redo.");
                return;
            }

            // Restore the board and placed ships
            player.PlacedShips = nextState.Value.PlacedShips;
            gameRoom.SetBoard(nextState.Value.BoardState);

            _db.GameRooms[gameRoom.Name] = gameRoom;
            _db.Connections[context.ConnectionId] = player;

            await _notificationService.NotifyGroup(
                clients,
                gameRoom.Name,
                "BoardUpdated",
                gameRoom.Name,
                gameRoom.Board);

            await _notificationService.NotifyClient(
                clients,
                context.ConnectionId,
                "UpdatedShipsConfig",
                player.GetAllowedShipsConfig(gameRoom.ShipsConfig));
        }
    }
    
    public async Task HandlePlayerCommand(
        HubCallerContext context,
        IHubCallerClients clients,
        string command)
    {
        var commandParts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (commandParts.Length == 0)
        {
            await _notificationService.NotifyClient(clients, context.ConnectionId, "UnknownCommand", "No command provided.");
            return;
        }

        var action = commandParts[0].ToLower();

        if (_commands.TryGetValue(action, out var playerCommand))
        {
            var commandContext = new CommandContext(_gameFacade, clients, context, _notificationService);
            await playerCommand.Execute(commandContext, commandParts);
        }
        else
        {
            await _notificationService.NotifyClient(clients, context.ConnectionId, "UnknownCommand", $"Unknown command: {command}");
        }
    }
}