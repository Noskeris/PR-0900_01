using BattleShipAPI.Enums;
using BattleShipAPI.Enums.Avatar;
using BattleShipAPI.Facade;
using BattleShipAPI.Models;
using Microsoft.AspNetCore.SignalR;

namespace BattleShipAPI.Hubs
{
    public class GameHub : Hub
    {
        private readonly GameFacade _gameFacade;
        private readonly CommandFacade _commandFacade;

        public GameHub(GameFacade gameFacade, CommandFacade commandFacade)
        {
            _gameFacade = gameFacade;
            _commandFacade = commandFacade;
        }

        public async Task JoinSpecificGameRoom(UserConnection connection)
        {
            await _gameFacade.HandleAction("JoinSpecificGameRoom", Context, Clients, connection);
        }

        public async Task ConfirmGameMode(GameMode gameMode)
        {
            await _gameFacade.HandleAction("ConfirmGameMode", Context, Clients, gameMode);
        }

        public async Task GenerateBoard()
        {
            await _gameFacade.HandleAction("GenerateBoard", Context, Clients);
        }

        public async Task ChangeAvatar(HeadType headType, AppearanceType appearanceType)
        {
            var avatarData = new AvatarRequest { HeadType = headType, AppearanceType = appearanceType };
            await _gameFacade.HandleAction("ChangeAvatar", Context, Clients, avatarData);
        }

        public async Task AddShip(PlacedShip placedShipData)
        {
            await _gameFacade.HandleAction("AddShip", Context, Clients, placedShipData);
        }

        public async Task SetPlayerToReady()
        {
            await _gameFacade.HandleAction("SetPlayerToReady", Context, Clients);
        }

        public async Task StartGame()
        {
            await _gameFacade.HandleAction("StartGame", Context, Clients);
        }

        public async Task PlayerTurnTimeEnded()
        {
            await _gameFacade.HandleAction("PlayerTurnTimeEnded", Context, Clients);
        }

        public async Task AttackCell(int x, int y, AttackType attackType = AttackType.Normal)
        {
            var attackData = new AttackRequest { X = x, Y = y, AttackType = attackType };
            await _gameFacade.HandleAction("AttackCell", Context, Clients, attackData);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _gameFacade.HandleAction("OnDisconnected", Context, Clients);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task RestartGame()
        {
            await _gameFacade.HandleAction("RestartGame", Context, Clients);
        }

        public async Task UndoShipPlacement()
        {
            await _commandFacade.UndoShipPlacement(Context, Clients);
        }

        public async Task RedoShipPlacement()
        {
            await _commandFacade.RedoShipPlacement(Context, Clients);
        }

        public async Task HandlePlayerCommand(string command)
        {
            await _commandFacade.HandlePlayerCommand(Context, Clients, command);
        }
    }
}
