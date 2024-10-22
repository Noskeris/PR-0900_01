using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.GameItems.SuperAttacks;
using BattleShipAPI.Prototype;

namespace BattleShipAPI.Models;

public class GameRoomSettings : IPrototype<GameRoomSettings>
{
    public Ships Ships { get; }
    
    public SuperAttacks SuperAttacks { get; }
    
    public Board Board { get; }

    public int TimerDuration { get; }
    
    public GameRoomSettings(Ships ships, SuperAttacks superAttacks, Board board, int timerDuration)
    {
        Ships = ships;
        SuperAttacks = superAttacks;
        Board = board;
        TimerDuration = timerDuration;
    }
    
    public GameRoomSettings Clone()
    {
        return new GameRoomSettings(Ships.Clone(), SuperAttacks.Clone(), Board.Clone(), TimerDuration);
    }
}