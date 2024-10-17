using BattleShipAPI.Builders;
using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.GameItems.SuperAttacks;
using BattleShipAPI.Models;

namespace BattleShipAPI.Factories;

public class TwoPlayersGameSettingsFactory : AbstractGameSettingsFactory
{
    public override Board Board { get; }
    public override SuperAttacks SuperAttacksConfig { get; }
    public override Ships ShipsConfig { get; }

    public TwoPlayersGameSettingsFactory(List<UserConnection> players)
    {
        Board = new TwoPlayersBoard(players);

        var shipsBuilder = new ShipsBuilder();
        ShipsConfig = new TwoPlayersShips(shipsBuilder);

        var superAttacksBuilder = new SuperAttacksBuilder();
        SuperAttacksConfig = new TwoPlayersSuperAttacks(superAttacksBuilder);
    }

    public override GameRoomSettings BuildGameRoomSettings()
    {
        return new GameRoomSettings(ShipsConfig, SuperAttacksConfig, Board);
    }
}