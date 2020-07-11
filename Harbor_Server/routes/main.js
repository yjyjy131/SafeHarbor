var express = require('express');
var router = express.Router();

router.get('/', function (req, res, next) {
    if (req.session.userid == undefined) {
        if (req.session.isGuest) {
            res.render('mainmenu.html', {
                session: req.session
            });
        }
        else
            res.redirect("/");
    }
    else {
        res.render('mainmenu.html', {
            session: req.session
        });
    }
});

router.get('/log', function(req, res, next){
    res.redirect("/log");
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