using BattleShipAPI.Command;
using BattleShipAPI.Enums;
using BattleShipAPI.Flyweight;
using BattleShipAPI.Interpreter;
using BattleShipAPI.Notifications;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Facade
{
    public class CommandFacade
    {
        private readonly INotificationService _notificationService;
        private readonly GameFacade _gameFacade;
        private readonly InMemoryDB _db;
        private readonly FlyweightCommandFactory _commandFactory;

        public CommandFacade(
            INotificationService notificationService,
            GameFacade gameFacade)
        {
            _commandFactory = new FlyweightCommandFactory();
            _db = InMemoryDB.Instance;
            _notificationService = notificationService;
            _gameFacade = gameFacade;
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            _commandFactory.GetCommand("ready", () => new ReadyCommand());
            _commandFactory.GetCommand("start", () => new StartGameCommand());
            _commandFactory.GetCommand("gamemode", () => new ConfirmGameModeCommand());
            _commandFactory.GetCommand("generateboard", () => new GenerateBoardCommand());
            _commandFactory.GetCommand("addship", () => new AddShipCommand());
            _commandFactory.GetCommand("undoplacement", () => new UndoPlacementCommand());
            _commandFactory.GetCommand("redoplacement", () => new RedoPlacementCommand());
            _commandFactory.GetCommand("fleet", () => new PlaceFleetCommand());
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
                player.PlacedShips = previousState.PlacedShips;
                gameRoom.SetBoard(previousState.BoardState);

                _db.GameRooms[gameRoom.Name] = gameRoom;
                _db.Connections[context.ConnectionId] = player;

                await _notificationService.NotifyGroup(
                    clients,
                    gameRoom.Name,
                    "BoardUpdated",
                    gameRoom.Name,
                    previousState.BoardState);

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
                player.PlacedShips = nextState.PlacedShips;
                gameRoom.SetBoard(nextState.BoardState);

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
            //var commandParts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            //if (commandParts.Length == 0)
            //{
            //    await _notificationService.NotifyClient(clients, context.ConnectionId, "UnknownCommand", "No command provided.");
            //    return;
            //}

            //var action = commandParts[0].ToLower();

            //var playerCommand = _commandFactory.GetCommand(action, null);
            //if (playerCommand != null)
            //{
            //    var commandContext = new CommandContext(_gameFacade, clients, context, _notificationService);
            //    await playerCommand.Execute(commandContext, commandParts);
            //}
            //else
            //{
            //    await _notificationService.NotifyClient(clients, context.ConnectionId, "UnknownCommand", $"Unknown command: {command}");
            //}
            var commandContext = new CommandContext(_gameFacade, clients, context, _notificationService);
            var interpreter = new CommandInterpreter();
            await interpreter.InterpretAsync(commandContext, command);
        }
    }
}
