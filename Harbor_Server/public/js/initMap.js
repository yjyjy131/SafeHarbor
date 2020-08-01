// opSystem 소켓 통신
// TODO: Ajax , db 거치지 않고 socket 으로 화면 깜빡임 없는 polyline 애니메이션 

var socket = io.connect('http://localhost:8000'); 
var areaRadius = 0;
var shipCoordinates = new Array();

var shipArea = new google.maps.Polyline({
  path: shipCoordinates,
  geodesic: false,
  strokeColor: '#FF0000',
  strokeOpacity: 1.0,
  strokeWeight: 2
});

var circle = new google.maps.Circle({
  strokeColor: '#FF0000', 
  strokeOpacity: 0.8, 
  strokeWeight: 1, 
  fillColor: '#FF0000', 
  fillOpacity: 0.2,
  center: {lat: 35.514447, lng: 129.389802}, // gpsDatas.center
  radius: areaRadius
}); 

socket.on('news', function (data) { 
    console.log(data.serverData);
}); 

// userid ??? db or websocket.id?
socket.emit('client connected', 
{ clientData : '클라이언트 접속', clientType : 'opw', userid : 'userid'}); 


socket.on('operator gps stream', function (data) {
   if (shipCoordinates.length != 0) 
    {
      for(key in shipCoordinates){
        shipCoordinates[key] =null
       }
    }
    shipCoordinates = coordinate_segment(data);
    areaRadius = calcRadius(data);
    setDataQuery(data);
})


function initMap() {
  var ulsan = {lat: 35.497021, lng: 129.391589};
  var map = new google.maps.Map(
  document.getElementById('map'), {zoom: 13, center: ulsan});
  //var marker = new google.maps.Marker({position: ulsan, map: map});

  circle.setMap(map);
  shipArea.setMap(map);
} 

// 위도 y 경도 x , 상1 우2 하3 좌4 순서 
function coordinate_segment(gpsDatas) {
  var coordinates = new Array();
  coordinates[0] = new google.maps.LatLng(gpsDatas.front[1], gpsDatas.right[0]);
  coordinates[1] = new google.maps.LatLng(gpsDatas.front[1], gpsDatas.left[0]);
  coordinates[2] = new google.maps.LatLng(gpsDatas.back[1], gpsDatas.left[0]);
  coordinates[3] = new google.maps.LatLng(gpsDatas.back[1], gpsDatas.right[0]);
  coordinates[4] = new google.maps.LatLng(gpsDatas.front[1], gpsDatas.right[0]);
  return coordinates;
}

function calcRadius (gpsData) {
  var radius = 
  sqrt(Math.pow(gpsDatas.right[1]-gpsDatas.center[1], 2) + 
  Math.pow(gpsDatas.front[1]-gpsDatas.center[1], 2)
  ) 
  return radius;
}

// 위도 y 경도 x , 상1 우2 하3 좌4 순서 
function setDataQuery (gpsData) {
  $('#centerlat').text(gpsData.center[1]);
  $('#centerlong').text(gpsData.center[0]);
  
  $('#frontlat').text(gpsData.front[1]);
  $('#frontlong').text(gpsData.front[0]);

  $('#rightlat').text(gpsData.front[1]);
  $('#rightong').text(gpsData.front[0]);

  $('#backlat').text(gpsData.front[1]);
  $('#backlong').text(gpsData.front[0]);

  $('#leftlat').text(gpsData.front[1]);
  $('#leftlong').text(gpsData.front[0]);
  
  $('#speed').text(data.speed);
}

function detectCollision (gpsData) {
  $('#danger').text();
}

/* 테스트값 
  var shipArea = new google.maps.Polyline({
    path: [
      {lat: 35.515646 , lng: 129.389240},
      {lat: 35.515646, lng: 129.390270},
      {lat: 35.513262, lng: 129.390270},
      {lat: 35.513262, lng: 129.389240},
      {lat: 35.515646, lng: 129.389240}
      ],
    geodesic: false,
    strokeColor: '#FF0000',
    strokeOpacity: 1.0,
    strokeWeight: 2
  });

  var circle = new google.maps.Circle({
    strokeColor: '#FF0000', 
    strokeOpacity: 0.8, 
    strokeWeight: 1, 
    fillColor: '#FF0000', 
    fillOpacity: 0.2,
    center: {lat: 35.514447, lng: 129.389802}, // gpsDatas.center
    radius: 180 // calcRadius()
  }); 

  center: {lat: 35.514447, lng: 129.389802}
 */