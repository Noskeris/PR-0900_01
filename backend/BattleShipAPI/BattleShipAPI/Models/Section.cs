namespace BattleShipAPI.Models;

public class Section
{
    public int StartX { get; }
    public int StartY { get; }
    public int EndX { get; }
    public int EndY { get; }
    
    public Section(int startX, int startY, int endX, int endY)
    {
        StartX = startX;
        StartY = startY;
        EndX = endX;
        EndY = endY;
    }
}