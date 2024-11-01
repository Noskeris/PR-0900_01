import React from "react";
import {
  Paper,
  Typography,
  Divider,
} from "@mui/material";
import AvatarComponent from "./AvatarComponent";

const AvatarContainer = ({ config, changeAvatar }) => {
    const [headType, setHeadType] = React.useState(config.headType === 'RoundHeaded' ? 1 : 2);
    const [appearance, setAppearance] = React.useState(config.appearance.type === 'Hair' ? 2 : 1);

    React.useEffect(() => {
        changeAvatar({headType: headType, appearance: appearance});
    }, [headType, appearance]);
    
  return (
    <Paper elevation={2} sx={{ p: 2, maxHeight: 450, overflowY: "auto" }}>
      <Typography variant="h5" align="center" gutterBottom>
        Build your avatar
      </Typography>
      <Divider sx={{ mb: 1 }} />
      <AvatarComponent config={config} changeHead={setHeadType} changeAppearance={setAppearance} />
    </Paper>
  );
};

export default AvatarContainer;
