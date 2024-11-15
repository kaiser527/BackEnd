const {
  getAllUsers,
  createNewUser,
  updateUserById,
  deleteUserById,
} = require("../services/CRUD");

const getHomePage = (req, res) => {
  return res.send("Backend Side");
};

const getUserPage = async (req, res) => {
  let results = await getAllUsers();
  return res.status(200).json(results);
};

const postCreateUser = async (req, res) => {
  let { email, name, city } = req.body;
  if (!email || !name || !city) {
    res.status(500).json({
      errCode: 1,
      errMessage: "Missing required paramenters !",
    });
  } else {
    await createNewUser(email, name, city);
    return res.status(200).json({
      errCode: 0,
      message: "Create user succeed !",
    });
  }
};

const putUpdateUser = async (req, res) => {
  let userId = req.params.id;
  let { email, name, city } = req.body;
  if (!email || !name || !city) {
    res.status(500).json({
      errCode: 1,
      errMessage: "Missing required paramenters !",
    });
  } else {
    await updateUserById(email, name, city, userId);
    return res.status(200).json({
      errCode: 0,
      message: `User with ID: ${userId} is updated !`,
    });
  }
};

const deleteUser = async (req, res) => {
  let userId = req.params.id;
  await deleteUserById(userId);
  return res.status(200).json({
    errCode: 0,
    message: `User with ID: ${userId} is deleted !`,
  });
};

module.exports = {
  getHomePage,
  getUserPage,
  postCreateUser,
  putUpdateUser,
  deleteUser,
};
