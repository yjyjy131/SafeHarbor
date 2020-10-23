var http = require('http');
var socketio = require('socket.io');

var server = http.createServer(function(request,response){ 

    response.writeHead(200,{'Content-Type':'text/html'});
response.end("Hello node.js!!");
}).listen(8080, function(){ 
    console.log('Server is running...');
});

//랜덤 값 출력
function getRandomInt(min, max) { //min ~ max 사이의 임의의 정수 반환
    return Math.floor(Math.random() * (max - min)) + min;
}

//Number to String
function hexColour(c) {
  if (c < 256) {
    return Math.abs(c).toString(16);
  }
  return 0;
}

// 소켓 서버를 생성한다. 
var io = socketio.listen(server);
io.sockets.on('connection', function (socket){
	var myTimer;
	console.log('Socket ID : ' + socket.id + ', Connect');

	socket.on('operator gps stream', function(data){
		console.log('ClientType: '+data.ClientType+'\ngpsX: '+data.gpsX+'\ngpsY: '+data.gpsY+'\nlocation: '+data.location+'\ntime: '+data.time+'\nrotation: '+data.rotation);
		var message = {
			server : '123',
			data : '456'
		};
		socket.emit('serverMessage', message);
		console.log('전송완료');
	});
	socket.on('drone data stream', function(data){
		console.log('ClientType: '+data.ClientType+'\ngpsX: '+data.gpsX+'\ngpsY: '+data.gpsY+'\nspeed: '+data.speed+'\nangle: '+data.angle);
	
	});

	socket.on('request control stream', function(data){
		var control=new Object();
		control.speed=hexColour(getRandomInt(0,4));
		control.angle=hexColour(getRandomInt(0,5));
		var controlJSON=JSON.stringify(control);
		console.log('안드로이드로 데이터 전송!\n'+'speed: '+control.speed+'\nangle: '+control.angle);
		socket.emit('control stream', controlJSON);
	});


	socket.on('disconnect',function(data){
		console.log('접속종료');
		socket.disconnect();
	});

});
