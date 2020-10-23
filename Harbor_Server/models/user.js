module.exports = (sequelize, DataTypes) => {
  var user = sequelize.define('user', {
    userid: {
      type: DataTypes.STRING,
      allowNull: false,
      primaryKey: true
    },
    isOperator: {
      type: DataTypes.BOOLEAN,
      allowNull: false,
    },
    password: {
      type: DataTypes.STRING,
      allowNull: false
    },
    salt:{
      type: DataTypes.STRING
    }
  });

  return user;
};