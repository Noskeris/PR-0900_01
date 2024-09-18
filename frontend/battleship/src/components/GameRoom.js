import { Grid, Box, Typography, Button } from "@mui/material";
import MessageContainer from "./MessageContainer";
import BoardComponent from "./BoardComponent";

const GameRoom = ({
  messages,
  generateBoardAction,
  isModerator,
  board,
  username,
  playerId,
}) => {
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
          <Button
            variant="outlined"
            color="secondary"
            disabled={!isModerator}
            onClick={generateBoardAction}
          >
            Generate Board
          </Button>
        </Grid>
        <Grid item xs={12}>
          {board ? (
            <BoardComponent
              board={board}
              username={username}
              playerId={playerId}
            />
          ) : (
            <Typography>Waiting for board generation...</Typography>
          )}
        </Grid>
      </Grid>
    </Box>
  );
};

export default GameRoom;
