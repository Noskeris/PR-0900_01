using BattleShipAPI.Memento;

namespace BattleShipAPI.Models;

public class PlacingActionHistory
{
    private List<PlacementMemento> _history = new List<PlacementMemento>();
    private int _currentPointer = -1;

    public void AddInitialState(PlacementMemento memento)
    {
        if (_history.Count > 0)
        {
            return;
        }

        _history.Add(memento);
        _currentPointer++;
    }

    public void AddAction(PlacementMemento memento)
    {
        // If we have undone some actions, and now we add a new action,
        // we remove all "future" states.
        if (_currentPointer < _history.Count - 1)
        {
            _history = _history.Take(_currentPointer + 1).ToList();
        }

        _history.Add(memento);
        _currentPointer++;
    }

    public bool CanUndo() => _currentPointer > 0;
    public bool CanRedo() => _currentPointer < _history.Count - 1;

    public PlacementMemento? Undo()
    {
        if (!CanUndo())
        {
            return null;
        }

        _currentPointer--;
        return _history[_currentPointer];
    }

    public PlacementMemento? Redo()
    {
        if (!CanRedo())
        {
            return null;
        }

        _currentPointer++;
        return _history[_currentPointer];
    }
}