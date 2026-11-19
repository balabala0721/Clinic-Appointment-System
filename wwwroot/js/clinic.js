// Shared helpers for the appointment + scheduling pages.
// Rule used everywhere: put server/AI text on the page with .text(), never .html(),
// so a name or AI reply containing <script> can't run (XSS).

const Clinic = {
    slotsUrl: '/Appointments/Slots',

    // Anti-forgery token rendered once in _Layout. ASP.NET reads it from this header.
    token() {
        return $('input[name="__RequestVerificationToken"]').first().val();
    },

    postJson(url, data) {
        return $.ajax({
            url: url,
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: { 'RequestVerificationToken': Clinic.token() }
        });
    },

    errorMessage(xhr) {
        if (xhr.status === 429) return 'Too many requests. Please wait a minute and try again.';
        if (xhr.status === 401 || xhr.status === 403) return 'Please log in again.';
        return 'Something went wrong. Please try again.';
    },

    showAlert(message, ok) {
        $('#ajax-alert')
            .removeClass('d-none alert-success alert-danger')
            .addClass(ok ? 'alert-success' : 'alert-danger')
            .text(message);
        window.scrollTo({ top: 0, behavior: 'smooth' });
    },

    // Loads free slots and renders them as buttons inside $container.
    // onPick(value, label) runs when the user clicks one.
    loadSlots(doctorId, date, $container, onPick) {
        $container.empty();
        if (!doctorId || !date) return;

        $container.append($('<p class="text-muted small mb-0">').text('Loading...'));
        $.getJSON(Clinic.slotsUrl, { doctorId: doctorId, date: date })
            .done(function (slots) {
                $container.empty();
                if (slots.length === 0) {
                    $container.append($('<p class="text-muted small mb-0">').text('No free slots on this day.'));
                    return;
                }
                slots.forEach(function (s) {
                    $('<button type="button" class="btn btn-outline-primary btn-sm slot-btn">')
                        .text(s.label)
                        .on('click', function () {
                            $container.find('.slot-btn').removeClass('active');
                            $(this).addClass('active');
                            onPick(s.value, s.label);
                        })
                        .appendTo($container);
                });
            })
            .fail(function () {
                $container.empty().append($('<p class="text-danger small mb-0">').text('Could not load slots.'));
            });
    }
};
