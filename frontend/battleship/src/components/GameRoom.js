import { Grid, Box, Typography, Button } from "@mui/material";
import MessageContainer from "./MessageContainer";
import BoardComponent from "./BoardComponent";
import React, { useState } from 'react';

import { PlayerFleet } from "./PlayerFleet";

const AVAILABLE_SHIPS = [
  {
    name: 'carrier',
    length: 5,
    placed: null,
  },
  {
    name: 'battleship',
    length: 4,
    placed: null,
  },
  {
    name: 'cruiser',
    length: 3,
    placed: null,
  },
  {
    name: 'submarine',
    length: 3,
    placed: null,
  },
  {
    name: 'destroyer',
    length: 2,
    placed: null,
  },
];

const GameRoom = ({
  messages,
  generateBoardAction,
  isModerator,
  board,
  setBoard,
  username,
  playerId,
  gameState,
  startGame,
}) => {
  const [currentlyPlacing, setCurrentlyPlacing] = useState(null);
  const [placedShips, setPlacedShips] = useState([]);
  const [availableShips, setAvailableShips] = useState(AVAILABLE_SHIPS);

  const selectShip = (shipName) => {
    let shipIdx = availableShips.findIndex((ship) => ship.name === shipName);
    const shipToPlace = availableShips[shipIdx];

    setCurrentlyPlacing({
      ...shipToPlace,
      orientation: 'horizontal',
      position: null,
    });
  };

  const placeShip = (currentlyPlacing) => {
    setPlacedShips([
      ...placedShips,
      {
        ...currentlyPlacing,
        placed: true,
      },
    ]);

    setAvailableShips((previousShips) =>
      previousShips.filter((ship) => ship.name !== currentlyPlacing.name)
    );

    setCurrentlyPlacing(null);
  };

  const readyToPlay = () => {
    console.log("ready to play", username);
  };

  const rotateShip = (event) => {
  if (currentlyPlacing != null && event.button === 2) {
    setCurrentlyPlacing({
      ...currentlyPlacing,
      orientation:
        currentlyPlacing.orientation === 'vertical' ? 'horizontal' : 'vertical',
    });
  }
};

  return (
    <>
    <Box sx={{ p: 5 }}>
      <Grid container spacing={2}>
         <Grid item xs={10}>
           <Typography variant="h2">GameRoom</Typography>
         </Grid>
         <Grid item xs={12}>
          <MessageContainer messages={messages} />
         </Grid>
         {gameState === 1 && (
          <Grid item xs={12}>
            <Button
              variant="outlined"
              color="secondary"
              disabled={!isModerator}
              onClick={generateBoardAction}
            >
              Generate Board
            </Button>
          </Grid>
        )}
        {gameState === 2 && (
          <>
            <Grid item xs={12}>
              <Typography>Place your ships on the board!</Typography>
            </Grid>
            {isModerator && (
              <Grid item xs={12}>
                <Button
                  variant="outlined"
                  color="primary"
                  onClick={startGame}
                >
                  Start Game
                </Button>
              </Grid>
            )}
            <PlayerFleet
              availableShips={availableShips}
              selectShip={selectShip}
              currentlyPlacing={currentlyPlacing}
              readyToPlay={readyToPlay}
            />
          </>
        )}
        {gameState === 3 && (
          <Grid item xs={12}>
            <Typography>Game in progress!</Typography>
          </Grid>
        )}
        <Grid item xs={12}>
          {board ? (
            <BoardComponent
              board={board}
              setBoard={setBoard}
              username={username}
              playerId={playerId}
            
              currentlyPlacing={currentlyPlacing}
              setCurrentlyPlacing={setCurrentlyPlacing}
              rotateShip={rotateShip}
              placeShip={placeShip}
              placedShips={placedShips}
            />
          ) : (
            <Typography>Waiting for board generation...</Typography>
          )}
        </Grid>
      </Grid>
    </Box>
    </>
  );
};

export default GameRoom;
