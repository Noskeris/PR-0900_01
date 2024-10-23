using BattleShipAPI.Enums;
using System.Drawing;

namespace BattleShipAPI.Models
{
    public class UserConnection
    {
        public string PlayerId { get; set; } = string.Empty;
        
        public string Username { get; set; } = string.Empty;
        
        public string GameRoomName { get; set; } = string.Empty;
        
        public bool IsModerator { get; set; }
        
        public bool CanPlay { get; set; }
        
        public bool HasDisconnected { get; set; }
        
        public List<PlacedShip> PlacedShips { get; set; } = new();
        
        public List<SuperAttackConfig> UsedSuperAttacks { get; set; } = new();
        
        public List<ShipConfig> GetAllowedShipsConfig(List<ShipConfig> shipsConfig)
        {
            return shipsConfig.Select(shipConfig => new ShipConfig()
            {
                ShipType = shipConfig.ShipType,
                Count = shipConfig.Count - PlacedShips.Count(x => x.ShipType == shipConfig.ShipType),
                Size = shipConfig.Size,
                HasShield = shipConfig.HasShield,
                HasMobility = shipConfig.HasMobility
            }).ToList();
        }
        
        public List<SuperAttackConfig> GetAllowedSuperAttacksConfig(List<SuperAttackConfig> superAttacksConfig)
        {
            return superAttacksConfig.Select(superAttackConfig => new SuperAttackConfig()
            {
                AttackType = superAttackConfig.AttackType,
                Count = superAttackConfig.Count - UsedSuperAttacks.Count(x => x.AttackType == superAttackConfig.AttackType)
            }).ToList();
        }

        public bool TryUseSuperAttack(AttackType attackType, List<SuperAttackConfig> gameSuperAttacksConfig)
        {
            if (attackType == AttackType.Normal)
            {
                return true;
            }
            
            var allowedSuperAttacks = GetAllowedSuperAttacksConfig(gameSuperAttacksConfig);
            
            var superAttack = allowedSuperAttacks.FirstOrDefault(x => x.AttackType == attackType);
            
            if (superAttack == null || superAttack.Count <= 0)
            {
                return false;
            }
            
            UsedSuperAttacks.Add(superAttack);
            
            return true;
        }
    }
}
