let js = jQuery.noConflict(true);
let dataTable;
js(document).ready(function () {
    loadDataTable();
});



function loadDataTable() {
    dataTable = js('#tblCat').DataTable({
        "pageLength": 5,
        "lengthMenu": [[5, 10, 25, 50, -1], [5, 10, 25, 50, "All"]],
        pagingType: 'simple_numbers',
        language: {
            paginate: {
                previous: 'Previous',
                next: 'Next'
            }
        },
        "ajax": {
            type: 'Get',
            url: '/admin/category/getall'
        },
        "columns": [
            { data: 'name', 'width': '25%' },
            { data: 'displayOrder', 'width': '25%' },
            
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-80 btn-group" role="group">
                        <a href="/admin/category/edit?id=${data}" class="btn btn-success mx-2"><i class="bi bi-pencil-square"></i> Edit</a>
                        <a onClick=Delete("/admin/category/delete?id=${data}") class="btn btn-danger mx-2"><i class="bi bi-trash-fill"></i> Delete</a>
                        
                    </div>`
                },
                'width': '50%'
            }
        ]
    });
}

function Delete(url) {
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
            js.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }
    });
}