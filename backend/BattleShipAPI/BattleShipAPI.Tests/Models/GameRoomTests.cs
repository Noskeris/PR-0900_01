using BattleShipAPI.Enums;
using BattleShipAPI.Models;

namespace BattleShipAPI.Tests.Models
{
    public class GameRoomTests
    {
        private readonly GameRoom _gameRoom;
        private readonly List<UserConnection> _players;
        private readonly List<ShipConfig> _defaultShipConfig;

        public GameRoomTests()
        {
            _gameRoom = new GameRoom();
            _players = new List<UserConnection>
            {
                new UserConnection { PlayerId = "1", Username = "Alice", CanPlay = true },
                new UserConnection { PlayerId = "2", Username = "Bob", CanPlay = true },
                new UserConnection { PlayerId = "3", Username = "John", CanPlay = true }
            };
            _defaultShipConfig = new List<ShipConfig>
            {
                new ShipConfig { ShipType = ShipType.Battleship, Count = 1 },
                new ShipConfig { ShipType = ShipType.Carrier, Count = 1 },
                new ShipConfig { ShipType = ShipType.Destroyer, Count = 1 },
                new ShipConfig { ShipType = ShipType.Submarine, Count = 1 },
                new ShipConfig { ShipType = ShipType.Cruiser, Count = 1 }
            };

            _gameRoom.Settings = new GameRoomSettings { ShipsConfig = _defaultShipConfig };
        }

        [Fact]
        public void GameRoom_Name_ShouldHaveDefaultEmptyString()
        {
            var defaultName = _gameRoom.Name;
            Assert.Equal(string.Empty, defaultName);
        }

        [Fact]
        public void GameRoom_Name_ShouldAllowSettingAndGettingValue()
        {
            var newName = "Battle Zone";
            _gameRoom.Name = newName;
            Assert.Equal(newName, _gameRoom.Name);
        }

        [Fact]
        public void GameRoom_State_ShouldHaveDefaultNotStarted()
        {
            var defaultState = _gameRoom.State;
            Assert.Equal(GameState.NotStarted, defaultState);
        }

        [Fact]
        public void GameRoom_State_ShouldAllowSettingAndGettingValue()
        {
            var newState = GameState.InProgress;
            _gameRoom.State = newState;
            Assert.Equal(newState, _gameRoom.State);
        }

        [Fact]
        public void GameRoom_Settings_ShouldAllowSettingAndGettingCustomShipsConfig()
        {
            var customShipsConfig = new List<ShipConfig>
            {
                new ShipConfig { ShipType = ShipType.Battleship, Count = 2 },
                new ShipConfig { ShipType = ShipType.Destroyer, Count = 1 },
                new ShipConfig { ShipType = ShipType.Submarine, Count = 3 }
            };
            var newSettings = new GameRoomSettings { ShipsConfig = customShipsConfig };
            _gameRoom.Settings = newSettings;

            Assert.Equal(newSettings, _gameRoom.Settings);
            Assert.Equal(3, _gameRoom.Settings.ShipsConfig.Count);
            Assert.Equal(customShipsConfig[0].ShipType, _gameRoom.Settings.ShipsConfig[0].ShipType);
            Assert.Equal(customShipsConfig[0].Count, _gameRoom.Settings.ShipsConfig[0].Count);
            Assert.Equal(customShipsConfig[1].ShipType, _gameRoom.Settings.ShipsConfig[1].ShipType);
            Assert.Equal(customShipsConfig[1].Count, _gameRoom.Settings.ShipsConfig[1].Count);
            Assert.Equal(customShipsConfig[2].ShipType, _gameRoom.Settings.ShipsConfig[2].ShipType);
            Assert.Equal(customShipsConfig[2].Count, _gameRoom.Settings.ShipsConfig[2].Count);
        }

        [Fact]
        public void GameRoom_Settings_ShouldHaveDefaultShipsConfig()
        {
            var shipsConfig = _gameRoom.Settings.ShipsConfig;
            Assert.Equal(5, shipsConfig.Count);
            Assert.Contains(shipsConfig, s => s.ShipType == ShipType.Battleship && s.Count == 1);
        }

