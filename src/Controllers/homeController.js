import {
  getAllUsersService,
  createNewUserService,
  updateUserService,
  deleteUserService,
} from "../services/userServices";

const getHomePage = (req, res) => {
  res.send("BackEnd Side");
};

const getAllUsers = async (req, res) => {
  let id = req.query.id;
  if (!id) {
    res.status(500).json({
      errCode: 1,
      errMessage: "Missing id!",
    });
  } else {
    let results = await getAllUsersService(id);
    res.status(200).json({
      errCode: 0,
      message: "Ok",
      results,
    });
  }
};

const postCreateNewUser = async (req, res) => {
  let { email, password, username } = req.body;
  if (!email || !password || !username) {
    res.status(500).json({
      errCode: 2,
      errMessage: "Missing required paramenter!",
    });
  } else {
    let message = await createNewUserService(req.body);
    res.status(200).json(message);
  }
};

const putUpdateUser = async (req, res) => {
  let { email, password, username } = req.body;
  if (!email || !password || !username) {
    res.status(500).json({
      errCode: 2,
      errMessage: "Missing required paramenter!",
    });
  } else {
    let message = await updateUserService(req.body);
    res.status(200).json(message);
  }
};

const handleDeleteUser = async (req, res) => {
  let id = req.query.id;
  if (!id) {
    res.status(500).json({
      errCode: 1,
      errMessage: "Missing id!",
    });
  } else {
    let message = await deleteUserService(id);
    res.status(200).json(message);
  }
};

export {
  getHomePage,
  getAllUsers,
  postCreateNewUser,
  handleDeleteUser,
  putUpdateUser,
};
