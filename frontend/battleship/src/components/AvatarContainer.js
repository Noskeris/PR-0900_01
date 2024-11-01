import React from "react";
import {
  Typography,
  Button,
  Paper,
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
      <Button
            style={{ marginBottom: '10px' }}
            variant="contained"
            color="secondary"
            onClick={() => setHeadType(config.headType === 'RoundHeaded' ? 2 : 1)}
            fullWidth
        >
            Another head
        </Button>
        
        <Button
            style={{ marginBottom: '20px' }}
            variant="contained"
            color="secondary"
            onClick={() => setAppearance(config.appearance.type === 'Hair' ? 2 : 1)}
            fullWidth
        >
            Another appearance
        </Button>
      <AvatarComponent config={config} />
    </Paper>
  );
};

export default AvatarContainer;
