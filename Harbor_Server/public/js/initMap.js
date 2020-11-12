var map;
var dronelat = 35.497021;
var dronelng = 129.391589;
var bounds = 0;
var userid = document.getElementById('main').dataset.userid;
var cirRadius = [400, 400];

// var aa= 35.4881010;
// var bb = 129.391585;
// var a1 = 35.4781030;
// var b1 = 129.391545;
// var testCenter =  [ new google.maps.LatLng(aa, bb), new google.maps.LatLng(a1, b1) ];
// var testRadius = 400;
//   $(document).ready(function(){
//     $('#shipSizeBtn').click(function(){
//     testRadius = $('#shipSize').val();
//     console.log(testRadius);
//     });
//   });

// var socket = io.connect('localhost:8000');
var socket = io.connect('http://'+ document.location.hostname+':33337/');

socket.emit('operator gps stream', 
  { clientData : '드론 관제 접속', clientType : 'opw', userid : userid }
); 

var droneCenter = [];
socket.on('operator gps stream', function (data) {
  console.log("드론 정보 수신 성공");
  dronelng = data.center[0];
  dronelat = data.center[1];
  console.log("드론아이디 : " + data.userid + "/ 위치 : " + dronelng + " " + dronelat);
  droneCenter =  new google.maps.LatLng(dronelat, dronelng);
  if (droneCenter.length == 0){
    droneCenter[0] = data.userid;
  } else {
    droneCenter[1] = data.userid;
  }
})

// 실제 드론 gps 값 
function initMap() {
  map = new google.maps.Map(
    document.getElementById('googleMap'), { zoom: 13, center: droneCenter[0] }
    );

  google.maps.event.addListenerOnce(map, 'tilesloaded', function(){ 
    // createArea(map, testCenter[0], testRadius);
    // createArea(map, testCenter[1], testRadius);
    createArea(map, droneCenter[0]);
    createArea(map, droneCenter[1]);
    collisionCheck();
  });
}

function createArea(map, droneCenter) {
    var circleOption = {
        center : droneCenter ,
        fillColor: '#3878c7',
        fillOpacity: 0.6,
        map: map,
        radius: 30, 
        strokeColor: '#3878c7',
        storkeOpacity: 1,
        strokeWeight: 0.5
    }
    var circle = new google.maps.Circle(circleOption);

    bounds = computingOffset(droneCenter, circleOption.radius);
    var recOption = {
      strokeColor: "#FF0000",
      strokeOpacity: 0.8,
      strokeWeight: 2,
      fillColor: "#FF0000",
      fillOpacity: 0.35,
      map: map,
      zIndex: 0, 
      bounds: bounds
    }
    var rectangle = new google.maps.Rectangle(recOption);
   
    circle.setMap(map);
    rectangle.setMap(map);
}

google.maps.event.addDomListener(window, 'load', initMap);

function computingOffset(center, cirRadius){
  var spherical = google.maps.geometry.spherical; 
  var areaRadi = cirRadius * 0.8; 
  var areaRadi2 = cirRadius * 0.5;
    var north = spherical.computeOffset(center, areaRadi, 10); // 0
    var west  = spherical.computeOffset(center, areaRadi2, -80); // -90
    var south = spherical.computeOffset(center, areaRadi, 200); // 180
    var east  = spherical.computeOffset(center, areaRadi2, 80); // 90

    var bounds = {
      north: north.lat(),
      south: south.lat(),
      east: east.lng(),
      west: west.lng() 
    }

    return bounds;
}

var colCheck = false;
function collisionCheck(){
    // 2번째 객체 생성 전 error handling
    try {
      var distance = google.maps.geometry.spherical.computeDistanceBetween (droneCenter, droneCenter2);
     }
     catch(e) {
        console.log('초기값 설정 중');
     }

    var totalRadi = cirRadius[0] + cirRadius[1];
    if ( distance <= totalRadi * 0.8){
      if (!colCheck){
        colCheck = true;
        alert('충돌');
        $("#danger").text('충돌');
        $('#colliInfo').fadeIn(500);
        $('#lat').text(bounds.south);
        $('#lng').text(bounds.east);

        var currentdate = new Date(); 
        var datetime = currentdate.getFullYear() + "/"
                + (currentdate.getMonth()+1) + "/" 
                + currentdate.getDate() + " "
                + currentdate.getHours() + ":"  
                + currentdate.getMinutes() + ":" 
                + currentdate.getSeconds();
        $('#when').text(datetime);
        
        body.push({'index': index, 'mmsi(1)':$('#mmsi1').text(), 'mmsi(2)':$('#mmsi2').text(), 'lat':bounds.south, 'long':bounds.east, 'Timestamp':datetime})
        index ++;
        //$('#colliInfo').show();
      }
    } else if ( distance <= totalRadi * 1.2){
      $("#danger").text('매우 위험');
      $("#danger").css("color", "#DF0101");
      $("#danger").css("font-weight", "bold");
    } else if ( distance <= totalRadi * 1.8){
      $("#danger").text('위험');
      $("#danger").css("color", "#FFFF00");
      $("#danger").css("font-weight", "bold");
    } else {
        $("#danger").text('보통');
    }
}

$('#colliBtn').on('click', function(){
  $('#colliInfo').fadeOut(500);
})


// page reload
$('#mapBtn1').on('click', function(){
  map.panTo(droneCenter);
})

$('.tabBtn').on('click', function(){
  $('.tabBtn').removeClass('on');
  $(this).addClass('on');
})