using BattleShipAPI.Models;

namespace BattleShipAPI.GameItems.Ships
{
    public class LevelShips : Ships
    {
        public override List<ShipConfig> ShipsConfig { get; }

        public LevelShips(List<ShipConfig> shipConfigs)
        {
            ShipsConfig = shipConfigs;
        }
    }
}
