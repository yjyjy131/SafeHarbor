module.exports = {
  up: (queryInterface, Sequelize) => {
    return queryInterface.createTable('users', {
      userid: {
        type: Sequelize.STRING,
        allowNull: false,
        primaryKey: true
      },
      isOperator: {
        type: DataTypes.BOOLEAN,
        allowNull: false,
      },
      password: {
        type: Sequelize.STRING,
        allowNull: false
      },
      salt:{
        type: Sequelize.STRING
      },
      createdAt: {
        allowNull: false,
        type: Sequelize.DATE
      },
      updatedAt: {
        allowNull: false,
        type: Sequelize.DATE
      }
    });
  },
  down: (queryInterface, Sequelize) => {
    return queryInterface.dropTable('users');
  }
};