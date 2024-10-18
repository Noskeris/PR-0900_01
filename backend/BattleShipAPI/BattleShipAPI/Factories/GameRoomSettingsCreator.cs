using BattleShipAPI.Models;

namespace BattleShipAPI.Factories;

public static class GameRoomSettingsCreator
{
    public static Dictionary<int, GameRoomSettings> GameSettingsTemplates = new Dictionary<int, GameRoomSettings>();

    public static GameRoomSettings GetGameRoomSettings(List<UserConnection> players)
    {
        if (!GameSettingsTemplates.TryGetValue(players.Count, out var template))
        {
            // 4. DESIGN PATTERN: Abstract Factory
            template = GetGameFactory(players).BuildGameRoomSettings();
            GameSettingsTemplates[players.Count] = template;
        }
        
        // 7. DESIGN PATTERN: Prototype
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
}