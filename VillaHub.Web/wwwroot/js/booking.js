var dataTable;

$(document).ready(function () {
    const urlParams = new URLSearchParams(window.location.search);
    const status = urlParams.get('status');

    // Initialize DataTable
    loadDataTable(status);
});

function loadDataTable(status) {
    // detect RTL from Razor
    var isRTL = window.isRTL || false;

    dataTable = $('#tblBookings').DataTable({
        ajax: {
            url: '/Booking/GetAllBookings?status=' + status
        },
        columns: [
            { data: 'id', width: "10%" },
            { data: 'name', width: "12%" },
            { data: 'phone', width: "10%", className: "phone-field" },
            { data: 'email', width: "15%" },
            { data: 'status', width: "10%" },
            { data: 'checkInDate', width: "10%" },
            { data: 'nights', width: "10%" },
            { data: 'totalCost', width: "8%" },
            {
                data: 'id',
                render: function (data) {
                    return `<div justify-content-center btn-group">
                               <a href="/booking/bookingDetails?bookingId=${data}" class="btn btn-outline-warning mx-2">
                              ${isRTL
                                ? `<span>تفاصيل</span><i class="bi bi-info-square ms-2"></i>`
                                : `<span>Details</span><i class="bi bi-info-square ms-2"></i>`}
                                </a>
                            </div>`;
                },
                width: "10%"
            }
        ],

        // ✅ Apply RTL language
        language: isRTL ? {
            decimal: "",
            emptyTable: "لا توجد بيانات متاحة",
            info: " إظهار _START_ إلى _END_ من أصل _TOTAL_ سجلات",
            infoEmpty: "لا توجد سجلات",
            infoFiltered: "(تمت التصفية من إجمالي _MAX_ سجلات)",
            lengthMenu: "إظهار_MENU_ سجلات",
            loadingRecords: "جار التحميل...",
            processing: "جارٍ المعالجة...",
            search: "بحث :",
            zeroRecords: "لم يتم العثور على سجلات",
            paginate: {
                first: "الأول",
                last: "الأخير",
                next: "التالي",
                previous: "السابق"
            }
        } : {},

        // ✅ After initialization
        initComplete: function () {
            if (isRTL) {
                // Add RTL attributes & class
                $("#tblBookings").addClass("table-rtl");
                $("#tblBookings_wrapper").attr("dir", "rtl").addClass("rtl");
            }
        }
    });
}
