import { useFormik } from 'formik';
import * as Yup from 'yup'; // For validation (optional)
import React from 'react';

const WaitingRoom = ({ joinGameRoom }) => {

  // Set up Formik
  const formik = useFormik({
    initialValues: {
      username: '',
      gameRoom: '',
    },
    validationSchema: Yup.object({
      username: Yup.string()
        .required('Username is required'),
      gameRoom: Yup.string()
        .required('Game room is required'),
    }),
    onSubmit: (values) => {
      // Call the joinGameRoom function with form values
      joinGameRoom(values.username, values.gameRoom);
    },
  });

  return (
    <form onSubmit={formik.handleSubmit}>
      <div>
        <label htmlFor="username">Username</label>
        <input
          id="username"
          name="username"
          type="text"
          onChange={formik.handleChange}
          onBlur={formik.handleBlur}
          value={formik.values.username}
        />
        {formik.touched.username && formik.errors.username ? (
          <div>{formik.errors.username}</div>
        ) : null}
      </div>

      <div>
        <label htmlFor="gameRoom">Game Room</label>
        <input
          id="gameRoom"
          name="gameRoom"
          type="text"
          onChange={formik.handleChange}
          onBlur={formik.handleBlur}
          value={formik.values.gameRoom}
        />
        {formik.touched.gameRoom && formik.errors.gameRoom ? (
          <div>{formik.errors.gameRoom}</div>
        ) : null}
      </div>

      <button type="submit">Join</button>
    </form>
  );
};

export default WaitingRoom;