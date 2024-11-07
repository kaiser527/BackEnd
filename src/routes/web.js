const express = require("express"); //import express
const router = express.Router(); //khai bao router
const {
  getHomePage,
  getUserPage,
  postCreateUser,
  putUpdateUser,
  deleteUser,
} = require("../controllers/homeController");

router.get("/", getHomePage);

router.get("/users", getUserPage);

router.post("/create-user", postCreateUser);

router.put("/update-user/:id", putUpdateUser);

router.delete("/delete-user/:id", deleteUser);

module.exports = router;
