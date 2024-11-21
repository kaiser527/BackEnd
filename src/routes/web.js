import express from "express";
import {
  getHomePage,
  getAllUsers,
  postCreateNewUser,
  putUpdateUser,
  handleDeleteUser,
} from "../Controllers/homeController";

const router = express.Router();

/**
 *
 * @param {*} app : express app
 */

const initWebRoutes = (app) => {
  //path, handler
  router.get("/", getHomePage);
  router.get("/users", getAllUsers);
  router.post("/create", postCreateNewUser);
  router.put("/update", putUpdateUser);
  router.delete("/delete", handleDeleteUser);

  return app.use("/", router);
};

export default initWebRoutes;