        [Fact]
        public void GetNextTurnPlayerId_ShouldReturnFirstPlayer_WhenTurnPlayerIdIsEmpty()
        {
            var nextTurnPlayerId = _gameRoom.GetNextTurnPlayerId(_players);
            Assert.Equal("1", nextTurnPlayerId);
        }

        [Fact]
        public void GetNextTurnPlayerId_ShouldReturnNextPlayer_WhenTurnPlayerIdIsSet()
        {
            _gameRoom.GetNextTurnPlayerId(_players); 
            var nextTurnPlayerId = _gameRoom.GetNextTurnPlayerId(_players);
            Assert.Equal("2", nextTurnPlayerId);
        }

        [Fact]
        public void GetNextTurnPlayerId_ShouldReturnEmptyString_WhenNoPlayerCanPlay()
        {
            var noActivePlayers = new List<UserConnection>
            {
                new UserConnection { PlayerId = "1", Username = "Alice", CanPlay = false },
                new UserConnection { PlayerId = "2", Username = "Bob", CanPlay = false }
            };
            var nextTurnPlayerId = _gameRoom.GetNextTurnPlayerId(noActivePlayers);
            Assert.Equal(string.Empty, nextTurnPlayerId);
        }

        [Fact]
        public void GetNextTurnPlayerId_ShouldResetToFirstPlayer_WhenTurnPlayerIdIsNotFound()
        {
            var playersSubset = _players.Take(2).ToList();
            var nextTurnPlayerId = _gameRoom.GetNextTurnPlayerId(playersSubset);
            Assert.Equal("1", nextTurnPlayerId);
        }

        [Fact]
        public void GetNextTurnPlayerId_ShouldReturnSamePlayer_WhenOnlyOnePlayerCanPlay()
        {
            var players = new List<UserConnection>
            {
                new UserConnection { PlayerId = "1", Username = "Alice", CanPlay = true },
                new UserConnection { PlayerId = "2", Username = "Bob", CanPlay = false }
            };
            var firstTurnPlayerId = _gameRoom.GetNextTurnPlayerId(players);
            var nextTurnPlayerId = _gameRoom.GetNextTurnPlayerId(players);
            Assert.Equal("1", firstTurnPlayerId);
            Assert.Equal("1", nextTurnPlayerId);
        }

        [Fact]
        public void GetNextTurnPlayerId_ShouldWrapAroundToFirstPlayer_WhenAtEndOfList()
        {
            _gameRoom.GetNextTurnPlayerId(_players);
            _gameRoom.GetNextTurnPlayerId(_players);
            _gameRoom.GetNextTurnPlayerId(_players);
            var nextTurnPlayerId = _gameRoom.GetNextTurnPlayerId(_players);
            Assert.Equal("1", nextTurnPlayerId);
        }

        [Fact]
        public void GetNextTurnPlayerId_ShouldWrapAroundToFirstPlayer_WhenOnePlayerDisconnects()
        {
            var firstTurnPlayerId = _gameRoom.GetNextTurnPlayerId(_players);
            var secondTurnPlayerId = _gameRoom.GetNextTurnPlayerId(_players);
            var thirdTurnPlayerId = _gameRoom.GetNextTurnPlayerId(_players);
            _players.RemoveAll(x => x.Username == "John");
            var nextTurnPlayerIdAfterRemoval = _gameRoom.GetNextTurnPlayerId(_players);

            Assert.Equal("1", firstTurnPlayerId);
            Assert.Equal("2", secondTurnPlayerId);
            Assert.Equal("3", thirdTurnPlayerId); 
            Assert.Equal("1", nextTurnPlayerIdAfterRemoval); 
        }

        [Fact]
        public void TryFullySinkShip_ShouldReturnFalse_WhenCellDoesNotHaveShip()
        {
            var board = new Board(10, 10);
            _gameRoom.Board = board;
            var player = new UserConnection();

            var result = _gameRoom.TryFullySinkShip(0, 0, player);
            Assert.False(result);
        }

