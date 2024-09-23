import React, { useEffect, useState } from "react";
import {
  Grid,
  Box,
  Typography,
  Button,
  Paper,
  Divider,
  Stack,
} from "@mui/material";
import MessageContainer from "./MessageContainer";
import BoardComponent from "./BoardComponent";
import { PlayerFleet } from "./PlayerFleet";

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
  shipsToPlace,
  addShip,
  playerReady,
  playerTurn,
  attackCell,
  restartGame,
}) => {
  const [currentlyPlacing, setCurrentlyPlacing] = useState(null);
  const [availableShips, setAvailableShips] = useState(shipsToPlace);

  useEffect(() => {
    setAvailableShips(shipsToPlace);
  }, [shipsToPlace]);

  const selectShip = (shipName) => {
    let shipIdx = availableShips.findIndex((ship) => ship.name === shipName);
    const shipToPlace = availableShips[shipIdx];

    setCurrentlyPlacing({
      ...shipToPlace,
      orientation: "horizontal",
      position: null,
    });
  };

  const placeShip = (currentlyPlacing) => {
    setAvailableShips((previousShips) =>
      previousShips.filter((ship) => ship.name !== currentlyPlacing.name)
    );

    setCurrentlyPlacing(null);
  };

  const rotateShip = (event) => {
    if (currentlyPlacing != null && event.button === 2) {
      setCurrentlyPlacing({
        ...currentlyPlacing,
        orientation:
          currentlyPlacing.orientation === "vertical"
            ? "horizontal"
            : "vertical",
      });
    }
  };

  return (
    <Box
      sx={{
        p: 5,
        width: "103%",
        overflowX: "hidden", // Prevent horizontal scrolling
      }}
    >
      <Grid container spacing={2}>
        {/* Left Column: Game Board */}
        <Grid item xs={12} md={8}>
          <Paper elevation={3} sx={{ p: 2 }}>
            <Typography variant="h4" align="center" gutterBottom>
              Game Room
            </Typography>
            <Divider sx={{ mb: 2 }} />
            {board ? (
              <BoardComponent
                board={board}
                setBoard={setBoard}
                username={username}
                playerId={playerId}
                currentlyPlacing={currentlyPlacing}
                setCurrentlyPlacing={setCurrentlyPlacing}
                rotateShip={rotateShip}
                addShip={addShip}
                gameState={gameState}
                placeShip={placeShip}
                playerTurn={playerTurn}
                attackCell={attackCell}
              />
            ) : (
              <Typography variant="h6" align="center">
                Waiting for board generation...
              </Typography>
            )}
          </Paper>
        </Grid>

        {/* Right Column: Controls, PlayerFleet, and Messages */}
        <Grid item xs={12} md={4}>
          <Stack spacing={2}>
            {/* Conditionally render Controls Box */}
            {(gameState === 1 && isModerator) ||
            (gameState === 2 && isModerator) ||
            (gameState === 4 && isModerator) ? (
              <Paper elevation={3} sx={{ p: 2 }}>
                <Stack spacing={2} alignItems="center">
                  {gameState === 1 && isModerator && (
                    <Button
                      variant="contained"
                      color="primary"
                      onClick={generateBoardAction}
                      fullWidth
                    >
                      Generate Board
                    </Button>
                  )}
                  {gameState === 2 && isModerator && (
                    <Button
                      variant="contained"
                      color="primary"
                      onClick={startGame}
                      fullWidth
                    >
                      Start Game
                    </Button>
                  )}
                  {gameState === 4 && isModerator && (
                    <Button
                      variant="contained"
                      color="secondary"
                      onClick={restartGame}
                      fullWidth
                    >
                      Restart Game
                    </Button>
                  )}
                </Stack>
              </Paper>
            ) : null}

            {/* PlayerFleet Box */}
            {gameState === 2 && (
              <Paper elevation={3} sx={{ p: 2 }}>
                <Typography variant="h6" align="center" gutterBottom>
                  Place your ships
                </Typography>
                <PlayerFleet
                  availableShips={availableShips}
                  selectShip={selectShip}
                  currentlyPlacing={currentlyPlacing}
                  playerReady={playerReady}
                />
              </Paper>
            )}

            {/* Messages Box */}
            <MessageContainer messages={messages} />
          </Stack>
        </Grid>
      </Grid>
    </Box>
  );
};

export default GameRoom;
