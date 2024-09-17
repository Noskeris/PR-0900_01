import { Grid, Box, Typography } from '@mui/material';
import MessageContainer from './MessageContainer';
import SendMessageForm from './SendMessageForm';

const GameRoom = ({ messages, sendMessage }) => {
  return (
    <Box sx={{ p: 5 }}>
      <Grid container spacing={2}>
        <Grid item xs={10}>
          <Typography variant="h2">GameRoom</Typography>
        </Grid>
        <Grid item xs={12}>
          <MessageContainer messages={messages} />
        </Grid>
        <Grid item xs={12}>
         <SendMessageForm sendMessage={sendMessage}></SendMessageForm>
        </Grid>
      </Grid>
    </Box>
  );
};

export default GameRoom;
