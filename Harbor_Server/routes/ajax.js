const models = require("../models");

exports.getSignUp = function(req, res) {
    var result = models.user;
    result.findAll({
      where: { userid: req.query.userid }
    }).then(result => {
       res.send(false);
      
    }).catch(function (err) {
      res.send(true);
    })
}

exports.getLogDatas = function(req, res) {
    let datas = req.query.data;
    var result = models.control_log;
   
    if ( datas == 0) { // All
      result.findAll({
      }).then(result => {
          res.send(result);
      });
    } else if ( datas == 1) { // UserName
      result.findAll({
        order: [
          ['userid', 'ASC']
      ]
      }).then(result => {
          res.send(result);
      });
    } else if ( datas == 2) { // Time
      result.findAll({
        order: [
          ['userid', 'DESC']
      ]
      }).then(result => {
          res.send(result);
      });
    } else { // default
      var result = models.control_log.findAll({
      }).then(result => {
          res.send(result);
      });
    }
}


