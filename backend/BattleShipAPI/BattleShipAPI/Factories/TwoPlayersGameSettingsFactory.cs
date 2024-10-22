using BattleShipAPI.Builders;
using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.GameItems.SuperAttacks;
using BattleShipAPI.Models;

namespace BattleShipAPI.Factories;

// 2. DESIGN PATTERN: Factory
public class TwoPlayersGameSettingsFactory : AbstractGameSettingsFactory
{
    public override Board Board { get; }
    public override SuperAttacks SuperAttacksConfig { get; }

    public TwoPlayersGameSettingsFactory(List<UserConnection> players)
    {
        Board = new TwoPlayersBoard();

        //var shipsBuilder = new ShipsBuilder();
        //ShipsConfig = new TwoPlayersShips(shipsBuilder);

        var superAttacksBuilder = new SuperAttacksBuilder();
        SuperAttacksConfig = new TwoPlayersSuperAttacks(superAttacksBuilder);
    }

    public override GameRoomSettings BuildGameRoomSettings(Ships shipsConfig, int t)
    {
        return new GameRoomSettings(shipsConfig, SuperAttacksConfig, Board, t);
    }
}