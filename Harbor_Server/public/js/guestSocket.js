// guestSystem 소켓 통신
//TODO: 비디오 값 수신
var socket = io.connect('http://localhost:8000'); 

socket.on('news', function (data) { 
   console.log(data.serverData);
});

// userid ??? db or websocket.id?
socket.emit('client connected', 
{ clientData : '클라이언트 접속', clientType : 'ctw', userid : 'userid'}); 

socket.on('drone data stream', function (data) {
    $('#userid').text(data.userid);
    $('#gpsX').text(data.gpsX);
    $('#gpsY').text(data.gpsY);
    $('#speed').text(data.speed);
})

