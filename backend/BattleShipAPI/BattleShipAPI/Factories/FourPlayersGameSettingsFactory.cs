using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.GameItems.SuperAttacks;
using BattleShipAPI.Models;

namespace BattleShipAPI.Factories;

public class FourPlayersGameSettingsFactory : AbstractGameSettingsFactory
{
    public override Board Board { get; }
    public override SuperAttacks SuperAttacksConfig { get; }
    public override Ships ShipsConfig { get; }

    public FourPlayersGameSettingsFactory(List<UserConnection> players)
    {
        Board = new FourPlayersBoard(players);
        ShipsConfig = new FourPlayersShips();
        SuperAttacksConfig = new FourPlayersSuperAttacks();
    }

    public override GameRoomSettings BuildGameRoomSettings()
    {

        return new GameRoomSettings(ShipsConfig, SuperAttacksConfig, Board);
    }
}
