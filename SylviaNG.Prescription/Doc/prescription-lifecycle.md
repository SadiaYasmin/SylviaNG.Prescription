# Prescription Lifecycle (Epic E, US-029–035)

## What it is

Everything downstream of authoring: a doctor's own draft/finalized lists, a read-only
single-prescription view for any authorized role, a patient's prescription history panel,
client-side PDF export with A4 pagination, a real scannable QR code, and a public
no-login verification page.

## Why it was built

Fourth and final link in Phase 3's core dependent chain — the payoff for Epic D.
Combined into the same `feature/prescription-authoring` branch as Epic D rather than a
separate branch/day, same reasoning as the earlier Patient+Consultation combination:
Epic E has no standalone value without D (there's nothing to list, view, print, or
verify without a real authored prescription).

## Backend

- `GetMyDraftPrescriptionsHandler`/`GetMyFinalizedPrescriptionsHandler` (US-029/030):
  doctor-own-scoped (`PrescriptionVisibilityScope` with `ownOnly: true`), sorted by
  `SavedAt`/`FinalizedAt` descending. Drafts optionally filter by `?patientId=`
  (deep-link from the patient-history panel).
- `GetPrescriptionDetailsHandler` (US-031): the broader visibility scope
  (`ownOnly: false`) — Admin sees all, Doctor also sees care-team prescriptions they
  didn't author, Staff sees prescriptions for patients they registered. Out-of-scope is
  reported as 404, never 403, same convention as every other visibility check in this
  codebase.
- `GetPatientPrescriptionHistoryHandler` (US-032): same broader scope, filtered to one
  patient, feeds the "patient's past prescriptions" panel during authoring.
- `VerifyPrescriptionHandler` (US-035): `[AllowAnonymous]`, looked up by `DisplayCode`
  (never a raw numeric id), and — critically — only ever matches `Status == Finalized`.
  An in-progress draft's display code 404s exactly like an unknown one; nothing about
  the response distinguishes "doesn't exist" from "exists but isn't finalized yet."
- All three read handlers (`GetPrescriptionDetails`/`GetPatientPrescriptionHistory`/
  `VerifyPrescription`) return the same `PrescriptionDocumentResponse` shape used by
  authoring, with the resolved `TemplateConfig` + `HospitalSettings` embedded inline —
  View/Print/Verify never need a second protected-endpoint call just to render.

## Frontend

- **Draft list / Finalized list** (`pages/prescription-management/draft-list`,
  `finalized-list`): table pages, "Continue"/"View" and "View"/"Download PDF" row
  actions respectively. Finalized list highlights a just-finalized row via Angular
  router navigation `state` (`justFinalizedId`), same mechanism the prototype used via
  React Router state.
- **View page** (`pages/prescription-management/prescription-view`,
  `/prescriptions/view/:id`): reuses `app-template-preview` in read-only mode
  (`editable=false`). `?action=print` and `?action=download` query params auto-trigger
  the corresponding action on load, matching the prototype's deep-link contract.
- **PDF export** (`pages/prescription-management/pdf/prescription-pdf.util.ts`): adds
  `jspdf` + `html2canvas` (neither was installed before this epic). A lighter-weight
  version of the prototype's dedicated pagination engine: since the page is already
  rendered live (no hidden-DOM measurement pass needed), it measures each block-level
  element already present in the DOM (`header`, `footer`, `.patient-block`, `.section`/
  `.card`/`.compact-grid`/etc. — the union of class names across all three templates)
  and greedily packs them into A4 content-height budgets, capturing each page as its own
  `html2canvas` region via the `y`/`height` clip options rather than five separate
  canvases. A `ContinuationHeader`-equivalent (hospital/doctor/patient name + "Page X of
  Y") is rendered and captured onto every page after the first. **Scoped down from the
  prototype's version**: no mid-list splitting of an oversized medicine table with a
  repeated heading — a section that's individually taller than one A4 page still gets
  its own page rather than being split further. Browser-native printing (the Print
  button) gets the same "never split an element" guarantee for free via `break-inside:
  avoid` print CSS on the same class union, added to all three templates' stylesheets.
- **QR code** (`shared/template-preview/prescription-qr/`): extended, not replaced —
  `PrescriptionQrComponent` now accepts an optional `qrValue`; when set (a finalized
  prescription only, via each template's `verifyUrl` getter), it renders a real
  scannable QR (`qrcode` npm package) encoding `${origin}/verify?id=<DisplayCode>`
  instead of the decorative placeholder pattern Epic H left behind specifically for this
  epic to fill in (see that component's original doc comment).
- **Public verify page** (`src/app/verify/`): a new top-level module, registered as a
  third sibling in `app.routes.ts` next to `login` and the guarded `''` branch — never
  nested under the authenticated `pages` tree, so it carries no `authGuard` and no Shell
  chrome. Reads `?id=`, calls the anonymous verify endpoint, renders the same read-only
  template component inside a minimal standalone header. A missing/invalid id shows a
  clear "Not Found" state, never a stack trace or someone else's data.

## Verified

Covered together with Epic D in `Doc/prescription-authoring.md`'s Verified section — the
same backend/frontend test runs and the same manual smoke-test walkthrough exercise both
epics in one continuous flow (author → save → finalize → list → view → download → public
verify), since splitting the verification wouldn't reflect how the feature is actually
used.
