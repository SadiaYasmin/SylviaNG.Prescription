# Prescription Template Engine & Hospital Branding (Epic H, US-045–052)

## What it is

Admin-only management of two things: the hospital's single global branding record
(`HospitalSettings`) and a set of `PrescriptionTemplate` records admins create,
configure, and enable/disable for doctors to use. Each template has a base layout
*type* (Classic/Corporate/Government), a language, and a large config block
(header/footer/style/visibility/labels/print settings). A live preview renders the
real three layout variants with placeholder clinical content, both as a list-page
thumbnail and inside the editor.

## Why it was built

Phase 2 of the build order — independent of Patients/Consultations, but Epic D
(Prescription Authoring) can't finalize a prescription without a doctor having a
resolvable preferred template, so this had to land before the Phase 3 clinical chain
(Patients → Consultations → Prescription Authoring → Lifecycle) starts.

## Backend

- **Two new entities**, both `: Audit`: `PrescriptionTemplate` (`Name`, `Type` enum
  `TemplateTypeEnum`, `Language` enum `TemplateLanguageEnum`, `Enabled`,
  `IsSystemDefault`, `ConfigJson`) and `HospitalSettings` (singleton row — name,
  logo/seal as base64 strings, address, phone, emergency number, email, website,
  slogan EN+BN, license number).
- **`ConfigJson` design**: the cohesive header/footer/style/visibility/print/labels
  config is serialized as one JSON string column (`System.Text.Json`) rather than
  ~30 individual nullable columns or EF's `.ToJson()` owned-entity mapping — the
  latter isn't reliably portable across this project's three configured providers
  (SqlServer/Npgsql/Oracle). `Name`/`Type`/`Language`/`Enabled` stay real columns
  since list/filter queries need them. Deserializes into a `TemplateConfig` DTO
  (`Application/Features/Templates/Models/TemplateConfig.cs`) at the mapping layer.
