import React, { useState } from 'react';
import ReactDOM from 'react-dom/client'; // Update the import for ReactDOM
import { WelcomeScreen } from './WelcomeScreen';
import { Game } from './Game/Game.js';
import { Header } from './Header';

import './css/style.css';

export const App = () => {
  const [appState, setAppState] = useState('welcome'); // play or welcome

  const startPlay = () => {
    setAppState('play');
  };

  // Renders either Welcome Screen or Game
  return (
    <>
      <Header />
      {appState === 'play' ? <Game /> : <WelcomeScreen startPlay={startPlay} />}
    </>
  );
};

// Updated to use ReactDOM.createRoot()
const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(<App />);