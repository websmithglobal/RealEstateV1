function Select2Helper(modalId, type = null) {
    // Initialize Select2 inside the modal
    $('#' + modalId + ' .select2-modal').each(function () {
        $(this).select2({
            dropdownParent: $('#' + modalId),
            width: '100%'
        });

        // Optional: custom styling (e.g., increased dropdown height)
        //if (type === 1) {
        //    $(this).next('.select2-container').css('min-height', '45px');
        //}
    });
}