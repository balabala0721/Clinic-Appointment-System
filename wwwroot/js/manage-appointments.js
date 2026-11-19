// Admin/staff: reschedule an appointment on behalf of a patient (FR 7.16).
// Uses the same /Appointments/Reschedule endpoint as patients; the server skips the
// 2-hour patient cutoff for staff but still checks the slot is free.
$(function () {
    $('.js-reschedule').on('click', function () {
        $('.reschedule-row[data-id="' + $(this).data('id') + '"]').toggleClass('d-none');
    });

    $('.reschedule-row .js-new-date').on('change', function () {
        const $row = $(this).closest('.reschedule-row');
        const $confirm = $row.find('.js-confirm-reschedule').prop('disabled', true);
        $row.removeData('newTime');

        Clinic.loadSlots($row.data('doctor'), $(this).val(), $row.find('.js-new-slots'), function (value) {
            $row.data('newTime', value);
            $confirm.prop('disabled', false);
        });
    });

    $('.reschedule-row .js-confirm-reschedule').on('click', function () {
        const $row = $(this).closest('.reschedule-row');
        $(this).prop('disabled', true);

        Clinic.postJson('/Appointments/Reschedule', {
            appointmentId: $row.data('id'),
            date: $row.find('.js-new-date').val(),
            startTime: $row.data('newTime')
        })
            .done(function (res) {
                Clinic.showAlert(res.message, res.success);
                if (res.success) setTimeout(function () { location.reload(); }, 800);
            })
            .fail(function (xhr) { Clinic.showAlert(Clinic.errorMessage(xhr), false); });
    });
});
