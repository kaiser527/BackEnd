import db from "../models/index";

const getAllUsersService = (userId) => {
  return new Promise(async (resolve, reject) => {
    try {
      let users = {};
      if (userId === "all") {
        users = await db.User.findAll();
      } else {
        users = await db.User.findOne({
          where: { id: userId },
        });
      }
      resolve(users);
    } catch (e) {
      reject(e);
    }
  });
};

const createNewUserService = (data) => {
  return new Promise(async (resolve, reject) => {
    try {
      let check = await checkUserEmail(data.email);
      if (check) {
        resolve({
          errCode: -1,
          errMessage: "User is already exist!",
        });
      } else {
        await db.User.create({
          email: data.email,
          password: data.password,
          username: data.username,
        });
        resolve({
          errCode: 0,
          message: "Create new user succeed!",
        });
      }
    } catch (e) {
      reject(e);
    }
  });
};

const checkUserEmail = (userEmail) => {
  return new Promise(async (resolve, reject) => {
    try {
      let user = await db.User.findOne({
        where: { email: userEmail },
      });
      if (user) resolve(true);
      else resolve(false);
    } catch (e) {
      reject(e);
    }
  });
};

const updateUserService = (data) => {
  return new Promise(async (resolve, reject) => {
    try {
      let user = await db.User.findOne({
        where: { id: data.id },
      });
      if (!data.id) {
        resolve({
          errCode: 1,
          errMessage: "Missing id!",
        });
      } else {
        let check = await checkUserEmail(data.email);
        if (check) {
          resolve({
            errCode: -1,
            errMessage: "User is already exist!",
          });
        } else {
          if (user) {
            user.email = data.email;
            user.password = data.password;
            user.username = data.username;
            await user.save();
            resolve({
              errCode: 0,
              message: "Update user succeed!",
            });
          } else {
            resolve({
              errCode: -2,
              errMessage: "User not found!",
            });
          }
        }
      }
    } catch (e) {
      reject(e);
    }
  });
};

const deleteUserService = (userId) => {
  return new Promise(async (resolve, reject) => {
    try {
      let user = await db.User.findOne({
        where: { id: userId },
      });
      if (user) {
        await user.destroy();
        resolve({
          errCode: 0,
          message: "Delete user succeed!",
        });
      } else {
        resolve({
          errCode: -2,
          errMessage: "User not found!",
        });
      }
    } catch (e) {
      reject(e);
    }
  });
};

export {
  getAllUsersService,
  createNewUserService,
  updateUserService,
  deleteUserService,
};
