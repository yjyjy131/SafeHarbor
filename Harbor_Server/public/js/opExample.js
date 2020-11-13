var numDeltas = [450, 450];
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

var deltaLat = [], deltaLng = [];

var cirRadius = [];
var circles = []; // circle 들의 latlng을 저장한다. 

var circleObj = [];
var rectObj = [];
var objIndex = 0;

var contentString = '<b>Null message</b><br>';

var toggle = [false, false];
/*
var alertwindow = new google.maps.Infowindw({
  size: new google.maps.Size(150,50)
})*/

var cirRadi = [500, 500];
var currentBtn = 1;

var ulsan = [ {lat: 35.497021, lng: 129.391589},
  {lat:35.463198, lng:129.388737},
  {lat:35.493938, lng:129.398428}
];

var header = [];
var body = [];
var keys = [];
var index = 0;

var audio = new Audio('/sound/beep-24.mp3');

var play = false;
$('#mapBtn1').on('click', function(){
  tabChange();
  if (play == false){
    currentBtn = 1;
    map.panTo(ulsan[0]);
    varInitialize();
  }
})

$('#mapBtn2').on('click', function(){
  tabChange();
  if (play == false){
    currentBtn = 2;
    map.panTo(ulsan[1]);
    varInitialize(); 
  }
})

$('#mapBtn3').on('click', function(){
  tabChange();
  if (play == false){
    currentBtn = 3;
    map.panTo(ulsan[2]);
    varInitialize(); 
  }
})

function tabChange() {
  play = false;

  clearInterval(circleTimer[0]);
  clearInterval(circleTimer[1]);

    if(!toggle[1]){
      clearInterval(circleTimer[0]);
      clearInterval(circleTimer[1]); 
      toggle[0] = true;
      toggle[1] = true;
    }

    circleObj[0].setMap(null);
    circleObj[1].setMap(null);
    
    reset = true;

}

google.maps.event.addDomListener(window, 'load', initMap);

function initMap() {

  map = new google.maps.Map(
    document.getElementById('googleMap'), {zoom: 13, center: ulsan[0]});
    google.maps.event.addListenerOnce(map, 'tilesloaded', function(){
        deltaLat = [];
        deltaLng = [];

        $('#play').on('click', function(){
          play = true;
          if (circles[0] == null){ 
            for (var i=0; i<start.length; i++){
              createArea(start[i], destination[i], map, i, cirRadi[i], numDeltas[i]);
            }
          }
        });
    }
  );
}

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
    
    circleObj[objIndex] = new google.maps.Circle(circleOption);
    //var circle = new google.maps.Circle(circleOption);

    var startPos = start;
    var endPos = end;
    
    cirRadius[content] = circleObj[objIndex].getRadius();

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

    if (currentBtn == 3 && content == 1){
      circleOption.fillOpacity = 0;
      circleOption.strokeOpacity = 0;
      recOption.fillOpacity = 0;
      recOption.strokeOpacity = 0;
    }

    rectObj[objIndex] = new google.maps.Rectangle(recOption);
   
    objIndex++;
    
    $('#play').on('click', function(){
      play = true;
      if(reset){
          if(toggle[0]){
            toggle[0] = false;
            circleTimer[content] = setInterval ( function() {createCirTimer(startPos, circleOption, recOption, content) }, 20); 
          } else if (toggle[1]){ 
            toggle[1] = false;
            circleTimer[content] = setInterval ( function() {createCirTimer(startPos, circleOption, recOption, content) }, 20); 
          }
      }
    });

    $('#replay').on('click', function(){
      play = false;
      clearInterval(circleTimer[0]);
      clearInterval(circleTimer[1]);

        if(!toggle[1]){
          clearInterval(circleTimer[0]);
          clearInterval(circleTimer[1]); 
          toggle[0] = true;
          toggle[1] = true;
        }

        circleObj[0].setMap(null);
        circleObj[1].setMap(null);
        
        varInitialize();
        reset = true;

      //varInitialize();
      //circleTimer[0] = setInterval ( function() {createCirTimer(circle, rectangle, startPos, circleOption, recOption, content) }, 20); 
      //circleTimer[1] = setInterval ( function() {createCirTimer(circle, rectangle, startPos, circleOption, recOption, content) }, 20); 

    })

   // setInterval Anonymous func
   circleTimer[content] = setInterval ( function(){ 
     createCirTimer(startPos, circleOption, recOption, content) }, 20); 

}

function createCirTimer(startPos, circleOption, recOption, content){
    startPos.lat += deltaLat[content];
    startPos.lng += deltaLng[content];
    
    var latlng = new google.maps.LatLng(startPos.lat,startPos.lng);
    circleOption.center = latlng; 

    circleObj[content].setOptions(circleOption);
    //circleObj[1].setOptions(circleOption);

    //point = latlng;
    recOption.bounds = computingOffset(latlng, content);
    rectObj[content].setOptions(recOption);

    circles[content] = latlng;
    collisionCheck();
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
  play = false;
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
function collisionCheck(){
    // 2번째 객체 생성 전 error handling
    try {
      var distance = google.maps.geometry.spherical.computeDistanceBetween (circles[0], circles[1]);
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
        $("#danger").css("color", "#FFD400");
    }
}

$('#colliBtn').on('click', function(){
  $('#colliInfo').fadeOut(500);
})

function varInitialize(){
  circleTimer = [];

  if (currentBtn == 1){
    cirRadi = [500, 500];
    numDeltas = [450, 450];
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
    cirRadi = [500, 500];
    numDeltas = [200, 10];

    start = [
      {lat: 35.486726, lng: 129.397937},
      {lat:35.504331, lng: 129.399319}
    ];

    destination =[
      {lat:  35.500341, lng:129.398696},
      {lat:35.504331, lng: 129.399319}
    ];
    
    $('#mmsi1').text('12345678');
    $('#mmsi2').text('87654321');  
  }

  deltaLat = [];
  deltaLng = [];

  cirRadius = [];

  circleObj[0].setMap(null);
  circleObj[1].setMap(null);
  rectObj[0].setMap(null);
  rectObj[1].setMap(null);
  objIndex = 0; 
 
  circles = [];
  toggle = [false, false];
  colCheck = false;
}

$('.tabBtn').on('click', function(){
  $('.tabBtn').removeClass('on');
  $(this).addClass('on');
})

function exportDataToCSVFile(header, keys, body) {
  var csv = '';
  csv = csv.replace(/\s+/, "");
  csv = header.join(',');
  csv+='\n';

  $.each(body, function(index, rows){
    if(rows){
      var tmp = [];
      $.each(keys, function(index, key){
        key && tmp.push(rows[key])
      })
      csv+=tmp.join(',');
      csv+='\n';
    }
  })

  var BOM = '%EF%BB%BF'; // 한글깨짐
  var csvData = 'data:application/csv;charset=utf-8,'+BOM+',' + encodeURIComponent(csv);
  $(this)
    .attr({
    'download': 'temp.csv',
    'href': csvData,
    'target': '_blank'
  });
}

$('#excelDownload').on('click', function(event){
  header.push('mmsi(1)');
  header.push('mmsi(2)');
  header.push('lat');
  header.push('long');
  header.push('Timestamp');

  keys.push('index');
  keys.push('mmsi(1)');
  keys.push('mmsi(2)');
  keys.push('lat');
  keys.push('long');
  keys.push('Timestamp');
  exportDataToCSVFile.apply(this, [ header, keys, body ])
})

