using BattleShipAPI.Enums;
using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.GameItems.SuperAttacks;

namespace BattleShipAPI.Models;

public class GameRoomSettings
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
}