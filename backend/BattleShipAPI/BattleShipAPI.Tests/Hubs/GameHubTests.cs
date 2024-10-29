using AutoFixture.Xunit2;
using BattleShipAPI.Enums;
using BattleShipAPI.Hubs;
using BattleShipAPI.Models;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;
using Moq;

public class GameHubTests
{
    private Mock<HubCallerContext> _mockContext;
    private Mock<ISingleClientProxy> _mockCaller;
    private Mock<IHubCallerClients> _mockClients;
    private Mock<IGroupManager> _mockGroups;
    private InMemoryDB _db;
    private GameHub _hub;

    private void Initialize()
    {
        _mockContext = new Mock<HubCallerContext>();
        _mockCaller = new Mock<ISingleClientProxy>();
        _mockClients = new Mock<IHubCallerClients>();
        _mockGroups = new Mock<IGroupManager>();

        _db = new InMemoryDB();
        _hub = new GameHub(_db)
        {
            Context = _mockContext.Object,
            Clients = _mockClients.Object,
            Groups = _mockGroups.Object
        };

        _mockClients.Setup(clients => clients.Caller).Returns(_mockCaller.Object);
        _mockClients.Setup(clients => clients.Group(It.IsAny<string>())).Returns(_mockCaller.Object);
        _mockGroups.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Theory]
    [AutoData]
    public async Task JoinSpecificGameRoom_ShouldJoinFailed_UsernameAlreadyTakenInRoom(UserConnection connection)
    {
        Initialize();

        var existingConnection = new UserConnection
        {
            Username = connection.Username,
            GameRoomName = connection.GameRoomName
        };

        _db.Connections.TryAdd("existingConnectionId", existingConnection);

        await _hub.JoinSpecificGameRoom(connection);

        VerifyCallerSendCoreAsync("JoinFailed", Times.Once(), ["Username already taken in this room"]);
        Assert.False(_db.Connections.Values.Contains(connection));
    }

    [Theory]
    [InlineAutoData(GameState.InProgress)]
    [InlineAutoData(GameState.PlacingShips)]
    [InlineAutoData(GameState.Finished)]
    public async Task JoinSpecificGameRoom_ShouldJoinFailed_GameAlreadyStarted(GameState gameState,
        UserConnection connection)
    {
        Initialize();

        var gameRoom = new GameRoom();
        gameRoom.State = gameState;
        gameRoom.Name = connection.GameRoomName;

        _db.GameRooms.TryAdd(connection.GameRoomName, gameRoom);

        await _hub.JoinSpecificGameRoom(connection);

        VerifyCallerSendCoreAsync("JoinFailed", Times.Once(), ["Game has already started."]);
        Assert.False(_db.Connections.Values.Contains(connection));
    }

    [Theory]
    [AutoData]
    public async Task JoinSpecificGameRoom_ShouldJoinFailed_GameRoomIsFull(string gameRoomName)
    {
        Initialize();

        var connections = new List<UserConnection>
        {
            new UserConnection { GameRoomName = gameRoomName, Username = "User1" },
            new UserConnection { GameRoomName = gameRoomName, Username = "User2" },
            new UserConnection { GameRoomName = gameRoomName, Username = "User3" },
            new UserConnection { GameRoomName = gameRoomName, Username = "User4" }
        };

        connections.ForEach(p => _db.Connections.TryAdd(p.Username, p));

        var connection = new UserConnection()
        {
            GameRoomName = gameRoomName,
            Username = "User0"
        };

        await _hub.JoinSpecificGameRoom(connection);

        VerifyCallerSendCoreAsync("JoinFailed", Times.Once(), ["Game room is full."]);
        Assert.False(_db.Connections.Values.Contains(connection));
    }

    [Theory]
    [AutoData]
    public async Task JoinSpecificGameRoom_ShouldJoin_FirstPlayerIsModerator(UserConnection connection,
        string connectionId)
    {
        Initialize();
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

        await _hub.JoinSpecificGameRoom(connection);

        Assert.True(_db.Connections.Values.Contains(connection));

        var player = _db.Connections.Values.FirstOrDefault(p => p.Username == connection.Username);
        Assert.NotNull(player);

        _mockGroups.Verify(x => x.AddToGroupAsync(player.PlayerId, player.GameRoomName, It.IsAny<CancellationToken>()),
            Times.Once);
        VerifyCallerSendCoreAsync("SetModerator", Times.Once(), [true]);
    }

    [Theory]
    [AutoData]
    public async Task GenerateBoard_ShouldNotGenerate_WhenNoPlayers(UserConnection connection, string connectionId)
    {
        Initialize();
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
        connection.IsModerator = true;
        _db.Connections.TryAdd(connectionId, connection);
        _db.GameRooms.TryAdd(connection.GameRoomName, new GameRoom { State = GameState.NotStarted });

        await _hub.GenerateBoard();

        Assert.Null(_db.GameRooms[connection.GameRoomName].Board);
    }

