import React from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { TextField, Button, Box, Typography, Grid, Paper } from '@mui/material';

const WaitingRoom = ({ joinGameRoom }) => {
  const formik = useFormik({
    initialValues: {
      username: '',
      gameRoomName: '',
    },
    validationSchema: Yup.object({
      username: Yup.string().required('Username is required'),
      gameRoomName: Yup.string().required('Game room name is required'),
    }),
    onSubmit: (values) => {
      joinGameRoom(values.username, values.gameRoomName);
    },
  });

  return (
      <Paper elevation={3} sx={{ padding: 4, maxWidth: 400, width: '100%' }}>
        <Typography variant="h4" component="h1" gutterBottom align="center">
          Join a Game Room
        </Typography>
        <form onSubmit={formik.handleSubmit}>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                id="username"
                name="username"
                label="Username"
                variant="outlined"
                value={formik.values.username}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                error={
                  formik.touched.username && Boolean(formik.errors.username)
                }
                helperText={formik.touched.username && formik.errors.username}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                id="gameRoomName"
                name="gameRoomName"
                label="Game Room Name"
                variant="outlined"
                value={formik.values.gameRoomName}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                error={
                  formik.touched.gameRoomName &&
                  Boolean(formik.errors.gameRoomName)
                }
                helperText={
                  formik.touched.gameRoomName && formik.errors.gameRoomName
                }
              />
            </Grid>
            <Grid item xs={12}>
              <Button
                color="primary"
                variant="contained"
                fullWidth
                type="submit"
              >
                Join
              </Button>
            </Grid>
          </Grid>
        </form>
      </Paper>
  );
};

export default WaitingRoom;
