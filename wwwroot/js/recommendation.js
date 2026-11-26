// AI Doctor Recommendation page. All AI/server text goes on the page with .text().
$(function () {
    const URL = '/DoctorRecommendation/Recommend';
    const $result = $('#recommend-result');
    const $btn = $('#recommend-btn');
    const $msg = $('#recommend-msg');

    $('#recommend-form').on('submit', function (e) {
        e.preventDefault();
        const symptoms = $('#symptoms').val().trim();
        if (symptoms.length < 3) {
            Clinic.inline($msg, 'Please describe your symptoms in a few words.', false);
            $('#symptoms').addClass('is-invalid').trigger('focus');
            return;
        }
        $('#symptoms').removeClass('is-invalid');
        Clinic.inline($msg, '', false);

        Clinic.busy($btn, true, 'Thinking...');
        $result.empty();
        const $loading = $('<div class="card-clean">').appendTo($result);
        $('<div class="loading-row mb-3">').append($('<span class="spinner-border" aria-hidden="true">'))
            .append(document.createTextNode('Looking at your symptoms and the doctors\' free times...')).appendTo($loading);
        for (let i = 0; i < 3; i++) $('<span class="skeleton mb-2">').css('width', (90 - i * 20) + '%').appendTo($loading);

        Clinic.postJson(URL, { symptoms: symptoms })
            .done(render)
            .fail(function (xhr) { $result.empty(); showError(Clinic.errorMessage(xhr)); })
            .always(function () { Clinic.busy($btn, false); });
    });

    function render(res) {
        $result.empty();
        if (!res.success) return showError(res.message);

        if (res.isEmergency) {
            $('<div class="alert alert-danger" role="alert">')
                .append($('<i class="bi bi-exclamation-octagon" aria-hidden="true">'))
                .append($('<div>').append($('<strong>').text('Possible emergency. ')).append(document.createTextNode(res.message)))
                .appendTo($result);
            return;
        }

        const $card = $('<div class="card-clean">').appendTo($result);
        $('<div class="text-muted small">').text('Suggested specialisation').appendTo($card);
        $('<h2 class="h4 mb-2">').text(res.specialisation).appendTo($card);
        if (res.reason) $('<p>').text(res.reason).appendTo($card);
        if (res.urgency === 'urgent') {
            $('<div class="alert alert-warning" role="alert">')
                .append($('<i class="bi bi-exclamation-triangle" aria-hidden="true">'))
                .append($('<div>').text('Try to be seen today. If symptoms get worse, go to an emergency department.'))
                .appendTo($card);
        }

        if (res.doctors.length === 0) {
            Clinic.emptyState($card, 'bi-calendar-x', 'No free times soon', 'Try booking directly - new times open every day.', true);
        }

        res.doctors.forEach(function (d) {
            const $doc = $('<div class="border-top pt-3 mt-3">').appendTo($card);
            const $head = $('<div class="d-flex justify-content-between align-items-center gap-2 mb-2">').appendTo($doc);
            $('<strong>').text(d.name).appendTo($head);
            $('<span class="text-muted small">').text('RM ' + d.fee).appendTo($head);

            const $chips = $('<div class="slot-chips">').appendTo($doc);
            if (d.slots.length === 0) $('<span class="text-muted small">').text('No free times soon.').appendTo($chips);
            d.slots.forEach(function (s) {
                // Hands over to the normal booking page with this doctor and date preselected.
                $('<a class="slot-chip text-decoration-none d-inline-flex align-items-center justify-content-center">')
                    .attr('href', '/Appointments/Book?doctorId=' + encodeURIComponent(d.doctorId) + '&date=' + encodeURIComponent(s.date))
                    .text(s.dateLabel + ', ' + Clinic.fmtTime(s.time))
                    .appendTo($chips);
            });
            $('<a class="small d-inline-block mt-2">')
                .attr('href', '/DoctorSchedules/Doctor/' + encodeURIComponent(d.doctorId))
                .text('View weekly schedule')
                .appendTo($doc);
        });

        if (res.message) $('<p class="text-muted small mt-3 mb-0">').text(res.message).appendTo($card);
    }

    function showError(message) {
        $('<div class="alert alert-secondary" role="alert">')
            .append($('<i class="bi bi-info-circle" aria-hidden="true">'))
            .append($('<div>').text(message))
            .appendTo($result);
    }
});
