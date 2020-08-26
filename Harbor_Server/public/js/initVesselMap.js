$('.tabBtn').on('click', function(){
    var idx = $('.tabBtn').index(this);
    $('.tab_content').hide();
    $('.tab_content').eq(idx).show();
})


