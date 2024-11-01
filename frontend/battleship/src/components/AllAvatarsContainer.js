import React from "react";
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
import AvatarComponent from "./AvatarComponent";

const AllAvatarsContainer = ({ avatars }) => {
    console.log(avatars);
  return (
    <Paper elevation={2} sx={{ p: 2, maxHeight: 450, overflowY: "auto" }}>
      <Typography variant="h6" align="center" gutterBottom>
        Players
      </Typography>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: '16px' }}>
          {avatars.map((avatar, index) => (
            <>
              <AvatarComponent key={index} config={avatar.avatar} />
              <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
                <Typography 
                  variant="body1" 
                  align="center" 
                  gutterBottom 
                  style={{ textDecoration: avatar.canPlay === false ? 'line-through' : 'none' }}>
                  {avatar.nickname}
                </Typography>
              </div>
            </>
          ))}
        </div>
    </Paper>
  );
};

export default AllAvatarsContainer;
