// My appointments: cancel (with confirmation) + reschedule (same doctor, new date/time).
$(function () {
    // A change made in the support chat -> reload the list (the chat reopens where it was)
    $(document).on('clinic:appointmentsChanged', function () { location.reload(); });

    const cutoffHours = $('#my-appointments').data('cutoffHours');
    const ruleText = cutoffHours ? 'Changes are only possible up to ' + cutoffHours + ' hours before an appointment.' : '';

    // ---------------------------------------------------------------- cancel

    $('.js-cancel').on('click', function () {
        const $btn = $(this);
        const $row = $btn.closest('.appt-row');
        const $msg = $row.find('.js-row-msg');

        Clinic.confirm({
            title: 'Cancel this appointment?',
            message: 'Your appointment with ' + $row.data('summary') + ' will be cancelled and the time given to someone else. ' +
                     'This can\'t be undone - you would need to book again. ' + ruleText,
            confirmText: 'Cancel appointment',
            cancelText: 'Keep appointment',
            danger: true
        }).then(function () {
            Clinic.inline($msg, '', false);
            Clinic.busy($btn, true);
            Clinic.postJson('/Appointments/Cancel', { appointmentId: $row.data('id') })
                .done(function (res) {
                    if (!res.success) { Clinic.inline($msg, res.message, false); return; }
                    Clinic.toast(res.message, true);
                    $row.fadeOut(200, function () {
                        $row.remove();
                        const left = $('.appt-row').length;
                        $('#upcoming-count').text(left);
                        $('#upcoming-empty').prop('hidden', left > 0);
                    });
                })
                .fail(function (xhr) { Clinic.inline($msg, Clinic.errorMessage(xhr), false); })
                .always(function () { Clinic.busy($btn, false); });
        });
    });

    // ------------------------------------------------------------ reschedule

    $('.js-reschedule').on('click', function () {
        const $row = $(this).closest('.appt-row');
        const $panel = $row.find('.reschedule-panel').toggleClass('d-none');
        const open = !$panel.hasClass('d-none');
        $row.find('.appt-actions .js-reschedule').attr('aria-expanded', open);
        if (open) $panel.find('.js-new-date').trigger('focus');
    });

    $('.js-new-date').on('change', function () {
        const $row = $(this).closest('.appt-row');
        const $confirm = $row.find('.js-confirm-reschedule').prop('disabled', true);
        Clinic.inline($row.find('.js-msg'), '', false);
        $row.removeData('newTime');

        // Same doctor - only the date/time changes.
        Clinic.loadSlots($row.data('doctor'), $(this).val(), $row.find('.js-new-slots'), function (value) {
            $row.data('newTime', value);
            $confirm.prop('disabled', false);
        });
    });

    $('.js-confirm-reschedule').on('click', function () {
        const $btn = $(this);
        const $row = $btn.closest('.appt-row');
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
                location.reload();      // list order and IDs change
            })
            .fail(function (xhr) { Clinic.inline($msg, Clinic.errorMessage(xhr), false); Clinic.busy($btn, false); });
    });
});
