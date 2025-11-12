$(document).ready(function () {
    GetDataWithPaging();
    $('#userModal').on('shown.bs.modal', function () {
        Select2Helper('userModal');
    });
});

function OpenModal(type) {
    ClearData();
    if (type === 'add') {
        $("#userModalLabel").text("Add New User");
        $("#BtnSaveUser").text("Save");
        $("#passwordFields").show(); // Show password fields for add mode
    }
    $("#userModal").modal("show");
}

function validate() {
    // Clear previous errors
    $("#fullNameError, #emailError, #mobileNumberError, #roleError, #passwordError, #confirmPasswordError").text("");
    const userIDP = $("#Id").val() || 0;
    const fullName = $("#fullName").val().trim();
    const email = $("#email").val().trim();
    const mobileNumber = $("#mobileNumber").val().trim();
    const role = $("#role").val();
    const password = $("#password").val().trim();
    const confirmPassword = $("#confirmPassword").val().trim();
    let isValid = true;
    let firstInvalidField = null;
    if (!role) {
        $("#roleError").text("Please select a Role.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#role";
        // Move error message after the visible select2 container
        const select2Container = $("#role").next(".select2-container");
        if (select2Container.length > 0) {
            $("#roleError").insertAfter(select2Container);
        }
    }
    // Validate Full Name
    if (!fullName) {
        $("#fullNameError").text("Please enter Full Name.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#fullName";
    } else if (fullName.length > 200) {
        $("#fullNameError").text("Full name cannot exceed 200 characters.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#fullName";
    } else if (/[@#$%]/.test(fullName)) {
        $("#fullNameError").text("Full name cannot contain @, #, $, or % characters.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#fullName";
    }
    // Validate Email
    if (!email) {
        $("#emailError").text("Please enter Email.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#email";
    } else if (!emailRegex.test(email)) {
        $("#emailError").text("Please enter a valid email address.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#email";
    }
    // Validate Mobile Number
    if (!mobileNumber) {
        $("#mobileNumberError").text("Please enter Mobile Number.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#mobileNumber";
    } else if (!mobileNumberRegex.test(mobileNumber)) {
        $("#mobileNumberError").text("Please enter a valid mobile number.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#mobileNumber";
    } else if (mobileNumber.length > 15) {
        $("#mobileNumberError").text("Mobile number cannot exceed 15 characters.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#mobileNumber";
    }
    if (userIDP == 0) {
        if (!password) {
            $("#passwordError").text("Please enter Password.");
            isValid = false;
            if (!firstInvalidField) firstInvalidField = "#password";
        } else if (password.length < 6) {
            $("#passwordError").text("Password must be at least 6 characters long.");
            isValid = false;
            if (!firstInvalidField) firstInvalidField = "#password";
        }
        if (!confirmPassword) {
            $("#confirmPasswordError").text("Please enter confirm Password.");
            isValid = false;
            if (!firstInvalidField) firstInvalidField = "#confirmPassword";
        } else if (password !== confirmPassword) {
            $("#confirmPasswordError").text("Passwords do not match!");
            isValid = false;
            if (!firstInvalidField) firstInvalidField = "#confirmPassword";
        }
    }
    // Focus on the first invalid field if any
    if (firstInvalidField) {
        $(firstInvalidField).focus();
    }
    return isValid;
}

function Save() {
    const isValid = validate();
    if (!isValid) return;

    const fullName = $("#fullName").val().trim();
    const email = $("#email").val().trim();
    const mobileNumber = $("#mobileNumber").val().trim();
    const role = $("#role").val();
    const userIDP = $("#Id").val() || 0;

    const model = {
        UserIDP: userIDP,
        FullName: fullName,
        Email: email,
        PhoneNumber: mobileNumber,
        Role: role
    };

    // Only include password fields for new user
    if (userIDP == 0) {
        model.Password = $("#password").val();
        model.ConfirmPassword = $("#confirmPassword").val();
    }

    $.ajax({
        url: "/Register/Save",
        type: "POST",
        data: model,
        success: function (res) {
            if (res.code === 1) {
                // Success
                showToast(res.message || "User saved successfully.", "success");
                $("#userModal").modal("hide");
                GetDataWithPaging();
                ClearData();
            }
            else if (res.code === 99) {
                // Warning: duplicate email or phone
                showToast(res.message || "Email or phone number already exists.", "warning");
            }
            else {
                // Error
                showToast(res.message || "User save failed.", "error");
            }
        },
        error: function (xhr) {
            const errorMsg = xhr.responseJSON ? xhr.responseJSON.message : "An error occurred.";
            showToast(errorMsg, "error");
        }
    });
}

function edit(id) {
    console.log(id);
    $.ajax({
        url: '/Register/GetByID',
        type: 'POST',
        data: { id: id },
        success: function (res) {
            if (res.success && res.data) {
                const data = res.data; // already a JSON object
                console.log(data)
                $("#Id").val(data.userIDP);
                $("#fullName").val(data.fullName);
                $("#email").val(data.email);
                $("#mobileNumber").val(data.phoneNumber);
                // Fixed: Use roleId (or equivalent value field) instead of roleName to match <option value>
                $("#role").val(data.roleName).trigger('change'); // Assuming backend returns 'roleId' as the value; adjust if named differently (e.g., 'role')
                $("#userModalLabel").text("Edit User");
                $("#BtnSaveUser").text("Update");
                // Hide password fields in edit mode
                $("#passwordFields").hide();
                $("#userModal").modal("show");
            } else {
                showToastNoData();
            }
        },
        error: function () {
            showToast("Error loading user details.", "error");
        }
    });
}

function GetDataWithPaging() {
    if (!$.fn.DataTable.isDataTable('#dataTbl')) {
        $('#dataTbl').DataTable({
            "processing": true,
            "serverSide": true,
            "autoWidth": false,
            "ajax": {
                url: window.baseURL + "Register/GetDataWithPaging",
                type: "POST",
                datatype: "json"
            },
            "columns": [
                { "data": "srNo", "orderable": false },
                { "data": "fullName", "orderable": false },
                { "data": "email", "orderable": false },
                { "data": "phoneNumber", "orderable": false },
                { "data": "roleName", "orderable": false },
                {
                    "data": "isActive",
                    "orderable": false,
                    "render": function (data, type, row) {
                        return `
                            <button class="btn btn-sm ${data ? 'btn-success' : 'btn-danger'}"
                                    onclick="GeneralAction(${row.userIDP}, 2)">
                                ${data ? "Active" : "Inactive"}
                            </button>
                        `;
                    }
                },
                {
                    "data": "userIDP",
                    "orderable": false,
                    "render": function (data, type, row) {
                        return `
                            <button type="button" class="btn btn-primary btn-sm"
                                    onclick="edit('${data}')"
                                    data-bs-toggle="tooltip" data-bs-placement="top" title="Edit">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button type="button" class="btn btn-danger btn-sm"
                                    onclick="GeneralAction(${data}, 1)"
                                    data-bs-toggle="tooltip" data-bs-placement="top" title="Delete">
                                <i class="bi bi-trash"></i>
                            </button>
                        `;
                    }
                }
            ]
            // Removed invalid "order" index
        });
    } else {
        $('#dataTbl').DataTable().ajax.reload();
    }
}

function GeneralAction(id, actionType) {
    let title = 'Are you sure?';
    let text = actionType === 1
        ? "Do you really want to delete this user?"
        : "Do you want to update the user status?";
    Swal.fire({
        title: title,
        text: text,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Confirm',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: "POST",
                url: "/Register/GeneralAction",
                data: { ID: id, ActionType: actionType }, // Note: keys match C# method parameters
                success: function (mRes) {
                    showToast(mRes.message || "Action completed successfully.", mRes.success ? "success" : "error");
                    GetDataWithPaging();
                },
                error: function (xhr) {
                    const errorMsg = xhr.responseJSON ? xhr.responseJSON.message : "An error occurred.";
                    showToast(errorMsg, "error");
                }
            });
        }
    });
}

function ClearData() {
    $("#Id").val('');
    $("#fullName").val('');
    $("#email").val('');
    $("#mobileNumber").val('');
    $("#role").val('').trigger('change'); // Added trigger('change') for select2 to properly reset
    $("#password").val('');
    $("#confirmPassword").val('');
    $("#fullNameError, #emailError, #mobileNumberError, #roleError, #passwordError, #confirmPasswordError").text('');
}