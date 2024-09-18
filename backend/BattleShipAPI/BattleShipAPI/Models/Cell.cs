using BattleShipAPI.Enums;

namespace BattleShipAPI.Models
{
    public class Cell
    {
        public CellState State { get; set; } = CellState.Empty;
        public int OwnerId { get; set; }
    }
}
