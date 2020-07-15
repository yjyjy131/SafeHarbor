// logView.html 테이블 동적 생성

var logTable = new Array();
var html = "";

// logTable말고... db 접근해서 html 값 push
logTable.push({time:'1', gpsX:'1', gpsY:'13', speed:'4', angle:'2'});
for (i=0; i<20; i++)
logTable.push({time:'127.12', gpsX:'129.84', gpsY:'89.23', speed:'84', angle:'12'});

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