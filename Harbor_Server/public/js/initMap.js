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
  {lat:35.463198, lng:129.388737},
  {lat:35.493938, lng:129.398428}
];

var header = [];
var body = [];
var keys = [];
var index = 0;

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
  map.panTo(ulsan[2]);
  varInitialize();
})

google.maps.event.addDomListener(window, 'load', initMap);

function initMap() {

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

    if (currentBtn == 3 && content == 1){
      circleOption.fillOpacity = 0;
      circleOption.strokeOpacity = 0;
      recOption.fillOpacity = 0;
      recOption.strokeOpacity = 0;
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
    var distance = google.maps.geometry.spherical.computeDistanceBetween (circles[0], circles[1]);
    var totalRadi = cirRadius[0] + cirRadius[1];
    if ( distance <= totalRadi * 0.8){
      if (!colCheck){
        colCheck = true;
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
    cirRadi = [500, 500];
    numDeltas = [200, 0.5];
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

