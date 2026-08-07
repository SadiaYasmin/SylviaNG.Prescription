# Staff Management (Epic J, US-057–060)

## What it is

Admin-only CRUD for a staff member's profile plus doctor-assignment, a searchable/
filterable roster, and a separate read-only "assigned to me" view for a logged-in
Doctor. Follows directly on Epic I (Doctor Management) and closely mirrors its
pattern — same `User` + role-profile-table split, same account-provisioning and
soft-delete pipeline — but staff has no performance drill-down and instead
introduces a many-to-many staff↔doctor assignment.

## Why it was built

Per the agreed build order (STATUS.md), Staff Management is the second feature
after Auth, right behind Doctor Management — a staff member is assigned to specific
doctors, so Doctor Management's roster (Epic I) is a direct prerequisite.

## Backend

- **`Staff` entity** (`Domain/Entities/Staff.cs`) is 1:1 with `User` via `UserId`,
  same split as `Doctor`: username/email/active-status live solely on `User` and are
  joined in for every response DTO, never duplicated onto `Staff`.
- **`StaffDoctor` join entity** tracks the many-to-many staff↔doctor assignment. A
  staff member can support more than one doctor's chamber; a doctor can have more
  than one assigned staff member. Join rows are never removed on
  deactivation — they're the historical record of who supported whom, matching the
  no-orphaning principle used elsewhere.
- **Account creation reuses Epic A's pipeline**, exactly like Doctor: `CreateStaffHandler`
  calls `IAuthService.CreateUserAccountAsync` (role=Staff), then creates the `Staff`
  profile row plus the initial `StaffDoctor` link rows against the returned `UserId`.
- **Soft-delete, not hard-delete** (US-058-equivalent deactivation): sets
  `User.IsActive = false` and disables the Keycloak account via
  `IKeycloakAdminClient.SetUserEnabledAsync`, same as Doctor.
- **US-059 ("doctor views their own assigned staff")**: the controller resolves the
  caller's Keycloak subject (`User.FindFirstValue(ClaimTypes.NameIdentifier)`) and
  passes the raw string into `GetAssignedStaffQuery` — the **handler**, not the
  controller, resolves "which doctor is this" (`IUserRepository.GetByKeycloakIdAsync`
  → `IDoctorRepository.GetByUserIdAsync`), keeping the handler independently
  testable without a fake `ClaimsPrincipal`.
- **Two separate authorization surfaces on one controller**, unlike `DoctorsController`'s
  single class-level `[Authorize]`: Create/Update/Deactivate/GetList/GetDetails are
  `[Authorize(Roles = "Admin")]`, `GET assigned-to-me` is `[Authorize(Roles = "Doctor")]`.
- Phone validated server-side with the same BD-mobile regex as Doctor
  (`Application/Common/Validators/PhoneValidation.cs`).
- Routes: `POST/PUT/DELETE/GET /prescription/staff[/{id}]` (Admin),
  `GET /prescription/staff/assigned-to-me` (Doctor).
- **Naming gotcha**: the feature folder is `Application/Features/Staffs` (plural),
  not the `Staff` singular that would match Doctor's `Features/Doctors` analogue —
  a namespace segment named `Staff` collides with the `Staff` entity class
  (`CS0118: 'Staff' is a namespace but is used like a type`). Keep any future
  additions under `Staffs`.
- **Deviation from the "no new abstractions" plan**: the assignment join queries
  (list-with-assigned-doctors, doctor's-own-assigned-staff, diff-update assignments)
  read/write `IUnitOfWork.Context.StaffDoctors`/`.Doctors`/`.Staff`/`.Users` directly
  rather than through a dedicated join repository — an explicit choice to avoid a
  premature abstraction. The cost: these handlers are only testable against a real
  (InMemory) `ApplicationDBContext`, not pure Moq, so the test project picked up a
  `Microsoft.EntityFrameworkCore.InMemory` package and a shared
  `InMemoryDbContextFactory` test helper.

## Frontend

- New lazy-loaded `pages/staff-management/` module (`staff-list`, `manage-staff`),
  following Doctor Management's pattern — a routed "manage" page for add/edit, not
  a modal.
- **First route with two roles on the parent guard.** `pages-routing.module.ts`'s
  `staff` route uses `canActivate: [roleGuard]` + `data: { roles: ['Admin', 'Doctor'] }`
  so both roles can reach `staff-list`, but `manage-staff`/`manage-staff/:id` carry
  their own `roleGuard` + `data: { roles: ['Admin'] }` at the child-route level so a
  Doctor can't navigate directly to the edit form.
- **`StaffListComponent` is role-aware in one component** rather than two: an Admin
  gets the full roster (search/department/status filters, pagination, add/edit/
  deactivate actions, each row's assigned-doctor chips); a Doctor gets a read-only,
  unpaginated "assigned to me" view (search only, an "Assigned to you" chip instead
  of per-row doctor chips, no action column) backed by
  `StaffService.getAssignedToMe()`. Matches the reference prototype's
  role-branch-in-one-page approach for this feature (`prescription-prototype/src/pages/Staff.jsx`).
- **Doctor-assignment picker** in `ManageStaffComponent` is a checkbox list built
  from `DoctorService.getDoctors({ pageSize: 100, isActive: true })` — reusing
  Doctor Management's existing service rather than adding a new lightweight
  doctor-lookup endpoint.
- Admin-created staff get the same one-time credentials dialog and Reset Password
  flow as Doctor Management, reusing Epic A's `POST /auth/users/{userId}/reset-password`
  endpoint — no backend changes needed for either.
- No photo or date fields (staff has no `PhotoBase64`/`JoiningDate` equivalent), no
  performance/details page (no drill-down data exists for staff).

## Verified

- **Backend**: `dotnet build` clean, `dotnet test` — 100/100 passing (53
  pre-existing from Epic A/I + 47 new for Staff).
- **Frontend**: `ng build` succeeds (`staff-management-module` lazy chunk emitted
  alongside `doctor-management-module`), `ng test` — 63/63 passing (56 pre-existing
  + 7 new, covering `StaffService`'s HTTP contract).
- **`AddStaff` migration applied** to the local Postgres database
  (`dotnet ef database update`) — `Staff`/`StaffDoctors` tables and their indexes
  now exist. This was initially missed (flagged as blocked by Docker not starting
  during the build session) and caught when the live `staff-list` page threw
  "Something went wrong!" — a 500 from the roster query hitting tables that didn't
  exist yet. A full manual UI walkthrough of the admin/doctor flows (create →
  doctor logs in → doctor sees only their assigned staff and gets 403 on admin
  routes → admin searches/filters/edits/reassigns → deactivates → login fails),
  matching Doctor Management's verified writeup, is still worth doing as a final
  pass.
