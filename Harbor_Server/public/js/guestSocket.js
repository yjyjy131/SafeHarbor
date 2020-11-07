// guestSystem 소켓 통신
//TODO: 비디오 값 수신
var socket = io.connect('http://'+ document.location.hostname+':33337/');
var userid = document.getElementById('myDiv').dataset.userid;

socket.on('news', function (data) { 
   console.log(data.serverData);
});

// userid ??? db or websocket.id?
socket.emit('client connected', 
{ clientData : '클라이언트 접속', clientType : 'ctw', userid : userid }); 

socket.on('drone data stream', function (data) {
    $('#userid').text(data.userid);
    $('#gpsX').text(data.gpsX);
    $('#gpsY').text(data.gpsY);
    $('#gear').text(data.speed);
    $('#angle').text(data.angle);
})

