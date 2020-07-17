// logView.html 테이블 동적 생성

var logTable = new Array();
var html = "";

// logTable말고... db  
for (i=0; i<20; i++)
logTable.push({time: 'testIndex' + i, gpsX:'129.84', gpsY:'89.23', speed:'84', angle:'12'});

for (key in logTable){
    html += '<tr>';
    html += '<td>' + logTable[key].time + '</td>';
    html += '<td>' + logTable[key].gpsX + '</td>';
    html += '<td>' + logTable[key].gpsY + '</td>';
    html += '<td>' + logTable[key].speed + '</td>';
    html += '<td>' + logTable[key].angle + '</td>';
    html += '</tr>';
}

$("#tableCreate").empty();
$("#tableCreate").append(html);
$('#headerFixTable').fixheadertable({height: '200',minWidth:800,caption:'my header is fixed', zebra : true});