var express = require('express');
var router = express.Router();


/* 이 라우터는 이제 안씀. main.js를 사용할 것.*/

/* 소켓 통신 라우터 */

router.get('/droneSystem', function(req, res, next) {
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

router.get('/opSystem', function(req, res, next) {
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

router.get('/guestSystem', function(req, res, next) {
  res.render('guestSystem.html');
});
module.exports = router;
