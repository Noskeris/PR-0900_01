import { useFormik } from "formik";
import * as Yup from "yup";
import React from "react";

const WaitingRoom = ({ joinGameRoom }) => {
  const formik = useFormik({
    initialValues: {
      username: "",
      gameRoomName: "",
    },
    validationSchema: Yup.object({
      username: Yup.string().required("Username is required"),
      gameRoomName: Yup.string().required("Game room name is required"),
    }),
    onSubmit: (values) => {
      joinGameRoom(values.username, values.gameRoomName);
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
        <label htmlFor="gameRoomName">Game Room Name</label>
        <input
          id="gameRoomName"
          name="gameRoomName"
          type="text"
          onChange={formik.handleChange}
          onBlur={formik.handleBlur}
          value={formik.values.gameRoomName}
        />
        {formik.touched.gameRoomName && formik.errors.gameRoomName ? (
          <div>{formik.errors.gameRoomName}</div>
        ) : null}
      </div>

      <button type="submit">Join</button>
    </form>
  );
};

export default WaitingRoom;
