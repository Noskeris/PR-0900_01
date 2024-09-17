import { useState } from 'react';
import { Button, TextField, InputAdornment, Box } from '@mui/material';

const SendMessageForm = ({ sendMessage }) => {
  const [msg, setMessage] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    sendMessage(msg);
    setMessage(''); // Clear the input field
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ mb: 3 }}>
      <TextField
        label="Chat"
        variant="outlined"
        fullWidth
        value={msg}
        onChange={(e) => setMessage(e.target.value)}
        InputProps={{
          startAdornment: (
            <InputAdornment position="start">Chat</InputAdornment>
          ),
        }}
      />
      <Button 
        variant="contained" 
        type="submit" 
        color="primary" 
        disabled={!msg} 
        sx={{ mt: 1 }}
      >
        Send
      </Button>
    </Box>
  );
};

export default SendMessageForm;