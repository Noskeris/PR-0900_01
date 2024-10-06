import React, { useState } from "react";
import { Box, Button, Typography, Stack } from "@mui/material";

const SuperAttackSelector = ({ superAttacks, onSelectAttack }) => {
  const [selectedAttack, setSelectedAttack] = useState("normal");

  const handleSelectAttack = (attack) => {
    setSelectedAttack(attack);
    onSelectAttack(attack);
  };

  return (
    <Box sx={{ mt: 2 }}>
      <Typography variant="h6" align="center">
        Select Attack Type
      </Typography>
      <Stack spacing={2} direction="row" justifyContent="center">
        {superAttacks.map((attack) => (
          <Button
            key={attack.name}
            variant={selectedAttack === attack.name ? "contained" : "outlined"}
            onClick={() => handleSelectAttack(attack.name)}
            disabled={attack.count <= 0} // Disable if no attacks left
          >
            {attack.name} ({attack.count} left)
          </Button>
        ))}
        <Button
          variant={selectedAttack === "normal" ? "contained" : "outlined"}
          onClick={() => handleSelectAttack("normal")}
        >
          Normal
        </Button>
      </Stack>
    </Box>
  );
};

export default SuperAttackSelector;