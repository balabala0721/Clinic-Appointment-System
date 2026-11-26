// Admin overview charts (Chart.js). Data comes from <script id="chart-data"> rendered by the view.
// Colours are read from theme.css custom properties - no colours are hard-coded here.
$(function () {
    const dataEl = document.getElementById('chart-data');
    if (!dataEl || typeof Chart === 'undefined') return;
    const data = JSON.parse(dataEl.textContent);

    const css = getComputedStyle(document.documentElement);
    const v = function (name) { return css.getPropertyValue(name).trim(); };

    Chart.defaults.font.family = v('--font-family');
    Chart.defaults.color = v('--color-text-muted');
    Chart.defaults.borderColor = v('--color-border');
    Chart.defaults.plugins.tooltip.backgroundColor = v('--color-text');
    Chart.defaults.animation = window.matchMedia('(prefers-reduced-motion: reduce)').matches ? false : Chart.defaults.animation;

    // ---- Appointments per day (bar). Today's bar in the primary colour, others softer.
    const perDay = document.getElementById('chart-per-day');
    if (perDay) {
        const colours = data.perDay.values.map(function (_, i) {
            return i === data.perDay.todayIndex ? v('--color-primary') : v('--color-primary-soft');
        });
        new Chart(perDay, {
            type: 'bar',
            data: {
                labels: data.perDay.labels,
                datasets: [{
                    label: 'Appointments',
                    data: data.perDay.values,
                    backgroundColor: colours,
                    borderColor: v('--color-primary'),
                    borderWidth: 1,
                    borderRadius: 6,
                    maxBarThickness: 36
                }]
            },
            options: {
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    x: { grid: { display: false } },
                    y: { beginAtZero: true, ticks: { precision: 0 } }
                }
            }
        });
    }

    // ---- Status breakdown (doughnut). Same colour per status as the status badges.
    const byStatus = document.getElementById('chart-status');
    if (byStatus) {
        const statusVar = {
            Pending: '--color-warning', Confirmed: '--color-info', CheckedIn: '--color-primary',
            Completed: '--color-success', Cancelled: '--color-border-strong', NoShow: '--color-danger'
        };
        new Chart(byStatus, {
            type: 'doughnut',
            data: {
                labels: data.byStatus.labels,
                datasets: [{
                    data: data.byStatus.values,
                    backgroundColor: data.byStatus.keys.map(function (k) { return v(statusVar[k] || '--color-border-strong'); }),
                    borderColor: v('--color-surface'),
                    borderWidth: 2
                }]
            },
            options: {
                maintainAspectRatio: false,
                cutout: '62%',
                plugins: { legend: { position: 'bottom', labels: { boxWidth: 12, boxHeight: 12, usePointStyle: true } } }
            }
        });
    }
});
