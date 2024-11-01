import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom/client";
import { Header } from "./Header";
import WaitingRoom from "./components/WaitingRoom.js";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import GameRoom from "./components/GameRoom.js";
import "./css/style.css";
import { superAttackTypesMapping } from "./constants/SuperAttackTypes.js";
import { shipTypesMapping } from "./constants/ShipTypesMapping.js";

export const App = () => {
  const [connection, setConnection] = useState();
  const [messages, setMessages] = useState([]);
  const [isModerator, setIsModerator] = useState(false);
  const [board, setBoard] = useState(null);
  const [username, setUsername] = useState("");
  const [playerId, setPlayerId] = useState();
  const [gameState, setGameState] = useState(1);
  const [shipsToPlace, setShipsToPlace] = useState();
  const [playerTurn, setPlayerTurn] = useState();
  const [playerAvatarConfigs, setPlayerAvatarConfigs] = useState();

  const [turnEndTime, setTurnEndTime] = useState(null);
  const [timer, setTimer] = useState(0);
  const [superAttacks, setSuperAttacks] = useState();
  const [isPlayerReady, setIsPlayerReady] = useState(false);
  const [gameMode, setGameMode] = useState(null);


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
        setIsPlayerReady(false);
      });

      newConnection.on("ReceivePlayerId", (playerId) => {
        setPlayerId(playerId);
      });

      newConnection.on("SetPlayerAvatarConfigs", (playerAvatarConfigs) => {
        setPlayerAvatarConfigs(playerAvatarConfigs);
      });

      newConnection.on("GameStateChanged", (newGameState) => {
        console.log("Game state changed to:", newGameState);
        setGameState(newGameState);
        if(newGameState === 4){
          setGameMode(null);
        }
      });

      newConnection.on("BoardUpdated", (gameRoomName, board) => {
        setBoard(board);
      });

      newConnection.on("FailedToStartGame", (message) => {
        console.log ("Failed to start game:", message);
      });

      newConnection.on("PlayerReady", (message) => {
        setIsPlayerReady(true);
      });

      newConnection.on("PlayerTurn", (playerId, turnStartTime, turnDuration) => {
        setPlayerTurn(playerId);
        const startTime = new Date(turnStartTime);
        if(turnDuration === -1)
        {
          setTurnEndTime(-1);
          return;
        }
        setTurnEndTime(new Date(startTime.getTime() + turnDuration * 1000));
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

      newConnection.on("PlayerIsReady", (message) => {
        console.log("PlayerIsReady", message);
        setIsPlayerReady(true);
      })

      newConnection.on("UpdatedSuperAttacksConfig", (superAttacksConfig) => {
        const mappedSuperAttacks = superAttacksConfig
          .map((superAttack) => {
            console.log("superAttack", superAttack)
              const name = superAttackTypesMapping[superAttack.attackType]

              return {
                name: name,
                count: superAttack.count
              };
            }
        )
          .filter(Boolean);
        
        setSuperAttacks(mappedSuperAttacks);
        console.log("mappedSuperAttacks", mappedSuperAttacks)
      });

      newConnection.on("UpdatedShipsConfig", (shipsConfig) => {
        const mappedShips = shipsConfig
          .map((ship) => {
            console.log("ship", ship)
            const name = shipTypesMapping[ship.shipType];
            
            if (ship.count > 0) {
              return {
                name: name,
                length: ship.size,
                placed: null,
                count: ship.count,
                hasShield: ship.hasShield,
                hasMobility: ship.hasMobility
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
      await connection.invoke("GenerateBoard", gameMode);
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
      setGameMode(null);
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
      setIsPlayerReady(true);
    } catch (error) {
      console.log("Error SetPlayerToReady", error);
    }
  }

  const attackCell = async (x, y, attackType = 1) => {
    try {
      await connection.invoke("AttackCell", x, y, attackType);
      console.log("attackCell invoked");
    } catch (error) {
      console.log("Error attackCell", error);
    }
  }

  const playerTurnTimeEnded = async () => {
    try {
      await connection.invoke("PlayerTurnTimeEnded");
      console.log("PlayerTurnTimeEnded invoked");
    } catch (error) {
      console.log("Error PlayerTurnTimeEnded", error);
    }
  }

  const confirmGameMode = async (gameMode) => {
    try {
      await connection.invoke("ConfirmGameMode", gameMode);
      setGameMode(gameMode);
      console.log("ConfirmGameMode invoked");
    } catch (error) {
      console.log("Error PlayerTurnTimeEnded", error);
    }
  }

  const sendCommand = async (command) => {
    try {
      await connection.invoke("HandlePlayerCommand", command);
      console.log("HandlePlayerCommand invoked");
    } catch (error) {
      console.log("Error HandlePlayerCommand", error);
    }
  }

  const changeAvatar = async (avatar) => {
    try {
      await connection.invoke("ChangeAvatar", avatar.headType, avatar.appearance);
      console.log("ChangeAvatar invoked");
    } catch (error) {
      console.log("Error ChangeAvatar", avatar.headType, avatar.appearance);
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
          timer={timer}
          setTimer={setTimer}
          turnEndTime={turnEndTime}
          playerTurnTimeEnded={playerTurnTimeEnded}
          superAttacks={superAttacks}
          isPlayerReady={isPlayerReady}
          confirmGameMode={confirmGameMode}
          sendCommand={sendCommand}
          playerAvatarConfigs={playerAvatarConfigs}
          changeAvatar={changeAvatar}
        />
      )}
    </>
  );
};

const root = ReactDOM.createRoot(document.getElementById("root"));
root.render(<App />);
