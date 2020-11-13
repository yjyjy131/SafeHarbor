var map;
var dronelat = 37.512881;
var dronelng = 127.058583;
var bounds = 0;
var userid = document.getElementById('main').dataset.userid;
var cirRadius = [400, 400, 400, 400, 400, 400, 400];
var audio = new Audio('/sound/beep-24.mp3');
audio.muted = true;

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

//var socket = io.connect('localhost:8000');
var socket = io.connect('http://'+ document.location.hostname+':33337/');

socket.emit('client connected', 
  { clientData : '드론 관제 접속', clientType : 'opw', userid : userid }
); 

// 드론 중앙 초기값
var droneCenter = [ new google.maps.LatLng(37.512881, 127.058583), new google.maps.LatLng(37.512891, 127.058593) ];
var userId = [];

var circles = [];
var rectangles = [];
var mapCenter =  new google.maps.LatLng(37.512881, 127.058583)
// 실제 드론 gps 값 

function initMap() {   
    map = new google.maps.Map(
      document.getElementById('googleMap'), { zoom: 17, center: mapCenter }
      );
    
      google.maps.event.addListenerOnce(map, 'tilesloaded', function(){ 

        socket.on('operator gps stream', function (data) {

          //front, back, left, right, center
          if (data.location == 'center'){
            $('#centerlat').text(data.gpsX);
            $('#centerlong').text(data.gpsY);
          } else  if (data.location == 'front'){
            $('#frontlat').text(data.gpsX);
            $('#frontlong').text(data.gpsY);
          } else  if (data.location == 'back'){
            $('#backlat').text(data.gpsX);
            $('#backlong').text(data.gpsY);
          } else  if (data.location == 'left'){
            $('#leftlat').text(data.gpsX);
            $('#leftlong').text(data.gpsY);
          } else  if (data.location == 'right'){
            $('#rightlat').text(data.gpsX);
            $('#rightlong').text(data.gpsY);
          } 

          var userExist = false;
          var userIdChk = data.userid;

          for (var iterable of userId) { 
            // 유저 존재하면 true로 바뀜
            if(userIdChk === iterable){
              userExist = true;
            }
          }

          // 존재하지 않으면 유저배열에 푸시
          if (!userExist){
            console.log(data.userid + "유저 존재x , 배열에 푸시합니다");
            userId.push(data.userid);
            }

          // 푸시된 userId확인 
          for (var iterable of userId) {    
              console.log("유저 배열 확인 " + iterable);
            }

          // 현재 유저의 index 값 
          var currentIndex = 0;
          for (var iterable of userId) {
            if (iterable === data.userid){ 
               console.log("현재 유저 index 값 " + currentIndex);
               break; 
            }
            currentIndex ++;
          }
            // 해당 유저에 맞는 droneCenter 생성 
          droneCenter[currentIndex] =  new google.maps.LatLng(data.gpsX, data.gpsY);
          
          console.log("userExist" + userExist);
            // 처음 들어온 user면 area 생성
          if (!userExist){
             console.log(data.userid + "의 새로운 area 생성");
             createArea(map, droneCenter[currentIndex], currentIndex);
          }

        });

      });

}

function createArea(map, droneCenter, index) {
    var circleOption = {
        center : droneCenter ,
        fillColor: '#3878c7',
        fillOpacity: 0.6,
        map: map,
        radius: 20, 
        strokeColor: '#3878c7',
        storkeOpacity: 1,
        strokeWeight: 0.5,
        zIndex: 0
    }
    circles[index] = new google.maps.Circle(circleOption);

    bounds = computingOffset(droneCenter, circleOption.radius);
    var recOption = {
      strokeColor: "#FF0000",
      strokeOpacity: 0.8,
      strokeWeight: 2,
      fillColor: "#FF0000",
      fillOpacity: 0.35,
      map: map,
      zIndex: 1, 
      bounds: bounds
    }
    rectangles[index] = new google.maps.Rectangle(recOption);
   
    //rectangles[index].setOptions(recOption);
    circles[index].setOptions(circleOption);
    rectangles[index].setOptions(recOption);

    setInterval (
      function() { 
        changeGps(index, circleOption ,recOption);
        console.log('gps 체크');
      }, 1000); 
      
    setInterval (
      function() { 
        collisionCheck();
        console.log('충돌체크');
      }, 1000);
}

