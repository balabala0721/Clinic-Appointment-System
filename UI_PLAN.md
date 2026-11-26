# UI redesign plan

Branch: `ui-redesign`. Goal: one calm, consistent interface for Patient, Doctor, Staff and Admin.

## 1. Audit (state before the redesign)

### Views per role

| Role | Pages (controller/action → view) | Module owner |
|---|---|---|
| Anonymous | `Account/Login` (dev test login), `Acc/Login`, `Acc/Register` | Account (dev login) / Auth module (teammate) |
| Patient | `PatientAcc/Index` (placeholder "Welcome, Patient"), `Appointments/Book`, `Appointments/My`, `DoctorRecommendation/Index`, `DoctorSchedules/Doctor/{id}` (Week view), `Reports/*` | Appointments + AI (you), Auth + Reports (teammates) |
| Doctor | `DoctorAcc/Index` (renders `_Layout.cshtml` as its page – blank), `Appointments/Doctor`, `DoctorSchedules/Mine` (Week view + leave form/list), `Drugs/*`, `Reports/*` | Appointments + Schedules (you), Drugs + Reports (teammate) |
| Staff | `DoctorSchedules/Index` (view-only), `DoctorSchedules/Conflicts` | Schedules (you) |
| Admin | `AdminAcc/Index` (renders `_Layout.cshtml` – blank; `AdminAcc/AdminAcc.cshtml` = "Hello World", unused), `Appointments/Manage`, `DoctorSchedules/Index`, `/Edit`, `/Leaves`, `/Conflicts`, `Drugs/*`, `Reports/*` | mixed |

### Shared layout, partials, assets

- `Views/Shared/_Layout.cshtml` – self-described "placeholder": top navbar, role links, "Switch user" button. No Staff menu, no active-page highlight, no mobile menu (links overflow at 375px), shows the email instead of the user's name.
- `_Alerts` (TempData Success/Error + `#ajax-alert`), `_SupportChat` (floating AI chat, patients only), `_ValidationScriptsPartial`.
- CSS: `wwwroot/css/clinic-modules.css` (small, a few variables, hard-coded colours such as `#fff`, `#f8f9fb`, `#dc3545`).
- JS: `clinic.js` (token, postJson, showAlert, loadSlots), `booking.js`, `my-appointments.js`, `manage-appointments.js`, `recommendation.js`, `support-chat.js`.
- Libraries: Bootstrap **5.1.0** (teammates already use 5.3-only classes such as `text-body-secondary`), jQuery, jquery-validation. **Bootstrap Icons are used by the Drugs pages (`<i class="bi ...">`) but never loaded**, so those icons are invisible.

### JavaScript dependencies that must keep working

| Script | Selectors / attributes it relies on |
|---|---|
| clinic.js | `input[name=__RequestVerificationToken]` (rendered once in the layout), `#ajax-alert`, `.slot-btn` inside a slot container |
| booking.js | `#specialisation`, `#doctor` + `option[data-spec]`, `#date`, `#slots`, `#book-btn`, `#doctor-schedule-link`, `#reason`; event `clinic:appointmentsChanged` |
| my-appointments.js | `.appt-row[data-id][data-doctor]`, `.js-cancel`, `.js-reschedule`, `.reschedule-panel`, `.js-new-date`, `.js-new-slots`, `.js-confirm-reschedule`; event `clinic:appointmentsChanged` |
| manage-appointments.js | `.js-reschedule[data-id]`, `.reschedule-row[data-id][data-doctor]`, `.js-new-date`, `.js-new-slots`, `.js-confirm-reschedule` |
| recommendation.js | `#recommend-form`, `#symptoms`, `#recommend-btn`, `#recommend-result`, `.slot-grid` |
| support-chat.js | `#support-chat[data-user]`, `#sc-toggle`, `#sc-badge`, `#sc-panel`, `#sc-clear`, `#sc-close`, `#sc-log`, `#sc-chips .sc-chip`, `#sc-form`, `#sc-input`, `#sc-send`, `.chat-msg`, `.chat-note`, `.chat-proposal`, `.sc-*`; dispatches `clinic:appointmentsChanged` |

All existing scripts already write user/AI text with `.text()` – keep it that way.

### Inconsistencies and usability problems

1. No real landing page for any role: patients land on Book, doctors on their schedule, staff/admin on Manage. Teammates' `PatientAcc/DoctorAcc/AdminAcc` dashboards are empty.
2. Navigation differs per role only by link list; current page not highlighted; no mobile menu; Staff has no menu at all.
3. Four different date formats (`ddd, d MMM yyyy`, `d MMM`, `d MMM yy`, `dddd, d MMMM yyyy`) and 24-hour times. Money as `RM 40` / `RM @x.ToString("F2")`.
4. Status shown as raw text (`CheckedIn`, `NoShow`) or as ad-hoc badges with different colours per page (`bg-light`, `bg-warning`, …).
5. Browser `confirm()` used for cancel, remove working day, reset demo data and clear chat; Reject leave and Cancel/No-show status changes have no confirmation at all.
6. AJAX feedback goes to a single `#ajax-alert` at the top of the page and scrolls the user away from what they clicked; buttons are not consistently disabled while a request runs.
7. Booking is one long form; slots are an unordered row of 24-hour buttons.
8. My Appointments shows Upcoming and History stacked; the cancellation rule is a small footnote.
9. Admin status change is a `<select>` + Update button – destructive choices (Cancelled, NoShow) look the same as harmless ones.
10. Empty states are plain grey text without a next action.
11. 404 shows the browser/ASP.NET default page; `Program.cs` points production errors at `/Home/Error`, which does not exist.
12. Teammates' pages use their own containers, headings (`h2`), `shadow-sm` cards and `table-bordered` tables – they look like a different app.
13. Mobile: the floating chat button can cover the last button on a page.
14. **Staff role is broken end to end:** `Role` enum has no `Staff` value (DB constraint allows it), there is no Staff user in the demo data, real login (`AccController`) has no Staff redirect, and a teammate's commit changed `Cancel`, `Reschedule`, `Manage`, `UpdateStatus` from `StaffOrAdmin` to `Admin`.

