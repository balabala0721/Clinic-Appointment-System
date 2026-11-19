// My appointments: cancel + reschedule.
$(function () {
    // A change made in the support chat -> reload the list (the chat reopens where it was)
    $(document).on('clinic:appointmentsChanged', function () { location.reload(); });

    $('.js-cancel').on('click', function () {
        const $row = $(this).closest('.appt-row');
        if (!confirm('Cancel this appointment?')) return;

        Clinic.postJson('/Appointments/Cancel', { appointmentId: $row.data('id') })
            .done(function (res) {
                Clinic.showAlert(res.message, res.success);
                if (res.success) $row.remove();
            })
            .fail(function (xhr) { Clinic.showAlert(Clinic.errorMessage(xhr), false); });
    });

    $('.js-reschedule').on('click', function () {
        $(this).closest('.appt-row').find('.reschedule-panel').toggleClass('d-none');
    });

    $('.js-new-date').on('change', function () {
        const $row = $(this).closest('.appt-row');
        const $confirm = $row.find('.js-confirm-reschedule').prop('disabled', true);
        $row.removeData('newTime');

        // Same doctor - only the date/time changes.
        Clinic.loadSlots($row.data('doctor'), $(this).val(), $row.find('.js-new-slots'), function (value) {
            $row.data('newTime', value);
            $confirm.prop('disabled', false);
        });
    });

    $('.js-confirm-reschedule').on('click', function () {
        const $row = $(this).closest('.appt-row');
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
