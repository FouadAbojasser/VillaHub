
$(document).ready(function () {
    loadBookingsPieChart();
});

function loadBookingsPieChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/BookingsSTSPieChartData",
        type: 'GET',
        dataType: 'json',
        success: function (data) {
      
            loadPieChart("bookingsSTSPieChart", data);

            $(".chart-spinner").hide();
        }
    });
}

function loadPieChart(id, data) {
    var chartColors = getChartColorsArray(id);
    var options = {
        colors: chartColors,
        series: data.series,
        labels: data.labels,
        chart: {
            type: 'pie',
            width: '100%', // or a fixed value like 400
            height: 350,
            toolbar: {
                show: true,
                tools: {
                    download: true,
                    selection: false,
                    zoom: false,
                    zoomin: false,
                    zoomout: false,
                    pan: false,
                    reset: false
                }
            }
        },
        stroke: {
            show: false
        },
        legend: {
            position: 'bottom',
            horizontalAlign: 'center',
            labels: {
                colors: "#fff",
                useSeriesColors: true
            },
        },
    };
    var chart = new ApexCharts(document.querySelector("#" + id), options);
    chart.render();
}