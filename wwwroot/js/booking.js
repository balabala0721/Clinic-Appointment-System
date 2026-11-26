// Book page: step-by-step flow  1 specialisation -> 2 doctor -> 3 date -> 4 time -> 5 confirm -> 6 done.
// One <section data-step="n"> is visible at a time; the stepper (#stepper) shows progress.
// All names/text go on the page with .text() (XSS-safe).
$(function () {
    const $root = $('#booking');
    if ($root.length === 0) return;

    const $stepper = $('#stepper');
    const $date = $('#date');
    const $slots = $('#slots');
    const $bookBtn = $('#book-btn');
    const $msg = $('#book-msg');
    const $scheduleLink = $('#doctor-schedule-link');

    const state = { step: 1, spec: null, doctor: null, date: $date.val(), time: null, timeLabel: null };

    // ------------------------------------------------------------- navigation

    function goTo(step, quiet) {
        state.step = step;
        $root.find('[data-step]').each(function () {
            this.hidden = parseInt(this.getAttribute('data-step'), 10) !== step;
        });
        $stepper.find('[data-step-indicator]').each(function () {
            const n = parseInt(this.getAttribute('data-step-indicator'), 10);
            $(this).toggleClass('is-done', n < step).toggleClass('is-current', n === step)
                   .attr('aria-current', n === step ? 'step' : null);
        });
        $stepper.prop('hidden', step === 6);

        if (step === 4) loadTimes();
        if (step === 5) fillSummary();

        if (quiet) return;      // first render: don't move focus or scroll
        // Move focus to the new step's title so screen readers announce it
        const $title = $root.find('[data-step="' + step + '"] [tabindex="-1"]').first();
        if ($title.length) $title[0].focus({ preventScroll: true });
        $stepper[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    $root.on('click', '[data-go]', function () { goTo(parseInt($(this).data('go'), 10)); });
    $root.on('click', '[data-next]', function () { goTo(parseInt($(this).data('next'), 10)); });

    // ------------------------------------------------------- 1. specialisation

    $root.on('change', 'input[name="spec"]', function () {
        state.spec = this.value;
        $('[data-step="1"] [data-next]').prop('disabled', false);
        filterDoctors();
    });

    function filterDoctors() {
        $root.find('input[name="doctor"]').each(function () {
            const match = state.spec === '' || state.spec === null || String($(this).data('spec')) === state.spec;
            $(this).closest('.choice').prop('hidden', !match);
            if (!match && this.checked) { this.checked = false; selectDoctor(null); }
        });
    }

    // --------------------------------------------------------------- 2. doctor

    $root.on('change', 'input[name="doctor"]', function () { selectDoctor(this); });

    function selectDoctor(input) {
        state.doctor = input ? {
            id: input.value,
            name: $(input).data('name'),
            spec: $(input).data('specName'),
            fee: $(input).data('fee')
        } : null;
        state.time = null;
        $('[data-step="2"] [data-next]').prop('disabled', !state.doctor);
        $root.find('.js-doctor-name').text(state.doctor ? state.doctor.name : '');
        $scheduleLink.toggleClass('d-none', !state.doctor)
                     .attr('href', state.doctor ? '/DoctorSchedules/Doctor/' + state.doctor.id : '#');
    }

    // ----------------------------------------------------------------- 3. date

    function setDate(iso) {
        state.date = iso;
        state.time = null;
        $date.val(iso);
        $root.find('.js-day').each(function () {
            const on = $(this).data('date') === iso;
            $(this).toggleClass('active', on).attr('aria-pressed', on);
        });
        $('[data-step="3"] [data-next]').prop('disabled', !iso);
    }

    $root.on('click', '.js-day', function () { setDate($(this).data('date')); goTo(4); });
    $date.on('change', function () { setDate($date.val()); });

    // ----------------------------------------------------------------- 4. time

    function loadTimes() {
        state.time = null;
        $('[data-step="4"] [data-next]').prop('disabled', true);
        $root.find('.js-date-label').text(dateLabel(state.date));
        if (!state.doctor || !state.date) { goTo(state.doctor ? 3 : 2); return; }

        Clinic.loadSlots(state.doctor.id, state.date, $slots, function (value, label) {
            state.time = value;
            state.timeLabel = label;
            $('[data-step="4"] [data-next]').prop('disabled', false);
        });
    }

    // A booking made in the support chat changes which times are free
    $(document).on('clinic:appointmentsChanged', function () { if (state.step === 4) loadTimes(); });

    // -------------------------------------------------------------- 5. confirm

    function fillSummary() {
        Clinic.inline($msg, '', false);
        $('#summary-doctor').text(state.doctor.name);
        $('#summary-spec').text(state.doctor.spec || '-');
        $('#summary-date').text(dateLabel(state.date));
        $('#summary-time').text(state.timeLabel);
        $('#summary-fee').text(state.doctor.fee);
    }

    $bookBtn.on('click', function () {
        if (!state.time) { goTo(4); return; }
        Clinic.inline($msg, '', false);
        Clinic.busy($bookBtn, true);

        Clinic.postJson('/Appointments/Book', {
            doctorId: parseInt(state.doctor.id, 10),
            date: state.date,
            startTime: state.time,
            reason: $('#reason').val()
        })
            .done(function (res) {
                if (res.success) {
                    $('#done-message').text(res.message);
                    $('#done-doctor').text(state.doctor.name);
                    $('#done-when').text(dateLabel(state.date) + ', ' + state.timeLabel);
                    $('#reason').val('');
                    goTo(6);
                    Clinic.toast('Appointment booked.', true);
                } else {
                    // e.g. someone else just took the slot - explain and let them pick again
                    Clinic.inline($msg, res.message + ' Go back to choose another time.', false);
                }
            })
            .fail(function (xhr) { Clinic.inline($msg, Clinic.errorMessage(xhr), false); })
            .always(function () { Clinic.busy($bookBtn, false); });
    });

    $root.on('click', '[data-restart]', function () {
        $root.find('input[name="spec"], input[name="doctor"]').prop('checked', false);
        state.spec = null;
        selectDoctor(null);
        filterDoctors();
        $('[data-step="1"] [data-next]').prop('disabled', true);
        setDate($date.attr('min'));
        goTo(1);
    });

    // ---------------------------------------------------------------- helpers

    // "2026-11-28" -> "Fri, 28/11/2026"
    function dateLabel(iso) {
        if (!iso) return '';
        const p = iso.split('-');
        const d = new Date(parseInt(p[0], 10), parseInt(p[1], 10) - 1, parseInt(p[2], 10));
        return d.toLocaleDateString('en-GB', { weekday: 'short' }) + ', ' + Clinic.fmtDate(iso);
    }

    // ------------------------------------------------------------------ start

    // Came from "Find a doctor" or a doctor's schedule: ?doctorId=..&date=..  -> jump to the times
    const pre = String($root.data('preselectedDoctor') || '');
    const $preDoctor = pre ? $root.find('input[name="doctor"][value="' + pre.replace(/\D/g, '') + '"]') : $();
    setDate(state.date);
    if ($preDoctor.length) {
        const spec = String($preDoctor.data('spec'));
        $root.find('input[name="spec"][value="' + spec.replace(/\D/g, '') + '"]').prop('checked', true).trigger('change');
        $preDoctor.prop('checked', true).trigger('change');
        goTo(4, true);
    } else {
        goTo(1, true);
    }
});
