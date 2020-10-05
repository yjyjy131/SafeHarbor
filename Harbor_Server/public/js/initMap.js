var map;

google.maps.event.addDomListener(window, 'load', initMap);

// 실제 드론 gps 값 
function initMap() {

  map = new google.maps.Map(
    document.getElementById('googleMap'), {zoom: 13, center: ulsan[0]});
    google.maps.event.addListenerOnce(map, 'tilesloaded', function(){
        deltaLat = [];
        deltaLng = [];
    }
  );
}

function createArea(start, end, map, content, cirRadi) {
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

    var rectangle = new google.maps.Rectangle(recOption);
   
}


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

function collisionCheck(circleNum){
    
}

// page reload
$('#mapBtn1').on('click', function(){
  currentBtn = 1;
  map.panTo(ulsan[0]);
  varInitialize();
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