- **System-default fallback guarantee**: exactly one template — Classic, English,
  `IsSystemDefault = true` — is seeded on startup (`TemplateEngineSeeder`, mirrors
  `AdminAccountSeeder`'s shape, idempotent) and both `DeleteTemplate` and disabling
  via `ToggleTemplateEnabled` reject that row with a 400. This is what makes "a
  doctor's disabled/deleted preferred template falls back to the classic default"
  (US-050) concretely enforceable once Epic D/E consume this API — there is always
  at least one guaranteed enabled template to resolve to. Corporate/Government
  instances are **not** pre-seeded — "choose a base type" (US-046) is a genuine
  admin creation action, not a pre-populated gallery.
- **`CreateTemplate` never accepts client-supplied config** — it takes `Name`+`Type`+
  `Language` only, and the server fills `ConfigJson` from `TemplateDefaults` (a
  static class with per-type defaults and full EN/BN 44-key label dictionaries).
  `UpdateTemplate` is the only place client-edited config is accepted.
  `DuplicateTemplate` always produces `IsSystemDefault = false` on the clone.
- **Controllers**: `TemplatesController` (`/prescription/templates`) is class-level
  `[Authorize(Roles = "Admin")]`, matching `DoctorsController`'s pattern exactly —
  pure admin surface. `HospitalSettingsController` (`/prescription/hospital-settings`)
  splits roles per-action like `StaffController`: `GET` is
  `[Authorize(Roles = "Admin,Doctor")]` (doctors will need real branding data once
  Epic D's authoring/preview exists), `PUT` stays `Admin`-only.
- **New `BadRequestException`** (`Application/Common/Exceptions/`) — didn't exist
  before this feature; added following the existing `NotFoundException` convention
  and wired into `GlobalExceptionHandlerMiddleware` as HTTP 400. Used for the
  system-default delete/disable rejection.
- **Namespace/type-name collision, same shape as Epic J's `Staff` gotcha**:
  `Domain.Entities.HospitalSettings` and the feature folder
  `Application/Features/HospitalSettings` share the exact name — a namespace
  segment shadows a same-named type for bare identifier lookup within that
  namespace tree (`CS0118`). Unlike Staff (which renamed the folder to `Staffs`),
  here the fix was to simply never spell the bare `HospitalSettings` type name
  inside code living in the colliding namespace — `var` for inferred entity
  references in handlers, fully-qualified `Domain.Entities.HospitalSettings` in the
  equally-colliding test namespace `Tests.Handlers.HospitalSettings`. Worth knowing
  before adding anything else under a feature folder that matches an entity name.
- New EF migration: `AddTemplatesAndHospitalSettings`.

## Frontend

- New lazy-loaded `pages/template-management/` module (`template-list`,
  `manage-template`, `hospital-settings`), admin-only route (`/templates`, single
  `roleGuard` at the parent — no doctor-facing route yet; the doctor's
  template-picker modal is Epic K's job, consuming `GET /templates` but not built
  here).
- **`template-list` renders real thumbnail previews per card**, not just
  name/type/language text — since the visual layout *is* the point of a template
  gallery. Because `GET /prescription/templates` intentionally returns light
  summaries (no `config`, per the "no pagination — small list" design), the list
  component does a `forkJoin` of per-id `GET /{id}` calls to hydrate each card's
  preview after the initial list loads. Acceptable at this list's expected size
  (a handful of templates); would need revisiting if the template count ever grew
  large.
- **Shared rendering engine** (`shared/template-preview/`), built once so it can
  be reused unmodified by Epic D/E's real prescription view/print later, not just
  Epic H's editor/list: a dispatcher component picks one of
  `classic-template`/`corporate-template`/`government-template` by `type`. All
  three consume the same new `shared/patient-info-block/` component for the
  proportional, non-truncating Name/Age/Sex/Phone/Blood-Group/Allergies/Date/Rx-No.
  layout (US-052) — built once as a real shared component rather than duplicated
  per variant.
- **Government color-lock is structural, not just a CSS class toggle**:
  `government-template.component.ts`'s `rootStyle`/`headerStyle`/`footerStyle`
  getters deliberately never read `config.style.accentColor`/
  `config.header.bgColor`/`config.footer.bgColor` — hardcoded ink/border-only
  values are used regardless of what's in the config object, so the "Government
  layout is always monochrome" rule survives even if someone edits the CSS later.
  The Header/Footer/Style tabs in the editor still show hint text under those
  controls when `type === 'Government'` — the inputs stay enabled since the stored
  values are harmless/inert, they're just never rendered for that type.
- **Reset-labels / language-switch logic** ported as two pure, exported,
  unit-tested functions in `manage-template.component.ts`:
  `labelsMatchLanguageDefaults(labels, language)` (deep-equality against that
  language's default dictionary) and `toLangCode` (backend's PascalCase `En`/`Bn`
  → the lowercase `en`/`bn` the shared label-default helpers use). On the Language
  tab's language change: if the current labels still exactly match the *old*
  language's defaults (never customized), swap wholesale to the new language's
  defaults; otherwise the admin's customization is left untouched. The explicit
  "Reset to language defaults" button always does an unconditional overwrite. A
  drift banner shows whenever current labels don't match the current language's
  defaults.
- **7 real editor tabs** — Header, Footer, Style, Visibility, Labels, Language,
  Print — closing two gaps the reference prototype had (its own spec called for a
  Print tab and a proper Language tab, but its actual UI never built either; Print
  settings just sat unused in stored config and Language was a bare dropdown
  outside the tab strip).
- **Hospital Settings' `licenseNumber`/`seal` are actually rendered** (small text +
  seal image in the template footer, across all three variants) — the reference
  prototype captured and persisted both fields but never displayed them anywhere,
  an acknowledged prototype gap this build closes instead of carrying forward.
- **Native `<input type="color">` instead of PrimeNG's `p-colorpicker`** for every
  color field — PrimeNG's control emits a non-`#`-prefixed hex string that isn't
  directly usable as the CSS color value the config model stores, and native color
  inputs can't represent `null`, so an explicit "Clear" button sits next to each
  one for the null case.
- **`IUpdateTemplateRequest` sends an optional `language` field** alongside
  `name`/`config` on `PUT /{id}`, even though the endpoint's primary documented
  contract is `{ name, config }` — the Language tab needs to persist a language
  change and there's no separate endpoint for it. The backend's model binding
  ignores unmapped JSON properties by default, so this is a no-op if unused, but
  worth knowing about if the backend's `UpdateTemplateRequest`/handler ever needs
  updating to actually read `language` in a later pass.

## Verified

- **Backend**: `dotnet build` clean, `dotnet test` — 154/154 passing (100
  pre-existing from Epic A/I/J + 54 new for Templates/HospitalSettings).
- **Frontend**: `ng build` succeeds (`template-management-module` lazy chunk
  emitted alongside the other feature modules; only a pre-existing initial-bundle
  budget warning, unrelated to this feature since all new code lives in the lazy
  chunk), `ng test` — 103/103 passing (63 pre-existing + 40 new).
- **`AddTemplatesAndHospitalSettings` migration generated but not yet applied** —
  local Postgres wasn't reachable this session (`Failed to connect to
  127.0.0.1:5432`, same Docker-not-running situation noted in Epic J's session).
  Before treating this feature as fully verified end-to-end: get the local stack
  up, run `dotnet ef database update`, then manually walk: admin creates a
  Corporate template → edits all 7 tabs → sees the live preview update → duplicates
  it → disables it → attempts to disable/delete the system-default template
  (blocked) → edits Hospital Settings (logo/slogan/license/seal) → preview
  reflects it → switches a template's language and confirms untouched labels are
  preserved vs. the reset behavior.
