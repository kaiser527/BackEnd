const {
  getAllUsers,
  createNewUser,
  updateUserById,
  deleteUserById,
} = require("../services/CRUD");

const getHomePage = (req, res) => {
  //process data
  res.send("Backend Side");
};

const getUserPage = async (req, res) => {
  let results = await getAllUsers();
  return res.json(results);
};

const postCreateUser = async (req, res) => {
  let { email, name, city, userId } = req.body;
  await createNewUser(email, name, city, userId);
  return res.send("create-user");
};

const putUpdateUser = async (req, res) => {
  let userId = req.params.id;
  let { email, name, city } = req.body;
  await updateUserById(email, name, city, userId);
  return res.send("update-user");
};

const deleteUser = async (req, res) => {
  let userId = req.params.id;
  await deleteUserById(userId);
  res.send("delete-user");
};

module.exports = {
  getHomePage,
  getUserPage,
  postCreateUser,
  putUpdateUser,
  deleteUser,
};
