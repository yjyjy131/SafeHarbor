var express = require('express');
var router = express.Router();


/* GET home page. */
router.get('/', function (req, res, next) {
  /*
  var mysql = require('mysql');
  var connection = mysql.createConnection({
    host: '127.0.0.1',
    user: 'root',
    password: 'qwer123',
    database: 'new_schema'
  });

  connection.connect();

  connection.query('SELECT 1 + 1 AS solution', function (error, results, fields) {
    if (error) throw error;
    console.log('The solution is: ', results[0].solution);
  });

  connection.end();
  */

  res.render('index.html', {
        session : req.session
  });
});



module.exports = router;