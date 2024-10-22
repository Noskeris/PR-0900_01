using BattleShipAPI.Builders;
using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.GameItems.Ships;
using BattleShipAPI.GameItems.SuperAttacks;
using BattleShipAPI.Models;

namespace BattleShipAPI.Factories;

public class ThreePlayersGameSettingsFactory : AbstractGameSettingsFactory
{
    public override Board Board { get; }
    public override SuperAttacks SuperAttacksConfig { get; }
    //public override Ships ShipsConfig { get; }

    public ThreePlayersGameSettingsFactory(List<UserConnection> players)
    {
        Board = new ThreePlayersBoard();
        
        //var shipsBuilder = new ShipsBuilder();
        //ShipsConfig = new ThreePlayersShips(shipsBuilder);

        var superAttacksBuilder = new SuperAttacksBuilder();
        SuperAttacksConfig = new ThreePlayersSuperAttacks(superAttacksBuilder);
    }

    public override GameRoomSettings BuildGameRoomSettings(Ships shipsConfig, int t)
    {
        return new GameRoomSettings(shipsConfig, SuperAttacksConfig, Board, t);
    }
}
