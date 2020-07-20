module.exports = (sequelize, DataTypes) => {
    var control_log = sequelize.define('control_log', {
      userid: {
        type: DataTypes.STRING,
        allowNull: false,
        primaryKey: true
      },
      speed: {
        type: DataTypes.FLOAT,
        allowNull: false,
      },
      angle: {
        type: DataTypes.FLOAT,
        allowNull: false
      },
      gpsX:{
        type: DataTypes.FLOAT,
        allowNull: false
      },      
      gpsY:{
        type: DataTypes.FLOAT,
        allowNull: false
      },   
      time:{
        type: DataTypes.DATE,
        primaryKey: true
      }
    });
    control_log.associate = function(models) {
        control_log.belongsTo(models.user, {
            foreignKey: 'userid',
        });
    };
    return control_log;
  };