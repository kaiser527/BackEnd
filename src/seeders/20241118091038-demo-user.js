"use strict";

/** @type {import('sequelize-cli').Migration} */
module.exports = {
  async up(queryInterface, Sequelize) {
    /**
     * Add seed commands here.
     *
     * Example:
     * await queryInterface.bulkInsert('People', [{
     *   name: 'John Doe',
     *   isBetaMember: false
     * }], {});
     */
    await queryInterface.bulkInsert(
      "Users",
      [
        {
          email: "seed1@gmail.com",
          password: "123456",
          username: "Seeder 1",
          createdAt: "2024/3/2",
          updatedAt: "2024/5/4",
        },
        {
          email: "seed2@gmail.com",
          password: "123456",
          username: "Seeder 2",
          createdAt: "2024/3/2",
          updatedAt: "2024/5/4",
        },
        {
          email: "seed3@gmail.com",
          password: "123456",
          username: "Seeder 3",
          createdAt: "2024/3/2",
          updatedAt: "2024/5/4",
        },
      ],
      {}
    );
  },

  async down(queryInterface, Sequelize) {
    /**
     * Add commands to revert seed here.
     *
     * Example:
     * await queryInterface.bulkDelete('People', null, {});
     */
  },
};
