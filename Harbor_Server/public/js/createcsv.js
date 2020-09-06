// DB collision check 
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
    var header = [];
    header.push('mmsi(1)');
    header.push('mmsi(2)');
    header.push('lat');
    header.push('long');
    header.push('Timestamp');
    var body = [];
    body.push({'index':0, 'mmsi(1)':'a1', 'mmsi(2)':'b1', 'lat':'c4', 'long':'d1', 'Timestamp':'e1'})
    body.push({'index':1, 'mmsi(1)':'a2', 'mmsi(2)':'b2', 'lat':'c3', 'long':'d2', 'Timestamp':'e2'})
    body.push({'index':2, 'mmsi(1)':'a3', 'mmsi(2)':'b3', 'lat':'c2', 'long':'d3', 'Timestamp':'e3'})
    body.push({'index':3, 'mmsi(1)':'a4', 'mmsi(2)':'b4', 'lat':'c1', 'long':'d4', 'Timestamp':'e4'})
    var keys = [];
    keys.push('index');
    keys.push('mmsi(1)');
    keys.push('mmsi(2)');
    keys.push('lat');
    keys.push('long');
    keys.push('Timestamp');
    exportDataToCSVFile.apply(this, [ header, keys, body ])
})
  
  