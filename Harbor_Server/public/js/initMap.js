var map;
var dronelat = 35.497021;
var dronelng = 129.391589;
var bounds = 0;
var userid = document.getElementById('myDiv').dataset.userid;

//var socket = io.connect('localhost:8000');
var socket = io.connect('http://'+document.location.hostname+':33337/');

socket.on('news', function (data) { 
  console.log(data.serverData);
}); 

socket.emit('operator gps stream', 
  { clientData : '드론 관제 접속', clientType : 'opw', userid : userid }
); 

socket.on('operator gps stream', function (data) {
  console.log("드론 정보 수신 성공");
  dronelng = data.center[0];
  dronelat = data.center[1];
})

var droneCenter =  new google.maps.LatLng(dronelat, dronelng);

// 실제 드론 gps 값 
function initMap() {
  map = new google.maps.Map(
    document.getElementById('googleMap'), { zoom: 13, center: droneCenter }
    );

  google.maps.event.addListenerOnce(map, 'tilesloaded', function(){ 
       createArea(map);
  });
}

function createArea(map) {
    var circleOption = {
        center : droneCenter ,
        fillColor: '#3878c7',
        fillOpacity: 0.6,
        map: map,
        radius: 400, 
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

// page reload
$('#mapBtn1').on('click', function(){
  map.panTo(droneCenter);
})

$('.tabBtn').on('click', function(){
  $('.tabBtn').removeClass('on');
  $(this).addClass('on');
})