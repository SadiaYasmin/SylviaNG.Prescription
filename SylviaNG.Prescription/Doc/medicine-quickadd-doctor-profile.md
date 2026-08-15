# Phase 4 — Medicine Catalog, Quick Add Presets, Doctor Profile

Epics F (Medicine Catalog & Prescribing Analytics, US-036–040), G (Quick Add Presets,
US-041–044), and K (Doctor Profile & Personal Settings, US-061–065), built together on
`feature/phase-4-catalog-presets-profile` per Sadia's call — none of the three blocks the
others, matching the precedent already set for combining B+C and D+E. All three had minimal
Epic D stubs (read-only medicine search, doctor-scoped Quick Add add/list/delete, a
preferences screen with just template+signature) that this branch turns into the real
epics — extending existing entities/endpoints, not redesigning them.

Build order: **F → K → G** (F is the most self-contained and G's Medicine presets reference
the catalog; K exercises the self-service pattern a second time before G's trickier seeding
logic; G is the most design-heavy piece, built last with two other reference modules already
in place).

---

## Epic F — Medicine Catalog & Prescribing Analytics

- **Two response shapes, not one with an optional field**: `MedicineSummaryResponse`
  (existing, unchanged — brand/generic/strength/dosageForm/category) for Staff and the
  authoring autocomplete; `MedicineCatalogResponse` (adds `manufacturer`, `active`,
  `totalPrescribed`) for Admin/Doctor's catalog management screen. A Staff-authenticated
  request's JSON body structurally cannot contain `totalPrescribed` (US-040) — not merely
  hidden client-side.
