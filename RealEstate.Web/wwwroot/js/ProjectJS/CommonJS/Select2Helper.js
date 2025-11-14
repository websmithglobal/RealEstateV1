function Select2Helper(modalId, type = null) {
    $('#' + modalId + ' .select2-modal').each(function () {
        $(this).select2({
            theme: "bootstrap-5",
            dropdownParent: $('#' + modalId),   // Keep dropdown inside modal
            width: $(this).data('width')
                ? $(this).data('width')
                : $(this).hasClass('w-100')
                    ? '100%'
                    : 'style',
            placeholder: $(this).data('placeholder') || 'Select an option',
            allowClear: true
        });

        // Optional: custom styling based on 'type'
        // if (type === 1) {
        //     $(this).next('.select2-container').css('min-height', '45px');
        // }
    });
}