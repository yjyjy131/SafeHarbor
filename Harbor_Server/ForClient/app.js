const SerialPort = require('serialport');
const io = require('socket.io');
/*
// Promise approach
SerialPort.list().then(ports => {
  ports.forEach(function(port) {
    console.log(port.path);
    console.log(port.pnpId);
    console.log(port.manufacturer);
  });
});
*/
const sp = new SerialPort("COM5", { baudRate:9600, autoOpen:false });

function timeDelay(timeout) {
	return new Promise((resolve) => {
		setTimeout(resolve, timeout);
	});
}

sp.open(function() {
  timeDelay(1000);
  
	sp.on("error", function(error) {
		console.log("Error : ", error.message);
  });

  port.on('data',function (data) {
    console.log('Read and Send Data : ' + data);
    //var controlTimeVal = new Date();
    //var split = string.split(',');
    //socket.emit('control stream', { gear: split[0] , angle : split[0], controlTime: controlTimeVal });
  });
});

/*
var socket = io.connect(porcess.argv[0]); 
//var socket = io.connect('localhost:8000'); 
socket.on('news', function (data) { 
    console.log(data.serverData);
}); 

// userid ??? db or websocket.id?
socket.emit('client connected', 
{ clientData : '드론 컨트롤러 접속', clientType : 'ctw', userid : userid}); 
*/

//module.exports = app;
