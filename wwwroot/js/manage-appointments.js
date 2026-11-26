// Admin/staff: reschedule an appointment on behalf of a patient (FR 7.16).
// Uses the same /Appointments/Reschedule endpoint as patients; the server skips the
// 2-hour patient cutoff for staff but still checks the slot is free.
$(function () {
    $('.js-reschedule').on('click', function () {
        const $btn = $(this);
        const $row = $('.reschedule-row[data-id="' + $btn.data('id') + '"]').toggleClass('d-none');
        const open = !$row.hasClass('d-none');
        $btn.attr('aria-expanded', open).text(open ? 'Close' : 'Reschedule');
        if (open) $row.find('.js-new-date').trigger('focus');
    });

    $('.reschedule-row .js-new-date').on('change', function () {
        const $row = $(this).closest('.reschedule-row');
        const $confirm = $row.find('.js-confirm-reschedule').prop('disabled', true);
        Clinic.inline($row.find('.js-msg'), '', false);
        $row.removeData('newTime');

        Clinic.loadSlots($row.data('doctor'), $(this).val(), $row.find('.js-new-slots'), function (value) {
            $row.data('newTime', value);
            $confirm.prop('disabled', false);
        });
    });

    $('.reschedule-row .js-confirm-reschedule').on('click', function () {
        const $btn = $(this);
        const $row = $btn.closest('.reschedule-row');
        const $msg = $row.find('.js-msg');
        Clinic.inline($msg, '', false);
        Clinic.busy($btn, true);

        Clinic.postJson('/Appointments/Reschedule', {
            appointmentId: $row.data('id'),
            date: $row.find('.js-new-date').val(),
            startTime: $row.data('newTime')
        })
            .done(function (res) {
                if (!res.success) { Clinic.inline($msg, res.message, false); Clinic.busy($btn, false); return; }
                Clinic.toastAfterReload(res.message);
                location.reload();
            })
            .fail(function (xhr) { Clinic.inline($msg, Clinic.errorMessage(xhr), false); Clinic.busy($btn, false); });
    });
});
