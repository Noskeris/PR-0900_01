import React from "react";
import {
  Paper,
  Typography,
  Divider,
  List,
  ListItem,
  ListItemText,
} from "@mui/material";

const MessageContainer = ({ messages }) => {
  return (
    <Paper elevation={2} sx={{ p: 2, maxHeight: 400, overflowY: "auto" }}>
      <Typography variant="h5" align="center" gutterBottom>
        Messages
      </Typography>
      <Divider sx={{ mb: 1 }} />
      <List>
        {messages && messages.length > 0 ? (
          messages.map((msg, index) => (
            <ListItem key={index} alignItems="flex-start">
              <ListItemText
                primary={msg.msg}
                secondary={`- ${msg.username}`}
              />
            </ListItem>
          ))
        ) : (
          <ListItem>
            <ListItemText primary="No messages available" />
          </ListItem>
        )}
      </List>
    </Paper>
  );
};

export default MessageContainer;
