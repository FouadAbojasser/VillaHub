var dataTable;

$(document).ready(function () {

    loadDataTable();

});

function loadDataTable() {

    dataTable = $('#tbusersTable').DataTable({

        "ajax": {
            url: '/User/GetAllUsers'
        },

        "columns": [
            {
                data: null, width: "5%",
                render: function (data, type, row, meta) {
                    return meta.row + 1; // start index from 1
                }
            },
            { data: 'name', "width": "15%" },
            { data: 'email', "width": "20%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'country', "width": "10%" },
            {
                data: 'createdAt',
                width: "10%",
                render: function (data) {
                    if (!data) return "";
                    // Convert to Date and format as short date
                    var date = new Date(data);
                    return date.toLocaleDateString(); // e.g., 7/29/2025 (based on browser locale)
                }
            },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group">
                        <a href="/User/UserDetails?Id=${data}" class="btn btn-outline-warning mx-2">
                            <i class="bi bi-pencil-square"></i> Details
                        </a>
                    </div>`
                }
            }

        ]
    });
}