var map = L.map('map').setView([55.7558, 37.6173], 5);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);
var markerGroup = L.layerGroup().addTo(map);
var elements = new Map();

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
function setElementDefaultStyle(element) {
    element.eachLayer(function (layer) {
        layer.setStyle({
            fillColor: "green",
            color: "green"
        });
    });
}
function resetAllElementsStyle() {
    elements.forEach(function (element, key) {
        setElementDefaultStyle(element);
    });
}
function loadData() {
    $.ajax({
        url: '/api/objects',
        type: 'GET',
        success: function (data) {
            data.forEach(function (obj) {
                var geoJson = JSON.parse(obj.geoData);
                var element = L.geoJSON(geoJson, {
                    pointToLayer: function (feature, latlng) {
                        var marker = L.circleMarker(latlng, {
                            radius: 8,
                            weight: 1,
                            opacity: 1,
                            fillOpacity: 0.6
                        });
                        return marker;
                    }
                });
                setElementDefaultStyle(element);
                element.addTo(map).on('click', function () {
                    highlightObjectRelations(obj.id);
                });

                    elements.set(obj.id, element);
            });
        },
        error: function () {
            alert('Error while receiving data');
        }
    });
}

function highlightObject(objectId) {
    if (!elements.has(objectId)) return;
    var element = elements.get(objectId);
    element.eachLayer(function (layer) {
        layer.setStyle({
            fillColor: "purple",
            color: "purple"
        });
    });
}

function highlightObjectRelations(objectId) {
    resetAllElementsStyle();
    highlightObject(objectId);
    $.ajax({
        url: '/api/objects/' + objectId + '/related', // API для получения связанных объектов
        method: 'GET',
        success: function (relatedObjects) {
            relatedObjects.forEach(function (relatedObjectId) {
                highlightObject(relatedObjectId);
            });
        }
    });
}

$(function () {
    loadData();
});