    [Theory]
    [AutoData]
    public async Task GenerateBoard_ShouldGenerateBoard_ForTwoPlayers(
        UserConnection connection1,
        UserConnection connection2,
        string connectionId)
    {
        Initialize();
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
        connection1.IsModerator = true;
        connection2.GameRoomName = connection1.GameRoomName;
        _db.Connections.TryAdd(connectionId, connection1);
        _db.Connections.TryAdd("connection2", connection2);
        _db.GameRooms.TryAdd(connection1.GameRoomName, new GameRoom
        {
            Name = connection1.GameRoomName,
            State = GameState.NotStarted
        });

        await _hub.GenerateBoard();

        var gameRoom = _db.GameRooms[connection1.GameRoomName];
        Assert.NotNull(gameRoom.Board);
        Assert.Equal(GameState.PlacingShips, gameRoom.State);
        VerifyCallerSendCoreAsync("BoardGenerated", Times.Once(), [connection1.GameRoomName, gameRoom.Board]);
    }

    [Theory]
    [AutoData]
    public async Task GenerateBoard_ShouldGenerateBoard_ForThreePlayers(
        UserConnection connection1,
        UserConnection connection2,
        UserConnection connection3,
        string connectionId)
    {
        Initialize();
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
        connection1.IsModerator = true;
        connection2.GameRoomName = connection1.GameRoomName;
        connection3.GameRoomName = connection1.GameRoomName;
        _db.Connections.TryAdd(connectionId, connection1);
        _db.Connections.TryAdd("connection2", connection2);
        _db.Connections.TryAdd("connection3", connection3);
        _db.GameRooms.TryAdd(connection1.GameRoomName, new GameRoom
        {
            Name = connection1.GameRoomName,
            State = GameState.NotStarted
        });

        await _hub.GenerateBoard();

        var gameRoom = _db.GameRooms[connection1.GameRoomName];
        Assert.NotNull(gameRoom.Board);
        Assert.Equal(GameState.PlacingShips, gameRoom.State);
        VerifyCallerSendCoreAsync("BoardGenerated", Times.Once(), [connection1.GameRoomName, gameRoom.Board]);
    }

    [Theory]
    [AutoData]
    public async Task GenerateBoard_ShouldGenerateBoard_ForFourPlayers(
        UserConnection connection1,
        UserConnection connection2,
        UserConnection connection3,
        UserConnection connection4,
        string connectionId)
    {
        Initialize();
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
        connection1.IsModerator = true;
        connection2.GameRoomName = connection1.GameRoomName;
        connection3.GameRoomName = connection1.GameRoomName;
        connection4.GameRoomName = connection1.GameRoomName;
        _db.Connections.TryAdd(connectionId, connection1);
        _db.Connections.TryAdd("connection2", connection2);
        _db.Connections.TryAdd("connection3", connection3);
        _db.Connections.TryAdd("connection4", connection4);
        _db.GameRooms.TryAdd(connection1.GameRoomName, new GameRoom
        {
            Name = connection1.GameRoomName,
            State = GameState.NotStarted
        });

        await _hub.GenerateBoard();

        var gameRoom = _db.GameRooms[connection1.GameRoomName];
        Assert.NotNull(gameRoom.Board);
        Assert.Equal(GameState.PlacingShips, gameRoom.State);

        VerifyCallerSendCoreAsync("BoardGenerated", Times.Once(), [connection1.GameRoomName, gameRoom.Board]);
    }

    [Theory]
    [AutoData]
    public async Task GenerateBoard_ShouldNotGenerate_WhenGameAlreadyStarted(
        UserConnection connection,
        string connectionId)
    {
        Initialize();
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
        connection.IsModerator = true;
        _db.Connections.TryAdd(connectionId, connection);
        _db.GameRooms.TryAdd(connection.GameRoomName, new GameRoom { State = GameState.InProgress });

        await _hub.GenerateBoard();

        Assert.Null(_db.GameRooms[connection.GameRoomName].Board);
    }

    [Theory]
    [AutoData]
    public async Task GenerateBoard_ShouldNotGenerate_WhenUserIsNotModerator(
        UserConnection connection,
        string connectionId)
    {
        Initialize();
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
        connection.IsModerator = false;
        _db.Connections.TryAdd(connectionId, connection);
        _db.GameRooms.TryAdd(connection.GameRoomName, new GameRoom { State = GameState.NotStarted });

        await _hub.GenerateBoard();

        Assert.Null(_db.GameRooms[connection.GameRoomName].Board);
    }