        [Fact]
        public void TryFullySinkShip_ShouldReturnTrue_WhenShipIsFullyDamaged()
        {
            var board = new Board(10, 10);
            board.Cells[0][0].State = CellState.HasShip;
            _gameRoom.Board = board;

            var player = new UserConnection
            {
                PlayerId = "1",
                PlacedShips = new List<PlacedShip>
                {
                    new PlacedShip { StartX = 0, StartY = 0, EndX = 0, EndY = 0, ShipType = ShipType.Carrier }
                }
            };

            var result = _gameRoom.TryFullySinkShip(0, 0, player);
            Assert.True(result);
            Assert.Equal(CellState.SunkenShip, _gameRoom.Board.Cells[0][0].State);
        }

        [Fact]
        public void TryFullySinkShip_ShouldReturnFalse_WhenShipIsPartiallyDamaged()
        {
            var board = new Board(10, 10);
            board.Cells[0][0] = new Cell { State = CellState.DamagedShip, OwnerId = "1" };
            board.Cells[0][1] = new Cell { State = CellState.HasShip, OwnerId = "1" };
            board.Cells[0][2] = new Cell { State = CellState.HasShip, OwnerId = "1" };
            _gameRoom.Board = board;

            var player = new UserConnection
            {
                PlayerId = "1",
                PlacedShips = new List<PlacedShip>
                {
                    new PlacedShip { StartX = 0, StartY = 0, EndX = 0, EndY = 2 }
                }
            };

            var result = _gameRoom.TryFullySinkShip(0, 1, player);
            Assert.False(result);
            Assert.Equal(CellState.DamagedShip, _gameRoom.Board.Cells[0][1].State);
        }

        [Fact]
        public void TryFullySinkShip_ShouldSinkShip_WhenAllCellsAreDamaged()
        {
            var board = new Board(10, 10);
            board.Cells[0][0] = new Cell { State = CellState.DamagedShip, OwnerId = "1" };
            board.Cells[0][1] = new Cell { State = CellState.HasShip, OwnerId = "1" };
            _gameRoom.Board = board;

            var player = new UserConnection
            {
                PlayerId = "1",
                PlacedShips = new List<PlacedShip>
                {
                    new PlacedShip { StartX = 0, StartY = 0, EndX = 0, EndY = 1 }
                }
            };

            var result = _gameRoom.TryFullySinkShip(0, 1, player);
            Assert.True(result);
            Assert.Equal(CellState.SunkenShip, _gameRoom.Board.Cells[0][0].State);
            Assert.Equal(CellState.SunkenShip, _gameRoom.Board.Cells[0][1].State);
        }

        [Fact]
        public void HasAliveShips_ShouldReturnTrue_WhenPlayerHasAliveShips()
        {
            var board = new Board(10, 10);
            board.Cells[0][0].State = CellState.HasShip;
            _gameRoom.Board = board;

            var player = new UserConnection
            {
                PlacedShips = new List<PlacedShip>
                {
                    new PlacedShip { StartX = 0, StartY = 0, EndX = 0, EndY = 0 }
                }
            };

            var result = _gameRoom.HasAliveShips(player);
            Assert.True(result);
        }

        [Fact]
        public void SinkAllShips_ShouldSinkAllPlayerShips()
        {
            var board = new Board(10, 10);
            board.Cells[0][0].State = CellState.HasShip;
            board.Cells[1][0].State = CellState.HasShip;
            _gameRoom.Board = board;

            var player = new UserConnection
            {
                PlacedShips = new List<PlacedShip>
                {
                    new PlacedShip { StartX = 0, StartY = 0, EndX = 0, EndY = 0 },
                    new PlacedShip { StartX = 1, StartY = 0, EndX = 1, EndY = 0 }
                }
            };

            _gameRoom.SinkAllShips(player);
            Assert.All(player.PlacedShips, ship =>
                Assert.Equal(CellState.SunkenShip, _gameRoom.Board.Cells[ship.StartX][ship.StartY].State));
        }
    }
}
