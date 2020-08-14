var numDeltas = 1000;
var map;
var infowindow = null;

var circles = [
  {lat: 35.514020, lng: 129.391979},
  {lat: 35.469623, lng: 129.393169}
];

var destination =[
  {lat: 35.469623, lng: 129.393169},
  {lat: 35.514020, lng: 129.391979}
];

var deltaLat = [];
var deltaLng = [];

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

    for (var i=0; i<circles.length; i++){
      console.log(circles[i], destination[i]);
    }

    google.maps.event.addListenerOnce(map, 'tilesloaded', function(){
        for (var i=0; i<circles.length; i++){
          createCircle(circles[i], destination[i], map, i);
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

  /*
    var endPoints = {
      start1: {lat: 35.497021, lng: 129.401589 },
      start2: {lat: 35.497021, lng: 129.381589 }  
    }
  */
    var startPos = start;
    var endPos = end;
  
    deltaLat[Number(content)] = (endPos.lat - startPos.lat)/numDeltas;
    deltaLng[Number(content)] = (endPos.lng - startPos.lng)/numDeltas;

    var circleTimer = setInterval (function() {
      startPos.lat += deltaLat[Number(content)];
      startPos.lng += deltaLng[Number(content)];
      
      var latlng = new google.maps.LatLng(startPos.lat,startPos.lng);
      circleOption.center = latlng; 
      circle.setOptions(circleOption);
    }, 20);
}

function collisionCheck (){
  console.log('')
}
