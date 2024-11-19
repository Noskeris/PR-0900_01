using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using BattleShipAPI.Models;
using BattleShipAPI.Enums;

namespace LoadTest
{
    class Program
    {
        private static readonly string HubUrl = "https://localhost:7085/game";
        private static readonly int NumberOfUsers = 1000;
        private static readonly TimeSpan TestDuration = TimeSpan.FromMinutes(5);

        // Aggregate variables for response times
        static long joinRoomTotalTime = 0;
        static int joinRoomCount = 0;
        static long joinRoomMinTime = long.MaxValue;
        static long joinRoomMaxTime = 0;

        static long generateBoardTotalTime = 0;
        static int generateBoardCount = 0;
        static long generateBoardMinTime = long.MaxValue;
        static long generateBoardMaxTime = 0;

        static long addShipTotalTime = 0;
        static int addShipCount = 0;
        static long addShipMinTime = long.MaxValue;
        static long addShipMaxTime = 0;

        static long setPlayerReadyTotalTime = 0;
        static int setPlayerReadyCount = 0;
        static long setPlayerReadyMinTime = long.MaxValue;
        static long setPlayerReadyMaxTime = 0;

        static long startGameTotalTime = 0;
        static int startGameCount = 0;
        static long startGameMinTime = long.MaxValue;
        static long startGameMaxTime = 0;

        static long attackCellTotalTime = 0;
        static int attackCellCount = 0;
        static long attackCellMinTime = long.MaxValue;
        static long attackCellMaxTime = 0;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting load test...");

            // CancellationToken to control the test duration
            using var cts = new CancellationTokenSource(TestDuration);

            // List to keep track of all client tasks
            var clientTasks = new List<Task>();

            for (int i = 0; i < NumberOfUsers; i++)
            {
                int userId = i;
                clientTasks.Add(Task.Run(() => SimulateUser(userId, cts.Token), cts.Token));

                // Throttle the creation of clients to prevent overwhelming the system
                await Task.Delay(10, cts.Token); // Adjust the delay as needed
            }

            // Wait for all tasks to complete or cancellation
            try
            {
                await Task.WhenAll(clientTasks);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Load test completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in load test: {ex}");
            }
            finally
            {
                // Calculate and display metrics
                CalculateAndDisplayMetrics();
            }
        }

