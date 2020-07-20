function initMap() {
    var seoul = {lat: 35.497021, lng: 129.391589};
    var map = new google.maps.Map(
    document.getElementById('map'), {zoom: 13, center: seoul});
    var marker = new google.maps.Marker({position: seoul, map: map});
} 