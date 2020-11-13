var map;
var dronelat = 37.512881;
var dronelng = 127.058583;
var bounds = 0;
var userid = document.getElementById('main').dataset.userid;
var cirRadius = [400, 400];
var audio = new Audio('/sound/beep-24.mp3');

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

// userid 에 값이 추가될때마다 map 생성 

// 전 id 와 현재 id 구분 ?
// 모든 userid 배열 돌면서 체크 
// userid 없으면 userid 에 값 추가 

socket.on('operator gps stream', function (data) {
  $('#centerlat').text(data.gpsX);
  $('#centerlong').text(data.gpsY);

  //console.log("드론 정보 수신 성공");
  //console.log("드론아이디 : " + data.userid + "/ 위치 : " + data.gpsX + " " + data.gpsY);
  var userExist = false;
  var userIdChk = data.userid;
  
  // 배열에 유저 있는지 체크 
  for (var userId of iterable) { 
    console.log("현재 userid 배열 체크" + userId); // 10, 20, 30 
    if(userIdChk === userId[iterable]){
      userExist = true;
    }
  }

  if (!userExist)
  userId.push(data.userid);

  // 현재 유저의 index 값 
  var currentIndex = 0;
  var myUser;
  for (var userId of iterable) {
   if (userIdChk[iterable] === data.userid){
      break;
   }
    currentIndex ++;
  }

  // 해당 유저에 맞는 droneCenter 생성 
  droneCenter[currentIndex] =  new google.maps.LatLng(data.gpsX, data.gpsY);
  
  createArea(map, droneCenter[0], 0);
  // if (userId.length == 0){
  //   userId[0] = data.userid;
  //   console.log('drone0의 uesrid : ' + userId[0]);
  // } else if (userId.length == 1) {
  //   userId[1] = data.userid;
  //   console.log('drone1의 userid : ' + userId[1]);
  // } else if (userId.length == 2 ){
  //   if (data.userid === userId[0]){
  //     droneCenter[0] =  new google.maps.LatLng(data.gpsX, data.gpsY);
  //     console.log('drone0의 GPS : ' + droneCenter[0]);
  //   } else {
  //     droneCenter[1] =  new google.maps.LatLng(data.gpsX, data.gpsY);
  //     console.log('drone1의 GPS : ' + droneCenter[1]);
  //   }
  // }

})

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
          $('#centerlat').text(data.gpsX);
          $('#centerlong').text(data.gpsY);

          var userExist = false;
          var userIdChk = data.userid;

          console.log(data.userid + " 소켓 전송 확인");

          for (var userId of iterable) { 
            console.log("현재 접속된 userid 배열 체크" + userId); // 10, 20, 30 
            // 유저 존재하면 true로 바뀜
            if(userIdChk === userId[iterable]){
              userExist = true;
            }
          }

          // 존재하지 않으면 유저배열에 푸시
          if (!userExist){
            console.log(data.userid + "유저 존재x , 배열에 푸시합니다");
            userId.push(data.userid);
            }

          // 푸시된 userId확인 
          for (var userId of iterable) {    
              console.log("유저 배열 확인 " + iterable);
            }

          // 현재 유저의 index 값 
          var currentIndex = 0;
          for (var userId of iterable) {
            if (userIdChk[iterable] === data.userid){ 
                break; 
            }
            currentIndex ++;
          }

            // 해당 유저에 맞는 droneCenter 생성 
          droneCenter[currentIndex] =  new google.maps.LatLng(data.gpsX, data.gpsY);
          
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
      }, 100); 
      
    setInterval (
      function() { 
        collisionCheck();
        console.log('충돌체크');
      }, 100);
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
function collisionCheck(){
    // 2번째 객체 생성 전 error handling
    try {
      var distance = google.maps.geometry.spherical.computeDistanceBetween (droneCenter[0], droneCenter[1]);
     }
     catch(e) {
        console.log('초기값 설정 중');
     }

    var totalRadi = cirRadius[0] + cirRadius[1];
    if ( distance <= totalRadi * 0.8){
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
    } else if ( distance <= totalRadi * 1.2){
      audio.play();
      $("#danger").text('매우 위험');
      $("#danger").css("color", "#DF0101");
      $("#danger").css("font-weight", "bold");
    } else if ( distance <= totalRadi * 1.8){
      audio.play();
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

function changeGps (index, circleOption ,recOption){
  //console.log(index + ":  " + droneCenter[0] + " " + userId[index]);
  circleOption.center = droneCenter[index];
  circles[index].setOptions(circleOption);

  recOption.bounds = computingOffset(droneCenter[index], cirRadius[index]);
  rectangles[index].setOptions(recOption);

  map.setCenter(droneCenter[0]);
  collisionCheck();
}
   
