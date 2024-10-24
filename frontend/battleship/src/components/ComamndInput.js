import React, { useState } from "react";
import { TextField, Button, Paper, Stack } from "@mui/material";

const CommandInput = ({ sendCommand }) => {
  const [command, setCommand] = useState("");

  const handleCommandSubmit = async () => {
    if (command) {
      try {
        await sendCommand(command);
        setCommand(""); 
      } catch (error) {
        console.error("Command submission failed: ", error);
      }
    }
  };

  return (
    <Paper elevation={3} sx={{ p: 2 }}>
      <Stack spacing={2}>
        <TextField
          label="Enter Command"
          variant="outlined"
          fullWidth
          value={command}
          onChange={(e) => setCommand(e.target.value)}
          onKeyPress={(e) => {
            if (e.key === "Enter") {
              handleCommandSubmit();
            }
          }}
        />
        <Button variant="contained" onClick={handleCommandSubmit}>
          Send Command
        </Button>
      </Stack>
    </Paper>
  );
};

export default CommandInput;
