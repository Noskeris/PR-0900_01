using BattleShipAPI.GameItems.Boards;

namespace BattleShipAPI.Models
{
    public class PlacingActionHistory
    {
        private List<(List<IPlacedShip> PlacedShips, Board BoardState)> _history = new List<(List<IPlacedShip> PlacedShips, Board BoardState)>();

        private int _currentPointer = -1;

        public void AddInitialState(List<IPlacedShip> placedShips, Board board)
        {
            if ( _history.Count > 0)
            {
                return;
            }

            var boardClone = board.Clone();

            _history.Add((new List<IPlacedShip>(placedShips), boardClone));

            _currentPointer++;
        }

        public void AddAction(List<IPlacedShip> placedShips, Board board)
        {
            if (_currentPointer < _history.Count - 1)
            {
                _history = _history.Take(_currentPointer + 1).ToList();
            }

            var boardClone = board.Clone();

            _history.Add((new List<IPlacedShip>(placedShips), boardClone));

            _currentPointer++;
        }

        public bool CanUndo() => _currentPointer > 0;

        public bool CanRedo() => _currentPointer < _history.Count - 1;

        public (List<IPlacedShip> PlacedShips, Board BoardState)? Undo()
        {
            if (!CanUndo())
            {
                return null;
            }

            _currentPointer--;
            return _history[_currentPointer];
        }

        public (List<IPlacedShip> PlacedShips, Board BoardState)? Redo()
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