        private static async Task SimulateUser(int userId, CancellationToken cancellationToken)
        {
            Console.WriteLine($"User{userId} starting...");

            var connection = new HubConnectionBuilder()
                .WithUrl(HubUrl, options =>
                {
                    options.HttpMessageHandlerFactory = (message) =>
                    {
                        if (message is System.Net.Http.HttpClientHandler clientHandler)
                        {
                            // Ignore SSL certificate errors (for testing purposes)
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                (sender, certificate, chain, sslPolicyErrors) => { return true; };
                        }
                        return message;
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            connection.Closed += async (error) =>
            {
                Console.WriteLine($"Connection closed for User{userId}: {error?.Message}");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync(cancellationToken);
            };

            try
            {
                await connection.StartAsync(cancellationToken);
                Console.WriteLine($"User{userId} connected.");

                var gameRoomName = $"Room{userId % 10}"; // Distribute users across 10 rooms
                var userConnection = new UserConnection
                {
                    Username = $"User{userId}",
                    GameRoomName = gameRoomName
                };

                // Time the JoinSpecificGameRoom method
                var stopwatch = Stopwatch.StartNew();
                await connection.InvokeAsync("JoinSpecificGameRoom", userConnection, cancellationToken);
                stopwatch.Stop();
                long elapsedTime = stopwatch.ElapsedMilliseconds;
                Interlocked.Add(ref joinRoomTotalTime, elapsedTime);
                Interlocked.Increment(ref joinRoomCount);
                Interlocked.Exchange(ref joinRoomMinTime, Math.Min(Interlocked.Read(ref joinRoomMinTime), elapsedTime));
                Interlocked.Exchange(ref joinRoomMaxTime, Math.Max(Interlocked.Read(ref joinRoomMaxTime), elapsedTime));

                Console.WriteLine($"User{userId} joined room {gameRoomName} in {elapsedTime} ms.");

                // Random delay to simulate real user behavior
                await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 3)), cancellationToken);

                // Determine if the user is the moderator for the room
                bool isModerator = userId % 10 == 0; // First user in each room is the moderator

                if (isModerator)
                {
                    Console.WriteLine($"User{userId} is moderator for room {gameRoomName}.");
                    // Time the GenerateBoard method
                    stopwatch.Restart();
                    await connection.InvokeAsync("GenerateBoard", cancellationToken);
                    stopwatch.Stop();
                    elapsedTime = stopwatch.ElapsedMilliseconds;
                    Interlocked.Add(ref generateBoardTotalTime, elapsedTime);
                    Interlocked.Increment(ref generateBoardCount);
                    Interlocked.Exchange(ref generateBoardMinTime, Math.Min(Interlocked.Read(ref generateBoardMinTime), elapsedTime));
                    Interlocked.Exchange(ref generateBoardMaxTime, Math.Max(Interlocked.Read(ref generateBoardMaxTime), elapsedTime));

                    Console.WriteLine($"User{userId} generated board for room {gameRoomName} in {elapsedTime} ms.");
                }

                // Random delay
                await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 3)), cancellationToken);

                // Place ships
                var placedShips = GenerateRandomShips(userId);
                foreach (var ship in placedShips)
                {
                    stopwatch.Restart();
                    await connection.InvokeAsync("AddShip", ship, cancellationToken);
                    stopwatch.Stop();
                    elapsedTime = stopwatch.ElapsedMilliseconds;
                    Interlocked.Add(ref addShipTotalTime, elapsedTime);
                    Interlocked.Increment(ref addShipCount);
                    Interlocked.Exchange(ref addShipMinTime, Math.Min(Interlocked.Read(ref addShipMinTime), elapsedTime));
                    Interlocked.Exchange(ref addShipMaxTime, Math.Max(Interlocked.Read(ref addShipMaxTime), elapsedTime));

                    Console.WriteLine($"User{userId} placed ship {ship.ShipType} in {elapsedTime} ms.");
                    await Task.Delay(TimeSpan.FromSeconds(0.5), cancellationToken); // Slight delay between placing ships
                }

                // Time the SetPlayerToReady method
                stopwatch.Restart();
                await connection.InvokeAsync("SetPlayerToReady", cancellationToken);
                stopwatch.Stop();
                elapsedTime = stopwatch.ElapsedMilliseconds;
                Interlocked.Add(ref setPlayerReadyTotalTime, elapsedTime);
                Interlocked.Increment(ref setPlayerReadyCount);
                Interlocked.Exchange(ref setPlayerReadyMinTime, Math.Min(Interlocked.Read(ref setPlayerReadyMinTime), elapsedTime));
                Interlocked.Exchange(ref setPlayerReadyMaxTime, Math.Max(Interlocked.Read(ref setPlayerReadyMaxTime), elapsedTime));

                Console.WriteLine($"User{userId} is ready in {elapsedTime} ms.");

                // Random delay
                await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 3)), cancellationToken);

                if (isModerator)
                {
                    // Time the StartGame method
                    stopwatch.Restart();
                    await connection.InvokeAsync("StartGame", cancellationToken);
                    stopwatch.Stop();
                    elapsedTime = stopwatch.ElapsedMilliseconds;
                    Interlocked.Add(ref startGameTotalTime, elapsedTime);
                    Interlocked.Increment(ref startGameCount);
                    Interlocked.Exchange(ref startGameMinTime, Math.Min(Interlocked.Read(ref startGameMinTime), elapsedTime));
                    Interlocked.Exchange(ref startGameMaxTime, Math.Max(Interlocked.Read(ref startGameMaxTime), elapsedTime));

                    Console.WriteLine($"User{userId} started the game in room {gameRoomName} in {elapsedTime} ms.");
                }

                // Simulate gameplay
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Wait before making an attack
                    await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 3)), cancellationToken);

                    // Generate random attack coordinates
                    int x = new Random().Next(0, 19);
                    int y = new Random().Next(0, 19);

                    // Time the AttackCell method
                    stopwatch.Restart();
                    await connection.InvokeAsync("AttackCell", x, y, cancellationToken);
                    stopwatch.Stop();
                    elapsedTime = stopwatch.ElapsedMilliseconds;
                    Interlocked.Add(ref attackCellTotalTime, elapsedTime);
                    Interlocked.Increment(ref attackCellCount);
                    Interlocked.Exchange(ref attackCellMinTime, Math.Min(Interlocked.Read(ref attackCellMinTime), elapsedTime));
                    Interlocked.Exchange(ref attackCellMaxTime, Math.Max(Interlocked.Read(ref attackCellMaxTime), elapsedTime));

                    Console.WriteLine($"User{userId} attacked cell ({x}, {y}) in {elapsedTime} ms.");
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when the cancellationToken is canceled
                Console.WriteLine($"User{userId} operation canceled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"User{userId} error: {ex}");
            }
            finally
            {
                await connection.DisposeAsync();
                Console.WriteLine($"User{userId} disconnected.");
            }
        }

        private static void CalculateAndDisplayMetrics()
        {
            Console.WriteLine("\n--- Load Test Metrics ---\n");

            void DisplayAggregatedStats(string operationName, long totalTime, int count, long minTime, long maxTime)
            {
                if (count > 0)
                {
                    var average = totalTime / count;
                    Console.WriteLine($"{operationName} - Count: {count}, Avg: {average} ms, Min: {minTime} ms, Max: {maxTime} ms");
                }
                else
                {
                    Console.WriteLine($"{operationName} - No data collected.");
                }
            }

            DisplayAggregatedStats("JoinSpecificGameRoom", joinRoomTotalTime, joinRoomCount, joinRoomMinTime, joinRoomMaxTime);
            DisplayAggregatedStats("GenerateBoard", generateBoardTotalTime, generateBoardCount, generateBoardMinTime, generateBoardMaxTime);
            DisplayAggregatedStats("AddShip", addShipTotalTime, addShipCount, addShipMinTime, addShipMaxTime);
            DisplayAggregatedStats("SetPlayerToReady", setPlayerReadyTotalTime, setPlayerReadyCount, setPlayerReadyMinTime, setPlayerReadyMaxTime);
            DisplayAggregatedStats("StartGame", startGameTotalTime, startGameCount, startGameMinTime, startGameMaxTime);
            DisplayAggregatedStats("AttackCell", attackCellTotalTime, attackCellCount, attackCellMinTime, attackCellMaxTime);

            // Calculate throughput
            var totalAttacks = attackCellCount;
            var totalDurationSeconds = TestDuration.TotalSeconds;
            var throughput = totalAttacks / totalDurationSeconds;

            Console.WriteLine($"\nTotal Attacks: {totalAttacks}");
            Console.WriteLine($"Test Duration: {totalDurationSeconds} seconds");
            Console.WriteLine($"Throughput: {throughput:F2} attacks per second");
        }

        private static List<PlacedShip> GenerateRandomShips(int userId)
        {
            // Implement logic to generate random ship placements
            // For simplicity, we'll return predefined ships
            return new List<PlacedShip>
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
        }
    }
}
