using BattleShipAPI.Enums;

namespace BattleShipAPI.State;

public static class GameStateFactory
{
    public static IGameState GetHandler(GameState state) => state switch
    {
        GameState.NotStarted => new NotStartedState(),
        GameState.GameModeConfirmed => new GameModeConfirmedState(),
        GameState.PlacingShips => new PlacingShipsState(),
        GameState.InProgress => new InProgressState(),
        GameState.Finished => new FinishedState(),
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };
}