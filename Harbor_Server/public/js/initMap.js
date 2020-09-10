var numDeltas = [100, 100];
var map;
// var infowindow = null;
var circleTimer = [];
var reset = false;

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

var contentString = '<b>Null message</b><br>';

var toggle = [false, false];
/*
var alertwindow = new google.maps.Infowindw({
  size: new google.maps.Size(150,50)
})*/

var cirRadi = [500, 500];
var currentBtn = 1;

var ulsan = [ {lat: 35.497021, lng: 129.391589},
  {lat:35.463198, lng:129.388737}
];

$('#mapBtn1').on('click', function(){
  currentBtn = 1;
  map.panTo(ulsan[0]);
  varInitialize();
})

$('#mapBtn2').on('click', function(){
  currentBtn = 2;
  map.panTo(ulsan[1]);
  varInitialize();
})

$('#mapBtn3').on('click', function(){
  currentBtn = 3;
  varInitialize();
})

google.maps.event.addDomListener(window, 'load', initMap);

function initMap() {

  // 애니메이션 
  // 충돌 감지 circle 클리킹 
  // 충돌지점 latlng 수정 

  /* 
  상황1. 선박와 어선 충돌
  상황2. 어선과 어선 충돌
  상황3. 선박이 제대로 정박하지 못하고 영역에 충돌
  */

 /*
  infowindow = new google.maps.InfoWindow(
    { 
      size: new google.maps.Size(150,50),
      content: contentString
    });
  */

  map = new google.maps.Map(
    document.getElementById('googleMap'), {zoom: 13, center: ulsan[0]});
    google.maps.event.addListenerOnce(map, 'tilesloaded', function(){
        deltaLat = [];
        deltaLng = [];

        $('#play').on('click', function(){
          if (circles[0] == null){ 
            for (var i=0; i<start.length; i++){
              createArea(start[i], destination[i], map, i, cirRadi[i], numDeltas[i]);
            }
          }
        });
    }
  );
}

//$('#mapBtn1').on('click', function(){
//  initMap();
//});

function createArea(start, end, map, content, cirRadi, numDeltaVal) {
    var circleOption = {
        center : start,
        fillColor: '#3878c7',
        fillOpacity: 0.6,
        map: map,
        zIndex: 1,
        radius: cirRadi, 
        strokeColor: '#3878c7',
        storkeOpacity: 1,
        strokeWeight: 0.5
    }
    var circle = new google.maps.Circle(circleOption);

    var startPos = start;
    var endPos = end;
    
    cirRadius[content] = circle.getRadius();

    deltaLat[Number(content)] = (endPos.lat - startPos.lat)/numDeltaVal;
    deltaLng[Number(content)] = (endPos.lng - startPos.lng)/numDeltaVal;

    var point = new google.maps.LatLng(startPos.lat, startPos.lng);
    bounds = computingOffset(point, content);
  
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

    /*
    var infoOption = {
      size: new google.maps.Size(10,10),
      position: {lat: 35.497021, lng: 129.391589}
    }
    */

    var rectangle = new google.maps.Rectangle(recOption);
   
    $('#play').on('click', function(){
      if(!reset){
        if(toggle[0]){
          toggle[0] = false;
          circleTimer[content] = setInterval ( function() {createCirTimer(circle, rectangle, startPos, circleOption, recOption, content) }, 20); 
        } else if (toggle[1]){
          toggle[1] = false;
          circleTimer[content] = setInterval ( function() {createCirTimer(circle, rectangle, startPos, circleOption, recOption, content) }, 20); 
        }
      }
    });

    $('#replay').on('click', function(){
        if(!toggle[1]){
          clearInterval(circleTimer[0]);
          clearInterval(circleTimer[1]); 
          toggle[0] = true;
          toggle[1] = true;
        }

        rectangle.setMap(null);
        circle.setMap(null);
        
        varInitialize();
        reset = true;
      //varInitialize();
      //circleTimer[0] = setInterval ( function() {createCirTimer(circle, rectangle, startPos, circleOption, recOption, content) }, 20); 
      //circleTimer[1] = setInterval ( function() {createCirTimer(circle, rectangle, startPos, circleOption, recOption, content) }, 20); 

    })
 
   // setInterval Anonymous func
   circleTimer[content] = setInterval ( function() {createCirTimer(circle, rectangle, startPos, circleOption, recOption, content) }, 20); 

}

function createCirTimer(circle, rectangle,  startPos, circleOption, recOption, content){
    startPos.lat += deltaLat[content];
    startPos.lng += deltaLng[content];
    
    var latlng = new google.maps.LatLng(startPos.lat,startPos.lng);
    circleOption.center = latlng; 
    circle.setOptions(circleOption);

    //point = latlng;
    recOption.bounds = computingOffset(latlng, content);
    rectangle.setOptions(recOption);

    circles[content] = latlng;
    collisionCheck(content);
    /*
    infowindow.setPosition(circles[content]);

   google.maps.event.addListener(circle, 'click', function(event) {
       infowindow.setContent('<div style="color:black"> MMSI : ' 
       + content + '<br> IMO: ' + content + '<br> 속도 : 100 ' );
       infowindow.open(map,circle);
  });
  */
}

// stop playing button
$('#stop').on('click', function(){
  clearInterval(circleTimer[0]);
  clearInterval(circleTimer[1]);

  toggle[0] = true;
  toggle[1] = true;
});

function computingOffset(center, content){
  var spherical = google.maps.geometry.spherical; 
  var areaRadi = cirRadius[content] * 0.8; 
  var areaRadi2 = cirRadius[content] * 0.5;
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

// collision infowindow

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
        $('#colliInfo').fadeIn(500);
        //$('#lat').text(bounds.south);
        //$('#lng').text(bounds.east);

        var currentdate = new Date(); 
        var datetime = currentdate.getFullYear() + "/"
                + (currentdate.getMonth()+1) + "/" 
                + currentdate.getDate() + " "
                + currentdate.getHours() + ":"  
                + currentdate.getMinutes() + ":" 
                + currentdate.getSeconds();
        $('#when').text(datetime);
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
  } else {
    console.log('Nothing in the arr');
  }
}

$('#colliBtn').on('click', function(){
  // $('#colliInfo').hide();
  $('#colliInfo').fadeOut(500);
})

function varInitialize(){
  //infowindow = null;
  circleTimer = [];

  if (currentBtn == 1){
    cirRadi = [500, 500];
    numDeltas = [100, 100];
    start = [
      {lat: 35.514020, lng: 129.391979},
      {lat: 35.469623, lng: 129.393169}
    ];
    
      destination =[
      {lat: 35.469623, lng: 129.393169},
      {lat: 35.514020, lng: 129.391979}
    ];

    $('#mmsi1').text('12345678');
    $('#mmsi2').text('87654321');  
  } else if (currentBtn == 2){
    cirRadi = [500, 200];
    numDeltas = [200, 80];
    start = [
      {lat: 35.457782, lng: 129.388296},
      {lat: 35.453227, lng: 129.380651}
    ];
    destination =[
      {lat: 35.489482, lng: 129.396625},
      {lat: 35.467445, lng: 129.388097}
    ];
    
    $('#mmsi1').text('12345678');
    $('#mmsi2').text('87654321');  

  } else if (currentBtn == 3){

  }

  deltaLat = [];
  deltaLng = [];

  cirRadius = [];
  circles = [];
  toggle = [false, false];
  colCheck = false;
}

$('.tabBtn').on('click', function(){
  $('.tabBtn').removeClass('on');
  $(this).addClass('on');
})