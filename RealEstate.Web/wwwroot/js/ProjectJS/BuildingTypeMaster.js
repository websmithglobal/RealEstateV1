$(document).ready(function () {
    getData();

    // Initialize select2 when modal opens
    $('#buildingTypeModal').on('shown.bs.modal', function () {
        Select2Helper("buildingTypeModal");
    });
});


// -----------------------------
// Open Add/Edit Modal
// -----------------------------
function openModal(type) {
    clearData();

    if (type === "add") {
        $("#buildingTypeModalLabel").text("Add Building Type");
        $("#BtnSave").text("Save");
    }

    $("#buildingTypeModal").modal("show");
}


// -----------------------------
// Validation
// -----------------------------
function validate() {
    $(".form-error").text("");

    let isValid = true;

    const buildingTypeName = $("#BuildingTypeName").val().trim();
    const propertyType = $("#PropertyTypeIDF").val();

    if (!buildingTypeName) {
        $("#BuildingTypeNameError").text("Please enter Building Type Name.");
        isValid = false;
    }
    else if (buildingTypeName.length < 3) {
        $("#BuildingTypeNameError").text("Building Type Name must be at least 3 characters long.");
        isValid = false;
    }
    else if (buildingTypeName.length > 150) {
        $("#BuildingTypeNameError").text("Building Type Name cannot exceed 150 characters.");
        isValid = false;
    }

    if (!propertyType) {
        $("#PropertyTypeIDFError").text("Please select Property Type.");
        isValid = false;
    }

    return isValid;
}


// -----------------------------
// Save / Update
// -----------------------------
function save() {
    if (!validate()) return;

    const model = {
        BuildingTypeIDP: $("#BuildingTypeIDP").val(),
        BuildingTypeName: $("#BuildingTypeName").val().trim(),
        PropertyTypeIDF: $("#PropertyTypeIDF").val()
    };

    $.ajax({
        url: baseURL + "BuildingTypeMaster/Save",
        type: "POST",
        data: model,
        success: function (res) {

            const { outval, outmsg, fieldErrors } = res;

            // CASE 1: Success
            if (outval == 1) {
                showToast(outmsg, "success");
                $("#buildingTypeModal").modal("hide");
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


// -----------------------------
// Edit
// -----------------------------
function edit(id) {
    $.ajax({
        url: baseURL + "BuildingTypeMaster/GetByID",
        type: "POST",
        data: { id },
        success: function (res) {
            if (res.success && res.data) {

                const data = res.data;

                $("#BuildingTypeIDP").val(data.buildingTypeIDP);
                $("#BuildingTypeName").val(data.buildingTypeName || "");
                $("#PropertyTypeIDF").val(data.propertyTypeIDF).trigger("change");

                $("#buildingTypeModalLabel").text("Edit Building Type");
                $("#BtnSave").text("Update");

                $("#buildingTypeModal").modal("show");
            }
            else {
                showToast("No data found!", "warning");
            }
        },
        error: function (xhr, status, error) {
            console.error(xhr);

            // Show simple message to user
            showToast("Unable to process your request. Please try again.", "error");
        }

    });
}


// -----------------------------
// Load DataTable (Paging)
// -----------------------------
function getData() {

    if (!$.fn.DataTable.isDataTable("#dataTBL")) {

        $("#dataTBL").DataTable({
            processing: true,
            serverSide: true,
            ordering: false,
            ajax: {
                url: baseURL + "BuildingTypeMaster/GetDataWithPaging",
                type: "POST"
            },
            columns: [
                { data: "srNo" },
                { data: "buildingTypeName" },
                { data: "propertyTypeName" },

                {
                    data: "isActive",
                    render: function (data, type, row) {
                        return `
                            <button class="btn btn-sm ${data ? "btn-success" : "btn-danger"}"
                                    onclick="generalAction(${row.buildingTypeIDP}, 2)">
                                ${data ? "Active" : "Inactive"}
                            </button>`;
                    }
                },

                {
                    data: "buildingTypeIDP",
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
            ],
            language: {
                paginate: {
                    previous: "<i class='bi bi-chevron-left'></i>",
                    next: "<i class='bi bi-chevron-right'></i>"
                }
            }
        });

    } else {
        $("#dataTBL").DataTable().ajax.reload();
    }
}


// -----------------------------
// Delete / Status Change
// -----------------------------
function generalAction(id, actionType) {
    let title = 'Are you sure?';
    let text = actionType === 1
        ? "Do you really want to delete this user?"
        : "Do you want to update the user status?";
    Swal.fire({
        title: title,
        text: text,
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Yes, Confirm",
        cancelButtonText: "Cancel"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: baseURL + "BuildingTypeMaster/GeneralAction",
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


// -----------------------------
// Clear Form
// -----------------------------
function clearData() {
    $("#BuildingTypeIDP").val("");
    $("#BuildingTypeName").val("");
    $("#PropertyTypeIDF").val("").trigger("change");

    $(".form-error").text("");
}
