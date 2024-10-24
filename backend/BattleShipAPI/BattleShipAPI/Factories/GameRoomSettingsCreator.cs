using BattleShipAPI.Enums;
using BattleShipAPI.Factories.LevelsFactory;
using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.Models;

namespace BattleShipAPI.Factories;

public static class GameRoomSettingsCreator
{
    public static Dictionary<(GameMode, int), GameRoomSettings> GameSettingsTemplates = new Dictionary<(GameMode, int), GameRoomSettings>();

    public static GameRoomSettings GetGameRoomSettings(List<UserConnection> players, GameMode mode)
    {
        if (!GameSettingsTemplates.TryGetValue((mode, players.Count), out var template))
        {
            //board generation
            var board = GetGameBoard(players.Count);

            // 4. DESIGN PATTERN: Abstract Factory
            var abstractFactory = GetLevelFactory(mode);
            var shipSettings = abstractFactory.CreateShipsConfig();
            var supperAttacksSettings = abstractFactory.CreateSuperAttacksConfig();
            abstractFactory.SetTimer();
            var settings = abstractFactory.CreateGameRoomSettings(shipSettings, supperAttacksSettings, board);

            // DSIGN PATTERN: Clonable
            template = settings;
            GameSettingsTemplates[(mode, players.Count)] = template;
        }
        
        var gameRoomSettings = template.Clone();
        
        gameRoomSettings.Board.AssignBoardSections(players);
        
        return gameRoomSettings;
    }
    
    private static Board GetGameBoard(int playersCount)
    {
        return playersCount switch
        {
            2 => new TwoPlayersBoard(),
            3 => new ThreePlayersBoard(),
            4 => new FourPlayersBoard(),
            _ => throw new ArgumentException("Invalid number of players")
        };
    }

    private static AbstractLevelFactory GetLevelFactory(GameMode mode)
    {
        return mode switch
        {
            GameMode.Normal => new NormalLevelFactory(),
            GameMode.Rapid => new RapidLevelFactory(),
            GameMode.Mobility => new MobilityLevelFactory(),
            _ => throw new ArgumentException("Invalid game mode")
        };
    }
}