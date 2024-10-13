using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.GameItems.SuperAttacks;
using BattleShipAPI.Models;

namespace BattleShipAPI.Factories;

public class ThreePlayersGameSettingsFactory : AbstractGameSettingsFactory
{
    public override Board Board { get; }
    public override SuperAttacks SuperAttacksConfig { get; }
    public override Ships ShipsConfig { get; }

    public ThreePlayersGameSettingsFactory(List<UserConnection> players)
    {
        Board = new ThreePlayersBoard(players);
        ShipsConfig = new ThreePlayersShips();
        SuperAttacksConfig = new ThreePlayersSuperAttacks();
    }

    public override GameRoomSettings BuildGameRoomSettings()
    {
        return new GameRoomSettings(ShipsConfig, SuperAttacksConfig, Board);
    }
}
