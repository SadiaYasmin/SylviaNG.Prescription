# PrescriptionMS — User Stories

This document turns every feature in [feature.md](feature.md) into implementable user stories with acceptance criteria, so the real (full backend + frontend) build can be planned and estimated directly from it. Stories are grouped into epics that mirror `feature.md`'s sections, and numbered `US-001` upward for traceability (e.g. in tickets/PRs).

Roles referenced: **Admin**, **Doctor**, **Staff**, **Public/Guest** (no login), **System** (automated backend behavior, not triggered by a person clicking something).

---

## Epic A — Authentication & Session Management

### US-001: Log in with real credentials
**As a** Doctor/Staff/Admin, **I want to** log in with my username and password, **so that** only I can access my account and the system knows my real role.
- Login fails with a clear error on wrong username or password.
- Role is never selectable on the login form — it comes from the authenticated account.
- Password field is masked; no plaintext password is ever logged or stored.
- A successful login redirects to the role-appropriate dashboard (`/home`).

### US-002: Stay logged in securely, and log out
**As a** logged-in user, **I want to** remain authenticated across page reloads until my session expires or I log out, **so that** I don't have to re-enter credentials constantly but also isn't left permanently signed in on a shared device.
- Session token has a defined expiry; an expired session redirects to `/login`.
- "Logout" immediately invalidates the session (server-side), not just clears client storage.
- All protected routes redirect an unauthenticated visitor to `/login`.

### US-003: Server enforces role on every request
**As the** System, **I want to** validate the caller's role on every API request, **so that** a user can never see or modify data outside their role's permissions even if the frontend is bypassed.
- Every endpoint checks role/ownership server-side (e.g. a staff member's API calls are rejected if they try to fetch a patient they didn't register).
- Attempting a forbidden action returns a 403, not partial/filtered data that looks successful.

### US-004: Admin creates and resets doctor/staff accounts
**As an** Admin, **I want to** create doctor and staff login accounts (and reset a forgotten password), **so that** account provisioning is controlled rather than self-service.
- Creating an account requires a username unique across the system.
- A temporary/reset password flow exists so a locked-out doctor/staff doesn't need to contact engineering.

---

## Epic B — Patient Management

### US-005: Register a new patient
**As a** Staff member, **I want to** register a new patient with their demographic and clinical-flag details, **so that** they can be found and treated going forward.
- Required: name, phone (validated as a Bangladesh mobile number format). Optional: DOB, gender, address, blood group, allergy.
- If DOB is provided, age is computed automatically and the manual age field is disabled; if DOB is absent, a manually entered age is accepted.
- Allergy is chosen from the fixed master preset list, or "Other" with free text.
- On save, the new patient is attributed to the registering staff member (`registeredBy`) automatically — not user-editable.
- Duplicate-phone patients are allowed to be created (no hard block), matching current behavior, unless the real build's stakeholders decide otherwise.

### US-006: Edit an existing patient
**As a** Staff or Doctor, **I want to** edit a patient's record, **so that** outdated details (address, allergy, phone, blood group) can be corrected.
- Same field validation as registration (US-005) applies to edits.
- Editing is only available for patients within the editor's visibility scope (US-008).

### US-007: Search patients
**As a** Staff, Doctor, or Admin, **I want to** search patients by name or phone, **so that** I can find an existing record quickly instead of risking a duplicate registration.
- Search is scoped by the same visibility rules as the patient list (US-008) — search never surfaces a patient outside the searcher's authorized scope.

### US-008: Role-scoped patient visibility
**As the** System, **I want to** restrict which patients each Staff/Doctor can see, **so that** each doctor + their assigned staff operate as an isolated care team and Admin retains full oversight.
- A Staff member sees only patients where `registeredBy == me`.
- A Doctor sees only patients registered by a Staff member currently assigned to that Doctor.
- Admin sees every patient, with a visible "Registered By" column.
- This rule is enforced by the API (see US-003), not only by hiding rows in the UI.

