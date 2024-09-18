import React, { useState } from "react";
import ReactDOM from "react-dom/client"; // Update the import for ReactDOM
import { WelcomeScreen } from "./WelcomeScreen";
import { Game } from "./Game/Game.js";
import { Header } from "./Header";

import "./css/style.css";
import WaitingRoom from "./components/WaitingRoom.js";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import GameRoom from "./components/GameRoom.js";

export const App = () => {
  //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
  //FIRST IMPLEMENETATION OF GAME
  //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

  // const [appState, setAppState] = useState('welcome'); // play or welcome

  // const startPlay = () => {
  //   setAppState('play');
  // };

  // // Renders either Welcome Screen or Game
  // return (
  //   <>
  //     <Header />
  //     {appState === 'play' ? <Game /> : <WelcomeScreen startPlay={startPlay} />}
  //   </>
  // );

  //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
  // current implementation of signalr still in progress
  //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

  const [connection, setConnection] = useState();
  const [messages, setMessages] = useState([]);
  const [isModerator, setIsModerator] = useState(false);
  const [board, setBoard] = useState(null);
  const [username, setUsername] = useState("");
  const [playerId, setPlayerId] = useState();

  const joinGameRoom = async (username, gameRoom) => {
    try {
      const connection = new HubConnectionBuilder()
        .withUrl("https://localhost:7085/game")
        .configureLogging(LogLevel.Information)
        .build();

      connection.on("JoinSpecificGameRoom", (username, msg) => {
        setMessages((messages) => [...messages, { username, msg }]);
        console.log("msg: ", msg);
      });

      connection.on("RecieveSpecificMessage", (username, msg) => {
        setMessages((messages) => [...messages, { username, msg }]);
        console.log(msg);
      });

      connection.on("SetModerator", (isModerator) => {
        setIsModerator(isModerator);
      });

      connection.on("BoardGenerated", (gameRoom, generatedBoard) => {
        setBoard(generatedBoard);
        console.log("Board received:", generatedBoard);
      });

      connection.on("RecievePlayerId", (playerId) => {
        setPlayerId(playerId);
      });

      await connection.start();
      await connection.invoke("JoinSpecificGameRoom", { username, gameRoom });

      setConnection(connection);
      setUsername(username);
    } catch (error) {
      console.log(error);
    }
  };

  const generateBoardAction = async () => {
    try {
      await connection.invoke("GenerateBoard");
      console.log("UI generate board");
    } catch (error) {
      console.log(error);
    }
  };

  return (
    <>
      <div>Hello !!!!!</div>
      {!connection ? (
        <WaitingRoom joinGameRoom={joinGameRoom}></WaitingRoom>
      ) : (
        <GameRoom
          messages={messages}
          generateBoardAction={generateBoardAction}
          isModerator={isModerator}
          board={board}
          username={username}
          playerId={playerId}
        />
      )}
    </>
  );
};

const root = ReactDOM.createRoot(document.getElementById("root"));
root.render(<App />);
