// Floating AI support chat.
// - Toggle button bottom-right; unread badge when a reply arrives while minimised.
// - The conversation is kept in sessionStorage, so it survives moving between pages
//   and is cleared when the browser tab is closed. Nothing is stored on the server.
// - All text goes onto the page with .text(), never .html() (XSS-safe).
$(function () {
    const $root = $('#support-chat');
    if ($root.length === 0) return;                  // not a patient, or AI not configured

    // ---- settings
    const SEND_URL = '/SupportChat/Send';
    const MAX_HISTORY_SENT = 10;                      // keep in sync with AiSettings.MaxHistoryMessages
    const MAX_STORED_ITEMS = 60;                      // older chat lines are dropped from the tab's storage
    const STORAGE_KEY = 'clinicSupportChat:v1:' + $root.data('user');   // per user, so switching accounts starts fresh
    const WELCOME = 'Hi! I\'m the clinic\'s support assistant. I can help you book or change appointments, ' +
                    'find the right doctor, check your bills, or answer questions about the clinic.';

    const $toggle = $('#sc-toggle'), $panel = $('#sc-panel'), $badge = $('#sc-badge');
    const $log = $('#sc-log'), $input = $('#sc-input'), $send = $('#sc-send');

    // state.items: { kind: 'msg' | 'note' | 'link', role?, text, cls?, url? }
    let state = load();
    renderAll();
    setOpen(state.open, false);

    // ------------------------------------------------------------------ events

    $toggle.on('click', function () { setOpen(!state.open, true); });
    $('#sc-close').on('click', function () { setOpen(false, true); });

    $('#sc-clear').on('click', function () {
        if (!confirm('Start a new chat? This conversation will be cleared.')) return;
        state.items = [];
        addItem({ kind: 'msg', role: 'model', text: WELCOME });
        renderAll();
    });

    $('.sc-chip').on('click', function () { send($(this).text()); });

    $('#sc-form').on('submit', function (e) {
        e.preventDefault();
        send($input.val());
    });

    // --------------------------------------------------------------- sending

    function send(raw) {
        const text = (raw || '').trim();
        if (!text || $send.prop('disabled')) return;

        // History = earlier chat lines only (not notes/links), taken BEFORE adding this message
        const history = state.items
            .filter(function (i) { return i.kind === 'msg'; })
            .slice(-MAX_HISTORY_SENT)
            .map(function (i) { return { role: i.role, text: i.text }; });

        addItem({ kind: 'msg', role: 'user', text: text }, true);
        $input.val('');
        setBusy(true);
        const $typing = $('<div class="chat-msg model sc-typing">').text('Typing...').appendTo($log);
        scrollDown();

        Clinic.postJson(SEND_URL, { message: text, history: history })
            .done(function (res) {
                addItem({ kind: 'msg', role: 'model', text: res.reply, cls: res.isEmergency ? 'emergency' : '' }, true);
                if (res.disclaimer) addItem({ kind: 'note', text: res.disclaimer }, true);
                (res.links || []).forEach(function (l) { addItem({ kind: 'link', text: l.label, url: l.url }, true); });
                (res.proposals || []).forEach(addProposal);   // confirm cards are not saved - they go stale
                if (!state.open) bumpUnread();
            })
            .fail(function (xhr) { addItem({ kind: 'msg', role: 'model', text: Clinic.errorMessage(xhr) }, true); })
            .always(function () { $typing.remove(); setBusy(false); });
    }

    // ------------------------------------------------- confirm cards (proposals)

    // Confirm calls the SAME endpoints as the normal pages, so every rule is re-checked.
    function addProposal(p) {
        const $card = $('<div class="chat-proposal">');
        $('<div class="mb-2">').text(p.summary).appendTo($card);
        const $confirm = $('<button type="button" class="btn btn-sm btn-primary me-2">').text('Confirm');
        const $dismiss = $('<button type="button" class="btn btn-sm btn-outline-secondary">').text('Dismiss');

        $confirm.on('click', function () {
            $card.find('button').prop('disabled', true);
            runProposal(p)
                .done(function (res) {
                    $card.find('button').remove();
                    $('<div class="small">').addClass(res.success ? 'text-success' : 'text-danger').text(res.message).appendTo($card);
                    if (res.success) {
                        addItem({ kind: 'msg', role: 'model', text: 'Done: ' + p.summary });
                        $(document).trigger('clinic:appointmentsChanged');   // pages refresh their lists/slots
                    }
                })
                .fail(function (xhr) {
                    $card.find('button').prop('disabled', false);
                    addItem({ kind: 'msg', role: 'model', text: Clinic.errorMessage(xhr) }, true);
                });
        });
        $dismiss.on('click', function () { $card.remove(); });

        $card.append($confirm, $dismiss).appendTo($log);
        scrollDown();
    }

    function runProposal(p) {
        const time = p.startTime ? p.startTime + ':00' : null;    // "14:30" -> "14:30:00" for TimeOnly
        switch (p.type) {
            case 'book':
                return Clinic.postJson('/Appointments/Book', { doctorId: p.doctorId, date: p.date, startTime: time });
            case 'reschedule':
                return Clinic.postJson('/Appointments/Reschedule', { appointmentId: p.appointmentId, date: p.date, startTime: time });
            case 'cancel':
                return Clinic.postJson('/Appointments/Cancel', { appointmentId: p.appointmentId, reason: 'Cancelled via support chat' });
        }
        return $.Deferred().reject().promise();
    }

    // ------------------------------------------------------------- rendering

    function addItem(item, render) {
        state.items.push(item);
        if (state.items.length > MAX_STORED_ITEMS) state.items = state.items.slice(-MAX_STORED_ITEMS);
        save();
        if (render !== false) { renderItem(item); scrollDown(); }
    }

    function renderAll() {
        $log.empty();
        state.items.forEach(renderItem);
        scrollDown();
    }

    function renderItem(item) {
        if (item.kind === 'note') {
            $('<div class="chat-note">').text(item.text).appendTo($log);
        } else if (item.kind === 'link') {
            // Only same-site paths - a tampered sessionStorage can't inject a javascript: link
            if (typeof item.url === 'string' && item.url.charAt(0) === '/' && item.url.charAt(1) !== '/') {
                $('<a class="btn btn-sm btn-outline-primary sc-link">').attr('href', item.url).text(item.text + ' →').appendTo($log);
            }
        } else {
            $('<div class="chat-msg">').addClass(item.role === 'user' ? 'user' : 'model').addClass(item.cls || '')
                .text(item.text).appendTo($log);
        }
    }

    function setOpen(open, persist) {
        state.open = open;
        $panel.toggleClass('d-none', !open);
        $toggle.attr('aria-expanded', open).toggleClass('is-open', open);
        if (open) {
            state.unread = 0;
            scrollDown();
            $input.trigger('focus');
        }
        updateBadge();
        if (persist) save();
    }

    function bumpUnread() { state.unread = (state.unread || 0) + 1; updateBadge(); save(); }

    function updateBadge() {
        $badge.text(state.unread > 9 ? '9+' : state.unread).toggleClass('d-none', !state.unread);
    }

    function setBusy(busy) {
        $send.prop('disabled', busy);
        $input.prop('disabled', busy);
        $('.sc-chip').prop('disabled', busy);
        if (!busy && state.open) $input.trigger('focus');
    }

    function scrollDown() { $log.scrollTop($log[0].scrollHeight); }

    // ---------------------------------------------------------------- storage

    function load() {
        try {
            const saved = JSON.parse(sessionStorage.getItem(STORAGE_KEY));
            if (saved && Array.isArray(saved.items)) return saved;
        } catch (e) { /* storage blocked or corrupt - start fresh */ }
        return { open: false, unread: 0, items: [{ kind: 'msg', role: 'model', text: WELCOME }] };
    }

    function save() {
        try { sessionStorage.setItem(STORAGE_KEY, JSON.stringify(state)); } catch (e) { /* storage full/blocked - chat still works */ }
    }
});
