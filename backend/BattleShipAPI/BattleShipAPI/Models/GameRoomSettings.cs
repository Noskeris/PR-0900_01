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
    
    public GameRoomSettings(Ships ships, SuperAttacks superAttacks, Board board)
    {
        Ships = ships;
        SuperAttacks = superAttacks;
        Board = board;
    }
    
    public GameRoomSettings Clone()
    {
        return new GameRoomSettings(Ships.Clone(), SuperAttacks.Clone(), Board.Clone());
    }
}