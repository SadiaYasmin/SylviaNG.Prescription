# Prescription Authoring (Epic D, US-018–028)

## What it is

The core of the product: a live, inline-editable clinical document rendered directly
inside one of the three hospital-branded templates (Epic H) — what the doctor edits is
pixel-identical to what gets finalized/printed/verified. Three entry points converge on
one backend endpoint: opening a queued consultation (US-018), quick-creating for a
walk-in patient (US-019), and resuming a saved draft (US-020). Covers chief
complaints/history/vitals/diagnoses/investigations/medicines/advice/follow-up capture,
the duplicate-medicine guard, the bilingual English/বাংলা toggle, Quick Add insertion,
and the Save-as-Draft/Finalize lifecycle with its validation checklist.

## Why it was built

Third link in Phase 3's core dependent chain: Patient → Consultation → **Prescription
Authoring** → Lifecycle. Everything upstream (Epic B/C) exists to feed this.

## Cross-epic dependency decision

US-026's finalize checklist needs a doctor's signature + preferred template (Epic K);
US-022's medicine autocomplete needs a catalog (Epic F); US-025's Quick Add needs preset
lists (Epic G). None of those epics exist yet. Per Sadia's explicit choice, this epic
built **minimal stubs** for exactly what D needs — plain fields/endpoints, no polished
admin UI — leaving the full epics (catalog CRUD, preset manager UI, AI signature
background-removal, full profile page) for later as their own features:
- **Epic K stub**: `Doctor.PreferredTemplateId`/`SignatureBase64`/`PreferredLanguage` +
  a `prescription/doctors/me/preferences` self-service controller + a small frontend
  settings panel (`/prescriptions/preferences`) — no photo/license editing, no AI
  background removal, just plain base64 upload.
- **Epic F stub**: a `Medicine` entity, a 20-row `MedicineCatalogSeeder`, and a read-only
  `GET /prescription/medicines?search=` — no admin CRUD.
- **Epic G stub**: a `QuickAddPreset` entity (one table, five section types via
  `PayloadJson`) and add/list/delete endpoints — no edit, no auto-translate, no
  starter-seed.

## Backend

- **`PrescriptionRecord` entity** (`Domain/Entities/PrescriptionRecord.cs`) — named
  `PrescriptionRecord`, not the bare "Prescription" every other layer of this feature
  uses in routes/JSON/docs, because the project's own root namespace is
  `SylviaNG.Prescription` — a type literally named `Prescription` is unresolvable
  (`CS0118`, namespace-vs-type ambiguity) anywhere in this codebase. The same collision
  hit `HospitalSettings` (shadowed by the `Application.Features.HospitalSettings`
  namespace) in any file under `Application.Features.*` — those references are
  fully-qualified as `Domain.Entities.HospitalSettings` instead. Worth remembering
  before naming anything after the project itself.
- A `PrescriptionRecord` row is created (empty, `Draft`) the moment authoring starts —
  never on first save — so every draft has a stable `DisplayCode`/`PrescriptionId` from
  the first keystroke. `ConsultationId` is a unique 1:1 FK: a prescription never exists
  without exactly one consultation, including quick-create walk-ins, which silently mint
  one behind the scenes (`Consultation.RegisteredByStaffId` is now nullable — null means
  "quick-created by the doctor directly, no staff check-in").
- List-type sections (`ChiefComplaintsJson`/`HistoryJson`/`DiagnosesJson`/
  `InvestigationsJson`/`MedicinesJson`/`AdviceJson`) are serialized JSON text columns,
  same portable-across-providers pattern as `PrescriptionTemplate.ConfigJson`
  (`Application/Mappings/PrescriptionMappings.cs`), not EF owned-entity `.ToJson()`.
  Examination/vitals are discrete nullable string columns instead (fixed, known shape,
  matching the reference prototype — even BP stays free text like "120/80").
