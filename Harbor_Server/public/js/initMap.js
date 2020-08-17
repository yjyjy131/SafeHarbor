var numDeltas = 1000;
var map;
var infowindow = null;

var start = [
  {lat: 35.514020, lng: 129.391979},
  {lat: 35.469623, lng: 129.393169}
];

var destination =[
  {lat: 35.469623, lng: 129.393169},
  {lat: 35.514020, lng: 129.391979}
];

var deltaLat = [];
var deltaLng = [];

var cirRadius = [];
var circles = [];

function initMap() {

  infowindow = new google.maps.InfoWindow(
    {
      content: 'blah blah',
      size: new google.maps.Size(150,50)
    }
  )

  var ulsan = {lat: 35.497021, lng: 129.391589};
  map = new google.maps.Map(
    document.getElementById('map'), {zoom: 13, center: ulsan});

    google.maps.event.addListenerOnce(map, 'tilesloaded', function(){
        for (var i=0; i<start.length; i++){
          createCircle(start[i], destination[i], map, i);
        }
      });
}

google.maps.event.addDomListener(window, 'load', initMap);

function createCircle(start, end, map, content) {
    var circleOption = {
        center : start,
        fillColor: '#3878c7',
        fillOpacity: 0.6,
        map: map,
        radius: 500,
        strokeColor: '#3878c7',
        storkeOpacity: 1,
        strokeWeight: 0.5
    }
    var circle = new google.maps.Circle(circleOption);

    var startPos = start;
    var endPos = end;
    
    cirRadius[content] = circle.getRadius();
    deltaLat[Number(content)] = (endPos.lat - startPos.lat)/numDeltas;
    deltaLng[Number(content)] = (endPos.lng - startPos.lng)/numDeltas;

    // 원의 center 에서 반지름 값을 더한 곳의 lat, lng 필요 
    //var recLatLng1 = new google.maps.LatLng(startPos.lat + 0.01, startPos.lng + 0.01);
    //var recLatLng2 = new google.maps.LatLng(startPos.lat - 0.01, startPos.lng - 0.01);
    //console.log('이건데' + recLatLng1.lat() + ' ' + recLatLng1.lng()+ ' ' + recLatLng2.lat()+ ' ' + recLatLng2.lng());


    var point = new google.maps.LatLng(startPos.lat, startPos.lng);
    bounds = computingOffset(point);

    var recOption = {
      strokeColor: "#FF0000",
      strokeOpacity: 0.8,
      strokeWeight: 2,
      fillColor: "#FF0000",
      fillOpacity: 0.35,
      map: map,
      bounds: bounds
    }

    var rectangle = new google.maps.Rectangle(recOption);

    var circleTimer = setInterval (function() {
      startPos.lat += deltaLat[Number(content)];
      startPos.lng += deltaLng[Number(content)];
      
      var latlng = new google.maps.LatLng(startPos.lat,startPos.lng);
      circleOption.center = latlng; 
      circle.setOptions(circleOption);

      point = latlng;
      recOption.bounds = computingOffset(point, content);
      rectangle.setOptions(recOption);

      circles[content] = latlng;
      collisionCheck(content);
      //rectangle.setOptions(recOption);
    }, 20);

}

function computingOffset(center, content){
  var spherical = google.maps.geometry.spherical; 
  var areaRadi = cirRadius[content] * 0.8;
  
  var areaRadi2 = cirRadius[content] * 0.5;
    var north = spherical.computeOffset(center, areaRadi, 0); 
    var west  = spherical.computeOffset(center, areaRadi2, -90); 
    var south = spherical.computeOffset(center, areaRadi, 180); 
    var east  = spherical.computeOffset(center, areaRadi2, 90);

    var bounds = {
      north: north.lat(),
      south: south.lat(),
      east: east.lng(),
      west: west.lng() 
    }

    return bounds;
}

function collisionCheck(circleNum){
  var distance = google.maps.geometry.spherical.computeDistanceBetween (circles[0], circles[1]);

  var totalRadi = cirRadius[0] + cirRadius[1];
  if ( distance <= totalRadi){
    $("#danger").text('매우 위험');
  } else if ( distance <= totalRadi * 1.8){
    $("#danger").text('위험');
  } else {
      $("#danger").text('보통');
  }
}



