var express = require('express');
var router = express.Router();


////html에 session.userid, session.isGuest, session.isOperator 세가지 변수를 같이 전달함
/* GET home page. */
router.get('/', function (req, res, next) {
  if (req.session.userid == undefined) {
    if (req.session.isGuest) { //게스트로 로그인
      res.render('mainMenu.html', {
        session: req.session
      });
    }
    else { //로그인페이지로 다시이동
      res.render('index.html', { 
        session: req.session
      });
    }
  }
  else { //id를 가지고 로그인
    res.render('mainMenu.html', {
      session: req.session
    });
  }
});


router.get('/log', function(req, res, next){
  res.render('logView.html');
  //res.redirect("/log");
});

router.get('/droneSystem', function(req, res, next) {
  if(req.session.userid == undefined){
    res.redirect("/");
  }
  else{
      res.render('droneSystem.html');
  }
});

router.get('/opSystem', function(req, res, next) {
  if(req.session.userid == undefined){
    res.redirect("/");
  }
  else{
      res.render('opSystem.html');
  }
});

router.get('/guestSystem', function(req, res, next) {
  res.render('guestSystem.html');
});


module.exports = router;
