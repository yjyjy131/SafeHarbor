var socket = require('socket.io-client')('ws://ksyksy12.iptime.org:33337');
//var socket = notSocket.socket;
//var socket = io.connect("http://ksyksy12.iptime.org:33337"); 

//var socket = io.connect('localhost:8000'); 
socket.on('connect', function(){
  console.log("connected");
});
socket.on('disconnect', function(){
  console.log("disconnected");
});

socket.on('news', function (data) { 
  console.log(data.serverData);
}); 

// userid ??? db or websocket.id?
socket.emit('client connected', 
{ clientData : '드론 컨트롤러 접속', clientType : 'ctw', userid : 'test'}); 

socket.emit('control stream', { gear: 1 , angle : 1, controlTime: 1 });

const SerialPort = require('serialport');
const Readline = require('@serialport/parser-readline');
const sp = new SerialPort("COM5", { baudRate:9600, autoOpen:false});

const parser = sp.pipe(new Readline({ delimiter: '\r\n' }))

function timeDelay(timeout) {
	return new Promise((resolve) => {
		setTimeout(resolve, timeout);
	});
}

sp.open(function() {
  timeDelay(2000);
  
	parser.on("error", function(error) {
		console.log("Error : ", error.message);
  });

  parser.on('data',function (data) {
    var str = data;
    console.log('Read and Send Data : ' + data);
    var controlTimeVal = new Date();
    var split = str.split(" ");
    socket.emit('control stream', { gear: split[0] , angle : split[1], controlTime: controlTimeVal });
  });
});
//module.exports = app;