### US-009: Patient's saved history auto-carries into a new prescription
**As a** Doctor, **I want to** see a patient's previously recorded History pre-filled when I start a brand-new prescription for them, **so that** I don't have to re-type a returning patient's known history every visit.
- Pre-fill only happens for a genuinely new prescription (never overwrites a resumed draft's own in-progress History).
- Whatever is left in the History field at the moment a prescription is finalized becomes the patient's new Saved History for next time (verbatim replace, not merge/append).

---

## Epic C — Consultation & Queue Management

### US-010: Staff starts a consultation (issues a token)
**As a** Staff member, **I want to** register a consultation for a patient with one of my assigned doctors, **so that** the patient joins that doctor's queue with a token number.
- Token number is per-doctor, per-day, sequential (`T-01`, `T-02`, ...), starting over each calendar day.
- Consultation starts in `waiting` status.
- Token generation is atomic server-side (see US-057) so two simultaneous registrations for the same doctor never collide on the same token.

### US-011: Prevent duplicate active consultations
**As the** System, **I want to** detect that a patient already has an active (waiting/in-progress) consultation with the chosen doctor today, **so that** staff/doctors are guided to the existing encounter instead of accidentally creating a duplicate.
- On attempting to start a new consultation for such a patient, the user is prompted to either open the existing consultation or explicitly proceed to create a new one anyway.

### US-012: Prevent losing an unfinished draft
**As the** System, **I want to** detect that a patient has an unfinished draft prescription with this doctor (from any day), **so that** starting a "new" consultation doesn't orphan work already in progress.
- The doctor is prompted to continue the existing draft, view all of that patient's drafts, or explicitly start a fresh consultation anyway.

### US-013: Doctor opens a queued consultation
**As a** Doctor, **I want to** open the next waiting patient from my queue, **so that** I move directly into authoring their prescription.
- Opening a `waiting` consultation transitions it to `in_consultation`.
- The consultation's patient, doctor, and any already-linked prescription are loaded into the authoring view (§ Epic D).

### US-014: Doctor's Today's Queue widget
**As a** Doctor, **I want to** see today's waiting and in-progress patients on my dashboard, ordered by check-in time, **so that** I always know who's next.

### US-015: Staff's own queue widget
**As a** Staff member, **I want to** see the status of the patients I've registered today, **so that** I can tell them how long the wait might be.

### US-016: Admin monitors all consultations
**As an** Admin, **I want to** view every consultation across the hospital, filterable by date (today/yesterday/custom/range), doctor, status, and free-text search, **so that** I can monitor hospital-wide operations.
- Summary stat cards: Total, Waiting, In Progress, Completed for the current filter.
- A details modal shows registered-by, timestamps, and the linked prescription's status for a selected consultation.
- Result list is paginated.

### US-017: Consultation and prescription status always agree
**As the** System, **I want to** keep a consultation's status consistent with its linked prescription's status, **so that** "Draft"/"Finalized"/"Completed" never contradict each other anywhere in the UI.
- A `finalized` prescription always implies its consultation is `completed`.
- A non-finalized (or unlinked) prescription never leaves its consultation marked `completed`.
- This must be a transactional invariant enforced by the backend on every write that touches either record — not a periodic client-side repair pass (which is how the prototype currently patches it).

---

## Epic D — Prescription Authoring (Clinical Workflow)

### US-018: Start authoring from the queue
**As a** Doctor, **I want to** open an active consultation and land directly in a live, editable prescription document, **so that** authoring feels like writing directly on the prescription pad, not filling out a separate form.

### US-019: Quick-create for a walk-in, unqueued patient
**As a** Doctor, **I want to** pick any patient within my authorized scope directly from Create Prescription (without a pre-existing queued consultation), **so that** I can still write a prescription for someone staff didn't formally queue.
- Subject to the same duplicate-consultation (US-011) and unfinished-draft (US-012) guards.

### US-020: Resume a saved draft
**As a** Doctor, **I want to** reopen a previously saved draft and continue exactly where I left off, **so that** an interrupted consultation isn't lost.
- Reopening a draft never creates a second consultation record for the same encounter.

### US-021: Record chief complaints, history, examination/vitals, diagnoses, and investigations
**As a** Doctor, **I want to** capture chief complaint(s), history, structured vitals (BP, pulse, temperature, respiratory rate, SpO2, weight, height, blood sugar, pain score, heart rate), diagnosis(es), and investigation(s) as I go, **so that** the full clinical picture is on record.
- Each list-type section (complaints/history/diagnoses/investigations) supports add/edit/remove of individual entries.

### US-022: Prescribe medicines with duplicate protection
**As a** Doctor, **I want to** add medicine lines (name via autocomplete, strength, dosage, frequency, duration, instructions) and be blocked from accidentally adding the same medicine+strength twice, **so that** the Rx is unambiguous.
- Adding or editing a line to match an existing line's medicine+strength is rejected outright; the existing row is highlighted so the doctor edits it instead.
- Editing a row's own fields is never treated as "duplicating itself."
- Dosage/Frequency/Duration/Instructions each offer an autocomplete list of standard presets in the active language.

### US-023: Record advice and follow-up
**As a** Doctor, **I want to** add advice entries and a follow-up note, **so that** the patient knows aftercare and when to return.

### US-024: Toggle prescription language while authoring
**As a** Doctor, **I want to** switch a specific prescription between English and বাংলা while writing it, **so that** it matches the patient's/hospital's needs for that encounter.
- The toggle re-renders all standard labels and preset options in the chosen language immediately.
- The choice is remembered as the doctor's default for their next new prescription (any patient).
- Switching language never mutates an already-resumed draft's language behind the doctor's back — only an explicit toggle (or the one-time seed on a brand-new prescription) sets it.

### US-025: Quick Add inside authoring
**As a** Doctor, **I want to** insert one of my saved Quick Add presets into any relevant section with one click, **so that** repetitive entries are fast.
- Applies to Medicine, Diagnosis, Investigation, Advice, and Follow-Up sections.
- Quick Add Medicine still goes through the duplicate-medicine guard (US-022).

### US-026: Block finalize until required elements are present
**As the** System, **I want to** prevent finalizing a prescription that is missing a diagnosis, a medicine, the doctor's signature on file, or the doctor's preferred template, **so that** an incomplete or unusable prescription is never issued.
- All missing items are listed together (not one at a time) so the doctor can fix everything in one pass.

### US-027: Save as Draft
**As a** Doctor, **I want to** save my in-progress work as a draft at any point, **so that** I can pause and resume later without losing anything.
- Draft save stamps a "last saved" time and removes the consultation from the live queue.

### US-028: Finalize a prescription
**As a** Doctor, **I want to** finalize a prescription once it's complete, **so that** it becomes the permanent, unchangeable clinical record and can be printed/shared.
- Finalizing stamps a finalization time, marks the consultation `completed`, and updates the patient's Saved History (US-009).
- A finalized prescription can never be edited again through the authoring UI (shown read-only everywhere after).

---

## Epic E — Prescription Lifecycle: Lists, Viewing, Printing, Verification

### US-029: View my draft prescriptions
**As a** Doctor, **I want to** see a list of all my in-progress drafts, **so that** I can resume any of them.

### US-030: View my finalized prescriptions
**As a** Doctor, **I want to** see a list of all my finalized prescriptions, **so that** I can review past work.
- A prescription just finalized in this session is visually highlighted at the top of the list.

### US-031: View any single prescription read-only
**As a** Doctor, Staff, or Admin, **I want to** open a specific prescription by ID and see its full read-only rendering, **so that** I can review or audit it even if I didn't author it.

### US-032: See a patient's prescription history while authoring
**As a** Doctor, **I want to** see a patient's past prescriptions alongside the one I'm currently writing, **so that** I have full context without switching screens.

### US-033: Export a prescription as PDF
**As a** Doctor, **I want to** download a finalized (or in-progress) prescription as a print-accurate PDF, **so that** it can be handed to the patient or filed digitally.

### US-034: Multi-page A4 print pagination
**As a** Doctor or patient, **I want to** a long prescription (e.g. many medicines) to paginate correctly across true A4 page boundaries when printed, **so that** nothing is cut off mid-line and every page after the first repeats a short identifying header (hospital/doctor/patient name, "Page X of Y").

### US-035: Verify a prescription via QR code
**As a** pharmacy, patient, or third party (**Public/Guest**, no login), **I want to** scan the QR code on a printed prescription and see a read-only confirmation of its authenticity and content, **so that** I can trust the document without needing an account.
- The verification page shows exactly one prescription, matched by the ID encoded in the QR/link.
- An invalid/unknown ID shows a clear "not found" state, not an error page or someone else's data.

---

## Epic F — Medicine Catalog & Prescribing Analytics

### US-036: Browse and search the medicine catalog
**As a** Doctor, Staff, or Admin, **I want to** search the medicine catalog by brand or generic name, **so that** I can find the right medicine while prescribing or just browsing.

### US-037: Admin manages the medicine catalog *(New for real build)*
**As an** Admin, **I want to** add, edit, and deactivate medicines in the catalog, **so that** the reference list stays accurate without needing a data/engineering change.
- Prevents creating an exact duplicate brand+strength entry.
- Deactivating a medicine removes it from future autocomplete/search results without breaking historical prescriptions that already reference it.

### US-038: Admin sees hospital-wide prescribing totals
**As an** Admin, **I want to** see, for every medicine, the total number of finalized prescriptions that included it — across all doctors, **so that** I can spot the most/least prescribed drugs hospital-wide.
- The medicine list is sorted by this total, descending.

### US-039: Doctor sees their own prescribing totals
**As a** Doctor, **I want to** see the same "Total Prescribed" figure, but scoped only to my own finalized prescriptions, **so that** the number reflects my own practice, not the whole hospital.
- The medicine list is sorted by this doctor-scoped total, descending.

### US-040: Staff never sees prescribing analytics
**As a** Staff member, **I want to** see the plain medicine catalog with no prescription-count column or sorting, **so that** clinical-usage analytics stay limited to clinical/administrative roles.
- The "Total Prescribed" column and any related sort must not be present in the API response or the UI for a Staff-authenticated request — not merely hidden by CSS.

---

## Epic G — Quick Add Presets

### US-041: Manage Quick Add Medicine presets
**As a** Doctor, **I want to** search, add, edit, and delete my own frequently-prescribed medicine presets (with default strength/dosage/frequency/duration/instructions), **so that** prescribing them later is one click.
- Fields are stored referencing the shared bilingual preset dictionaries by id (not literal text), so a saved preset always renders in whichever language the prescription currently uses.

### US-042: Manage Quick Add Diagnosis / Investigation presets
**As a** Doctor, **I want to** search, add, edit, and delete my own frequent diagnosis and investigation shortcuts, **so that** those sections are equally fast to fill in.

### US-043: Manage Quick Add Advice / Follow-Up presets with auto-translate
**As a** Doctor, **I want to** search, add, edit, and delete my own advice/follow-up shortcuts, with the Bangla text automatically filled from a known-phrase dictionary when I type/edit the English text, **so that** I rarely have to type the Bangla version myself.
- Auto-fill only happens into an empty or not-manually-edited Bangla field.
- Overwriting a manually-entered Bangla translation always requires explicit confirmation first.
- The dictionary includes both the standard preset phrase list and the doctor's own previously-saved English→Bangla pairs.

### US-044: New doctor gets a starter Quick Add set
**As the** System, **I want to** seed a small starter set of Quick Add Medicine/Diagnosis/Investigation/Advice/Follow-Up entries for a doctor account the first time it's used, **so that** Quick Actions are useful immediately without any setup.
- Seeding never re-runs or overwrites a list the doctor has since customized (including a list they've deliberately emptied).

---

## Epic H — Prescription Template Engine & Hospital Branding

### US-045: Admin manages hospital identity
**As an** Admin, **I want to** set the hospital's name, logo, address, phone, emergency number, email, website, slogan (English + Bangla), license number, and seal image once, **so that** it's used automatically across every prescription template without per-template duplication.

### US-046: Admin creates a new prescription template
**As an** Admin, **I want to** create a new template by choosing a base type (Classic / Corporate / Government) and a language, **so that** I can offer doctors a hospital-branded layout.

### US-047: Admin customizes a template's header, footer, and style
**As an** Admin, **I want to** configure a template's header (color/height/logo size/name font/border), footer (color/height/QR message text in both languages/border), and general style (spacing/border radius/divider/accent color/font family incl. Bangla font/base font size/medicine-table style), **so that** it matches the hospital's visual identity.
- Government-type templates ignore color customization for header/footer/accent (always monochrome) — the editor communicates this rather than silently no-op-ing.

### US-048: Admin controls cosmetic visibility only
**As an** Admin, **I want to** toggle logo/slogan/footer/watermark visibility on a template, **so that** I can adjust branding elements without risking hiding clinically mandatory content.
- Patient info, doctor info, all clinical sections, Rx/medicines, advice, follow-up, QR, and signature can never be hidden via this control — they are always rendered.

### US-049: Admin edits every printed label per template
**As an** Admin, **I want to** edit any label/heading text on a template (auto-filled from the selected language's defaults) and reset all labels back to that language's defaults in one click, **so that** wording can be tailored without losing the ability to revert.
- Changing a template's language never silently overwrites labels the admin has already customized away from the previous language's defaults.

### US-050: Admin duplicates, enables/disables, and deletes templates
**As an** Admin, **I want to** duplicate an existing template as a starting point, toggle a template's availability to doctors, and delete a template, **so that** I can manage the offered layouts over time.
- Disabling or deleting a template that doctors currently prefer falls those doctors back to the classic default (with a warning shown before the destructive action).

### US-051: Preview a template with real branding before publishing
**As an** Admin, **I want to** see a live preview of the template using the real Hospital Settings branding (with placeholder clinical content), **so that** I know exactly how it will look before doctors use it.

### US-052: Patient info block never truncates
**As a** Doctor/Admin viewing any template, **I want to** see the Name/Age/Sex/Phone/Blood Group/Allergies/Date/Rx No. block lay out proportionally (long fields like Name/Allergies get more room) and wrap onto extra lines instead of clipping when a value is unusually long, **so that** no patient information is ever cut off, in either language, across Create/Preview/Finalized/Print.

---

## Epic I — Doctor Management (Admin)

### US-053: Admin adds/edits a doctor's professional profile
**As an** Admin, **I want to** create and edit a doctor's professional details (name, qualification, department, license/BMDC number, specialization, experience, gender, joining date, status, phone, email, photo), **so that** the roster stays accurate.
- Phone is validated as a Bangladesh mobile number.

### US-054: Admin browses/searches/filters doctors
**As an** Admin, **I want to** search doctors by name/specialization/license and filter by department and active/inactive status, with summary stat tiles (total/active doctors, total prescriptions, total medicine entries), **so that** I can manage a growing roster efficiently.

### US-055: Admin views a doctor's detailed performance
**As an** Admin, **I want to** drill into one doctor's stats — patients consulted, prescriptions created, avg. Rx/consultation, avg. medicines/Rx, an activity trend chart, busiest-hours chart, top medicines, recent prescriptions, and today's/this-month counters, **so that** I can evaluate and support that doctor's practice.

### US-056: Admin removes a doctor account
**As an** Admin, **I want to** delete a doctor account (with confirmation), **so that** departed staff no longer have access.
- Deleting a doctor must not silently orphan or corrupt their historical prescriptions/consultations — decide and document the real-build retention behavior (e.g. soft-delete/deactivate is strongly recommended over hard delete, given the audit implications in §15 of feature.md).

---

## Epic J — Staff Management

### US-057: Admin adds/edits staff and assigns doctors
**As an** Admin, **I want to** create and edit staff accounts and assign each to one or more doctors (many-to-many), **so that** front-desk coverage matches how doctors and staff actually work together.

### US-058: Admin browses/searches staff
**As an** Admin, **I want to** search the full staff list and see each member's assigned doctor(s) as chips, **so that** I can audit coverage at a glance.

### US-059: Doctor views their own assigned staff (read-only)
**As a** Doctor, **I want to** see which staff are assigned to me, **so that** I know who's registering my patients — without being able to add/edit/remove staff myself.

### US-060: Admin removes a staff account
**As an** Admin, **I want to** delete a staff account (with confirmation), **so that** departed staff lose access.

---

## Epic K — Doctor Profile & Personal Settings

### US-061: Doctor edits their own profile
**As a** Doctor, **I want to** edit my own name, qualification, department, license, phone (validated), and email, **so that** my information stays current on every prescription I issue.

### US-062: Doctor uploads a profile photo
**As a** Doctor, **I want to** upload (and remove) a profile photo, **so that** it's shown on my own settings page and the admin's Doctor Management page.

### US-063: Doctor uploads a signature with automatic background removal
**As a** Doctor, **I want to** upload a photo of my signature and have its background automatically removed, **so that** a clean transparent signature appears on every finalized prescription.
- Shows processing/success/error states; an invalid upload (wrong file type, unusable image) is rejected with a clear reason and never silently saved.
- A real (non-validation) processing failure keeps the original preview available with a one-click retry, without needing to re-upload.

### US-064: Doctor picks their preferred template
**As a** Doctor, **I want to** choose my preferred prescription template from the enabled list, **so that** my prescriptions look the way I want by default.
- Every new prescription snapshots the template in effect at creation time — changing the preference later only affects prescriptions created after the change.
- If my chosen template is later disabled/deleted, or I've never chosen one, I'm prompted on my dashboard to pick one before I can finalize anything.

### US-065: Doctor sees their remembered preferred language
**As a** Doctor, **I want to** see which prescription language (English/বাংলা) I last used, **so that** I know what a brand-new prescription will default to.

---

## Epic L — Bilingual Support (English / বাংলা)

### US-066: Every template ships translatable standard labels
**As an** Admin, **I want to** every standard prescription label to have an English and Bangla version, auto-filled by the template's chosen language and independently editable, **so that** hospital-specific wording can be maintained in both languages.

### US-067: Numbers render in Bangla digits when appropriate
**As a** Doctor/patient reading a Bangla-language prescription, **I want to** see numbers (like the date) shown in Bangla numerals, **so that** the document reads naturally in Bangla.

### US-068: Gender and other fixed vocabulary localize automatically
**As a** Doctor/patient reading a prescription, **I want to** gender and other standard fixed-choice fields to display in the active language automatically, **so that** nothing looks half-translated.

### US-069: Static bilingual dictionaries, not live translation
**As the** System, **I want to** resolve allergy names, dosage/frequency/duration/advice presets, Quick Add Advice/Follow-Up, and the QR verification message from parallel English/Bangla dictionaries at render time, **so that** bilingual output is instant, offline-capable, and predictable.
- Anything not present in a dictionary is never auto-translated — it's left for the doctor to enter manually in both languages. This is a deliberate, documented boundary for the real build (see feature.md §15.10), not a bug to "fix" by adding a translation API without a separate decision to do so.

### US-070: Bangla phonetic typing on free-text clinical fields
**As a** Doctor typing in Bangla, **I want to** type phonetically (Avro-style) in relevant free-text prescription fields and have it composed into Bangla script, **so that** I don't need a native Bangla keyboard.

### US-071: Patient info block stays correct in both languages
**As a** Doctor/Admin, **I want to** the Patient Information block to lay out correctly and never overlap or clip, regardless of whether English or Bangla labels/values (which are often different widths) are shown, **so that** there is no visual regression when switching languages.

---

## Epic M — Analytics & Reporting Dashboard

### US-072: Admin sees hospital-wide medicine/prescription analytics
**As an** Admin, **I want to** see top prescribed medicines, category breakdown, rarely-used medicines, co-prescribed medicine pairs, and a prescription volume trend, **so that** I understand prescribing patterns hospital-wide.

### US-073: Admin sees doctor performance analytics
**As an** Admin, **I want to** see a leaderboard and per-doctor detail of patients consulted, prescriptions created, medicines prescribed, avg. Rx/consultation, avg. meds/Rx, activity trend, and busiest consultation hours, **so that** I can identify strong performance and support gaps.

### US-074: Admin sees prescription volume trend with granularity toggle
**As an** Admin, **I want to** switch the prescription volume trend chart between Day/Week/Month granularity, **so that** I can zoom in on recent activity or out to long-term trends.

### US-075: Admin sees patient analytics
**As an** Admin, **I want to** see new vs. returning patient counts, new-registration trend, average visits per patient, top diagnoses, and chronic/repeat-diagnosis patterns, **so that** I understand the patient population.

### US-076: Admin sees an executive summary
**As an** Admin, **I want to** see one summary view with headline KPIs (total patients/prescriptions/medicines/doctors), month-over-month prescription and new-patient trends with % change, and top-5 medicines/diagnoses/active-doctors, **so that** I get the full picture in a single glance without opening every tab.

### US-077: Doctor sees their own scoped stats
**As a** Doctor, **I want to** see my own patient count, draft/finalized counts, assigned-staff count, patients consulted, a personal top-medicines card, and Today's Queue, **so that** my dashboard is immediately useful and never shows another doctor's data.

### US-078: Staff sees their own scoped stats
**As a** Staff member, **I want to** see how many patients I've registered, my own today's queue, and which doctor(s) I'm assigned to, **so that** my dashboard reflects only my own work.

### US-079: Analytics scale to a real database
**As the** System, **I want to** compute all analytics above via efficient backend queries/aggregations, **so that** dashboards stay fast as the number of patients/prescriptions grows well beyond what fits in a browser's memory (unlike the prototype, which recomputes everything client-side over the full dataset on every page load).

---

## Epic N — Backend Integrity & Platform (System stories)

These stories don't map to a single button in the UI — they're what make the rest of this document buildable as a real, correct, multi-user system.

### US-080: Real multi-user persistence
**As the** System, **I want to** store all entities (Users, Patients, Consultations, Prescriptions, Medicines, Templates, Hospital Settings, Quick Add lists) in a real, concurrently-accessible database, **so that** multiple staff/doctors can use the system at the same time without data loss or overwrite races.

### US-081: Atomic sequence generation
**As the** System, **I want to** generate prescription IDs, consultation IDs, and per-doctor daily token numbers atomically at the database layer, **so that** two simultaneous requests can never produce the same ID/token.

### US-082: Proper schema migrations
**As the** System, **I want to** manage all data-shape changes through a real migration tool, **so that** every environment's schema evolves predictably and auditable, replacing the prototype's ad-hoc client-side "backfill on load" migration pattern.

### US-083: File storage for images
**As the** System, **I want to** store uploaded images (hospital logo/seal, doctor avatars, signatures) in a real file/object storage layer and reference them by URL, **so that** database rows stay small and images can be served efficiently (CDN-able), instead of embedding base64 data URLs inline.

### US-084: Automated test coverage for business rules
**As the** System (and its maintainers), **I want to** automated tests covering the RBAC/visibility rules (Epics B, F, I, J), the finalize-validation rules (US-026), and the consultation/prescription status-consistency invariant (US-017), **so that** regressions in these rules are caught before release, not discovered by a user.

### US-085: Audit trail decision
**As an** Admin/compliance stakeholder, **I want to** a documented decision on whether patient registrations, prescription finalizations, and settings/template changes need a persistent audit log, **so that** compliance requirements (if any) are addressed deliberately rather than left as an accidental gap.

---

## Summary

| Epic | Stories |
|---|---|
| A — Authentication & Session Management | US-001 – US-004 |
| B — Patient Management | US-005 – US-009 |
| C — Consultation & Queue Management | US-010 – US-017 |
| D — Prescription Authoring | US-018 – US-028 |
| E — Prescription Lifecycle (Lists/View/Print/Verify) | US-029 – US-035 |
| F — Medicine Catalog & Prescribing Analytics | US-036 – US-040 |
| G — Quick Add Presets | US-041 – US-044 |
| H — Prescription Template Engine & Branding | US-045 – US-052 |
| I — Doctor Management (Admin) | US-053 – US-056 |
| J — Staff Management | US-057 – US-060 |
| K — Doctor Profile & Personal Settings | US-061 – US-065 |
| L — Bilingual Support | US-066 – US-071 |
| M — Analytics & Reporting Dashboard | US-072 – US-079 |
| N — Backend Integrity & Platform | US-080 – US-085 |

**85 user stories** across **14 epics**, covering every feature in [feature.md](feature.md).
