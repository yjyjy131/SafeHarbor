var createError = require('http-errors');
var express = require('express');
var path = require('path');
var cookieParser = require('cookie-parser');
var logger = require('morgan');

var indexRouter = require('./routes/index');
var userRouter = require('./routes/user');
//var mySocketRouter = require('./routes/mySocket');
var logRouter = require('./routes/log');
//var mainRouter = require('./routes/main');

var app = express();


//------------------------------------사전설정-----------------------------------------
// view engine setup
app.set('views', path.join(__dirname, 'views'));
app.set('view engine', 'ejs');
app.engine('html', require('ejs').renderFile);

app.use(logger('dev'));
app.use(express.json());
app.use(express.urlencoded({ extended: false }));
//app.use(cookieParser());
app.use(express.static(path.join(__dirname, 'public')));

//------------------------------------http서버생성-----------------------------------------
var debug = require('debug')('harbor-server:server');
var http = require('http');

/**
 * Get port from environment and store in Express.
 */
var port = normalizePort(process.env.PORT || '8000');
app.set('port', port);

/**
 * Create HTTP server.
 */
var server = http.createServer(app);

/**
 * Listen on provided port, on all network interfaces.
 */
server.listen(port);
server.on('error', onError);
server.on('listening', onListening);


//------------------------------------세션설정-----------------------------------------
const session = require('express-session');
app.use(session({
  secret: 'keyboard cat',
  resave: false,
  saveUninitialized: true,
  cookie: {
    maxAge: 24000 * 60 * 60 // 쿠키 유효기간 24시간
  }
}));
app.locals.session = session;


//------------------------------------소켓.io설정-----------------------------------------
var io = require('socket.io')().attach(server);
var socket_setup = require("./socket_setup");
socket_setup.attach_event(io);
app.locals.io = io;

//------------------------------------db모델설정(sequelize)-----------------------------------------
const models = require("./models/index.js");
models.sequelize.sync().then( () => {
  console.log(" DB 연결 성공");
}).catch(err => {
  console.log("연결 실패");
  console.log(err);
})


//------------------------------------라우터설정-----------------------------------------

app.use('/', indexRouter);
app.use('/user', userRouter);
//app.use('/mySocket', mySocketRouter);
app.use('/log', logRouter);
//app.use('/main', mainRouter);


//------------------------------------에러핸들링-----------------------------------------
// catch 404 and forward to error handler
app.use(function(req, res, next) {
  // set locals, only providing error in development
  console.log("주소 " + req.url + " 를 찾을 수 없습니다.");
  res.locals.message = "주소 " + req.url + " 를 찾을 수 없습니다.";
  res.locals.error = {"status" : 404, "stack" : ""};
  // render the error page
  res.status(404);
  res.render('error.html');
});

// error handler
app.use(function(err, req, res, next) {
  // set locals, only providing error in development
  res.locals.message = err.message;
  res.locals.error = req.app.get('env') === 'development' ? err : {};
  console.log(err);

  // render the error page
  res.status(err.status || 500);
  res.render('error.html');
});

//------------------------------------아래는 함수들-----------------------------------------

/**
 * Normalize a port into a number, string, or false.
 */
function normalizePort(val) {
  var port = parseInt(val, 10);

  if (isNaN(port)) {
    // named pipe
    return val;
  }

  if (port >= 0) {
    // port number
    return port;
  }

  return false;
}

/**
 * Event listener for HTTP server "error" event.
 */
function onError(error) {
  if (error.syscall !== 'listen') {
    throw error;
  }

  var bind = typeof port === 'string'
    ? 'Pipe ' + port
    : 'Port ' + port;

  // handle specific listen errors with friendly messages
  switch (error.code) {
    case 'EACCES':
      console.error(bind + ' requires elevated privileges');
      process.exit(1);
      break;
    case 'EADDRINUSE':
      console.error(bind + ' is already in use');
      process.exit(1);
      break;
    default:
      throw error;
  }
}

/**
 * Event listener for HTTP server "listening" event.
 */
function onListening() {
  var addr = server.address();
  var bind = typeof addr === 'string'
    ? 'pipe ' + addr
    : 'port ' + addr.port;
  debug('Listening on ' + bind);
}


//logView.html - Ajax 처리  
app.get('/api/get', function(req, res){
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
})


module.exports = app;
