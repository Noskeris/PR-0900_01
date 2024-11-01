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

const AllAvatarsContainer = ({ config, changeAvatar }) => {
    
  return (
    <Paper elevation={2} sx={{ p: 2, maxHeight: 450, overflowY: "auto" }}>
      <Typography variant="h6" align="center" gutterBottom>
        Players
      </Typography>
      <AvatarComponent config={config} />
    </Paper>
  );
};

export default AllAvatarsContainer;
