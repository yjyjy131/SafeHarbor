// logView.html ajax table create
function logTableCreate() {
    var sortSelect = document.getElementById("logSelcBox");
    var selectVal = sortSelect.options[sortSelect.selectedIndex].value;
    
    // 기본실행  : 2초마다 갱신하여 출력 
    // All : 페이지 갱신없이 모든 로그 출력
    if (selectVal == 0){
        $("#tableCreation").html('');
         $.ajax({
            url:'/logviewCreate',
             dataType:'json',
             type:'GET',
             data: {"data" : "0"},
              success: function(result){
                successFunc(result);
             } 
        });
    }

    //UserName
    if (selectVal == 1){
        $("#tableCreation").html('');
        $.ajax({
            url:'/logviewCreate',
            dataType:'json',
            type:'GET',
            data: {"data" : "1"},
            success: function(result){
                successFunc(result);
            }
        });
    }

    //time
    if (selectVal ==2){
        $("#tableCreation").html('');
        $.ajax({
            url:'/logviewCreate',
            dataType:'json',
            type:'GET',
            data: {"data" : "2"},
            success: function(result){
                successFunc(result);
            }
        });
    }   

}

function successFunc(result){
    console.log(result);
                //console.log(result[1].userid);
                var html='';
                for (key in result){
                    html += '<tr>';
                    html += '<td>' + result[key].userid + '</td>';
                    html += '<td>' + result[key].time + '</td>';
                    html += '<td>' + result[key].gpsX + '</td>';
                    html += '<td>' + result[key].gpsY + '</td>';
                    html += '<td>' + result[key].speed + '</td>';
                    html += '<td>' + result[key].angle + '</td>';
                }
                $("#tableCreation").empty();
                $("#tableCreation").append(html);
}

