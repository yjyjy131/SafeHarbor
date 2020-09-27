const express = require('express');
const crypto = require('crypto');
const router = express.Router();
const models = require("../models");
const { DH_UNABLE_TO_CHECK_GENERATOR } = require('constants');

/* id 설정관련 라우터 */

router.get('/sign_up', function(req, res, next) {
  res.render("signup.html");
});

router.post("/sign_up", async function(req,res,next){
  let body = req.body;

  let inputPassword = body.password; 
  let salt = Math.round((new Date().valueOf() * Math.random())) + ""; 
  let hashPassword = crypto.createHash("sha512").update(inputPassword + salt).digest("hex"); 
 
  models.user.create({
    userid: body.userid,
    isOperator: body.isOperator == undefined ? false : true,
    password: hashPassword,
    salt: salt
  })
  .then( result => {
    res.redirect("/endSignup");
  })
  .catch( err => {
    console.log(err);
  })
})

// 로그인 POST
router.post("/login", async function (req, res, next) {
  console.log(req.password);

  let body = req.body;
  if (body.isGuest) {
    req.session.isGuest = body.isGuest;
    req.redirect('/');
    return;
  }
  else {
    let result = await models.user.findOne({
      where: {
        userid: body.userid
      }
    });

    if(result == undefined){
      console.log("계정 없음");
      res.redirect("/");
      return;
    }

    let dbPassword = result.dataValues.password;
    let inputPassword = body.password;
    let salt = result.dataValues.salt;
    let hashPassword = crypto.createHash("sha512").update(inputPassword + salt).digest("hex");
    if (dbPassword === hashPassword) {
      console.log("비밀번호 일치");
      // 세션 설정
      req.session.userid = body.userid;
      req.session.isOperator = body.isOperator;
      req.session.isGuest = body.isGuest;
      res.redirect('/');
    }
    else {
      console.log("비밀번호 불일치");
      res.redirect("/");
    }
  }
});

// 로그아웃
router.get("/logout", function(req,res,next){
  console.log('로그아웃 처리~');
  console.log(req.session);
  req.session.destroy();
  res.clearCookie('userid');

  res.redirect("/");
})


/*

router.post("/sign_up", async function(req,res,next){
    let body = req.body;

    let inputPassword = body.password;
    let salt = Math.round((new Date().valueOf() * Math.random())) + "";
    let hashPassword = crypto.createHash("sha512").update(inputPassword + salt).digest("hex");

    let result = models.user.create({
        name: body.userName,
        email: body.userEmail,
        password: hashPassword,
        salt: salt
    })

    res.redirect("/user/sign_up");
})

// 메인 페이지
router.get('/', function(req, res, next) {
    res.send('환영합니다~');
});

// 로그인 GET
router.get('/login', function(req, res, next) {
    res.render("user/login");
});

// 로그인 POST
router.post("/login", async function(req,res,next){
    let body = req.body;

    let result = await models.user.findOne({
        where: {
            email : body.userEmail
        }
    });

    let dbPassword = result.dataValues.password;
    let inputPassword = body.password;
    let salt = result.dataValues.salt;
    let hashPassword = crypto.createHash("sha512").update(inputPassword + salt).digest("hex");

    if(dbPassword === hashPassword){
        console.log("비밀번호 일치");
        res.redirect("/user");
    }
    else{
        console.log("비밀번호 불일치");
        res.redirect("/user/login");
    }
});
*/
module.exports = router;