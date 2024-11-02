using BattleShipAPI.Enums;
using BattleShipAPI.Enums.Avatar;
using BattleShipAPI.Facade;
using BattleShipAPI.Models;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;

//TODO add ship shield logic
//TODO add ship mobility logic
//TODO DESIGN PATTERN decorator

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
            await _gameFacade.JoinSpecificGameRoom(Context, Clients, connection);
        }

        public async Task ConfirmGameMode(GameMode gameMode)
        {
            await _gameFacade.ConfirmGameMode(Context, Clients, gameMode);
        }

        public async Task GenerateBoard()
        {
            await _gameFacade.GenerateBoard(Context, Clients);
        }

        public async Task ChangeAvatar(HeadType headType, AppearanceType appearanceType)
        {
            await _gameFacade.ChangeAvatar(Context, Clients, headType, appearanceType);
        }

        public async Task AddShip(PlacedShip placedShipData)
        {
            await _gameFacade.AddShip(Context, Clients, placedShip);
        }

        public async Task SetPlayerToReady()
        {
            await _gameFacade.SetPlayerToReady(Context, Clients);
        }

        public async Task StartGame()
        {
            await _gameFacade.StartGame(Context, Clients);
        }

        public async Task PlayerTurnTimeEnded()
        {
            await _gameFacade.PlayerTurnTimeEnded(Context, Clients);
        }
        
        public async Task AttackCell(int x, int y, AttackType attackType = AttackType.Normal)
        {
            await _gameFacade.AttackCell(Context, Clients, x, y, attackType);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _gameFacade.HandleDisconnection(Context, Clients);
            await base.OnDisconnectedAsync(exception);
        }
        public async Task RestartGame()
        {
            await _gameFacade.RestartGame(Context, Clients);
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