## 2. Decisions (agreed before building)

- **Minimal wiring allowed:** only landing redirects, dashboard actions returning their own view (+ a new `StaffAccController`), a status-code page for 404/500, and the dev login's `Name` claim (full name, like the real login). No business logic, no DB schema change, no migration.
- **Restore Staff access:** add `Staff` to the `Role` enum (stored as a string – no schema change), add one demo Staff user, add the Staff redirect to the real login, and change the four actions back to `Roles.StaffOrAdmin`. *This reverts a teammate's authorization change – confirm with them.*
- **Dashboard data:** dashboards inject existing read-only services, plus small read-only `ClinicDbContext` queries for recent bills and the admin charts. Nothing is written.

## 3. Phased plan (one commit each)

1. **Audit + plan** – this file.
2. **Design system** – `wwwroot/css/theme.css` (all tokens at the top; Bootstrap 5.3 variables mapped onto them; components: buttons, cards, tables, forms, badges, alerts, modal, toasts, empty states, spinner/skeleton). Upgrade Bootstrap 5.1 → 5.3.3, add Bootstrap Icons and Chart.js locally. `Ui` helper for dates (dd/MM/yyyy), times (2:30 PM), money (RM 50.00) and the single status badge. `clinic.js`: toast, confirm modal, button busy state, 12-hour slot chips grouped Morning/Afternoon/Evening, `data-confirm` for plain forms.
3. **Layout + navigation + wiring** – sidebar layout with role menus, active page, user name/role/log out, mobile off-canvas menu, auth pages without sidebar; role dashboards as landing pages; Staff role restored; friendly 403/404/500.
4. **Role screens** – Patient dashboard, step-by-step booking, My Appointments tabs; Doctor dashboard timeline, schedule week, leave; Staff front desk; Admin overview with charts, management tables, leave review queue.
5. **Interaction pass + teammate pages** – loading/toast/inline errors on every AJAX action, confirm modals, empty states, form labels/placeholders, focus styles, ARIA; teammate pages restyled mostly through CSS.
6. **Verify** – `dotnet build`, every page per role at 1280px and 375px, fix, push `ui-redesign`.

## 4. Progress

| Phase | Commit |
|---|---|
| 1 Audit + plan | `aef831c` |
| 2 Design system | `eb7d427` |
| 3 Layout, navigation, wiring | `5a93dce` |
| 4 Role screens | `7495117` |
| 5 Interaction pass + teammate pages + verification fixes | next commit |

### Teammate files touched (all phases)

| File | Change |
|---|---|
| `Controllers/Account/AccController.cs` | Staff redirect after login (1 line) |
| `Controllers/Account/AdminAccController.cs`, `DoctorAccController.cs` | `Index` returns its own view instead of `_Layout` |
| `Controllers/Account/StaffAccController.cs` | **new** – Staff dashboard |
| `Models/Account/UserAccount.cs` | `Staff` added to the `Role` enum |
| `Views/PatientAcc/Index.cshtml`, `Views/DoctorAcc/Index.cshtml`, `Views/AdminAcc/Index.cshtml`, `Views/StaffAcc/Index.cshtml` | role dashboards (were empty placeholders) |
| `Views/Acc/Login.cshtml`, `Views/Acc/Register.cshtml` | removed inline `style="max-width"` only |
| `Views/Drugs/*`, `Views/Reports/*` | **not edited** – restyled through `theme.css` section 7 |

Also confirm with the teammate: `AppointmentsController` `Cancel/Reschedule/Manage/UpdateStatus` are back to `StaffOrAdmin`.

### Verification (phase 6)

- `dotnet build`: 0 errors, 0 warnings.
- Every page per role loaded in headless Chrome at 1280px and 375px: no JS errors, no horizontal page scroll, chat button covers no control.
- Driven end to end: patient booking (all 5 steps + confirmation), cancel via modal, Upcoming/Past tabs, reschedule panel, chat clear modal; admin Reject-leave modal (Go back sends nothing; Confirm posts `approve=false`).
- Known limits: bare `/Controller` URLs 404 because the default route is `Account/Login` (unchanged routing; menus link to `/Index`). Native date pickers follow the browser's locale. Demo DB has no drugs, so Drugs Details/Edit/Delete were not opened.
