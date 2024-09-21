namespace BattleShipAPI.Models
{
    public class UserConnection
    {
        public Guid PlayerId { get; set; }
        
        public string Username { get; set; } = string.Empty;
        
        public string GameRoomName { get; set; } = string.Empty;
        
        public bool IsModerator { get; set; }
        
        public bool CanPlay { get; set; }
        
        public List<PlacedShip> PlacedShips { get; set; } = new();
        
        public List<ShipConfig> GetAllowedShipsConfig(List<ShipConfig> shipsConfig)
        {
            return shipsConfig.Select(shipConfig => new ShipConfig()
            {
                ShipType = shipConfig.ShipType,
                Count = shipConfig.Count - PlacedShips.Count(x => x.ShipType == shipConfig.ShipType)
            }).ToList();
        }
    }
}
