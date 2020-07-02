// 클라이언트 타입 정의 {opd = 관제드론, opw = 관제 웹, ctd = 컨트롤 드론, ctw = 컨트롤 웹}
// 전송 데이터 종류 clientType, gpsX, gpsY, time(date타입), vidstream(무슨타입 ??), speed, angle
// 각자 이벤트 발생시 data에 필요한 데이터 넣어서 줄것 
var counts = {opd : 0, opw : 0, ctd : 0, ctw : 0};

module.exports.attach_event = function(io){
    io.on('connection', function (socket) {  
        
        socket.emit('news', { serverData : "서버 작동" });
        
        socket.on('client login', function (data) { 
            console.log(data.clientData);
            switch(data.client_type){
                case "opd" : socket.clientType = "opd"; break;
                case "opw" : socket.clientType = "opw"; break;
                case "ctd" : socket.clientType = "ctd";break;
                case "ctw" : socket.clientType = "ctw";break;
                default : break;
            }
          console.log(data);
        });

        socket.on('server to client', function (data) {
            switch(socket.clientType){
                case "opd" : break;
                case "opw" : break;
                case "ctd" : break;
                case "ctw" : break;
                default : break;
            }
          console.log(data);
        });

        socket.on('client to server', function (data) {
            switch(socket.clientType){
                case "opd" : break;
                case "opw" : break;
                case "ctd" : break;
                case "ctw" : break;
                default : break;
            }
          console.log(data);
        });
      
        socket.on('disconnect', function(){
            switch(socket.clientType){
                case "opd" : break;
                case "opw" : break;
                case "ctd" : break;
                case "ctw" : break;
                default : break;
            }
          console.log('접속이 종료되었습니다.');
        });
    });
}

function opdToServer(data){
    console.log("hi");
}

function opwToServer(data){

}

function ctdToServer(data){

}

function ctwToServer(data){

}
//module.exports.count = count;