// Shared helpers for every page (loaded by _Layout).
// Rule used everywhere: put server/AI/user text on the page with .text(), never .html(),
// so a name or AI reply containing <script> can't run (XSS).
//
//   Clinic.postJson(url, data)          AJAX POST with the anti-forgery token
//   Clinic.busy($btn, true|false)       loading state: spinner + disabled
//   Clinic.toast(message, ok)           small notice, top-right (bottom on phones)
//   Clinic.inline($el, message, ok)     message right next to the control that failed
//   Clinic.confirm({...})               confirmation modal -> promise (never confirm())
//   Clinic.loadSlots(...)               time chips grouped Morning / Afternoon / Evening
//   Clinic.fmtTime('14:30')             "2:30 PM"  (same format as Ui.Time in C#)
//   Clinic.statusBadge('NoShow')        <span class="status-badge status-noshow">No-show</span>
//
// Plain (non-AJAX) forms get two behaviours for free:
//   data-confirm="message" on a form or submit button  -> confirmation modal first
//     optional: data-confirm-title, data-confirm-ok (button text), data-confirm-danger="false"
//   every submit button shows a spinner and is disabled while the page posts.

const Clinic = {
    slotsUrl: '/Appointments/Slots',

    // ------------------------------------------------------------------ AJAX

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
        if (xhr.status === 401 || xhr.status === 403) return 'Your session has ended or you don\'t have access. Please log in again.';
        if (xhr.status === 0) return 'Can\'t reach the server. Check your connection and try again.';
        return 'Something went wrong. Please try again.';
    },

    // --------------------------------------------------------------- feedback

    // Button loading state. Keeps the original content and restores it after.
    busy($btn, on, busyText) {
        $btn = $($btn);
        if (on) {
            if ($btn.data('busy')) return;
            $btn.data('busy', true).data('restore', $btn.contents().clone());
            const $spin = $('<span class="spinner-border" role="status" aria-hidden="true">');
            $btn.empty().append($spin);
            $btn.append(document.createTextNode(busyText || $btn.data('busyText') || 'Please wait...'));
            $btn.prop('disabled', true).addClass('is-busy').attr('aria-busy', 'true');
        } else {
            if (!$btn.data('busy')) return;
            $btn.empty().append($btn.data('restore'));
            $btn.removeData('busy').removeData('restore')
                .prop('disabled', false).removeClass('is-busy').removeAttr('aria-busy');
        }
    },

    toast(message, ok) {
        ok = ok !== false;
        const $stack = $('#toast-stack');
        const $toast = $('<div class="toast" role="status" aria-live="polite" aria-atomic="true">')
            .addClass(ok ? 'is-ok' : 'is-error');
        const $body = $('<div class="toast-body">').appendTo($toast);
        $('<i aria-hidden="true">').addClass(ok ? 'bi bi-check-circle-fill' : 'bi bi-exclamation-circle-fill').appendTo($body);
        $('<div>').text(message).appendTo($body);
        $('<button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close">').appendTo($body);
        $toast.appendTo($stack);
        const t = new bootstrap.Toast($toast[0], { delay: ok ? 4000 : 7000 });
        $toast.on('hidden.bs.toast', function () { $toast.remove(); });
        t.show();
    },

    // Message under/next to a control. $el is an element with class "inline-msg".
    inline($el, message, ok) {
        $($el).removeClass('is-error is-ok').addClass(ok ? 'is-ok' : 'is-error')
              .attr('role', ok ? 'status' : 'alert').text(message || '');
    },

    // Success -> toast. Failure -> inline next to the action when $where is given,
    // otherwise the page-level message box under the page title.
    showAlert(message, ok, $where) {
        if (ok) { Clinic.toast(message, true); return; }
        if ($where && $($where).length) { Clinic.inline($where, message, false); return; }
        const $box = $('#ajax-alert');
        if ($box.length) {
            $box.removeClass('d-none alert-success').addClass('alert-danger').attr('role', 'alert').text(message);
            $box[0].scrollIntoView({ behavior: 'smooth', block: 'center' });
        } else {
            Clinic.toast(message, false);
        }
    },

    // Confirmation modal. Resolves when the user confirms; nothing happens on cancel.
    //   Clinic.confirm({ title, message, confirmText, cancelText, danger }).then(...)
    confirm(opts) {
        opts = opts || {};
        const d = $.Deferred();
        const $m = $('#confirm-modal');
        $m.find('.modal-title').text(opts.title || 'Are you sure?');
        $m.find('.confirm-message').text(opts.message || '');
        $m.find('.confirm-cancel').text(opts.cancelText || 'Go back');
        const $ok = $m.find('.confirm-ok').text(opts.confirmText || 'Confirm')
            .removeClass('btn-primary btn-danger').addClass(opts.danger === false ? 'btn-primary' : 'btn-danger');

        const modal = bootstrap.Modal.getOrCreateInstance($m[0]);
        let confirmed = false;
        $ok.off('click.confirm').on('click.confirm', function () { confirmed = true; modal.hide(); });
        $m.off('hidden.bs.modal.confirm').on('hidden.bs.modal.confirm', function () {
            if (confirmed) d.resolve(); else d.reject();
        });
        modal.show();
        return d.promise();
    },

    // ------------------------------------------------------------- formatting

    // "14:30" or "14:30:00" -> "2:30 PM"
    fmtTime(hhmm) {
        const parts = String(hhmm || '').split(':');
        const h = parseInt(parts[0], 10), m = parts[1] || '00';
        if (isNaN(h)) return hhmm;
        return ((h % 12) || 12) + ':' + m + ' ' + (h < 12 ? 'AM' : 'PM');
    },

    // "2026-11-25" -> "25/11/2026"
    fmtDate(iso) {
        const p = String(iso || '').split('-');
        return p.length === 3 ? p[2] + '/' + p[1] + '/' + p[0] : iso;
    },

    statusLabel(status) {
        return { CheckedIn: 'Checked in', NoShow: 'No-show', PartiallyPaid: 'Partly paid' }[status] || status;
    },

    statusBadge(status) {
        return $('<span class="status-badge">').addClass('status-' + String(status).toLowerCase())
            .text(Clinic.statusLabel(status));
    },

    // ------------------------------------------------------------------ slots

    // Loads free slots and renders them as chips inside $container, grouped
    // Morning (<12), Afternoon (12-17) and Evening (17+).
    // onPick(value, label) runs when the user picks one. Returns the jqXHR.
    loadSlots(doctorId, date, $container, onPick) {
        $container.empty().attr('aria-busy', 'false');
        if (!doctorId || !date) return null;

        $container.attr('aria-busy', 'true');
        const $skeleton = $('<div class="slot-chips" aria-hidden="true">');
        for (let i = 0; i < 6; i++) $('<span class="skeleton skeleton-chip">').appendTo($skeleton);
        $container.append($('<span class="visually-hidden">').text('Loading available times...'), $skeleton);

        return $.getJSON(Clinic.slotsUrl, { doctorId: doctorId, date: date })
            .done(function (slots) {
                $container.empty();
                if (slots.length === 0) {
                    Clinic.emptyState($container, 'bi-calendar-x', 'No free times on this day',
                        'Try another date - the doctor may be fully booked or not working that day.', true);
                    return;
                }
                const groups = { Morning: [], Afternoon: [], Evening: [] };
                slots.forEach(function (s) {
                    const h = parseInt(String(s.value).split(':')[0], 10);
                    groups[h < 12 ? 'Morning' : h < 17 ? 'Afternoon' : 'Evening'].push(s);
                });
                const icons = { Morning: 'bi-sunrise', Afternoon: 'bi-sun', Evening: 'bi-moon' };
                Object.keys(groups).forEach(function (name) {
                    if (groups[name].length === 0) return;
                    const $group = $('<div class="slot-group" role="group">').attr('aria-label', name);
                    $('<div class="slot-group-title">')
                        .append($('<i aria-hidden="true">').addClass('bi ' + icons[name]))
                        .append(document.createTextNode(name + ' (' + groups[name].length + ')'))
                        .appendTo($group);
                    const $chips = $('<div class="slot-chips">').appendTo($group);
                    groups[name].forEach(function (s) {
                        const label = Clinic.fmtTime(s.label);
                        $('<button type="button" class="slot-chip slot-btn" aria-pressed="false">')
                            .text(label)
                            .on('click', function () {
                                $container.find('.slot-btn').removeClass('active').attr('aria-pressed', 'false');
                                $(this).addClass('active').attr('aria-pressed', 'true');
                                onPick(s.value, label);
                            })
                            .appendTo($chips);
                    });
                    $group.appendTo($container);
                });
            })
            .fail(function (xhr) {
                $container.empty();
                $('<p class="inline-msg is-error mb-0" role="alert">')
                    .text('Could not load times. ' + Clinic.errorMessage(xhr)).appendTo($container);
            })
            .always(function () { $container.attr('aria-busy', 'false'); });
    },

    // Small empty state built safely (all text via .text()).
    emptyState($into, icon, title, text, compact) {
        const $e = $('<div class="empty-state">').toggleClass('is-compact', !!compact);
        $('<div class="empty-icon" aria-hidden="true">').append($('<i>').addClass('bi ' + icon)).appendTo($e);
        $('<div class="empty-title">').text(title).appendTo($e);
        if (text) $('<p class="empty-text">').text(text).appendTo($e);
        return $e.appendTo($into);
    }
};

