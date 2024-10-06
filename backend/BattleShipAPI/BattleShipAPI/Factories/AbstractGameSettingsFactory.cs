using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.GameItems.SuperAttacks;
using BattleShipAPI.Models;

namespace BattleShipAPI.Factories;

public abstract class AbstractGameSettingsFactory
{
    public abstract Board Board { get; }
    
    public abstract SuperAttacks SuperAttacksConfig { get; }
    
    public abstract Ships ShipsConfig { get; }
    
    public abstract GameRoomSettings BuildGameRoomSettings();
}
