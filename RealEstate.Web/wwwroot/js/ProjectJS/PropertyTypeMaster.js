$(document).ready(function () {
    getData();
});

// Open Modal
function openModal(type) {
    clearData();

    if (type === "add") {
        $("#propertyTypeModalLabel").text("Add Property Type");
        $("#BtnSave").text("Save");
        $("#propertyTypeModal").modal("show");
    }

}

// Validate
function validate() {
    $(".form-error").text("");

    let isValid = true;
    const propertyTypeName = $("#PropertyTypeName").val().trim();

    if (!propertyTypeName) {
        $("#PropertyTypeNameError").text("Please enter Property Type Name.");
        isValid = false;
    } else if (propertyTypeName.length < 3) {
        $("#PropertyTypeNameError").text("Property Type Name must be at least 3 characters.");
        isValid = false;
    }

    return isValid;
}

// Save
function save() {
    if (!validate()) return;

    const model = {
        PropertyTypeIDP: $("#PropertyTypeIDP").val(),
        PropertyTypeName: $("#PropertyTypeName").val().trim(),
    };

    $.ajax({
        url: baseURL + "PropertyTypeMaster/Save",
        type: "POST",
        data: model,
        success: function (res) {

            const { outval, outmsg, fieldErrors } = res;

            // CASE 1: Success
            if (outval == 1) {
                showToast(outmsg, "success");
                $("#propertyTypeModal").modal("hide");
                getData();
            }

            // CASE 2: Business validation (duplicate, etc.)
            else if (outval == 99) {
                showToast(outmsg, "warning");
            }

            // CASE 3: ModelState validation errors
            else if (fieldErrors && Object.keys(fieldErrors).length > 0) {

                showToast("Please correct the highlighted errors.", "warning");

                // Clear all previous error messages
                $(".form-error").text("");

                // Display each validation error in its matching span
                for (const [fieldName, errorMsg] of Object.entries(fieldErrors)) {

                    // Example: PropertyTypeName → #PropertyTypeNameError
                    const errorSpan = $("#" + fieldName + "Error");

                    if (errorSpan.length) {
                        errorSpan.text(errorMsg);
                    }

                    console.log(`${fieldName}: ${errorMsg}`);
                }
            }

            // CASE 4: Unexpected / unknown error
            else {
                showToast("Unable to process your request. Please try again.", "error");
                console.error("Error:", res);
            }
        },

        error: function (xhr, status, error) {
            console.error(xhr);

            // Show simple message to user
            showToast("Unable to process your request. Please try again.", "error");
        }
    });
}

// Edit
function edit(id) {
    $.ajax({
        url: baseURL + "PropertyTypeMaster/GetByID",
        type: "POST",
        data: { id },
        success: function (res) {
            if (res.success && res.data) {
                const data = res.data;

                $("#PropertyTypeIDP").val(data.propertyTypeIDP || 0);
                $("#PropertyTypeName").val(data.propertyTypeName || "");

                $("#propertyTypeModalLabel").text("Edit Property Type");
                $("#BtnSave").text("Update");
                $("#propertyTypeModal").modal("show");
            }
        }
    });
}

// DataTables
function getData() {
    if (!$.fn.DataTable.isDataTable("#dataTBL")) {
        $("#dataTBL").DataTable({
            processing: true,
            serverSide: true,
            "ordering": false,
            ajax: {
                url: baseURL + "PropertyTypeMaster/GetDataWithPaging",
                type: "POST"
            },
            columns: [
                { data: "srNo" },
                { data: "propertyTypeName" },
                {
                    data: "isActive",
                    render: function (d, t, r) {
                        return `
                            <button class="btn btn-sm ${d ? "btn-success" : "btn-danger"}"
                                    onclick="generalAction(${r.propertyTypeIDP}, 2)">
                                ${d ? "Active" : "Inactive"}
                            </button>`;
                    }
                },
                {
                    data: "propertyTypeIDP",
                    render: function (id) {
                        return `
                            <button class="btn btn-primary btn-sm" onclick="edit(${id})">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button class="btn btn-danger btn-sm" onclick="generalAction(${id}, 1)">
                                <i class="bi bi-trash"></i>
                            </button>`;
                    }
                }
            ]
        });
    } else {
        $("#dataTBL").DataTable().ajax.reload();
    }
}

// Delete / Status Change
function generalAction(id, actionType) {
    let title = 'Are you sure?';
    let text = actionType === 1
        ? "Do you really want to delete this user?"
        : "Do you want to update the user status?";
    Swal.fire({
        title: title,
        text: text,
        icon: "warning",
        showCancelButton: true
    }).then((res) => {
        if (res.isConfirmed) {
            $.ajax({
                url: baseURL + "PropertyTypeMaster/GeneralAction",
                type: "POST",
                data: { ID: id, ActionType: actionType },
                success: function (res) {

                    const { outval, outmsg } = res;

                    // CASE 1: Success
                    if (outval === 1) {
                        showToast(outmsg, "success");
                        getData();
                    }
                    // CASE 2: Failure / error
                    else {
                        showToast("Unable to process your request. Please try again.", "error");
                        console.error("Error:", res);
                    }
                },
                error: function (xhr, status, error) {
                    console.error(xhr);

                    // Show simple message to user
                    showToast("Unable to process your request. Please try again.", "error");
                }
            });
        }
    });
}

function clearData() {
    $("#PropertyTypeIDP, #PropertyTypeName").val("");
    $(".form-error").text("");
}
