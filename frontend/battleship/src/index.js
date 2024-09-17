import React, { useState } from 'react';
import ReactDOM from 'react-dom/client'; // Update the import for ReactDOM
import { WelcomeScreen } from './WelcomeScreen';
import { Game } from './Game/Game.js';
import { Header } from './Header';

import './css/style.css';
import WaitingRoom from './components/WaitingRoom.js';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import GameRoom from './components/GameRoom.js';

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

 const joinGameRoom = async (username, gameRoom) => {
   try {
    //initiliaze conn
    const connection = new HubConnectionBuilder()
                       .withUrl("https://localhost:7085/game")
                       .configureLogging(LogLevel.Information)
                       .build();

    // set up handlers
    connection.on("JoinSpecificGameRoom", (username, msg) => {
      setMessages((messages) => [...messages, { username, msg }]);
      console.log("msg: ", msg);
    });

    connection.on("RecieveSpecificMessage", (username, msg) => {
        setMessages(messages => [...messages, {username, msg}]);
        console.log(msg);
    });

    await connection.start();
    await connection.invoke("JoinSpecificGameRoom", {username, gameRoom});

    setConnection(connection);

   } catch (error) {
    console.log(error);
   }
 }

 const sendMessage = async(message) => {
  try {
    await connection.invoke("SendMessage", message);
  } catch (error) {
    console.log(error);
  }
 }

  return (
    <>
      <div>
        Hello !!!!!
      </div>
      {!connection 
      ? <WaitingRoom joinGameRoom={joinGameRoom}></WaitingRoom>
      : <GameRoom messages={messages} sendMessage={sendMessage}></GameRoom>
      }
    </>
  )
};

// Updated to use ReactDOM.createRoot()
const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(<App />);