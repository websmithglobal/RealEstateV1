function Select2Helper(ControlName, type = null) {
    const $element = $('.select2-modal');

    $element.select2({
        dropdownParent: $('#' + ControlName),
        width: '100%'
    });

    // Add height only if 'type' is provided and equals 1
    if (type === 1) {
        $element.next('.select2-container').css('height','200%');
    }
}