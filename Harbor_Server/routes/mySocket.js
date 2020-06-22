var express = require('express');
var router = express.Router();



/* 소켓 통신 라우터 */
router.get('/', function(req, res, next) {
  console.log(req.session.isOperator);
  if(req.session.userid == undefined){
    res.redirect("/");
  }
  else{
    if(req.session.isOperator)
      res.render('opSystem.html');
    else
      res.render('droneSystem.html');
  }
});
module.exports = router;
