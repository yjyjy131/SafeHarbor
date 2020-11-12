// 클라이언트 타입 정의 {opd = 관제드론, opw = 관제 웹, ctd = 컨트롤 드론, ctw = 컨트롤 웹}
// 전송 데이터 종류 clientType, gpsX, gpsY, time(date타입), vidstream(무슨타입 ??), speed, angle
// 각자 이벤트 발생시 data에 필요한 데이터 넣어서 줄것 
const models = require("./models/index.js");

var gpsDatas = { 'front' : null, 'back' : null, 'left' : null, 'right' : null, 'center' : null};
var droneWeb = null;
var io;

module.exports.attach_event = function(_io){
    io = _io;
    io.on('connection', function (socket) {  
        io.emit('news', { serverData : "서버 작동" });
        console.log("socket connected");
        //socket.emit('news', { serverData : "서버 작동" });

        
        //연결될 경우. 웹쪽은 data.userid 정보를 넣어서 같이 전달해야함
        socket.on('client connected', function (data) {
            socket.clientType = data.clientType;
            socket.userid = data.userid;
            onClientConnected(socket);
            console.log(data);
        });


        //웹에서 서버로 조종정보 전달(조종시)
        //speed, angle, time
        socket.on('control stream', function (data) {
          console.log('control stream :' + data.gear + ' ' + data.angle + ' ' + data.controlTime + '\n');
          //서버에서 드론으로 조종정보 전달
          io.in('ctd').emit('control stream', data);
        });
      

        //드론에서 서버로 드론정보 전달(조종시)
        // gpsX, gpsY, time, speed, angle
        socket.on('drone data stream', function(data){
            console.log('drone data stream \n' + data);
            var _userid;
            if(data != null){
                _userid = data.userid;
                models.control_log.create({
                    userid: _userid,
                    speed: data.speed,
                    angle: data.angle,
                    gpsX: 110,
                    gpsY: 34,
                    time: data.time
                  });
            }

            //서버에서 웹으로 드론정보 전달(조종시)
            io.in('ctw').emit('drone data stream', data);
        });        
        

        //드론에서 서버로 비디오 전달 (조종시)
        //비디오 데이터 (정하는중)
        socket.on('video stream', function (data) {
            console.log('video stream \n' + data);

            //서버에서 웹으로 비디오 전달 (미완성)
            io.in('opw').emit('video stream', data);
            //TODO
        });


        //드론에서 서버로 gps정보 전달(관제시)
        // gpsX, gpsY, time, location(front, back, left, right, center)
        socket.on('operator gps stream', function (data) {
            console.log('operator gps stream \n' + data.userid + " " + data.gpsX);
            io.in('opw').emit("operator gps stream", data);           

            // var isFull = true;
            // for(key in gpsDatas){
            //     if(gpsDatas[key] == null)
            //     isFull = false;
            // }

            // //서버에서 웹으로 gps정보 전달(관제시)
            // io.in('opw').emit("operator gps stream", gpsDatas);
            // for(key in gpsDatas){
            //     gpsDatas[key] = null;
            // }
        });

        
        // 컨트롤러에서 서버로 제어정보 전달
        socket.on('angleChange', function (data) {
            console.log("angle: " + data);
            // 서버에서 드론으로 제어정보 전달
            io.in('ctd').emit('control stream', data);
        })

        socket.on('gearChange', function(data) {
            console.log("gear: " + data);
            io.in('ctd').emit('control stream', data);
        })


        socket.on('disconnect', function (data) {
            console.log("disconnected");
            if(socket.clientType == "ctw")
                droneWeb = null;
        });
    });
}

function onClientConnected(socket){
    if(socket.clientType == undefined || socket.clientType == null){
        console.log("클라이언트 타입없음");
        return;
    }

    if(socket.clientType == "ctd" ||socket.clientType == "ctw" ){
        if(io.in(socket.clientType).clients.length != 0){
            console.log("이미 연결이 있습니다. 기존연결을 지웁니다. " + socket.clientType);   
            io.in(socket.clientType).clients((error, socketIds) => {
                if (error) throw error;

                socketIds.forEach(socketId => io.sockets.sockets[socketId].leave(socket.clientType));
            });
        }
        socket.join(socket.clientType);
        if(socket.clientType == "ctw")
            droneWeb = socket;
    }
    else{
        socket.join(socket.clientType);
    }

    //console.log(socket.clientType + " connected. cnt : " + counts[socket.clientType]);
    console.log(socket.clientType + " connected. cnt : " + io.sockets.adapter.rooms[socket.clientType].length);
}

//module.exports.count = count;