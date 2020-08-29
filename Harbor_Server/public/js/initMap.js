var numDeltas = 100;
var map;
var infowindow = null;
var circleTimer = [];

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

var contentString = '<b>aaaa</b><br>';

google.maps.event.addDomListener(window, 'load', initMap);

function initMap() {

  // 1. 맵 리로드 시 속도 오류
  // 2. 충돌 시 연락처 제공 : 해양 안전 심판원, 해양경찰청, 울산항만
  // 3. css 수정 
  // 4. 충돌 감지 시 circle 클리킹
  // 5. 애니메이션 상황 추가 , 탭 버튼 마다 다른 애니메이션 재생
  
  var ulsan = {lat: 35.497021, lng: 129.391589};

  infowindow = new google.maps.InfoWindow(
    { 
      size: new google.maps.Size(150,50),
      content: contentString
    });

  map = new google.maps.Map(
    document.getElementById('googleMap'), {zoom: 13, center: ulsan});
    google.maps.event.addListenerOnce(map, 'tilesloaded', function(){
        for (var i=0; i<start.length; i++){
          createArea(start[i], destination[i], map, i);
        }
    }
  );
}

//$('#mapBtn1').on('click', function(){
//  initMap();
//});



function createArea(start, end, map, content) {
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

    var infoOption = {
      size: new google.maps.Size(10,10),
      position: {lat: 35.497021, lng: 129.391589}
    }

    var rectangle = new google.maps.Rectangle(recOption);

    circleTimer[content] = setInterval (function() {
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

      /*
      google.maps.event.addListener(circle, 'mouseover', function(){
        console.log('mouseover');
        infowindow.setContent(contentString);
        infowindow.open(map, circle);
      });
      */

      infowindow.setPosition(circles[0]);

      // lat lng 표기 속도 조절 
      //infowindow.setContent('<div style="color:black"> MMSI : ' + content + '<br> 속도 : 100 <br> lat : ' 
      //+  startPos.lat + '<br> lng : ' + startPos.lng );

     google.maps.event.addListener(circle, 'click', function(event) {
         //infowindow.setPosition(event.latLng);
         infowindow.setContent('<div style="color:black"> Name: 수상드론호 <br> MMSI : ' 
         + content + '<br> IMO: ' + content + '<br> 속도 : 100 ' );
         infowindow.open(map,circle);
    });
  
    }, 20);

}

// stop playing button
$('#stop').on('click', function(){
  clearInterval(circleTimer[0]);
  clearInterval(circleTimer[1]);
});

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

var colCheck = false;
function collisionCheck(circleNum){
  if (circles[0]!= null && circles[1]!=null)
  {
    var distance = google.maps.geometry.spherical.computeDistanceBetween (circles[0], circles[1]);
    var totalRadi = cirRadius[0] + cirRadius[1];
    if ( distance <= totalRadi * 0.8){
      if (!colCheck){
        colCheck = true;
        alert('충돌');
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
  } else {
    console.log('Nothing in the arr');
  }
 
}

// embed vesselfinder map
function initVessel() {
  //$("#map").attr("style", "visibility: hidden")
  //document.getElementById('vesselfinder').src='https://www.vesselfinder.com/aismap?zoom=13&amp;lat=35.497021&amp;lon=129.391589&amp;width=100%&amp;height=400&amp;names=false&amp;track=false&amp;fleet=false&amp;fleet_name=false&amp;fleet_hide_old_positions=false&amp;clicktoact=false&amp;store_pos=true&amp;ra=http%3A%2F%2Flocalhost%3A8000%2F';
}




