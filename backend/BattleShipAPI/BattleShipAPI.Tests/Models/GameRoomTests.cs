using BattleShipAPI.Enums;
using BattleShipAPI.Models;

namespace BattleShipAPI.Tests.Models
{
    public class GameRoomTests
    {
        [Fact]
        public void GetNextTurnPlayerId_ShouldReturnFirstPlayer_WhenTurnPlayerIdIsEmpty()
        {
            // Arrange
            var gameRoom = new GameRoom();
            var players = new List<UserConnection>
            {
                new UserConnection { PlayerId = "1", Username = "Alice", CanPlay = true },
                new UserConnection { PlayerId = "2", Username = "Bob", CanPlay = true }
            };

            // Act
            var nextTurnPlayerId = gameRoom.GetNextTurnPlayerId(players);

            // Assert
            Assert.Equal("1", nextTurnPlayerId);
        }

        [Fact]
        public void GetNextTurnPlayerId_ShouldReturnNextPlayer_WhenTurnPlayerIdIsSet()
        {
            // Arrange
            var gameRoom = new GameRoom();
            var players = new List<UserConnection>
            {
                new UserConnection { PlayerId = "1", Username = "Alice", CanPlay = true },
                new UserConnection { PlayerId = "2", Username = "Bob", CanPlay = true }
            };

            // Act
            gameRoom.GetNextTurnPlayerId(players);
            var nextTurnPlayerId = gameRoom.GetNextTurnPlayerId(players);
            
            // Assert
            Assert.Equal("2", nextTurnPlayerId);
        }

        [Fact]
        public void TryFullySinkShip_ShouldReturnFalse_WhenCellDoesNotHaveShip()
        {
            // Arrange
            var gameRoom = new GameRoom
            {
                Board = new Board(10,10)
                {
                    Cells = new Cell[][] { new Cell[]{ new Cell { State = CellState.Empty } } }
                }
            };
            var player = new UserConnection();

            // Act
            var result = gameRoom.TryFullySinkShip(0, 0, player);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryFullySinkShip_ShouldReturnTrue_WhenShipIsFullyDamaged()
        {
            // Arrange
            var gameRoom = new GameRoom
            {
                Board = new Board(10, 10)
                {
                    Cells = new Cell[][] { new Cell[] { new Cell { State = CellState.HasShip, OwnerId = "1" } } }
                }
            };
            var player = new UserConnection
            {
                PlayerId = "1",
                PlacedShips = new List<PlacedShip>
                {
                    new PlacedShip { StartX = 0, StartY = 0, EndX = 0, EndY = 0 }
                }
            };

            // Act
            var result = gameRoom.TryFullySinkShip(0, 0, player);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasAliveShips_ShouldReturnTrue_WhenPlayerHasAliveShips()
        {
            // Arrange
            var gameRoom = new GameRoom
            {
                Board = new Board(10, 10)
                {
                    Cells = new Cell[][] { new Cell[] { new Cell { State = CellState.HasShip } } }
                }
            };
            var player = new UserConnection
            {
                PlacedShips = new List<PlacedShip>
                {
                    new PlacedShip { StartX = 0, StartY = 0, EndX = 0, EndY = 0 }
                }
            };

            // Act
            var result = gameRoom.HasAliveShips(player);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SinkAllShips_ShouldSinkAllPlayerShips()
        {
            // Arrange
            var gameRoom = new GameRoom
            {
                Board = new Board(10, 10)
                {
                    Cells = new Cell[][] { new Cell[] { new Cell { State = CellState.HasShip } } }
                }
            };
            var player = new UserConnection
            {
                PlacedShips = new List<PlacedShip>
                {
                    new PlacedShip { StartX = 0, StartY = 0, EndX = 0, EndY = 0 }
                }
            };

            // Act
            gameRoom.SinkAllShips(player);

            // Assert
            Assert.All(player.PlacedShips, ship => 
                Assert.Equal(CellState.SunkenShip, gameRoom.Board.Cells[ship.StartX][ship.StartY].State));
        }
    }
}
