import React, { useState } from "react";
import ReactDOM from "react-dom/client";
import { Header } from "./Header";
import WaitingRoom from "./components/WaitingRoom.js";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import GameRoom from "./components/GameRoom.js";
import "./css/style.css";
import GameComponent from './components/GameComponent.js';

export const App = () => {
  const shipTypesMapping = {
    1: { name: 'carrier', length: 5 },
    2: { name: 'battleship', length: 4 },
    3: { name: 'cruiser', length: 3 },
    4: { name: 'submarine', length: 2 },
    5: { name: 'destroyer', length: 1 },
  };
  const [connection, setConnection] = useState();
  const [messages, setMessages] = useState([]);
  const [isModerator, setIsModerator] = useState(false);
  const [board, setBoard] = useState(null);
  const [username, setUsername] = useState("");
  const [playerId, setPlayerId] = useState();
  const [gameState, setGameState] = useState(1);
  const [shipsToPlace, setShipsToPlace] = useState();
  const [playerTurn, setPlayerTurn] = useState();

  const joinGameRoom = async (usernameInput, gameRoomName) => {
    try {
      const newConnection = new HubConnectionBuilder()
        .withUrl("https://localhost:7085/game")
        .configureLogging(LogLevel.Information)
        .build();

      // Set up event handlers
      newConnection.on("JoinSpecificGameRoom", (username, msg) => {
        setMessages((prevMessages) => [...prevMessages, { username, msg }]);
        console.log("msg: ", msg);
      });

      newConnection.on("SetModerator", (isModerator) => {
        setIsModerator(isModerator);
      });

      newConnection.on("JoinFailed", (message) => {
        console.log("JoinFailed reason:", message)
      });

      newConnection.on("BoardGenerated", (gameRoom, generatedBoard) => {
        setBoard(generatedBoard);
        console.log("Board received:", generatedBoard);
      });

      newConnection.on("ReceivePlayerId", (playerId) => {
        setPlayerId(playerId);
      });

      newConnection.on("GameStateChanged", (newGameState) => {
        console.log("Game state changed to:", newGameState);
        setGameState(newGameState);
      });

      newConnection.on("BoardUpdated", (gameRoomName, board) => {
        setBoard(board);
      });

      newConnection.on("FailedToStartGame", (message) => {
        console.log ("Failed to start game:", message);
      });

      newConnection.on("PlayerReady", (message) => {
        console.log(message);
      });

      newConnection.on("PlayerTurn", (playerId) => {
        setPlayerTurn(playerId);
      })

      newConnection.on("FailedToAttackCell", (message) => {
        console.log("FailedToAttackCell", message);
      })

      newConnection.on("AttackResult", (message) => {
        console.log("AttackResult:", message);
      })

      newConnection.on("WinnerResult", (username, msg) => {
        setMessages((prevMessages) => [...prevMessages, { username, msg }]);
      });
      
      newConnection.on("GameLostResult", (username, msg) => {
        setMessages((prevMessages) => [...prevMessages, { username, msg }]);
      });

      newConnection.on("AvailableShipsForConfiguration", (shipConfig) => {
        //TODO LATER
      })

      newConnection.on("UpdatedShipsConfig", (shipsConfig) => {
        const mappedShips = shipsConfig
          .map((ship) => {
            const { name, length } = shipTypesMapping[ship.shipType];
            
            if (ship.count > 0) {
              return {
                name,
                length,
                placed: null,
                count: ship.count,
              };
            }
            
            return null; 
          })
          .filter(Boolean);
        
        setShipsToPlace(mappedShips);
        console.log("shipsTOPlaceMapp", mappedShips)
      });

      await newConnection.start();

      const userConnection = {
        Username: usernameInput,
        GameRoomName: gameRoomName,
      };

      await newConnection.invoke("JoinSpecificGameRoom", userConnection);

      setConnection(newConnection);
      setUsername(usernameInput);

    } catch (error) {
      console.log("Connection error: ", error);
    }
  };

  const generateBoardAction = async () => {
    try {
      await connection.invoke("GenerateBoard");
      console.log("UI generate board");
    } catch (error) {
      console.log("Generate board error: ", error);
    }
  };

  const startGame = async () => {
    try {
      await connection.invoke("StartGame");
      console.log("StartGame invoked");
    } catch (error) {
      console.log("Error starting game:", error);
    }
  };
  
  const restartGame = async () => {
    try {
      await connection.invoke("RestartGame");
      console.log("RestartGame invoked");
    } catch (error) {
      console.log("Error RestartGame :", error);
    }
  };

  const addShip = async (placedShip) => {
    try {
      await connection.invoke("AddShip", placedShip);
      console.log("AddShip invoked");
    } catch (error) {
      console.log("Error adding ship:", error);
    }
  };

  const playerReady = async () => {
    try {
      await connection.invoke("SetPlayerToReady");
      console.log("SetPlayerToReady invoked");
    } catch (error) {
      console.log("Error SetPlayerToReady", error);
    }
  }

  const attackCell = async (x, y) => {
    try {
      await connection.invoke("AttackCell", x, y);
      console.log("attackCell invoked");
    } catch (error) {
      console.log("Error attackCell", error);
    }
  }


  return (
    <>
      <Header />
      {!connection ? (
        <WaitingRoom joinGameRoom={joinGameRoom} />
      ) : (
        <GameRoom
          messages={messages}
          generateBoardAction={generateBoardAction}
          isModerator={isModerator}
          board={board}
          setBoard={setBoard}
          username={username}
          playerId={playerId}
          gameState={gameState}
          startGame={startGame}
          shipsToPlace={shipsToPlace}
          addShip={addShip}
          playerReady={playerReady}
          playerTurn={playerTurn}
          attackCell={attackCell}
          restartGame={restartGame}
        />
      )}
    </>
  );
};

const root = ReactDOM.createRoot(document.getElementById("root"));
root.render(<App />);
