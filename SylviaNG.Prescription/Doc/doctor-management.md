# Doctor Management (Epic I, US-053–056)

## What it is

Admin-only CRUD for a doctor's professional profile, plus a searchable/filterable
roster and a per-doctor performance drill-down. This is the first real feature built
on top of Epic A's auth foundation, and the first admin-only surface in either app.

## Why it was built

Per the agreed build order (STATUS.md), Doctor Management (and Staff Management,
Epic J) come right after Auth — the doctor roster is a prerequisite for Patient/
Consultation role-scoping later (a staff member is assigned to specific doctors;
that assignment needs real doctor rows to point at).

## Backend

- **`Doctor` entity** (`Domain/Entities/Doctor.cs`) is 1:1 with `User` via a unique
  `UserId` FK. It holds only the professional-profile fields (name, phone,
  qualification, department, license number, specialization, experience, gender,
  joining date, photo). Username/email/active-status are **not duplicated** here —
  they live solely on `User` and are joined in for every response DTO. This avoids a
  two-places-for-one-fact bug between `Doctor.IsActive` and `User.IsActive`.
- **Account creation reuses Epic A's pipeline**: `CreateDoctorHandler` calls the
  existing `IAuthService.CreateUserAccountAsync` (role=Doctor) to provision the
  Keycloak user + `User` row, then creates the `Doctor` profile row against the
  returned `UserId`. Nothing about Keycloak user creation was duplicated.
- **Soft-delete, not hard-delete** (US-056): deactivating a doctor sets
  `User.IsActive = false` and disables the Keycloak account
  (`IKeycloakAdminClient.SetUserEnabledAsync`, new). The `Doctor` row and any future
  historical data tied to it are never removed — per `feature.md`'s explicit
  recommendation against orphaning/corrupting historical prescriptions.
- **US-055 (performance drill-down) is real but honestly empty right now.**
  `Consultation`/`Prescription` don't exist yet (Epic C/D). `GetDoctorDetailsHandler`
  returns a real, stably-shaped `DoctorPerformanceStats` block (patients consulted,
  Rx counts, top medicines, trend, recent prescriptions) that is all zero/empty
  today — not a fabricated placeholder. The response contract won't need to change
  when Epic C/D land; only the query bodies will start returning real numbers.
  Same reasoning for `DoctorListSummary.TotalPrescriptions`/`TotalMedicineEntries`
  in the roster stat tiles (US-054) — hardcoded to 0 with a comment until Epic D/F.
- Phone is validated **server-side** with the same BD-mobile regex
  (`^01[3-9]\d{8}$`) as the frontend (`Application/Common/Validators/PhoneValidation.cs`)
  — the frontend is never the sole enforcement of this rule.
- Routes: `POST/PUT/DELETE/GET /prescription/doctors[/{id}]`, all
  `[Authorize(Roles = "Admin")]`. Doctors manage their *own* profile through a
  separate endpoint in the future Epic K, not this controller.

## Frontend

- New lazy-loaded `pages/doctor-management/` module (doctor-list, manage-doctor,
  doctor-details), following the existing `attendance-management` module's pattern:
  a routed "manage" page for add/edit (not a modal — matches this codebase's
  established convention, unlike the React reference prototype which uses one).
- **First admin-only route in the app.** `auth.guard.ts` only ever checked
  authentication, not role, so a new `roleGuard` (`@core/guards/role.guard.ts`) reads
  `route.data['roles']` and redirects to `/dashboard` on a role mismatch. Applied via
  `canActivate: [roleGuard]` + `data: { roles: ['Admin'] }` on the `doctors` route in
  `pages-routing.module.ts` — via `loadChildren` only, never an eager `imports` entry
  (the Epic A routing-guard-bypass lesson).
- **Nav is now role-aware.** `IMenuItem` gained an optional `roles?: string[]`;
  `MenuService` filters the static nav list against `AuthService.getRole()` and
  re-filters on every login/logout (subscribed to `currentUser$`) so the "Doctor
  Management" item appears/disappears live without a page reload.
- Admin-created doctors get a **one-time credentials dialog** (username, email if
  given, temporary password — not emailed, no email-sending infra exists anywhere in
  either repo) shown once right after creation, with a "copy all" button. **This is
  an explicit, accepted gap** (confirmed with Sadia), the same one already accepted
  for Epic A's admin-create-account/reset-password flows — real email delivery would
  need its own mail-service infrastructure and was deliberately deferred, not
  forgotten. Revisit if/when a concrete trigger appears (this feature, Epic J staff
  accounts, and self-service password reset would all share the same mail service).
- **Reset Password**, added mid-review at Sadia's request after noticing the edit
  form had no way to help a doctor who lost their temporary password before ever
  logging in. Wires the frontend up to Epic A's existing (previously unused)
  `POST /auth/users/{userId}/reset-password` endpoint — no backend changes needed.
  Shown as a button on the edit-doctor page only (`isEditMode`), gated behind a
  confirmation dialog, reusing the same one-time credentials-dialog component (with
  a "Password reset" header instead of "Doctor account created", and it does **not**
  navigate back to the list on close, unlike the create flow, since the admin is
  presumably still mid-edit).
- Photo is stored as a base64 data URL (matches the backend's `PhotoBase64` column)
  — same known limitation as the reference prototype: no object-storage layer yet.

## Verified end-to-end (2026-08-03)

Manually smoke-tested against the local Postgres+Keycloak stack (`docker compose up`)
and a running `ng serve`/`dotnet run`: admin creates a doctor → Keycloak account
works immediately (new doctor logs in, gets a `Doctor`-role token) → doctor's own
token gets a 403 from the admin-only `/doctors` endpoints → admin searches/filters
the roster → views the zero-state details page → edits the profile → resets the
doctor's password (new temporary password shown, admin stays on the edit page) →
deactivates → deactivated doctor's login now fails with 401 and disappears from the
active filter. UI-driven the same flow via a headless-Chromium script (nav item
visible only to Admin, add/edit/reset-password/deactivate-with-confirmation all
round-trip correctly, zero browser console errors). 53/53 backend tests pass (22
pre-existing + 31 new), 47/47 frontend tests pass (28 pre-existing/updated + 19 new).
