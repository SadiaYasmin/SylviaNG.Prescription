# Consultation & Queue Management (Epic C, US-010–017 minus US-012/017)

## What it is

Staff checks a patient in for one of their assigned doctors, which mints a
per-doctor daily token (`T-01`, `T-02`, ...) and a globally unique consultation
code (`CN-YYYY-####`) and puts the patient in that doctor's queue. Doctors open
the next waiting patient (which will land them in prescription authoring once
Epic D exists); Admin gets a hospital-wide, filterable monitoring view.

US-012 (unfinished-draft guard) and US-017 (consultation/prescription status
invariant) need the `Prescription` entity and were deferred to Epic D — see
that epic's doc for how they were closed out. **US-011 (duplicate-active-
consultation guard) is already implemented here**, not deferred — see below.

## Why it was built

Second link in Phase 3's core dependent chain: Patient → Consultation →
Prescription Authoring → Lifecycle.

## Backend

- **`Consultation` entity** (`Domain/Entities/Consultation.cs`) extends `Audit`
  but `new`-shadows the inherited `Status` int with its own
  `ConsultationStatusEnum` (`Waiting`/`InConsultation`/`Completed` — no `Draft`
  or `Cancelled` yet, both explicitly out of scope for this epic).
  `CheckInAt` is stamped manually in the handler (`DateTime.UtcNow`), same
  reason as `Patient.RegisteredAt` — nothing in this codebase's `SaveChanges`
  pipeline stamps `Audit.CreatedAt`.
- **`DisplayCode`/`TokenNumber` are minted via `ISequenceGenerator`**
  (`Application/Common/Services/ISequenceGenerator.cs`), never guessed or
  counted client-side — the first use of this atomic-upsert sequence mechanism
  in the codebase, reusable by any future feature needing a gap-free counter.
  `DisplayCode` uses counter key `"ConsultationId"` scoped to the year;
  `TokenNumber` uses `"ConsultationToken:{doctorId}"` scoped to the calendar
  date, so token numbers reset per doctor per day as required.
- **US-011 (duplicate-active-consultation guard) is already enforced** in
  `CreateConsultationHandler`: before minting anything, it checks for an
  existing `Waiting`/`InConsultation` consultation for the same patient *on
  that day, regardless of which doctor* — a bit broader than the story's
  literal "same doctor" wording, deliberately, since one patient shouldn't be
  in two queues at once. If found, the handler returns
  `{ DuplicateFound: true, ExistingConsultation: ... }` instead of creating a
  new row; there's no "force create anyway" escape hatch — staff must resolve
  or use the existing entry.
- **One `GetConsultationListHandler`** (Admin-only, US-016): filters by
  `ConsultationDateModeEnum` (Today/Yesterday/Custom/Range), doctor, status,
  and free-text search (patient name/phone/token — joined against
  `Patient`/`Doctor`), paginated, plus a `ConsultationListSummary` (Total/
  Waiting/InProgress/Completed) computed over the **filtered** set, matching
  the reference prototype's stat-tile behavior. No `Draft` bucket yet (see
  Epic D's doc for where this gets extended).
- **`GetTodaysQueueHandler`** (Doctor, US-014) and **`GetMyQueueHandler`**
  (Staff, US-015): both resolve the caller via `CallerContextResolver`, filter
  to today + Waiting/InConsultation, order by `CheckInAt`. No pagination —
  a single day's queue is expected to stay small.
- **`OpenConsultationHandler`** (Doctor, US-013): `Waiting` → `InConsultation`
  transition only; does not yet load a linked prescription (there isn't one —
  that's Epic D's `StartOrResumePrescription` flow, which will call this same
  transition as part of a larger flow).
- Routes on `ConsultationsController` (`prescription/consultations`, no
  class-level `[Authorize]` — same reasoning as `PatientsController`/
  `StaffController`, since different actions need different roles):
  `POST /` (Staff, create), `POST /{id}/open` (Doctor), `GET /today-queue`
  (Doctor), `GET /my-queue` (Staff), `GET /my-assigned-doctors` (Staff, for the
  create-consultation dialog's doctor picker), `GET /` and `GET /{id}` (Admin).
- Business rules enforced: a staff member can only create a consultation for a
  doctor currently in their own `StaffDoctors` assignment set
  (`BadRequestException` otherwise); consultations can only be created for
  today (`VisitDate`, if supplied, must equal today).

## Frontend

- New lazy-loaded `pages/consultation-management/` module with one page,
  `consultation-list` (the Admin monitoring view, US-016) — filters, stat
  tiles, paginated table, details modal.
- **Consultation creation lives inside Patient Management**, not as its own
  page: `pages/patient-management/create-consultation-dialog/` is a
  `p-dialog` launched from the patient list/detail (buttons in
  `pTemplate="footer"` per the standing modal convention). It calls
  `GET /my-assigned-doctors` to populate the doctor picker and
  `POST /prescription/consultations` to check the patient in, surfacing the
  duplicate-guard response inline if `DuplicateFound` comes back true.
- **No frontend consumer yet for `GET /today-queue` or `GET /my-queue`** — the
  Doctor/Staff dashboards are still the near-empty placeholder noted in
  `ARCHITECTURE.md`; those two endpoints exist and are tested but are only
  wired into the UI once Epic D needs a real "Today's Queue" widget as an
  entry point into prescription authoring (see that epic's doc).

## Verified

- **Backend**: `dotnet build` clean, `dotnet test` — 260/260 passing (211
  pre-existing + 49 new). `AddConsultation` migration applied to the local
  Postgres database.
- **Frontend**: `ng build` clean, `ng test` — 162/162 passing (125
  pre-existing + 37 new).
- Manual smoke test of the create-consultation dialog and Admin consultation
  list against the live local stack, including the duplicate-active-
  consultation prompt firing correctly.
