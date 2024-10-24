using BattleShipAPI.GameItems.Boards;

namespace BattleShipAPI.Models
{
    public class PlacingActionHistory
    {
        private List<(List<PlacedShip> PlacedShips, Board BoardState)> _history = new List<(List<PlacedShip> PlacedShips, Board BoardState)>();

        private int _currentPointer = -1;

        public void AddInitialState(List<PlacedShip> placedShips, Board board)
        {
            if ( _history.Count > 0)
            {
                return;
            }

            var boardClone = board.Clone();

            _history.Add((new List<PlacedShip>(placedShips), boardClone));

            _currentPointer++;
        }

        public void AddAction(List<PlacedShip> placedShips, Board board)
        {
            if (_currentPointer < _history.Count - 1)
            {
                _history = _history.Take(_currentPointer + 1).ToList();
            }

            var boardClone = board.Clone();

            _history.Add((new List<PlacedShip>(placedShips), boardClone));

            _currentPointer++;
        }

        public bool CanUndo() => _currentPointer > 0;

        public bool CanRedo() => _currentPointer < _history.Count - 1;

        public (List<PlacedShip> PlacedShips, Board BoardState)? Undo()
        {
            if (!CanUndo())
            {
                return null;
            }

            _currentPointer--;
            return _history[_currentPointer];
        }

        public (List<PlacedShip> PlacedShips, Board BoardState)? Redo()
        {
            if (!CanRedo())
            {
                return null;
            }

            _currentPointer++;
            return _history[_currentPointer];
        }
    }

}
