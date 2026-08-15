# Phase 5 — Bilingual Support (Epic L) + Backend Integrity & Platform (Epic N)

Epic L (Bilingual Support, US-066–071) and Epic N (Backend Integrity & Platform,
US-080–085), both cross-cutting per STATUS.md's Suggested Build Order, built together on
`feature/phase-5-bilingual-platform-integrity` — matching the Phase 4 precedent of bundling
multiple stories into one phase-branch. Research at the start of this session found several
stories already satisfied by existing work — US-066 (per-template bilingual labels), US-071
(patient-info-block's flex layout already tolerates longer Bangla text), US-080/081/082 (real
Postgres persistence, atomic `SequenceGenerator`, EF Core migrations were all already in
place from earlier epics) — so this branch covers only the real gaps.

---

## Epic L — Bilingual Support

### US-067/068 — Bangla digits, gender localization

`formatPatientInfoBlock()` (`shared/prescription-sections/prescription-display.util.ts`) is
the single choke point every template component's patient-info rendering goes through — it
previously took no language parameter at all, so a Bangla-language finalized prescription
showed age/sex/allergies in English. Now takes `language: 'en' | 'bn'`, passed from each
template component's own `@Input() language`. In Bangla mode: age and date convert to Bangla
numerals (`toBanglaDigits()`, `shared/utils/bangla-digits.util.ts`); sex localizes via a new
`GENDER_LABELS_BN` map (`shared/utils/gender-labels.util.ts`). Deliberately **not** converted:
`rxNo`, phone, doctor license/reg numbers — these are identifiers/codes, not "read numbers."
Scope is prescription-rendering only — the rest of the admin UI (gender dropdowns, patient
forms) stays English regardless of a given prescription's language, matching how templates
are per-template-language rather than a whole-app-locale switch.

### US-069 — extend static bilingual dictionaries

- **Quick Add Medicine starters**: `QuickAddPresetSeeder.cs`'s 3 Medicine starters gained
  parallel `dosageBn`/`frequencyBn`/`durationBn`/`instructionsBn` fields alongside the
  existing English ones — additive, not a shape change, so the current (English-only)
  `medicine-list-input` UI keeps working unchanged. Wiring the authoring UI to pick a variant
  by document language is an explicit follow-up, not built this phase.
- **Allergy presets**: `AllergyPresetEnum` (None/Penicillin/Dust/Seafood/Latex) had no Bangla
  labels anywhere — added `ALLERGY_LABELS_BN` alongside the existing `ALLERGY_LABELS` map in
  `prescription-display.util.ts` (this is a frontend-only concern; there's no backend
  dictionary for the English labels either, so none was added for Bangla).
- **Allergy "Other" free text**: had no suggestion list at all. New
  `@core/constants/allergy-other-suggestions.ts` — a fixed `{en, bn}[]` of common allergens,
  offered via a native `<datalist>` on the patient form's "Other" input. Never constrains what
  gets typed/saved (same non-enforcing pattern as Quick Add presets); only `en` is wired into
  the form today, `bn` is seed data for a future bilingual admin UI.
- QR message and Advice/FollowUp phrases were already bilingual (`TemplateDefaults.cs`,
  `AdviceFollowUpPhraseDictionary.cs`) — untouched.

### US-070 — Bangla phonetic (Avro-style) typing

`avro-phonetic` (the npm package originally planned) turned out to be a jQuery plugin with no
standalone function API — swapped for **`nodejs-avro-phonetic`**, which exposes a plain
`parse(text): string` function, zero dependencies, confirmed working (`parse('ami banglay
gan gai')` → `আমি বাংলায় গান গাই`). Added to `angular.json`'s
`allowedCommonJsDependencies` (CJS-only package); a hand-written ambient `.d.ts` covers the
one function used (`shared/types/nodejs-avro-phonetic.d.ts`).

New `BanglaPhoneticInputDirective` (`shared/directives/bangla-phonetic-input.directive.ts`,
selector `appBanglaPhoneticInput`): listens for `input`, tracks the trailing
whitespace/punctuation-delimited word, and on a word-boundary character replaces it with its
Bengali transliteration — standard Avro-editor UX. Mutates the host element's DOM value
directly then redispatches a synthetic `input` event so whatever's already bound
(`ngModel`/manual `(blur)` handlers) picks up the change, rather than owning the field's value
itself — a `replaying` reentrancy guard stops the redispatch from re-triggering itself.

