// AI Doctor Recommendation page.
$(function () {
    const URL = '/DoctorRecommendation/Recommend';
    const $result = $('#recommend-result');
    const $btn = $('#recommend-btn');

    $('#recommend-form').on('submit', function (e) {
        e.preventDefault();
        const symptoms = $('#symptoms').val().trim();
        if (symptoms.length < 3) return;

        $btn.prop('disabled', true).text('Thinking...');
        $result.empty();

        Clinic.postJson(URL, { symptoms: symptoms })
            .done(render)
            .fail(function (xhr) { showError(Clinic.errorMessage(xhr)); })
            .always(function () { $btn.prop('disabled', false).text('Get suggestion'); });
    });

    function render(res) {
        if (!res.success) return showError(res.message);

        if (res.isEmergency) {
            $('<div class="alert alert-danger">')
                .append($('<strong>').text('Possible emergency. '))
                .append(document.createTextNode(res.message))
                .appendTo($result);
            return;
        }

        $('<h2 class="h6 mb-1">').text('Suggested: ' + res.specialisation).appendTo($result);
        if (res.reason) $('<p class="mb-3">').text(res.reason).appendTo($result);
        if (res.urgency === 'urgent') {
            $('<div class="alert alert-warning py-2">').text('Try to be seen today. If symptoms get worse, go to an emergency department.').appendTo($result);
        }

        res.doctors.forEach(function (d) {
            const $card = $('<div class="border rounded p-3 mb-2">');
            const $head = $('<div class="d-flex justify-content-between mb-2">').appendTo($card);
            $('<strong>').text(d.name).appendTo($head);
            $('<span class="text-muted small">').text('RM ' + d.fee).appendTo($head);

            const $slots = $('<div class="slot-grid">').appendTo($card);
            if (d.slots.length === 0) {
                $('<span class="text-muted small">').text('No free slots soon.').appendTo($slots);
            }
            d.slots.forEach(function (s) {
                // Hands over to the normal booking page with this doctor preselected.
                $('<a class="btn btn-outline-primary btn-sm">')
                    .attr('href', '/Appointments/Book?doctorId=' + d.doctorId + '&date=' + s.date)
                    .text(s.dateLabel + ' ' + s.time)
                    .appendTo($slots);
            });
            $('<a class="small d-inline-block mt-2">')
                .attr('href', '/DoctorSchedules/Doctor/' + d.doctorId)
                .text('View weekly schedule')
                .appendTo($card);
            $card.appendTo($result);
        });

        $('<p class="text-muted small mt-2 mb-0">').text(res.message).appendTo($result);
    }

    function showError(message) {
        $('<div class="alert alert-secondary">').text(message).appendTo($result);
    }
});
