// Book page: specialisation filter -> doctor -> date -> slot -> book.
$(function () {
    const $spec = $('#specialisation');
    const $doctor = $('#doctor');
    const $date = $('#date');
    const $slots = $('#slots');
    const $bookBtn = $('#book-btn');
    const $scheduleLink = $('#doctor-schedule-link');
    let selectedTime = null;

    // Hide doctors that don't match the chosen specialisation.
    $spec.on('change', function () {
        const spec = $spec.val();
        $doctor.find('option[data-spec]').each(function () {
            $(this).prop('hidden', spec !== '' && $(this).data('spec').toString() !== spec);
        });
        if ($doctor.find('option:selected').prop('hidden')) $doctor.val('');
        refresh();
    });

    $doctor.on('change', refresh);
    $date.on('change', refresh);

    function refresh() {
        selectedTime = null;
        $bookBtn.prop('disabled', true);

        const doctorId = $doctor.val();
        $scheduleLink.toggleClass('d-none', !doctorId).attr('href', '/DoctorSchedules/Doctor/' + doctorId);

        Clinic.loadSlots(doctorId, $date.val(), $slots, function (value) {
            selectedTime = value;
            $bookBtn.prop('disabled', false);
        });
    }

    $bookBtn.on('click', function () {
        if (!selectedTime) return;
        $bookBtn.prop('disabled', true);

        Clinic.postJson('/Appointments/Book', {
            doctorId: parseInt($doctor.val(), 10),
            date: $date.val(),
            startTime: selectedTime,
            reason: $('#reason').val()
        })
            .done(function (res) {
                Clinic.showAlert(res.message, res.success);
                if (res.success) $('#reason').val('');
                refresh();      // slot list changes either way (booked, or someone else took it)
            })
            .fail(function (xhr) {
                Clinic.showAlert(Clinic.errorMessage(xhr), false);
                $bookBtn.prop('disabled', false);
            });
    });

    // A booking made in the support chat changes which slots are free
    $(document).on('clinic:appointmentsChanged', refresh);

    refresh();
});