- **`StartOrResumePrescriptionHandler`** is the single entry point (mirrors the
  prototype's one `/prescriptions` route disambiguated by params): `consultationId`
  (open from queue), `patientId` (quick-create, US-019), or `prescriptionId` (resume,
  US-020). Runs the US-011/US-012 guards for the quick-create path and returns the full
  render payload (prescription + patient + doctor + resolved template config + hospital
  settings) in one call.
- **US-011 (duplicate-active-consultation) and US-012 (unfinished-draft) guards** now
  exist on *two* entry points: `CreateConsultationHandler` (Staff's check-in flow, Epic
  C — US-011 already existed there, US-012 was added this epic) and
  `StartOrResumePrescriptionHandler`'s quick-create branch (both guards, new). The two
  paths deliberately differ on one point: Staff's check-in never lets `Force` override
  the active-duplicate guard ("Force is intentionally ignored" — Epic C's original
  design), but the doctor's own quick-create does honor `Force` there too, since it's
  already an exception path bypassing the queue entirely.
- **`MedicineDuplicateGuard`** (`Application/Features/Prescriptions/`): normalizes
  medicine name + strength (trim, case-insensitive) and rejects the whole write with
  `BadRequestException` on a collision — enforced in both `SaveDraftPrescriptionHandler`
  and `FinalizePrescriptionHandler`, not just the frontend.
- **Finalize validation** (US-026, `FinalizePrescriptionHandler`): diagnoses ≥ 1,
  medicines ≥ 1, doctor has a signature, doctor has a preferred template — all missing
  items collected into one `BadRequestException` message, not reported one at a time.
- **Transactional status invariant** (US-017, finally implementable now
  `PrescriptionRecord` exists): `FinalizePrescriptionHandler` and
  `SaveDraftPrescriptionHandler` update `PrescriptionRecord.Status` and
  `Consultation.Status` in the same `SaveChangesAsync()` call — never two separate saves.
  Finalize also overwrites `Patient.SavedHistory` (US-009) with the current History
  section, joined into one string (`Patient.SavedHistory` is a plain string column from
  Epic B, while `PrescriptionRecord`'s own History is a list — finalize joins with `\n`,
  and a brand-new prescription's History preloads as a single-item list seeded from that
  string).
- **`PrescriptionVisibilityScope.cs`**, same shape as `PatientVisibilityScope`: Admin
  sees all; Doctor sees prescriptions they authored, plus (for the single-view story
  only, `ownOnly: false`) any prescription for a patient in their care team; Staff sees
  prescriptions for patients they personally registered. Draft/Finalized *lists* are
  strictly doctor-own (`ownOnly: true`).
- **`TemplatesController` change**: its two `GET` actions were opened to Doctor
  (`[Authorize(Roles="Admin,Doctor")]`, previously Admin-only) so a doctor can list
  enabled templates and read one's config for the preferred-template picker. `POST`/
  `PUT`/`duplicate`/`toggle-enabled`/`DELETE` stay Admin-only.
- **`DeleteTemplateHandler` change**: now refuses to delete a template that's been used
  by any `PrescriptionRecord` (`BadRequestException`, not a raw DB FK-violation 500) —
  a finalized prescription must always be able to re-render its original template
  (US-064). `Doctor.PreferredTemplateId`'s FK is `SetNull` instead, so deleting a
  template a doctor merely *prefers* (not used) silently falls them back to "no
  preference," matching US-050's "falls back to default" intent without touching Epic
  H's delete flow further.
- Routes: `PrescriptionsController` (`prescription/prescriptions`, no class-level
  `[Authorize]` since actions need different roles — `start`/`{id}` PUT/`{id}/finalize`/
  `drafts`/`finalized` are Doctor-only, `{id}` GET and `patient/{id}/history` are
  Doctor/Staff/Admin, `verify/{displayCode}` is `[AllowAnonymous]`),
  `MedicinesController`, `QuickAddController`, `DoctorPreferencesController` (a
  deliberately separate controller from the Admin-only `DoctorsController` — class- and
  method-level `[Authorize]` AND together, so a Doctor-only self-service action can't
  live on that controller at all).

## Frontend

- **Central reuse decision**: `classic-template`/`corporate-template`/
  `government-template` (`shared/template-preview/`) were preview-only placeholders
  (Epic H). Extended all three with `@Input() editable` + `@Input() document` +
  `@Output() contentChange`, so the *same* three components now serve three modes:
  Epic H's original placeholder preview (`editable=false`, `document=null`, unchanged),
  live authoring (`editable=true`, real `document`), and read-only view/verify
  (`editable=false`, real `document`). Each section branches on `editable` inside the
  existing template markup — not a separate form screen.
- New shared components (`shared/prescription-sections/`, first `FormArray`-adjacent
  UI in this codebase, though implemented as plain array-mutate-and-emit rather than
  Angular `FormArray` for simplicity): `ChipListInputComponent` (chief complaints/
  history/investigations/advice), `DiagnosisListInputComponent` (textarea parsed to
  `{text, icd10}[]` on blur, mirroring the prototype's line-prefix/trailing-parenthesis
  convention), `VitalsInputComponent`, `MedicineListInputComponent` (autocomplete +
  client-side duplicate highlight on top of the server-side guard), `QuickAddSelectComponent`.
- **`IHospitalBranding`** (`shared/template-preview/hospital-branding.interface.ts`): a
  new minimal interface narrower than both `IHospitalSettings` (Epic H's full CRUD
  shape) and the new `IHospitalSettingsSnapshot` (embedded read-only in a prescription
  document) — both satisfy it structurally, so the three template components'
  `hospitalSettings` input works with either source without an adapter.
- **`Doctor's Today's Queue` widget** (dashboard, built in Epic C) now actually
  navigates: `openConsultation()` routes to `/prescriptions?consultationId=<id>` instead
  of transitioning status in place — the transition now happens inside
  `StartOrResumePrescriptionHandler`.
- **Prescription Preferences** (`/prescriptions/preferences`): template picker (enabled
  templates only) + plain file-upload signature (base64, no AI background removal) +
  read-only preferred-language display.
- **Real bug found and fixed during manual testing**: the vitals label array
  (`get vitals()`, 6 items including "BMI", built for Epic H's static placeholder
  preview) was being reused for the real 10-field `IExamination` shape
  (bp/pulse/temperature/respiratoryRate/spo2/weight/height/bloodSugar/painScore/
  heartRate) — indices past 5 fell back to raw camelCase keys, and indices 3–5 showed
  the wrong label entirely (respiratoryRate labeled "Weight (kg)", etc.). Fixed by
  adding a separate `examinationLabels` getter (all 10, no BMI) in each of the three
  template components, used only when real `document` data is bound. The lesson: this
  class of bug (silently-misaligned parallel arrays) is exactly why the "verify in a
  real browser" step of this workflow isn't optional — `ng build`/`ng test` both stayed
  green throughout.

## Verified

- **Backend**: `dotnet build` clean, `dotnet test` — 293/293 passing (260 pre-existing +
  33 new, all against the real local Postgres — the 3 `SequenceGeneratorTests` that need
  a live DB connection now pass too, once Docker was up). `AddPrescriptionAuthoring`
  migration applied to the local Postgres database.
- **Frontend**: `ng build` clean, `ng test` — 195/195 passing (168 pre-existing + 27 new).
- **Manual end-to-end smoke test** against the live local stack (Postgres + Keycloak,
  real JWTs, via a scripted Playwright walkthrough — not just a description): logged in
  as `sabrina.khatun` (a real seeded doctor, password reset via the Admin API for this
  test), hit the US-011 duplicate-active-consultation guard against genuine leftover
  data from an earlier session and used the new "Continue to that consultation" button
  to proceed, filled chief complaint/diagnosis/medicine, confirmed the duplicate-medicine
  guard blocks a repeated Napa 500mg line, toggled English → বাংলা → English, saved as
  draft, resumed the draft, hit the finalize checklist (blocked on missing signature +
  template), set both via Prescription Preferences, finalized successfully, confirmed
  the finalized list and read-only view render correctly, then logged out entirely and
  loaded the public `/verify?id=RX-2026-0001` page anonymously — rendered correctly with
  a real scannable QR code and a clean "not found" state for an invalid id.
- Left in the dev DB from this test: patient/consultation/prescription rows for
  `sabrina.khatun`'s care team (`RX-2026-0001`, finalized) — same harmless
  seed-adjacent residue convention as every prior epic's smoke test, no cleanup endpoint
  exists.
