const connection = require("../config/database");

const getAllUsers = async () => {
  let [results, fields] = await connection.query("select * from Users");
  return results;
};

const createNewUser = async (email, name, city) => {
  const [results, fields] = await connection.query(
    `insert into Users(email, name, city)
    values (?, ?, ?)
    `,
    [email, name, city]
  );
  return results;
};

const updateUserById = async (email, name, city, userId) => {
  const [results, fields] = await connection.query(
    `update Users
    set email=?, name=?, city=?
    where id=?
    `,
    [email, name, city, userId]
  );
};

const deleteUserById = async (userId) => {
  const [results, fields] = await connection.query(
    `delete from Users
    where id=?
    `,
    [userId]
  );
};

module.exports = {
  getAllUsers,
  createNewUser,
  deleteUserById,
  updateUserById,
};
