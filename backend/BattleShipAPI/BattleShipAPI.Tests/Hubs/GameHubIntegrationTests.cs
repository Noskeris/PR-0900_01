using BattleShipAPI.Enums;
using BattleShipAPI.Hubs;
using BattleShipAPI.Models;
using BattleShipAPI.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BattleShipAPI.Tests.Hubs
{
    public class GameHubIntegrationTests : IAsyncLifetime
    {
        private readonly IHost _server;
        private HubConnection _connection1;
        private HubConnection _connection2;
        private readonly InMemoryDB _db;

        public List<PlacedShip> PlacedShips1 = new List<PlacedShip>
        {
            new PlacedShip
            {
                ShipType = ShipType.Carrier,
                StartX = 0,
                StartY = 0,
                EndX = 4,
                EndY = 0
            },
            new PlacedShip
            {
                ShipType = ShipType.Battleship,
                StartX = 0,
                StartY = 1,
                EndX = 3,
                EndY = 1
            },
            new PlacedShip
            {
                ShipType = ShipType.Cruiser,
                StartX = 0,
                StartY = 2,
                EndX = 2,
                EndY = 2
            },
            new PlacedShip
            {
                ShipType = ShipType.Submarine,
                StartX = 0,
                StartY = 3,
                EndX = 1,
                EndY = 3
            },
            new PlacedShip
            {
                ShipType = ShipType.Destroyer,
                StartX = 0,
                StartY = 4,
                EndX = 0,
                EndY = 4
            }
        };

        public List<PlacedShip> PlacedShips2 = new List<PlacedShip>
        {
            new PlacedShip
            {
                ShipType = ShipType.Carrier,
                StartX = 10,
                StartY = 0,
                EndX = 14,
                EndY = 0
            },
            new PlacedShip
            {
                ShipType = ShipType.Battleship,
                StartX = 10,
                StartY = 1,
                EndX = 13,
                EndY = 1
            },
            new PlacedShip
            {
                ShipType = ShipType.Cruiser,
                StartX = 10,
                StartY = 2,
                EndX = 12,
                EndY = 2
            },
            new PlacedShip
            {
                ShipType = ShipType.Submarine,
                StartX = 10,
                StartY = 3,
                EndX = 11,
                EndY = 3
            },
            new PlacedShip
            {
                ShipType = ShipType.Destroyer,
                StartX = 10,
                StartY = 4,
                EndX = 10,
                EndY = 4
            }
        };

        public GameHubIntegrationTests()
        {
            _db = new InMemoryDB();

            _server = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup(context => new TestStartup(_db));
                    webBuilder.UseUrls("http://localhost:5001");
                })
                .Build();
        }


        public async Task InitializeAsync()
        {
            await _server.StartAsync();

            _connection1 = new HubConnectionBuilder()
                .WithUrl("http://localhost:5001/gamehub")
                .Build();

            _connection2 = new HubConnectionBuilder()
                .WithUrl("http://localhost:5001/gamehub")
                .Build();

            await _connection1.StartAsync();
            await _connection2.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _connection1.DisposeAsync();
            await _connection2.DisposeAsync();
            await _server.StopAsync();
            _server.Dispose();
        }

        [Fact]
        public async Task JoinGameRoom_UserCanJoinAndReceiveConfigurations()
        {
            var userConnection1 = new UserConnection
            {
                Username = "Player1",
                GameRoomName = "TestRoom1"
            };

            var userConnection2 = new UserConnection
            {
                Username = "Player2",
                GameRoomName = "TestRoom1"
            };

            string receivedPlayerId = null;

            _connection1.On<string>("ReceivePlayerId", playerId => { receivedPlayerId = playerId; });

            string receivedPlayerId2 = null;

            _connection2.On<string>("ReceivePlayerId", playerId => { receivedPlayerId2 = playerId; });

            await _connection1.InvokeAsync("JoinSpecificGameRoom", userConnection1);
            await _connection2.InvokeAsync("JoinSpecificGameRoom", userConnection2);

            Assert.Contains(receivedPlayerId, _db.Connections.Keys);
            Assert.Contains(receivedPlayerId2, _db.Connections.Keys);
            Assert.Equal("TestRoom1", _db.Connections[receivedPlayerId].GameRoomName);
            Assert.Equal("TestRoom1", _db.Connections[receivedPlayerId2].GameRoomName);
        }

        [Fact]
        public async Task StartGame_SufficientPlayers_GameStartsSuccessfully()
        {
            var userConnection1 = new UserConnection
            {
                Username = "Player1",
                GameRoomName = "TestRoom2"
            };
            var userConnection2 = new UserConnection
            {
                Username = "Player2",
                GameRoomName = "TestRoom2"
            };

            string receivedPlayerId = null;
            string receivedPlayerId2 = null;
            bool player1Ready = false;
            bool player2Ready = false;

            _connection1.On<string>("ReceivePlayerId", playerId => { receivedPlayerId = playerId; });

            _connection2.On<string>("ReceivePlayerId", playerId => { receivedPlayerId2 = playerId; });

            _connection1.On<string>("PlayerReady", msg => { player1Ready = true; });

            _connection2.On<string>("PlayerReady", msg => { player2Ready = true; });

            await _connection1.InvokeAsync("JoinSpecificGameRoom", userConnection1);
            await _connection2.InvokeAsync("JoinSpecificGameRoom", userConnection2);

            await _connection1.InvokeAsync("GenerateBoard");

            var sortedConnectionIds = _db.Connections.Keys.OrderBy(x => x).ToList();
            var firstConnectionId = sortedConnectionIds.First();

            if (firstConnectionId == _connection1.ConnectionId)
            {
                foreach (var ship in PlacedShips1)
                {
                    await _connection1.InvokeAsync("AddShip", ship);
                }

                foreach (var ship in PlacedShips2)
                {
                    await _connection2.InvokeAsync("AddShip", ship);
                }
            }
            else
            {
                foreach (var ship in PlacedShips2)
                {
                    await _connection1.InvokeAsync("AddShip", ship);
                }

                foreach (var ship in PlacedShips1)
                {
                    await _connection2.InvokeAsync("AddShip", ship);
                }
            }

            await _connection1.InvokeAsync("SetPlayerToReady");
            await _connection2.InvokeAsync("SetPlayerToReady");

            await _connection1.InvokeAsync("StartGame");

            var gameRoom = _db.GameRooms["TestRoom2"];
            Assert.Equal(GameState.InProgress, gameRoom.State);
            Assert.NotNull(gameRoom.Board);
        }
    }

    public class TestStartup
    {
        private readonly InMemoryDB _db;

        public TestStartup(InMemoryDB db) => _db = db;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddSingleton(_ => _db);
            services.AddTransient<GameHub>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GameHub>("/gamehub");
            });
        }
    }
}
