/*
//ArOLz3DSTlEph91xotAdXlVeJjjF57wBwxauMk/b8iUwzl0uttsIze0KT66YurXIcJabHGihF3exllih/xxLagoAAABHeyJvcmlnaW4iOiJodHRwOi8vbG9jYWxob3N0OjgwIiwiZmVhdHVyZSI6IlNlcmlhbCIsImV4cGlyeSI6MTYwNTg2NTUwOX0=
const serialConnect = document.getElementById('serialConnect');

document.addEventListener('DOMContentLoaded', () => {
    serialConnect.addEventListener('click', clickConnect);
  });

async function connect() {
    // 직렬 포트 열기
    port = await navigator.serial.requestPort();
    await port.open({ baudrate: 9600 });

    // 장치 연결 후 데이터 읽을 수 있도록 입력스트림/디코더 설정
    let decoder = new TextDecoderStream();
    inputDone = port.readable.pipeTo(decoder.writable);
    inputStream = decoder.readable;
  
    reader = inputStream.getReader();
    readLoop();
}

// 새 데이터가 도착하면 판독기는 두 가지 속성, 즉 value및 done부울을 반환합니다. 경우 done에 해당하는 포트는 폐쇄되었거나오고 더 이상 데이터가 없습니다.
async function readLoop() {
    // CODELAB: Add read loop here.
   while (true) {
    const { value, done } = await reader.read();
    if (value) {
      log.textContent += value + '\n';
    }
    if (done) {
      console.log('[readLoop] DONE', done);
      reader.releaseLock();
      break;
    }
  }
}

async function clickConnect() {
    await connect(); 
}

/////////////////////////////////////////////////////////////////////////
*/
var userid = document.getElementById('myDiv').dataset.userid;

var socket = io.connect('http://'+ document.location.hostname+':33337/'); 
//var socket = io.connect('localhost:8000'); 
var gearVal = 0;
var angleVal = 0;

socket.on('news', function (data) { 
    console.log(data.serverData);
}); 

// userid ??? db or websocket.id?
socket.emit('client connected', 
{ clientData : '드론 조종 클라이언트 접속', clientType : 'ctw', userid : userid}); 

socket.on('drone data stream', function (data) {
    $('#userid').text(data.userid);
    $('#gpsX').text(data.gpsX);
    $('#gpsY').text(data.gpsY);
    $('#gear').text(data.speed);
    $('#angle').text(data.angle);
})


//keyboard input event 
document.addEventListener('keydown', function(event){
    var controlTimeVal = new Date();
    console.log(event.keyCode);
    
    switch (true){
        case(event.keyCode >= 48 && event.keyCode <=51):
            //translate(gearVal, event.keyCode-48);
            gearVal = event.keyCode-48;
            $('#gear').text(gearVal + '단');
            $('#control_gear').fadeOut(280);
            $('#control_gear').fadeIn(280);
            break;

        case(event.keyCode == 39):
            angleVal += 0.4;  
            if (angleVal == 360) angleVal = 0;
            rotate(angleVal);
            $('#angle').text(angleVal.toFixed(3));
            break;

        case(event.keyCode == 37):
            angleVal -= 0.4;
            if (angleVal == -360) angleVal = 0;
            rotate(angleVal);
            $('#angle').text(angleVal.toFixed(3));
            break;

        default:      
        socket.emit('control stream', // 키보드 컨트롤 전송
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

    console.log(preGear+ '에서 ' + moveGear +'로 ' + movepx+'만큼 움직');
    $('#control_gear').css({ WebkitTransform: 'translate(' + movepx + 'px, 0px)'});
}