**Opt-in per component instance**, never global or forced — each clinical input component
(`chip-list-input` shared by C/C, H/O, Investigation, Advice; `diagnosis-list-input`;
`medicine-list-input`, one toggle covering all 5 fields per row, medicine name/strength
excluded since they're catalog lookups not phrases; and the Follow-Up input in each of the 3
template components) has its own local `banglaMode` boolean and a small toggle button.

---

## Epic N — Backend Integrity & Platform

### US-083 — file storage for images

`Doctor.PhotoUrl`/`SignatureUrl` and `HospitalSettings.LogoUrl`/`SealUrl` replace the old
`*Base64` columns (migration `RenameImageFieldsToUrls` — drop+add, no data-backfill; this
project has no staging/prod DB anywhere, only local dev, so existing base64 image data is
accepted as stale/reseeded rather than writing throwaway backfill code). New
`IFileStorageService`/`LocalDiskFileStorageService` writes to `wwwroot/uploads/{category}/`
(local disk — a deliberate choice, not cloud object storage, since nothing else in the
project has a cloud dependency), served via `app.UseStaticFiles()`. Upload *request* DTOs
(`UpdateDoctorPhotoRequest.PhotoBase64` etc.) are unchanged — only the *stored*/*returned*
shape moved to relative URLs (`/uploads/doctor-photos/{guid}.png`), resolved to absolute on
the frontend via `resolveAssetUrl()`/`BASE_URL_Host`.

**A real bug found and fixed along the way**: `UpdateHospitalSettingsHandler` and
`UpdateDoctorHandler` (admin doctor edit) originally always overwrote the image field with
whatever the request carried. That was safe under base64-symmetric GET/PUT (resubmitting the
same value was a no-op), but breaks once GET returns a URL and PUT needs base64 — a form save
that doesn't touch the image would now send `null` and silently wipe it. Fixed with an
explicit convention: `null` in the update request means "leave unchanged," `""` (empty
string) is the explicit "remove" signal. `UpdateDoctorPhotoHandler`/`UpdateDoctorSignatureHandler`
are unaffected — they're dedicated single-purpose endpoints only ever called from an explicit
upload/remove action, never a bundled form save, so `null` there still unambiguously means
"remove."

Renames cascade into both Epic K's self-service `preferences` screen and Epic I's admin
`manage-doctor`/`doctor-details` screens (shared `Doctor` entity/DTO family) — both updated
together.

### US-084 — targeted test gap-filling

Not a full audit — three specific additions:
- `FinalizePrescriptionHandlerTests`: the existing US-026 precondition `[Theory]` only ever
  tested diagnosis+medicine missing together, never individually — added
  `Handle_WhenMissingOnlyDiagnosisOrOnlyMedicine_ShouldThrowBadRequestException` with both
  individual cases.
- New `RoleVisibilityTests.cs`: a consolidated Admin/Staff/Doctor matrix directly against
  `PatientVisibilityScope` (the one shared implementation `GetPatientList`/`GetPatientDetails`/
  `UpdatePatient` all reuse) — proportionate scope, not re-testing what per-handler tests
  already cover elsewhere.
- `SaveDraftPrescriptionHandlerTests`: added an explicit test for the US-017 invariant's other
  direction (`Handle_ShouldNeverMarkTheConsultationCompleted`) — the "finalize completes the
  consultation" direction was already covered incidentally by `FinalizePrescriptionHandlerTests`.

### US-085 — audit trail

Deliberately deferred, no code. See `ARCHITECTURE.md`'s Phase 5 update and `STATUS.md`'s
Known Issues for the documented decision and the specific gap (`Audit.CreatedBy`/`UpdatedBy`
exist but nothing populates them).

---

## Verification

- Backend: `dotnet test` — 354/357 passing (the 3 failures are pre-existing
  `SequenceGeneratorTests`, unmodified by this branch, which need a live Postgres connection —
  Docker was unavailable this session).
- Frontend: `ng test --watch=false --browsers=ChromeHeadless` — 289/289 passing (was 279
  before this branch).
- `ng build` / `dotnet build` both clean, including after adding the `nodejs-avro-phonetic`
  CJS dependency.
- **Manual browser verification was not possible this session** — Docker/Postgres/Keycloak
  were unavailable, so there was no way to log in and exercise bilingual rendering, the
  photo/logo upload round-trip, or the phonetic-typing toggle live. Worth a manual walkthrough
  next session once Docker is reachable, especially: uploading a doctor photo/hospital
  logo/seal and confirming they render via URL and survive a page reload; opening a
  Bangla-language finalized prescription and confirming age/date/gender/allergies render in
  Bangla; trying the phonetic toggle on a real clinical field.
