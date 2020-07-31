// droneSystem.html 드론 조종 소켓 통신
var socket = io.connect('http://localhost:8000'); 
var gearVal = 0;
var angleVal = 0;

// userid ??? db or websocket.id?
socket.on('news', function (data) { 	
    socket.emit('client connected', 
                { clientData : '클라이언트 접속', clientType : 'ctw', userid : 'userid'}); 

     //TODO: 비디오 값 수신

    
}); 

//TODO: 현재 드론의 userid, gpsX, gpsY, speed 값 수신
//io.in('ctw').emit('drone data stream', data); 처리
/*
socket.on('drone data stream', function (data){
    console.log('연결되따');
    console.log(data);
});

document.addEventListener('drone data stream', function(event){ //??
    
    $('#userid').text(data.userid);
    $('#gpsX').text(data.gpsX);
    $('#gpsY').text(data.gpsY);
    $('#speed').text(data.speed);
});
*/

//keyboard input event 
document.addEventListener('keydown', function(event){
    var controlTimeVal = new Date();
    console.log(event.keyCode);
    
    switch (true){
        case(event.keyCode >= 48 && event.keyCode <=51):
            //translate(gearVal, event.keyCode-48);
            gearVal = event.keyCode-48;
            $('#gearKeyInput').text(gearVal + '단');
            $('#control_gear').fadeOut(280);
            $('#control_gear').fadeIn(280);
            break;

        case(event.keyCode == 39):
            angleVal += 0.4;  
            console.log(angleVal + '저렇게');
            if (angleVal == 360) angleVal = 0;
            rotate(angleVal);
            $('#angleKeyInput').text(angleVal.toFixed(3));
            break;

        case(event.keyCode == 37):
            angleVal -= 0.4;
            console.log(angleVal + '이렇게');
            if (angleVal == -360) angleVal = 0;
            rotate(angleVal);
            $('#angleKeyInput').text(angleVal.toFixed(3));
            break;

        default:      
        socket.emit('control stream', 
            { gear: gearVal , angle : angleVal, controlTime: controlTimeVal }); 
    }
    
})

// controller rotation
function rotate(degree) {
    $('#control_main').css({ WebkitTransform: 'rotate(' + degree + 'deg)'});
}

// gear translate
function translate(preGear, moveGear) {

    const distance = 10;
    var movepx = 0;

    /* animation 수정 필요 
    if (preGear < moveGear) {
        movepx = (moveGear-preGear)*distance;
    }

    if (preGear > moveGear) {
        movepx = -1*(moveGear-preGear)*distance;
    }
    */

    console.log(preGear+ '에서 ' + moveGear +'로 ' + movepx+'만큼 움직ㅇ');
    $('#control_gear').css({ WebkitTransform: 'translate(' + movepx + 'px, 0px)'});
}

