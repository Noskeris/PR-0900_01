import React, { useState } from 'react';
import { Button, Grid } from '@mui/material';
import { GameModeTypes } from '../constants/GameModeTypes';

const GameModeSelector = ({ confirmGameMode }) => {
  const [selectedMode, setSelectedMode] = useState(null);

  const handleModeSelect = (mode) => {
    setSelectedMode(mode);
  };

  const handleConfirm = () => {
    if (selectedMode) {
      const selectedModeValue = GameModeTypes[selectedMode];
      confirmGameMode(selectedModeValue); 
    } else {
      alert('Please select a game mode before confirming.');
    }
  };

  return (
    <div>
      <Grid container spacing={2}>
        <Grid item xs={4}>
          <Button
            variant={selectedMode === 'Normal' ? 'contained' : 'outlined'}
            color="primary"
            fullWidth
            onClick={() => handleModeSelect('Normal')}
          >
            Normal
          </Button>
        </Grid>
        <Grid item xs={4}>
          <Button
            variant={selectedMode === 'Rapid' ? 'contained' : 'outlined'}
            color="primary"
            fullWidth
            onClick={() => handleModeSelect('Rapid')}
          >
            Rapid
          </Button>
        </Grid>
        <Grid item xs={4}>
          <Button
            variant={selectedMode === 'Mobility' ? 'contained' : 'outlined'}
            color="primary"
            fullWidth
            onClick={() => handleModeSelect('Mobility')}
          >
            Mobility
          </Button>
        </Grid>
      </Grid>
      <Button
        variant="contained"
        color="secondary"
        onClick={handleConfirm}
        fullWidth
        style={{ marginTop: '20px' }}
      >
        Confirm Game Mode
      </Button>
    </div>
  );
};

export default GameModeSelector;
