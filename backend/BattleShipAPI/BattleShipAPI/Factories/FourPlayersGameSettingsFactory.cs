using BattleShipAPI.Builders;
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
        
        var shipsBuilder = new ShipsBuilder();
        ShipsConfig = new FourPlayersShips(shipsBuilder);

        var superAttacksBuilder = new SuperAttacksBuilder();
        SuperAttacksConfig = new FourPlayersSuperAttacks(superAttacksBuilder);
    }

    public override GameRoomSettings BuildGameRoomSettings()
    {

        return new GameRoomSettings(ShipsConfig, SuperAttacksConfig, Board);
    }
}
