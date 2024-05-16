let dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "pageLength": 5,
        "lengthMenu": [[5, 10, 25, 50, -1], [5, 10, 25, 50, "All"]],
        pagingType: 'simple_numbers',
        language: {
            paginate: {
                previous: 'Previous',
                next: 'Next'
            }
        },
        "ajax": { url: '/Admin/User/Getall' },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "email", "width": "15%" },
            { "data": "company.name", "width": "10%" },
            { "data": "role", "width": "10%" },
            {
                data: { id: "id", lockoutEnd: "lockoutEnd" },
                "render": function (data) {
                    let today = new Date().getTime();
                    let lockout = new Date(data.lockoutEnd).getTime();

                    if (lockout > today) {
                        return `
                        <div class="text-center">
                             <a onclick=LockUnlock('${data.id}') class="btn btn-danger text-white" style="cursor:pointer; width:100px;">
                                    <i class="bi bi-lock-fill"></i>  Lock
                                </a> 
                                <a href="/Admin/User/RoleManagement?userId=${data.id}" class="btn btn-danger text-white" style="cursor:pointer; width:150px;">
                                     <i class="bi bi-pencil-square"></i> Permission
                                </a>
                                <button onclick="deleteUser('${data.id}')" class="btn btn-outline-danger mx-2" style="width:150px;">
                                 <i class="bi bi-trash-fill"></i> Delete
                             </button>
                        </div>
                    `
                    }
                    else {
                        return `
                        <div class="text-center">
                              <a onclick=LockUnlock('${data.id}') class="btn btn-success text-white" style="cursor:pointer; width:100px;">
                                    <i class="bi bi-unlock-fill"></i>  UnLock
                                </a>
                                <a href="/Admin/User/RoleManagement?userId=${data.id}" class="btn btn-danger text-white" style="cursor:pointer; width:150px;">
                                     <i class="bi bi-pencil-square"></i> Permission
                                </a>
                                 <button onclick="deleteUser('${data.id}')" class="btn btn-outline-danger mx-2" style="width:150px;">
                                 <i class="bi bi-trash-fill"></i> Delete
                             </button>
                        </div>
                    `
                    }


                },
                "width": "50%"
            }
        ]
    });
}


function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
        }
    });
}

function deleteUser(userId) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: `/Admin/User/Delete?userId=${userId}`, // Update URL to match the user controller route
                type: 'POST', // Change type to POST since DELETE method is not allowed in standard HTML forms
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }
    });
}
