using BattleShipAPI.GameItems.Boards;
using BattleShipAPI.Iterator;

namespace BattleShipAPI.Models
{
    public class PlacingActionHistory
    {
        private List<(PlacedShipsCollection PlacedShips, Board BoardState)> _history = new List<(PlacedShipsCollection PlacedShips, Board BoardState)>();

        private int _currentPointer = -1;

        public void AddInitialState(PlacedShipsCollection placedShips, Board board)
        {
            if (_history.Count > 0)
            {
                return;
            }

            var boardClone = board.Clone();

            // Clone the PlacedShipsCollection
            var placedShipsClone = ClonePlacedShipsCollection(placedShips);

            _history.Add((placedShipsClone, boardClone));

            _currentPointer++;
        }

        public void AddAction(PlacedShipsCollection placedShips, Board board)
        {
            if (_currentPointer < _history.Count - 1)
            {
                _history = _history.Take(_currentPointer + 1).ToList();
            }

            var boardClone = board.Clone();

            // Clone the PlacedShipsCollection
            var placedShipsClone = ClonePlacedShipsCollection(placedShips);

            _history.Add((placedShipsClone, boardClone));

            _currentPointer++;
        }

        public bool CanUndo() => _currentPointer > 0;

        public bool CanRedo() => _currentPointer < _history.Count - 1;

        public (PlacedShipsCollection PlacedShips, Board BoardState)? Undo()
        {
            if (!CanUndo())
            {
                return null;
            }

            _currentPointer--;
            return _history[_currentPointer];
        }

        public (PlacedShipsCollection PlacedShips, Board BoardState)? Redo()
        {
            if (!CanRedo())
            {
                return null;
            }

            _currentPointer++;
            return _history[_currentPointer];
        }

        // Helper method to clone the PlacedShipsCollection
        private PlacedShipsCollection ClonePlacedShipsCollection(PlacedShipsCollection original)
        {
            var clone = new PlacedShipsCollection();
            var iterator = original.CreateIterator();
            while (iterator.HasNext())
            {
                var ship = iterator.Next();
                clone.Add(ship); // Shallow copy of the ship reference
            }
            return clone;
        }
    }
}
