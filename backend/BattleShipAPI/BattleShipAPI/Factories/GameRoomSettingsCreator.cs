using BattleShipAPI.Factories.LevelsFactory;
using BattleShipAPI.Models;

namespace BattleShipAPI.Factories;

public static class GameRoomSettingsCreator
{
    public static Dictionary<(string, int), GameRoomSettings> GameSettingsTemplates = new Dictionary<(string, int), GameRoomSettings>();

    public static GameRoomSettings GetGameRoomSettings(List<UserConnection> players, string mode)
    {
        if (!GameSettingsTemplates.TryGetValue((mode, players.Count), out var template))
        {
            // 4. DESIGN PATTERN: Abstract Factory

            //TODO FIX with supperattacks and using factories better
            var abstractFactory = GetLevelFactory(mode);
            var shipSettings = abstractFactory.CreateShipsConfig();
            var timer = abstractFactory.GetTimerDuration();
            var settings = GetGameFactory(players).BuildGameRoomSettings(shipSettings, timer);

            template = settings;
            GameSettingsTemplates[(mode, players.Count)] = template;
        }
        
        var gameRoomSettings = template.Clone();
        
        gameRoomSettings.Board.AssignBoardSections(players);
        
        return gameRoomSettings;
    }
    
    private static AbstractGameSettingsFactory GetGameFactory(List<UserConnection> players)
    {
        return players.Count switch
        {
            2 => new TwoPlayersGameSettingsFactory(players),
            3 => new ThreePlayersGameSettingsFactory(players),
            4 => new FourPlayersGameSettingsFactory(players),
            _ => throw new ArgumentException("Invalid number of players")
        };
    }

    private static AbstractLevelFactory GetLevelFactory(string mode)
    {
        return mode switch
        {
            "normal" => new NormalLevelFactory(),
            "rapid" => new RapidLevelFactory(),
            "mobility" => new MobilityLevelFactory(),
            _ => throw new ArgumentException("Invalid game mode")
        };
    }
}