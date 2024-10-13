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
        SuperAttacksConfig = new TwoPlayersSuperAttacks();
        ShipsConfig = new TwoPlayersShips();
    }

    public override GameRoomSettings BuildGameRoomSettings()
    {
        return new GameRoomSettings(ShipsConfig, SuperAttacksConfig, Board);
    }
}