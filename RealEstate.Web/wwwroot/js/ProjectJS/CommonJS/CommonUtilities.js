
// base URL
/*window.baseURL = '@Url.Content("~/")';*/


// Common RegExp
const emailRegex = /\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b/;
const mobileNumberRegex = /^(?=.*\d)[\d\s().-]{7,15}$/;

function showLoader() {
    document.getElementById("loader").style.display = "flex";
}

function hideLoader() {
    document.getElementById("loader").style.display = "none";
}

// Global Toaster Method
window.showToast = function (message, type = 'info') {
    // Define toast types and their corresponding titles
    const toastConfig = {
        success: { title: 'Success', method: 'success' },
        error: { title: 'Error', method: 'error' },
        warning: { title: 'Warning', method: 'warning' },
        info: { title: 'Info', method: 'info' }
    };

    // Get configuration for the specified type, default to info
    const config = toastConfig[type.toLowerCase()] || toastConfig.info;

    // Show the toast
    toastr[config.method](message, config.title);
};

// Global Toaster Method
window.showToastNoData = function () {
    // Show the toast
    toastr["error"]("No data found.", 'Error');
};

window.showWarning = function () {
    // Show the toast
    showToast('No data found.', 'warning');
};

// Alternative method that handles response objects directly
window.handleResponse = function (response, onSuccess = null) {
    if (response.outval == 1) {
        showToast(response.outmsg, 'success');
        if (typeof onSuccess === 'function') {
            onSuccess();
        }
    } else if (response.outval == 99) {
        showToast(response.outmsg, 'warning');
    } else {
        showToast(response.outmsg, 'error');
    }
};
function togglePasswordVisibility(inputId, iconId) {
    const input = document.getElementById(inputId);
    const icon = document.getElementById(iconId);

    const isPassword = input.type === "password";
    input.type = isPassword ? "text" : "password";

    icon.classList.toggle("ri-eye-line", !isPassword);
    icon.classList.toggle("ri-eye-off-line", isPassword);
}

// Example usage
function displayPassword() {
    togglePasswordVisibility("Password", "toggleIcon");
}

function displayCPassword() {
    togglePasswordVisibility("ConfirmPassword", "CtoggleIcon");
}

function clearValidationError(element) {
    $(element).removeClass("is-invalid");
}

function initDatepicker(selector) {
    $(selector).datepicker({
        format: "dd/mm/yyyy",
        autoclose: true,
        todayHighlight: true
    }).each(function () {
        const val = $(this).val();
        if (val) {
            $(this).datepicker('setDate', val);
        }
    });
}

function allowAlphanumeric(event, allowSpace) {
    var charCode = event.keyCode || event.which;
    var charStr = String.fromCharCode(charCode);
    var pattern;

    if (allowSpace) {
        pattern = /^[a-zA-Z0-9\s]+$/;
    }
    else {
        pattern = /^[a-zA-Z0-9]+$/;
    }

    if (!pattern.test(charStr)) {
        event.preventDefault();
        return false;
    }
    return true;
}
function allowAlphabet(event, allowSpace) {
    var charCode = event.keyCode || event.which;
    var charStr = String.fromCharCode(charCode);
    var pattern;

    if (allowSpace) {
        pattern = /^[a-zA-Z\s]+$/;
    }
    else {
        pattern = /^[a-zA-Z]+$/;
    }

    if (!pattern.test(charStr)) {
        event.preventDefault();
        return false;
    }
    return true;
}
function allowNumeric(event, allowDot) {
    var charCode = event.keyCode || event.which;
    var charStr = String.fromCharCode(charCode);
    var pattern;

    if (allowDot) {
        pattern = /^[0-9.]+$/;
    }
    else {
        pattern = /^[0-9]+$/;
    }

    if (!pattern.test(charStr)) {
        event.preventDefault();
        return false;
    }
    return true;
}