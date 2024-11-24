namespace BattleShipAPI.Iterator
{
    public interface IAggregate<T>
    {
        IIterator<T> CreateIterator();
    }
}
