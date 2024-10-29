using BattleShipAPI.Models;
using BattleShipAPI.Repository;
using System.Collections.Concurrent;
using Xunit;

public class InMemoryDBTests
{
    private readonly InMemoryDB _inMemoryDB;

    public InMemoryDBTests()
    {
        _inMemoryDB = new InMemoryDB();
    }

    [Fact]
    public void InMemoryDB_Constructor_InitializesCorrectly()
    {
        // Verify that the Connections dictionary is initialized and empty
        Assert.NotNull(_inMemoryDB.Connections);
        Assert.IsType<ConcurrentDictionary<string, UserConnection>>(_inMemoryDB.Connections);
        Assert.Empty(_inMemoryDB.Connections);

        // Verify that the GameRooms dictionary is initialized and empty
        Assert.NotNull(_inMemoryDB.GameRooms);
        Assert.IsType<ConcurrentDictionary<string, GameRoom>>(_inMemoryDB.GameRooms);
        Assert.Empty(_inMemoryDB.GameRooms);
    }

    [Fact]
    public void Connections_AddAndRetrieveEntry_WorksCorrectly()
    {
        var userConnection = new UserConnection { PlayerId = "player1", Username = "user1" };

        // Add an entry to the Connections dictionary
        _inMemoryDB.Connections["player1"] = userConnection;

        // Retrieve and verify the entry
        Assert.True(_inMemoryDB.Connections.TryGetValue("player1", out var retrievedConnection));
        Assert.Equal("player1", retrievedConnection.PlayerId);
        Assert.Equal("user1", retrievedConnection.Username);
    }

    [Fact]
    public void Connections_RemoveEntry_RemovesCorrectly()
    {
        var userConnection = new UserConnection { PlayerId = "player2", Username = "user2" };
        _inMemoryDB.Connections["player2"] = userConnection;

        // Remove the entry and verify it was removed
        Assert.True(_inMemoryDB.Connections.TryRemove("player2", out var removedConnection));
        Assert.Equal("player2", removedConnection.PlayerId);
        Assert.Equal("user2", removedConnection.Username);
        Assert.False(_inMemoryDB.Connections.ContainsKey("player2"));
    }

    [Fact]
    public void GameRooms_AddAndRetrieveEntry_WorksCorrectly()
    {
        var gameRoom = new GameRoom { Name = "room1" };

        // Add an entry to the GameRooms dictionary
        _inMemoryDB.GameRooms["room1"] = gameRoom;

        // Retrieve and verify the entry
        Assert.True(_inMemoryDB.GameRooms.TryGetValue("room1", out var retrievedGameRoom));
        Assert.Equal("room1", retrievedGameRoom.Name);
    }

    [Fact]
    public void GameRooms_RemoveEntry_RemovesCorrectly()
    {
        var gameRoom = new GameRoom { Name = "room2" };
        _inMemoryDB.GameRooms["room2"] = gameRoom;

        // Remove the entry and verify it was removed
        Assert.True(_inMemoryDB.GameRooms.TryRemove("room2", out var removedGameRoom));
        Assert.Equal("room2", removedGameRoom.Name);
        Assert.False(_inMemoryDB.GameRooms.ContainsKey("room2"));
    }
}
