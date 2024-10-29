using BattleShipAPI.Enums;
using BattleShipAPI.Models;

public class BoardTests
{
    [Fact]
    public void Board_Constructor_InitializesCorrectly()
    {
        var board = new Board(10, 10);
        Assert.Equal(10, board.XLength);
        Assert.Equal(10, board.YLength);
        Assert.NotNull(board.Cells);
        Assert.Equal(10, board.Cells.Length);
        Assert.Equal(10, board.Cells[0].Length);
    }

    [Fact]
    public void AssignBoardSection_AssignsOwnerCorrectly()
    {
        var board = new Board(10, 10);
        board.AssignBoardSection(0, 0, 1, 1, "owner1");

        for (int x = 0; x <= 1; x++)
        {
            for (int y = 0; y <= 1; y++)
            {
                Assert.Equal("owner1", board.Cells[x][y].OwnerId);
            }
        }
    }

    [Fact]
    public void TryPutShipOnBoard_PlacesShipCorrectly()
    {
        var board = new Board(10, 10);
        board.AssignBoardSection(0, 0, 1, 1, "owner1");
        var result = board.TryPutShipOnBoard(0, 0, 1, 1, "owner1");

        Assert.True(result);
        for (int x = 0; x <= 1; x++)
        {
            for (int y = 0; y <= 1; y++)
            {
                Assert.Equal(CellState.HasShip, board.Cells[x][y].State);
            }
        }
    }

    [Fact]
    public void TryPutShipOnBoard_FailsIfNotOwner()
    {
        var board = new Board(10, 10);
        board.AssignBoardSection(0, 0, 1, 1, "owner1");
        var result = board.TryPutShipOnBoard(0, 0, 1, 1, "owner2");

        Assert.False(result);
        for (int x = 0; x <= 1; x++)
        {
            for (int y = 0; y <= 1; y++)
            {
                Assert.NotEqual(CellState.HasShip, board.Cells[x][y].State);
            }
        }
    }

    [Fact]
    public void SinkShip_SinksShipCorrectly()
    {
        var board = new Board(10, 10);
        var ship = new PlacedShip { StartX = 0, StartY = 0, EndX = 1, EndY = 1 };
        board.SinkShip(ship);

        for (int x = 0; x <= 1; x++)
        {
            for (int y = 0; y <= 1; y++)
            {
                Assert.Equal(CellState.SunkenShip, board.Cells[x][y].State);
            }
        }
    }
}