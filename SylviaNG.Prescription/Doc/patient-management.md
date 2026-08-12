# Patient Management (Epic B, US-005–009)

## What it is

Staff can register a patient, Staff or Doctor can edit one, and all three roles
(Staff/Doctor/Admin) can search a patient roster — each scoped to a different set
of rows. A patient's verbatim History field carries forward into their next
prescription (US-009), though the write side of that (finalize replacing it) is
Epic D's job — this epic just gives the field a home.

Unlike Doctor (Epic I) and Staff (Epic J), `Patient` has **no `User` row and no
Keycloak account** — patients never log in. There's no account provisioning here,
just a profile row plus who registered it.

## Why it was built

First link in Phase 3's core dependent chain: Patient → Consultation →
Prescription Authoring → Lifecycle. Everything downstream needs a patient to
attach to.

## Backend

- **`Patient` entity** (`Domain/Entities/Patient.cs`) extends `Audit` but is not
  1:1 with `User` — no `UserId` FK, no navigation to `User`. Fields: `Name`,
  `Phone`, `DateOfBirth`/`Age` (mutually substitutable — see validation below),
  `Gender`, `Address`, `BloodGroup`, `AllergyPresetId`/`AllergyOtherText`,
  `SavedHistory`, `RegisteredByStaffId`, `RegisteredAt`.
- **`RegisteredAt` is set manually in `CreatePatientHandler`**, not left to
  `Audit.CreatedAt` — nothing in this codebase's `SaveChanges` pipeline actually
  stamps `CreatedAt` (`UtcDateTimeInterceptor` only normalizes timezones), so it
  would silently stay null. Worth remembering before relying on any inherited
  `Audit` timestamp field elsewhere without checking first.
- **`BloodGroupEnum`/`AllergyPresetEnum`** live in the same shared
  `Domain/Enums/Enum.cs` file as `GenderEnum`/`UserRoleEnum` — this codebase keeps
  all enums in one file, not one-per-file. `AllergyPresetEnum` only covers the 5
  fixed presets (None/Penicillin/Dust/Seafood/Latex); "Other" has no enum member —
  it's represented by `AllergyPresetId == null` + non-blank `AllergyOtherText`.
- **`PatientVisibilityScope`** (`Application/Features/Patients/PatientVisibilityScope.cs`)
  is the one new piece of logic this epic introduces: a shared helper resolving
  "who's calling" (`KeycloakId → User → Staff/Doctor`, mirroring
  `GetAssignedStaffHandler`'s resolution) and scoping a `Patient` queryable
  accordingly — Staff sees only `RegisteredByStaffId == me`; Doctor sees patients
  registered by any Staff currently in `StaffDoctors` under that doctor (Epic J's
  join table, joined unconditionally — there's no separate "assignment active"
  flag beyond normal soft-delete); Admin sees everyone. Reused by
  `GetPatientList`, `GetPatientDetails`, and `UpdatePatient` so this join logic
  lives in exactly one place.
- **One `GetPatientListHandler`**, not split by role like Staff's Admin/Doctor
  queries — Patient's three roles return the same response shape (an extra
  `RegisteredByName` field is always present; the frontend decides whether to
  render it), so one handler branching internally was simpler than three MediatR
  request types.
- **Out-of-scope access → `NotFoundException` (404), not 403.** No
  `ForbiddenException` exists in this codebase yet (only `NotFoundException`,
  `DuplicateException`, `InvalidCredentialsException`, `BadRequestException`), and
  404 also avoids confirming a patient id exists at all to someone who can't see
  it — verified live: a Staff member updating another Staff member's patient gets
  `404 {"decentMessage":"Entity \"Patient\" (1) was not found."}`.
- Reuses `PhoneValidation.BangladeshMobileRegex` verbatim (same regex as
  Doctor/Staff) — no new phone validation was written.
- Duplicate phone numbers are explicitly allowed — no uniqueness check anywhere,
  per spec.
- Routes: `POST /prescription/patients` (Staff), `PUT /{id}` (Staff, Doctor),
  `GET /` and `GET /{id}` (Staff, Doctor, Admin) — per-action `[Authorize]`, no
  class-level attribute, same reasoning as `StaffController` (roles differ per
  action).
- **`medicalHistory` (a doctor-maintained conditions list) is out of scope** for
  this epic — only `SavedHistory` (US-009) is a real column; nothing writes to it
  yet since that's Epic D's finalize step.
- No deactivate/delete — nothing in US-005–009 calls for one.

## Frontend

- New lazy-loaded `pages/patient-management/` module (`patient-list`,
  `manage-patient`), routed-page-not-modal like Doctor/Staff Management.
- **One list component for all three roles** — the backend already scopes which
  *rows* come back, so the component only decides which *columns/buttons* render:
  "Registered By" column and clinical columns (age/gender/blood group/allergy)
  are Admin-vs-non-Admin toggles, registered date is hidden for Doctor, the
  "Register Patient" button only shows for Staff, edit is available to Staff and
  Doctor (matching the PUT permission).
- **Search is genuinely debounced** (`Subject` + `debounceTime`/
  `distinctUntilChanged`) — this is actually the first real use of this
  codebase's already-defined-but-previously-unused `UI_CONFIG.searchDebounceTime`
  constant; Doctor/Staff's own search boxes use an explicit
  `keyup.enter` + button instead.
- `manage-patient` form: DOB present disables and clears the Age field's
  validators (Age is only meaningful when DOB is unknown); Allergy select shows
  the 5 presets plus an "Other" option that reveals a free-text field
  (`allergyPresetId` sent as `null` + `allergyOtherText` populated when "Other" is
  chosen). No `SavedHistory` field in this form — it's not editable through
  patient CRUD.
- No temporary-password dialog on create, unlike Doctor/Staff — registering a
  patient doesn't create a user account.
- `patients` route: `roleGuard` + `data: { roles: ['Admin','Doctor','Staff'] }` at
  the parent; the manage/add-edit child route is further restricted to
  `['Staff','Doctor']` — Create being Staff-only is still backend-enforced
  regardless, this guard is UX only.

## Verified

- **Backend**: `dotnet build` clean, `dotnet test` — 211/211 passing (154
  pre-existing + 57 new). `AddPatient` migration applied to the local Postgres
  database.
- **Frontend**: `ng build` clean (`patient-management-module` lazy chunk
  emitted), `ng test` — 125/125 passing (103 pre-existing + 22 new).
- **Manual end-to-end smoke test against the live local stack** (Postgres +
  Keycloak, real JWTs, not mocks) confirmed the full US-008 visibility matrix
  using existing seeded accounts (`staff.rayhan` assigned to Dr. Sabrina Khatun,
  `hasan.imam` assigned to Dr. Imran Hossain):
  - `staff.rayhan` registers a patient → sees only that patient in the list.
  - `hasan.imam` registers a different patient → sees only *that* patient, not
    Rayhan's.
  - Dr. Sabrina Khatun (Rayhan's assigned doctor) sees Rayhan's patient, correctly
    does **not** see Hasan's patient (registered by a staff member assigned to a
    different doctor).
  - Admin sees both, with `registeredByName` populated on each row.
  - Hasan attempting `PUT /prescription/patients/1` (Rayhan's patient) is
    rejected with a 404, confirming edit access is enforced server-side by
    visibility scope, not just hidden in the UI (US-006's explicit requirement).
  - Left in the dev DB from this test: patient ids 1 (`Patient From Rayhan`) and
    2 (`Patient From Hasan`) — harmless seed-adjacent data, no cleanup endpoint
    exists (Patient has no delete/deactivate in this epic).
