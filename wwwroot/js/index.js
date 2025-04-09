var map = L.map('map').setView([55.7558, 37.6173], 13);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);
var markerLayer = L.layerGroup().addTo(map);
var markers = [];

function openUploadPopup() {
    $('#uploadPopup').modal('show');
}

function closeUploadPopup() {
    $('#uploadPopup').modal('hide');
}
$('#uploadForm').on('submit', function (e) {
    e.preventDefault();
    var formData = new FormData();
    formData.append('csvFile', $('#csvFile')[0].files[0]);
    $.ajax({
        url: '/Home/UploadData',
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            alert('File loaded successfully!');
            $('#uploadPopup').modal('hide');
            loadData();
        },
        error: function () {
            alert('Upload exception');
        }
    });
});
function setDefaultStyle(marker) {
    marker.eachLayer(function (layer) {
        layer.setStyle({
            fillColor: "green",
            color: "green"
        });
    });
}

function loadData() {
    $.ajax({
        url: '/api/objects',
        type: 'GET',
        success: function (data) {
            data.forEach(function (obj) {
                var geoJson = JSON.parse(obj.geoData);
                var marker = L.geoJSON(geoJson, {
                    pointToLayer: function (feature, latlng) {
                        var markerColor =  'green';
                        var marker = L.circleMarker(latlng, {
                            radius: 8,
                            fillColor: markerColor,
                            color: markerColor,
                            weight: 1,
                            opacity: 1,
                            fillOpacity: 0.6
                        });
                        return marker;
                    }
                }).addTo(map).on('click', function () {
                    highlightObject(obj.id);
                });

                setDefaultStyle(marker);
                // Сохраняем маркер в массив
                markers.push({
                    marker: marker,
                    id: obj.id // Храним Id объекта для фильтрации
                });
            });
        },
        error: function () {
            alert('Error while receiving data');
        }
    });
}

function highlightObject(objectId) {
    $.ajax({
        url: '/api/objects/' + objectId + '/related', // API для получения связанных объектов
        method: 'GET',
        success: function (relatedObjects) {
            relatedObjects.push(objectId);
            markers.forEach(function (item) {
                setDefaultStyle(item.marker);
                relatedObjects.forEach(function (relatedObjectId) {

                    if (item.id === relatedObjectId) {
                        item.marker.eachLayer(function (layer) {
                            layer.setStyle({
                                fillColor: "purple",
                                color: "purple"
                            });
                        });
                    }
                });
            });
        }
    });
}

$(document).ready(function () {
    loadData();
});