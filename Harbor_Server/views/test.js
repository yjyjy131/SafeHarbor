
io.on('connection', function (socket) {  

    socket.on('myConnection', function (data) {
        socket.myName = data.myName;
        joinfunc(socket);
    });
      
    io.in('Jhon').emit('event', 'Test log...');

});

function joinfunc(socket){
        socket.join(socket.myData);    
}
