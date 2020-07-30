function initMap() {
    var seoul = {lat: 35.497021, lng: 129.391589};
    var map = new google.maps.Map(
    document.getElementById('map'), {zoom: 13, center: ulsan});
    //var marker = new google.maps.Marker({position: ulsan, map: map});

/* 테스트값 
  var flightPlanCoordinates = [
  {lat: 35.515646 , lng: 129.389240},
  {lat: 35.515646, lng: 129.390270},
  {lat: 35.513262, lng: 129.390270},
  {lat: 35.513262, lng: 129.389240},
  {lat: 35.515646, lng: 129.389240}
  ];

  center: {lat: 35.514447, lng: 129.389802}
 */

 // socket.on?
 var shipCoordinates = coordinate_segment(gpsDatas);

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
    center: {lat: 35.514447, lng: 129.389802}, // gpsDatas.center.gpsX
    radius: Number(calcRadius()) 
  }); 

  circle.setMap(map);
  shipArea.setMap(map);
} 

// 위도 y 경도 x , 상1 우2 하3 좌4 순서 
function coordinate_segment(gpsDatas) {
  var coordinates = new Array();
  coordinates[0] = new google.maps.LatLng(gpsDatas.front.gpsX, gpsDatas.right.gpsY);
  coordinates[1] = new google.maps.LatLng(gpsDatas.front.gpsX, gpsDatas.left.gpsY);
  coordinates[2] = new google.maps.LatLng(gpsDatas.back.gpsX, gpsDatas.left.gpsY);
  coordinates[3] = new google.maps.LatLng(gpsDatas.back.gpsX, gpsDatas.right.gpsY);
  coordinates[4] = new google.maps.LatLng(gpsDatas.front.gpsX, gpsDatas.right.gpsY);
  return coordinates;
}

function calcRadius (gpsData) {
  var radius = 
  sqrt(Math.pow(gpsDatas.right.gpsX-gpsDatas.center.gpsx, 2) + 
  Math.pow(gpsDatas.front.gpsX-gpsDatas.center.gpsx, 2)
  ) 
  return radius;
}
