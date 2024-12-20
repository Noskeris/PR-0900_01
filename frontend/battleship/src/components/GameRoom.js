import React, { useEffect, useState } from "react";
import {
  Grid,
  Box,
  Typography,
  Button,
  Paper,
  Divider,
  Stack,
  Avatar,
} from "@mui/material";
import MessageContainer from "./MessageContainer";
import BoardComponent from "./BoardComponent";
import { PlayerFleet } from "./PlayerFleet";
import SuperAttackSelector from "./SuperAttackSelector";
import GameModeSelector from "./GameModeSelector";
import CommandInput from "./ComamndInput";
import AvatarContainer from "./AvatarContainer";
import AllAvatarsContainer from "./AllAvatarsContainer";

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
  timer,
  setTimer,
  turnEndTime,
  playerTurnTimeEnded,
  superAttacks,
  isPlayerReady,
  confirmGameMode,
  sendCommand,
  playerAvatarConfigs,
  changeAvatar,
  allAvatars
}) => {
  const [currentlyPlacing, setCurrentlyPlacing] = useState(null);
  const [availableShips, setAvailableShips] = useState(shipsToPlace);
  const [attackType, setAttackType] = useState("normal");

  useEffect(() => {
    setAvailableShips(shipsToPlace);
  }, [shipsToPlace]);

  useEffect(() => {
    if (turnEndTime && turnEndTime !== -1) {
      const interval = setInterval(() => {
        const timeLeft = Math.max(0, (turnEndTime - new Date()) / 1000);
        setTimer(timeLeft.toFixed(1));
        if (timeLeft <= 0) {
          clearInterval(interval);
          if (playerId === playerTurn) {
            playerTurnTimeEnded();
          }
        }
      }, 100);
      return () => clearInterval(interval);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [turnEndTime]);

  useEffect(() => {
    if (playerTurn === playerId) {
      setAttackType("normal");
    }
  }, [playerTurn, playerId]);

  const handleSelectAttack = (attackName) => {
    setAttackType(attackName);
  };

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
    <>
    <Box
      sx={{
        p: 5,
        width: "70vw",
        overflowX: "hidden",
      }}
    >
      <Grid container spacing={2}>
        {/* Left Column: Game Board */}
        <Grid item xs={12} md={8}>
          <Paper
            elevation={3}
            sx={{
              p: 2,
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
            }}
          >
            <Typography variant="h4" align="center" gutterBottom>
              Game Room
            </Typography>
            <Divider sx={{ mb: 2 }} />

            {gameState === 3 && (
              <>
                <Typography variant="h6" color="primary" gutterBottom>
                  {playerTurn === playerId ? "Your Turn" : "Opponent's Turn"}
                </Typography>
                <Typography variant="h6" align="center" color="error">
                  {turnEndTime !== -1 ?
                  playerTurn === playerId
                  ? `Time left for turn: ${timer}`
                  : "Waiting for opponent..." 
                  : ""}

                </Typography>
                <SuperAttackSelector
                  superAttacks={superAttacks}
                  attackType={attackType}
                  onSelectAttack={handleSelectAttack}
                />
              </>
            )}

            {board ? (
              <Box sx={{ display: "flex", justifyContent: "center" }}>
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
                  attackType={attackType}
                />
              </Box>
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
            (gameState === 4 && isModerator) ||
            (gameState === 5 && isModerator) ? (
              <Paper elevation={3} sx={{ p: 2 }}>
                <Stack spacing={2} alignItems="center">
                  {gameState === 1 && isModerator && (
                    <>
                      <h1>Select Game Mode</h1>
                      <GameModeSelector confirmGameMode={confirmGameMode} />
                    </>
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
                  {gameState === 5 && isModerator && (
                    <Button
                      variant="contained"
                      color="primary"
                      onClick={generateBoardAction}
                      fullWidth
                    >
                      Generate Board
                    </Button>
                  )}
                </Stack>
              </Paper>
            ) : null}

            {/* PlayerFleet Box */}
            {gameState === 2 && (
              <>
              <Paper elevation={3} sx={{ p: 2 }}>
                <Typography variant="h6" align="center" gutterBottom>
                  Place your ships
                </Typography>
                <PlayerFleet
                  availableShips={availableShips}
                  selectShip={selectShip}
                  currentlyPlacing={currentlyPlacing}
                  playerReady={playerReady}
                  isPlayerReady={isPlayerReady}
                />
              </Paper>
              {playerAvatarConfigs && <AvatarContainer config={playerAvatarConfigs} changeAvatar={changeAvatar} />}
              </>
            )}

            {(gameState === 3 || gameState === 4) && (
              <>
              {allAvatars && <AllAvatarsContainer avatars={allAvatars} />}
              </>
            )}

            {/* Messages Box */}
            <MessageContainer messages={messages} />
          </Stack>
        </Grid>
      </Grid>
    </Box>
    {(gameState !== 3 && gameState !== 4) && (
      <CommandInput sendCommand={sendCommand} />
    )}
    </>
  );
};

export default GameRoom;