- **Two endpoints, not a role-branch on one**: `GET /prescription/medicines` stays cheap and
  role-agnostic (search only, no aggregation) because it's also the prescription-authoring
  autocomplete's hot path — every keystroke, for all three roles. `GET
  /prescription/medicines/catalog` (Admin/Doctor only) is the separate, heavier query that
  computes "Total Prescribed."
- **"Total Prescribed" is an in-memory aggregate, not a SQL join** — `PrescriptionRecord`'s
  medicine line items (`MedicinesJson`) are a free-typed text snapshot with no FK to
  `Medicine.MedicineId` (see `PrescriptionRecord`'s doc comment). `GetMedicineCatalogHandler`
  loads finalized prescriptions (scoped to the caller's own `DoctorId` for Doctor, unscoped
  for Admin), deserializes each line, and counts by the same normalized
  `MedicineDuplicateGuard.NormalizeKey(brand, strength)` used by the duplicate-line guard and
  the catalog's own create/update duplicate check. Revisit as a real SQL aggregate if a
  `MedicineId` FK is ever added to line items — explicitly out of scope here.
- Admin-only CRUD (`Create`/`Update`/`Deactivate`); deactivate never hard-deletes (historical
  prescriptions are already immutable text snapshots, unaffected either way) — it just drops
  the medicine out of future autocomplete/catalog results via the existing `Active` filter.
- Frontend: new `medicine-management` module (`medicine-list` + `manage-medicine`, mirroring
  `template-management`'s shape) at `/medicines`, nav entry for Admin/Doctor/Staff.
  `MedicineService.search()` stays untouched for the authoring autocomplete;
  `getCatalog()`/`getById()`/`create()`/`update()`/`deactivate()` are new.

## Epic G — Quick Add Presets

- **Update/Edit slice added** (previously add/list/delete only) — `SectionType` is immutable
  on edit; changing a preset's section is modeled as delete+recreate.
- **Starter-seed-on-first-use (US-044)**: a new `DoctorQuickAddSeedState` table
  (`DoctorId`+`SectionType` composite key, no surrogate id) tracks whether a doctor's section
  has ever been seeded — deliberately independent of the section's current row count, so a
  doctor who deliberately empties a previously-seeded list stays empty rather than being
  silently refilled. `QuickAddPresetSeeder.SeedIfNeededAsync` is called from
  `GetQuickAddPresetsHandler` (the list query) — self-triggering the first time any of the 5
  lists is opened, no coupling to doctor creation.
- **Payload shapes** (all JSON-serialized in `PayloadJson`, matching what
  `QuickAddSelectComponent.choose()` already `JSON.parse()`s and emits): Medicine mirrors the
  real Rx line shape (`medicine/strength/dosage/frequency/duration/instructions`); Diagnosis
  mirrors `DiagnosisItem{text, icd10}` (not a plain string — Diagnosis is the one section
  whose real content shape isn't a bare string list); Investigation is a plain string;
  Advice/FollowUp are bilingual `{en, bn}` objects, needed for US-043 and for a preset to
  "always render in whichever language the prescription currently uses" per the epic's own
  acceptance criterion.
- **Auto-translate (US-043)**: `AdviceFollowUpPhraseDictionary` is a small hardcoded
  English→Bangla dictionary (never a live translation call, per feature.md §13's explicit
  scope boundary), sharing its literal phrases with the seeder's starter set so the two don't
  drift as separately-maintained lists. `GET
  /prescription/quick-add/advice-phrase-dictionary` merges it with the calling doctor's own
  previously-saved Advice/FollowUp pairs (the doctor's own edits take precedence). Frontend:
  typing English looks up the trimmed/lowercased value; auto-fills Bangla only if that field
  is empty or hasn't been manually edited; always confirms before overwriting a
  manually-entered Bangla translation.
- Frontend: one generic `quick-add-management` module (`quick-add-list` +
  `manage-quick-add-preset`) shared by all 5 section types — parameterized by route
  (`/quick-add/:section`), not 5 near-duplicate component pairs, matching how the backend
  already models all 5 as one entity/enum. The manage form renders different fields per
  section via a `payloadShape` lookup (`medicine`/`diagnosis`/`text`/`bilingual`).

**Known gap, deliberately out of scope this pass**: `QuickAddSelectComponent` (the "Quick
Add [X]" one-click insert dropdown used during authoring) exists from the Epic D stub but is
not yet wired into the classic/corporate/government templates' C/C, H/O, Investigation, Dx,
Rx, or Advice sections. A doctor can now fully manage their presets, but can't yet insert one
with a click while authoring — that wiring touches 5 different content shapes (chip-list
strings vs. `DiagnosisItem` vs. `MedicineItem` vs. bilingual Advice/FollowUp) across 3
templates and deserves its own focused pass rather than being bolted on at the end of this
one. Flag for a fast follow.

## Epic K — Doctor Profile & Personal Settings

- Extends the existing `DoctorPreferencesController` (`prescription/doctors/me/...`) rather
  than adding a new controller — it already exists specifically to host Doctor-only
  self-service actions separate from the Admin-only `DoctorsController`. New sibling routes:
  `GET/PUT .../profile`, `PUT/DELETE .../photo` (existing `.../preferences` routes and URLs
  are unchanged).
- `UpdateDoctorProfileHandler` resolves the doctor via `CallerContextResolver` — never from a
  request-supplied id, so self-service can never target another doctor's record. Mirrors
  `UpdateDoctorHandler`'s field-write + duplicate-license check, omitting the
  `IsActive`/Keycloak-enable block (Admin-only). Self-editable fields are exactly US-061's
  list (name/qualification/department/license/phone/email) — Specialization/ExperienceYears/
  Gender/JoiningDate stay Admin-only.
- **US-063 signature background removal is client-side**, via `@imgly/background-removal`
  (confirmed choice, matching the reference prototype and feature.md's own suggested
  default) — no backend change needed; the existing signature endpoint already just stores a
  base64 PNG, now populated with an already-background-removed image.
  `SignatureProcessingService` wraps the library; the manage-profile flow is
  validate-up-front (invalid files never reach the API) → processing state (original file
  preview retained) → success/error, with a Retry button that re-runs processing on the
  already-held `File` object — no re-upload needed on a transient failure, per the literal
  acceptance criterion. Manually verified end-to-end in a real browser: the WASM model loads,
  processes, and round-trips through the API correctly.
- **US-064/065 were already correct from the Epic D stub** — verified, not re-built:
  `StartOrResumePrescriptionHandler` already snapshots `Doctor.PreferredTemplateId` into
  `PrescriptionRecord.TemplateId` at creation time; `FinalizePrescriptionHandler`'s checklist
  already blocks finalize without a preferred template; `preferences.component.html` already
  rendered `PreferredLanguage` read-only. The one missing piece was the *proactive* dashboard
  nudge — added to `DashboardComponent` (a banner for Doctor role when no preferred template
  is set, linking to Preferences).
- Frontend: expanded `PreferencesComponent` into the full profile page (not a new sibling
  screen — it's already Doctor-scoped and already owns template+signature) covering profile
  fields, photo upload/removal, and the background-removal signature pipeline. Extracted a
  small `shared/utils/image-upload.util.ts` (validate + read-as-data-URL) since this became
  the third+fourth copy of that logic across the codebase.

---

## Verification

- Backend: `dotnet test` — 349/349 passing (was 337 before this branch).
- Frontend: `ng test --watch=false --browsers=ChromeHeadless` — 279/279 passing (was 227
  before this branch, +14 from the earlier Epic D+E UI-polish session this same day, +38 from
  this branch).
- `ng build` / `dotnet build` both clean.
- Manual, real browser (Playwright): logged in as Admin — created a medicine, confirmed
  Total Prescribed sorts correctly against real finalized-prescription data (Napa=2,
  Amdocal=1, matching actual seeded prescriptions), deactivated a medicine with the
  confirmation dialog. Logged in as a seeded doctor (`sabrina.khatun`) — confirmed the
  starter Quick Add set seeds automatically on first open of each section, confirmed
  auto-translate fills Bangla correctly for a known phrase, confirmed the full Profile page
  loads real data and the signature upload pipeline runs the real WASM background-removal
  model end-to-end (processing → success, verified via console logs and screenshots — no
  errors, correct state transitions).