google.maps.event.addDomListener(window, 'load', initMap);

function computingOffset(center, cirRadius){
  var spherical = google.maps.geometry.spherical; 
  var areaRadi = cirRadius * 0.07; 
  var areaRadi2 = cirRadius * 0.05;
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
var myDis1, myDis2, myDis3, myDis4, min;
function collisionCheck(){
    // 2번째 객체 생성 전 error handling
    try {
      switch(droneCenter.length){
        case 2:
          myDis1 = google.maps.geometry.spherical.computeDistanceBetween (droneCenter[0], droneCenter[1]);
          break;

        case 3:
          myDis1 = google.maps.geometry.spherical.computeDistanceBetween (droneCenter[0], droneCenter[1]);
          myDis2 = google.maps.geometry.spherical.computeDistanceBetween (droneCenter[0], droneCenter[2]);
          min = Math.min(myDis1,myDis2);
          break;

        case 4:
          myDis1 = google.maps.geometry.spherical.computeDistanceBetween (droneCenter[0], droneCenter[1]);
          myDis2 = google.maps.geometry.spherical.computeDistanceBetween (droneCenter[0], droneCenter[2]);
          myDis3 = google.maps.geometry.spherical.computeDistanceBetween (droneCenter[0], droneCenter[3]);
          min = Math.min(myDis1,myDis2,myDis3);
          break;
        
        case 5:
          myDis1 = google.maps.geometry.spherical.computeDistanceBetween (droneCenter[0], droneCenter[1]);
          myDis2 = google.maps.geometry.spherical.computeDistanceBetween (droneCenter[0], droneCenter[2]);
          myDis3 = google.maps.geometry.spherical.computeDistanceBetween (droneCenter[0], droneCenter[3]);
          myDis4 = google.maps.geometry.spherical.computeDistanceBetween (droneCenter[0], droneCenter[4]);
          min = Math.min(myDis1,myDis2,myDis3,myDis4);
          break;
      }

     }
     catch(e) {
        console.log('초기값 설정 중');
     }

    var totalRadi = cirRadius[0] + cirRadius[1];
    if ( min <= totalRadi * 0.8){
      if (!colCheck){
        colCheck = true;
        //alert('충돌');
        $("#danger").text('충돌');
        $("#danger").css("color", "#DF0101");
        $("#danger").css("font-weight", "bold");
        $('#colliInfo').fadeIn(500);
        $('#lat').text(bounds.south);
        $('#lng').text(bounds.east);

        //$('#colliInfo').show();
      }
    } else if ( min <= totalRadi * 1.2){
      audio.play();
      $("#danger").text('매우 위험');
      $("#danger").css("color", "#DF0101");
      $("#danger").css("font-weight", "bold");

    } else if ( min <= totalRadi * 1.8){
      audio.play();
      $("#danger").text('위험');
      $("#danger").css("color", "#FFFF00");
      $("#danger").css("font-weight", "bold");
      
    } else {
      $("#danger").css("color", "#FFFFFF");
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

function changeGps (index, circleOption ,recOption){
  //console.log(index + ":  " + droneCenter[0] + " " + userId[index]);
  circleOption.center = droneCenter[index];
  circles[index].setOptions(circleOption);

  recOption.bounds = computingOffset(droneCenter[index], cirRadius[index]);
  rectangles[index].setOptions(recOption);

  map.setCenter(droneCenter[0]);
  collisionCheck();
}
   
