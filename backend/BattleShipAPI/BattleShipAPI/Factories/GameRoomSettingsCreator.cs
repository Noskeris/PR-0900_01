using BattleShipAPI.Models;

namespace BattleShipAPI.Factories;

public static class GameRoomSettingsCreator
{
    public static AbstractGameSettingsFactory GetGameFactory(List<UserConnection> players)
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