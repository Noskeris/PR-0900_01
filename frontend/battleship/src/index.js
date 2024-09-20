import React, { useState } from "react";
import ReactDOM from "react-dom/client";
import { Header } from "./Header";
import WaitingRoom from "./components/WaitingRoom.js";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import GameRoom from "./components/GameRoom.js";
import "./css/style.css";

export const App = () => {
  const [connection, setConnection] = useState();
  const [messages, setMessages] = useState([]);
  const [isModerator, setIsModerator] = useState(false);
  const [board, setBoard] = useState(null);
  const [username, setUsername] = useState("");
  const [playerId, setPlayerId] = useState();
  const [gameState, setGameState] = useState(0);

  const joinGameRoom = async (usernameInput, gameRoomName) => {
    try {
      const newConnection = new HubConnectionBuilder()
        .withUrl("https://localhost:7085/game")
        .configureLogging(LogLevel.Information)
        .build();

      newConnection.on("JoinSpecificGameRoom", (username, msg) => {
        setMessages((prevMessages) => [...prevMessages, { username, msg }]);
        console.log("msg: ", msg);
      });

      newConnection.on("RecieveSpecificMessage", (username, msg) => {
        setMessages((prevMessages) => [...prevMessages, { username, msg }]);
        console.log(msg);
      });

      newConnection.on("SetModerator", (isModerator) => {
        setIsModerator(isModerator);
      });

      newConnection.on("BoardGenerated", (gameRoom, generatedBoard) => {
        setBoard(generatedBoard);
        console.log("Board received:", generatedBoard);
      });

      newConnection.on("RecievePlayerId", (playerId) => {
        setPlayerId(playerId);
      });

      newConnection.on("GameStateChanged", (newGameState) => {
        console.log("Game state changed to:", newGameState);
        setGameState(newGameState);
      });

      await newConnection.start();

      const gameRoom = {
        GameRoomName: gameRoomName,
        GameState: 0,
      };

      const userConnection = {
        Username: usernameInput,
        GameRoom: gameRoom,
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
          username={username}
          playerId={playerId}
          gameState={gameState}
          startGame={startGame}
        />
      )}
    </>
  );
};

const root = ReactDOM.createRoot(document.getElementById("root"));
root.render(<App />);