    [Theory]
    [AutoData]
    public async Task SetPlayerToReady_ShouldSendPlayerNotReady_WhenShipsNotPlaced(UserConnection connection)
    {
        Initialize();
        connection.PlacedShips = new List<PlacedShip>();
        _mockContext.Setup(c => c.ConnectionId).Returns(connection.PlayerId);

        var gameRoom = new GameRoom
        {
            Name = connection.GameRoomName,
            State = GameState.PlacingShips,
            Settings = new GameRoomSettings()
        };

        _db.Connections[connection.PlayerId] = connection;
        _db.GameRooms[connection.GameRoomName] = gameRoom;

        await _hub.SetPlayerToReady();

        VerifyCallerSendCoreAsync("PlayerNotReady", Times.Once(), ["You have not placed all your ships."]);
    }

    [Theory]
    [AutoData]
    public async Task SetPlayerToReady_ShouldSetPlayerToReady_WhenShipsPlaced(UserConnection connection)
    {
        Initialize();
        connection.PlacedShips = new List<PlacedShip>();
        _mockContext.Setup(c => c.ConnectionId).Returns(connection.PlayerId);

        var gameRoom = new GameRoom
        {
            Name = connection.GameRoomName,
            State = GameState.PlacingShips,
            Settings = new GameRoomSettings()
        };

        _db.Connections[connection.PlayerId] = connection;
        _db.GameRooms[connection.GameRoomName] = gameRoom;

        gameRoom.Settings.ShipsConfig = new List<ShipConfig>
            { new ShipConfig { ShipType = ShipType.Carrier, Count = 1 } };
        connection.PlacedShips = new List<PlacedShip> { new PlacedShip { ShipType = ShipType.Carrier } };

        _db.Connections[connection.PlayerId] = connection;
        _db.GameRooms[connection.GameRoomName] = gameRoom;

        await _hub.SetPlayerToReady();

        VerifyCallerSendCoreAsync("PlayerReady", Times.Once(), ["You are ready to start the game."]);
    }

    [Theory]
    [InlineAutoData(GameState.NotStarted)]
    [InlineAutoData(GameState.InProgress)]
    [InlineAutoData(GameState.Finished)]
    public async Task SetPlayerToReady_ShouldNotSetPlayerToReady_WhenGameStateIsNotPlacingShips(
        GameState gameState,
        UserConnection connection)
    {
        Initialize();
        connection.PlacedShips = new List<PlacedShip>();
        _mockContext.Setup(c => c.ConnectionId).Returns(connection.PlayerId);

        var gameRoom = new GameRoom
        {
            Name = connection.GameRoomName,
            State = gameState,
            Settings = new GameRoomSettings()
        };

        _db.Connections[connection.PlayerId] = connection;
        _db.GameRooms[connection.GameRoomName] = gameRoom;

        await _hub.SetPlayerToReady();

        VerifyCallerSendCoreAsync("PlayerNotReady", Times.Never(), ["You have not placed all your ships."]);
    }

    [Theory]
    [InlineAutoData(GameState.PlacingShips)]
    [InlineAutoData(GameState.InProgress)]
    [InlineAutoData(GameState.Finished)]
    public async Task StartGame_ShouldNotStartGame_WhenGameStateIsNotNotStarted(GameState gameState,
        UserConnection connection)
    {
        Initialize();
        connection.IsModerator = true;
        var gameRoom = new GameRoom { Name = connection.GameRoomName, State = gameState };

        _db.Connections[connection.PlayerId] = connection;
        _db.GameRooms[connection.GameRoomName] = gameRoom;

        _mockContext.Setup(c => c.ConnectionId).Returns(connection.PlayerId);
        
        await _hub.StartGame();
        
        Assert.Equal(gameState, gameRoom.State);
        VerifyCallerSendCoreAsync("GameStateChanged", Times.Never(), [(int)gameRoom.State]);
    }

    [Fact]
    public async Task StartGame_ShouldNotStartGame_WhenUserIsNotModerator()
    {
        Initialize();
        var connection = new UserConnection { GameRoomName = "room1", PlayerId = "player1", IsModerator = false };
        var gameRoom = new GameRoom { Name = connection.GameRoomName, State = GameState.NotStarted };

        _db.Connections[connection.PlayerId] = connection;
        _db.GameRooms[connection.GameRoomName] = gameRoom;

        _mockContext.Setup(c => c.ConnectionId).Returns(connection.PlayerId);
        
        await _hub.StartGame();
        
        Assert.Equal(GameState.NotStarted, gameRoom.State);
        VerifyCallerSendCoreAsync("GameStateChanged", Times.Never(), [(int)gameRoom.State]);
    }

    private void VerifyCallerSendCoreAsync(string methodName, Times times, object[] args)
    {
        _mockCaller.Verify(caller => caller.SendCoreAsync(
            methodName, It.Is<object[]>(o => args[0].Equals(o[0])), It.IsAny<CancellationToken>()), times);
    }
}