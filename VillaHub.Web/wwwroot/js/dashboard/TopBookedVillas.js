$(document).ready(function () {
    TopBookedVillasLineChart();
});

function TopBookedVillasLineChart() {
    $(".chart-spinner").show();

    $.ajax({
        url: "/Dashboard/TopBookedVillasColumnChartData",
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            const chartData = data.map(item => ({
                x: `${item.villaName} (${item.villageName})`,
                y: item.bookings
            }));

            const options = {
                chart: {
                    type: 'bar',
                    height: 400,
                    toolbar: {
                        show: true
                    },
                    background: 'transparent' // ✅ No background
                },
                theme: {
                    mode: 'dark'
                },
                title: {
                    text: 'Top Booked Villas',
                    align: 'center',
                    style: {
                        color: '#ffffff',
                        fontSize: '16px'
                    }
                },
                legend: {
                    labels: {
                        colors: '#ffffff'
                    }
                },
                dataLabels: {
                    enabled: true,
                    style: {
                        colors: ['#ffffff']
                    }
                },
                xaxis: {
                    labels: {
                        rotate: -45,
                        style: {
                            fontSize: '12px',
                            color: '#ffffff'
                        }
                    }
                },
                yaxis: {
                    title: {
                        text: 'Number of Bookings',
                        style: {
                            color: '#ffffff'
                        }
                    },
                    labels: {
                        style: {
                            color: '#ffffff'
                        }
                    }
                },
                grid: {
                    borderColor: '#444444' 
                },
                series: [{
                    name: 'Bookings',
                    data: chartData
                }]
            };

            const chart = new ApexCharts(document.querySelector("#booking-column-chart"), options);
            chart.render();

            $(".chart-spinner").hide();
        },
        error: function () {
            $(".chart-spinner").hide();
            alert("Failed to load chart data.");
        }
    });
}
