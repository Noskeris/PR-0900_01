import React, { useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

const GameComponent = () => {
    useEffect(() => {
        const connection = new HubConnectionBuilder()
        .withUrl("https://localhost:7085/game")
        .configureLogging(LogLevel.Information)
        .build();

        let currentPlayer = null;
        let turnEndTime = null;
        let countdownTimer;

        // Start the connection
        connection.start().then(() => {
            console.log("Connected to the game hub.");
            connection.invoke("StartGame");
        }).catch(err => console.error(err.toString()));

        connection.on("PlayerTurn", (playerId, turnStartTime, turnDuration) => {
            currentPlayer = playerId;

            const startTime = new Date(turnStartTime);
            turnEndTime = new Date(startTime.getTime() + turnDuration * 1000);
            document.getElementById("playerTurn").innerText = "Player Turn: " + playerId;

            resetTimer();
            updateTimer();
        });

        connection.on("PlayerAction", (playerId, action) => {
            console.log(`${playerId} performed action: ${action}`);
        });

        function updateTimer() {
            const now = new Date();
            const timeLeft = Math.max(0, (turnEndTime - now) / 1000);

            document.getElementById("timer").innerText = timeLeft.toFixed(1) + "s";

            if (timeLeft > 0) {
                setTimeout(updateTimer, 100);
            } else {
                clearTimeout(countdownTimer);
            }
        }

        function resetTimer() {
            clearTimeout(countdownTimer);
        }

        document.getElementById("actionButton").addEventListener("click", function () {
            if (currentPlayer === connection.connectionId) {
                connection.invoke("PlayerAction", "some action here");
            }
        });

        return () => {
            connection.stop();
        };
    }, []);

    return (
        <div>
            <h2 id="playerTurn">Player Turn: </h2>
            <div id="timer">30s</div>
            <button id="actionButton">Perform Action</button>
        </div>
    );
};

export default GameComponent;
