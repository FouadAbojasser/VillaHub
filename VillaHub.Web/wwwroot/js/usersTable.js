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
                data: null,
                width: "5%",
                className: "text-center",
                render: function (data, type, row, meta) {
                    return meta.row + 1; // Row number
                }
            },
            {
                data: 'imageUrl',
                width: "7%",
                className: "text-center",
                render: function (data) {
                    const userImagePath = '/images/users/';
                    const defaultImagePath = '/images/others/default-user.png';

                    return data
                        ? `<img src="${userImagePath}${data}" alt="User" class="img-fluid rounded-circle" style="max-width:40px; height:40px;"/>`
                        : `<img src="${defaultImagePath}" class="img-fluid rounded-circle" style="max-width:40px; height:40px;"/>`;
                }
            },
            { data: 'name', width: "15%" },
            //{ data: 'role', width: "0%" },
            { data: 'email', width: "15%" },
            { data: 'phoneNumber', width: "12%" },
            { data: 'country', width: "8%" },
            {
                data: 'createdAt',
                width: "10%",
                className: "text-center",
                render: function (data) {
                    if (!data) return "";
                    var date = new Date(data);
                    return date.toISOString().split('T')[0];
                }
            },
            {
                data: 'status',
                width: "8%",
                className: "text-center",
                render: function (data) {
                    // Optional: color coding based on status
                    switch (data) {
                        case "Active": return `<span class="badge bg-success">${data}</span>`;
                        case "Blocked": return `<span class="badge bg-warning text-dark">${data}</span>`;
                        case "Deleted": return `<span class="badge bg-danger">${data}</span>`;
                        default: return `<span class="badge bg-secondary">${data}</span>`;
                    }
                }
            },
            {
                data: 'deletedAt',
                width: "10%",
                className: "text-center",
                render: function (data) {
                    if (!data) return `<span class="text-muted">-</span>`;

                    var date = new Date(data);

                    // Check for default .NET date (year 1)
                    if (date.getFullYear() <= 1) {
                        return `<span class="text-muted">-</span>`;
                    }

                    // Otherwise, format as local date
                    return date.toISOString().split('T')[0];
                }
            },
            {
                data: 'id',
                width: "10%",
                className: "text-center",
                orderable: false,
                render: function (data) {
                    return `
                        <div class="btn-group" role="group">
                            <a href="/User/UserDetails?Id=${data}" class="btn btn-sm btn-outline-warning">
                                <i class="bi bi-info-circle"></i> Details
                            </a>
                        </div>`;
                }
            }
        ],
        "responsive": true,
        "autoWidth": false
    });
}
