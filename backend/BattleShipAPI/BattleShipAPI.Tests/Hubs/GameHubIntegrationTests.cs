using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BattleShipAPI.Enums;
using BattleShipAPI.Hubs;
using BattleShipAPI.Models;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

public class GameHubIntegrationTests
{
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<ISingleClientProxy> _mockCaller; 
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly InMemoryDB _db;
    private readonly GameHub _hub;

    public GameHubIntegrationTests()
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

    [Fact]
    public async Task JoinSpecificGameRoom_NewUser_SuccessfullyJoins()
    {
        var connectionId = "test-connection-id";
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

        var userConnection = new UserConnection
        {
            Username = "TestUser",
            GameRoomName = "TestRoom"
        };

        // Act
        await _hub.JoinSpecificGameRoom(userConnection);

        // Assert
        Assert.True(_db.Connections.ContainsKey(connectionId));
        Assert.Equal(connectionId, _db.Connections[connectionId].PlayerId);
        Assert.Equal("TestUser", _db.Connections[connectionId].Username);
        Assert.Equal("TestRoom", _db.Connections[connectionId].GameRoomName);

        // Verify Clients.Caller SendCoreAsync calls
        _mockCaller.Verify(caller => caller.SendCoreAsync("SetModerator", It.Is<object[]>(o => (bool)o[0]), It.IsAny<CancellationToken>()), Times.Once);
        _mockCaller.Verify(caller => caller.SendCoreAsync("ReceivePlayerId", It.Is<object[]>(o => (string)o[0] == connectionId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task JoinSpecificGameRoom_UsernameAlreadyTaken_JoinFailed()
    {
        // Arrange
        var connectionId = "test-connection-id";
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

        var existingConnection = new UserConnection
        {
            PlayerId = "existing-connection-id",
            Username = "TestUser",
            GameRoomName = "TestRoom"
        };
        _db.Connections["existing-connection-id"] = existingConnection;

        var userConnection = new UserConnection
        {
            Username = "TestUser",
            GameRoomName = "TestRoom"
        };

        // Act
        await _hub.JoinSpecificGameRoom(userConnection);

        // Assert
        _mockCaller.Verify(caller => caller.SendCoreAsync("JoinFailed", It.Is<object[]>(o => (string)o[0] == "Username already taken in this room"), It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(_db.Connections.ContainsKey(connectionId));
    }
}
