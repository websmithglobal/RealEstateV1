
$(document).ready(function () {
    getData();
    $('#userModal').on('shown.bs.modal', function () {
        Select2Helper('userModal');
    });

});

function openModal(type) {
    ClearData();
    if (type === 'add') {
        $("#userModalLabel").text("Add New User");
        $("#BtnSaveUser").text("Save");
        $("#passwordFields").show();
    }
    $("#userModal").modal("show");
}

function validate() {
    // Clear previous errors
    $("#FullNameError, #EmailError, #PhoneNumberError, #RoleError, #PasswordError, #ConfirmPasswordError").text("");
    const userIDP = $("#Id").val() || 0;
    const fullName = $("#FullName").val().trim();
    const email = $("#Email").val().trim();
    const mobileNumber = $("#PhoneNumber").val().trim();
    const role = $("#Role").val();
    const password = $("#Password").val().trim();
    const confirmPassword = $("#ConfirmPassword").val().trim();
    let isValid = true;
    let firstInvalidField = null;
    if (!role) {
        $("#RoleError").text("Please select a role.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#Role";
        // Move error message after the visible select2 container
        const select2Container = $("#Role").next(".select2-container");
        if (select2Container.length > 0) {
            $("#RoleError").insertAfter(select2Container);
        }
    }

    // Validate Full Name
    if (!fullName) {
        $("#FullNameError").text("Please enter full name.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#FullName";
    }
    else if (!/^[A-Za-z\s]+$/.test(fullName)) {
        $("#FullNameError").text("Full name can contain only alphabets and spaces.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#FullName";
    }
    else if (fullName.length < 5) {
        $("#FullNameError").text("Full name must be at least 5 characters long.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#FullName";
    }
    else if (fullName.length > 200) {
        $("#FullNameError").text("Full name cannot exceed 200 characters.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#FullName";
    }
    else {
        $("#FullNameError").text("");
    }

    // Validate Email
    if (!email) {
        $("#EmailError").text("Please enter email.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#Email";
    }
    else if (email.length < 5) {
        $("#EmailError").text("Email must be at least 5 characters long.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#Email";
    }
    else if (email.length > 200) {
        $("#EmailError").text("Email cannot exceed 200 characters.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#Email";
    }
    else if (!emailRegex.test(email)) {
        $("#EmailError").text("Please enter a valid email address.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#Email";
    }
    else {
        $("#EmailError").text("");
    }

    // Validate Mobile Number
    if (!mobileNumber) {
        $("#PhoneNumberError").text("Please enter phone number.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#PhoneNumber";
    }
    else if (!mobileNumberRegex.test(mobileNumber)) {
        $("#PhoneNumberError").text("Please enter a valid phone number.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#PhoneNumber";
    }
    else if (mobileNumber.length < 5) {
        $("#PhoneNumberError").text("Phone number must be at least 5 digits long.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#PhoneNumber";
    }
    else if (mobileNumber.length > 15) {
        $("#PhoneNumberError").text("Phone number cannot exceed 15 digits.");
        isValid = false;
        if (!firstInvalidField) firstInvalidField = "#PhoneNumber";
    }
    else {
        $("#PhoneNumberError").text("");
    }

    if (userIDP == 0) {
        // Validate Password
        const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]).{6,20}$/;

        if (!password) {
            $("#PasswordError").text("Please enter password.");
            isValid = false;
            if (!firstInvalidField) firstInvalidField = "#Password";
        }
        else if (password.length < 6) {
            $("#PasswordError").text("Password must be at least 6 characters long.");
            isValid = false;
            if (!firstInvalidField) firstInvalidField = "#Password";
        }
        else if (password.length > 20) {
            $("#PasswordError").text("Password cannot exceed 20 characters.");
            isValid = false;
            if (!firstInvalidField) firstInvalidField = "#Password";
        }
        else if (!passwordRegex.test(password)) {
            $("#PasswordError").text("Password must contain at least one lowercase, one uppercase, one digit, and one special character.");
            isValid = false;
            if (!firstInvalidField) firstInvalidField = "#Password";
        }
        else {
            $("#PasswordError").text("");
        }

        // Validate Confirm Password
        if (!confirmPassword) {
            $("#ConfirmPasswordError").text("Please enter confirm password.");
            isValid = false;
            if (!firstInvalidField) firstInvalidField = "#ConfirmPassword";
        }
        else if (password !== confirmPassword) {
            $("#ConfirmPasswordError").text("Passwords do not match!");
            isValid = false;
            if (!firstInvalidField) firstInvalidField = "#ConfirmPassword";
        }
        else {
            $("#ConfirmPasswordError").text("");
        }
    }

    // Focus on the first invalid field if any
    if (firstInvalidField) {
        $(firstInvalidField).focus();
    }
    return isValid;
}

function save() {
    const isValid = validate();
    if (!isValid) return;

    const fullName = $("#FullName").val().trim();
    const email = $("#Email").val().trim();
    const mobileNumber = $("#PhoneNumber").val().trim();
    const role = $("#Role").val();
    const userIDP = $("#Id").val() || 0;

    const model = {
        UserIDP: userIDP,
        FullName: fullName,
        Email: email,
        PhoneNumber: mobileNumber,
        Role: role
    };

    if (userIDP == 0) {
        model.Password = $("#Password").val();
        model.ConfirmPassword = $("#ConfirmPassword").val();
    }

    $.ajax({
        url: baseURL + "Register/Save",
        type: "POST",
        data: model,
        success: function (res) {

            const { outval, outmsg, fieldErrors } = res;

            // CASE 1: Success
            if (outval == 1) {
                showToast(outmsg, "success");
                $("#userModal").modal("hide");
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
        error: function (xhr) {
            const errorMsg = xhr.responseJSON ? xhr.responseJSON.Outmsg || xhr.responseJSON.message : "An error occurred.";
            showToast(errorMsg, "error");
        }
    });
}

function edit(id) {
    $.ajax({
        url: baseURL + 'Register/GetByID',
        type: 'POST',
        data: { id: id },
        success: function (res) {
            if (res.success && res.data) {
                console.log(res)
                const data = res.data; // already a JSON object
                $("#Id").val(data.userIDP);
                $("#FullName").val(data.fullName);
                $("#Email").val(data.email);
                $("#PhoneNumber").val(data.phoneNumber);
                $("#Role").val(data.roleName).trigger('change');
                console.log(data.roleName)
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

function getData() {
    if (!$.fn.DataTable.isDataTable('#dataTBL')) {
        $('#dataTBL').DataTable({
            "processing": true,
            "serverSide": true,
            "ordering": false,
            "autoWidth": false,
            "ajax": {
                url: baseURL + "Register/GetDataWithPaging",
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
                                    onclick="generalAction(${row.userIDP}, 2)">
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
                                <i class="fadeIn animated bx bx-pencil"></i>
                            </button>
                            <button type="button" class="btn btn-danger btn-sm"
                                    onclick="generalAction(${data}, 1)"
                                    data-bs-toggle="tooltip" data-bs-placement="top" title="Delete">
                                <i class="fadeIn animated bx bx-trash-alt"></i>
                            </button>
                        `;
                    }
                }
            ]
        });
    } else {
        $('#dataTBL').DataTable().ajax.reload();
    }
}

function generalAction(id, actionType) {
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
                url: baseURL + "Register/GeneralAction",
                data: { ID: id, ActionType: actionType }, // Note: keys match C# method parameters
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
    $("#FullName").val('');
    $("#Email").val('');
    $("#PhoneNumber").val('');
    $("#Role").val('').trigger('change'); 
    $("#Password").val('');
    $("#ConfirmPassword").val('');
    $(".form-error").text('');
}