// --------------------------------------------------------------------------
// Page-wide behaviour for normal (non-AJAX) forms
// --------------------------------------------------------------------------
$(function () {
    // Confirmation for destructive form posts: <button data-confirm="..."> or <form data-confirm="...">
    $(document).on('submit', 'form', function (e) {
        const form = this;
        const submitter = (e.originalEvent && e.originalEvent.submitter) || null;
        const source = submitter && submitter.hasAttribute('data-confirm') ? submitter
                     : form.hasAttribute('data-confirm') ? form : null;

        if (source && form.dataset.confirmed !== '1') {
            e.preventDefault();
            Clinic.confirm({
                title: source.getAttribute('data-confirm-title') || 'Are you sure?',
                message: source.getAttribute('data-confirm'),
                confirmText: source.getAttribute('data-confirm-ok') || 'Confirm',
                danger: source.getAttribute('data-confirm-danger') !== 'false'
            }).then(function () {
                form.dataset.confirmed = '1';
                if (form.requestSubmit) form.requestSubmit(submitter || undefined);
                else { if (submitter && submitter.name) $('<input type="hidden">').attr('name', submitter.name).val(submitter.value).appendTo(form); form.submit(); }
            });
            return;
        }
        delete form.dataset.confirmed;

        // Loading state - after other handlers (e.g. client validation) had their say,
        // and after the browser has read the submit button's name/value.
        setTimeout(function () {
            if (e.isDefaultPrevented()) return;
            const $btn = submitter ? $(submitter) : $(form).find('button[type="submit"], button:not([type])').first();
            if ($btn.length && !$btn.is('[data-no-busy]')) Clinic.busy($btn, true, $btn.data('busyText') || 'Please wait...');
        }, 0);
    });

    // Coming back with the browser's Back button must not leave buttons spinning
    $(window).on('pageshow', function () {
        $('.btn.is-busy').each(function () { Clinic.busy($(this), false); });
    });

    // Server messages after a redirect (TempData) also appear as toasts
    $('[data-toast]').each(function () {
        Clinic.toast($(this).attr('data-toast'), $(this).attr('data-toast-ok') !== 'false');
    });
});
