var express = require('express');
var router = express.Router();
const models = require("../models");

router.get('/id', function(req, res, next){
    let result = await.models.control_log.findAll({
        where: { userid : req.query.userid},
        order : ['time', 'DESC']
    }).then(result => {
        res.render('logView', result);
    });
});

router.get('/all', function(req, res, next){
    let result = await.models.control_log.findAll({
        order : ['time', 'DESC']
    }).then(result => {
        res.render('logView', result);
    });
});


module.exports = router;
