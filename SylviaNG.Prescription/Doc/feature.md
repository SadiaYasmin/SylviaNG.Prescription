# PrescriptionMS — Feature Specification

**Purpose of this document.** This is the master feature list for building the **real, production version** of PrescriptionMS — a full backend + frontend system — based on everything already validated in the frontend-only prototype at `D:\millenium\prescription-prototype`. Every feature below is either already working in the prototype (frontend-only, `localStorage`-backed, mock auth) or is an explicit "harden for production" requirement needed to turn the prototype into a real multi-user system. Each feature notes **Status** (`Prototype` = already built and demoed in the frontend; `New for real build` = does not exist yet and must be designed/built).

This document is the input to [user-stories.md](user_stories.md), which breaks every feature here into implementable stories with acceptance criteria.

---

## 1. Roles & Access Model

| Role | Summary |
|---|---|
| **Admin** | Hospital administrator. Manages doctors, staff, prescription templates, hospital branding, and views hospital-wide analytics. Does not treat patients or write prescriptions. |
| **Doctor** | Registers/edits patients (within their care team), runs consultations, authors and finalizes prescriptions, configures personal Quick Add presets, profile, signature, and preferred template/language. |
| **Staff** | Front-desk/assistant role. Registers patients and starts consultations (token/queue) on behalf of one or more assigned doctors. Read-only visibility into medicines/patients they registered. |
| **Public / Guest** | No login. Can scan a prescription's QR code to view a read-only verification page for that single prescription. |

**Status:** `Prototype` (3 internal roles) + `New for real build` (a public "Patient" self-service role was mentioned in early planning docs but was never built and is **out of scope** here — see §7).

Key access rules already established and must be preserved:
- A **staff** member only sees patients *they personally registered*.
- A **doctor** only sees patients registered by *staff members assigned to them* (`assignedDoctorIds`), i.e. each doctor + their staff form one "care team."
- **Admin** sees everything (all patients, all doctors, all staff, all prescriptions) but never authors clinical content.
- Every list/table a role sees is scoped by these rules — this must be enforced **server-side** in the real build (the prototype enforces it only in the frontend, which is not acceptable once there's a real API).

---

## 2. Authentication & Session Management

**Status:** `Prototype` (mock) → **must be rebuilt for real** (`New for real build`).

- Login form: username, password, role selector.
- **Prototype behavior (not to carry over):** any username/password is accepted; role is whatever the user picks on the login form; session is just a JSON blob in `localStorage` with no expiry, no server validation.
- **Real build requirements:**
  - Real credential storage (hashed passwords, e.g. bcrypt/argon2), issued by an admin (accounts are not self-registered — an admin creates doctor/staff accounts, per Doctor/Staff Management below).
  - Session via server-issued token (JWT or server session) with expiry and refresh, not a client-trusted role.
  - Role is derived from the authenticated user's server-side record — never selectable by the client.
  - Logout invalidates the session.
  - Route/page access enforced both client-side (UX) and server-side (API authorization) — every API endpoint must independently check the caller's role, never trust the frontend's route guard alone.
  - Password reset / "forgot password" flow (does not exist in the prototype at all — new for real build).

---

## 3. Patient Management

**Status:** `Prototype`, fully working.

- Register a new patient: name (required), date of birth *or* age (DOB takes precedence and age is derived from it), gender, phone (required, validated as a Bangladesh mobile number format), address (with autocomplete suggestions from a fixed area list), blood group, allergy.
- Allergy is selected from a fixed master preset list (None, Penicillin, Dust, Seafood, Latex, ...) each with English + Bangla display text, or a free-text "Other" entry (never translated).
- Edit an existing patient's full record.
- Patient list view, scoped by role (see §1 access rules): Admin sees "Registered By" and full patient list; Doctor/Staff see clinical fields (age/gender/blood/allergy) inline; a patient ID is shown in a formatted display form (e.g. `PT-000123`) everywhere.
- Search patients by name or phone.
- A patient carries a running **Saved History** (verbatim free-text list of past diagnoses/conditions) that automatically preloads into a new prescription's "History of Present Illness" section (see §4) and is overwritten with whatever the doctor leaves in that field when a prescription is finalized — a simple "carry forward what was last written" model, not a coded diagnosis catalog.
- A patient also has a `medicalHistory` array (freeform list) as a distinct, doctor-maintained "known conditions" field, separate from the auto-carried saved history.

---

## 4. Consultation & Queue Management

**Status:** `Prototype`, fully working.

- Staff registers a **consultation** for a patient: pick patient, pick one of the doctors the staff member is assigned to. This generates:
  - A unique daily **token number** per doctor (`T-01`, `T-02`, ... resets each day per doctor).
  - A unique consultation ID (`CN-YYYY-####`).
- Consultation status lifecycle: `waiting` → `in_consultation` → `completed`, or `cancelled`; a `draft` status represents a consultation whose prescription was saved as a draft (removed from the live queue, resumable later).
- **Duplicate-consultation guard:** starting a new consultation for a patient who already has an active (waiting/in-progress) consultation with the same doctor today prompts the user to open the existing one instead of creating a duplicate.
- **Draft-in-progress guard:** starting a *new* consultation for a patient who has an unfinished draft prescription (from any prior day) with this doctor prompts the user to resume the existing draft, view all their drafts, or explicitly start fresh anyway.
- Doctor's **Today's Queue** (dashboard widget): shows waiting/in-progress patients for today, ordered by check-in time/token, with quick "open consultation" action.
- Staff's own dashboard queue widget (their registered patients' consultation status today).
- Admin's **Consultations** monitoring page: hospital-wide, filterable by date (today/yesterday/custom date/date range), doctor, status, and free-text search (patient/token/phone); shows summary stat cards (Total/Waiting/In Progress/Completed) and a paginated table with a consultation-detail modal (registered-by, timestamps, linked prescription status).
- A consultation and its linked prescription's status must always stay in sync (`finalized` prescription ⇒ `completed` consultation, and vice versa) — a self-healing/consistency rule that must be enforced by the backend (in the prototype this is patched client-side at load time; in the real build this must be a transactional invariant, not a client-side repair pass).

---

## 5. Prescription Authoring (Clinical Workflow)

**Status:** `Prototype`, fully working — this is the core of the product.

A prescription is authored **live, inline, directly inside the doctor's selected visual template** (not a separate plain form that gets rendered afterward) — what the doctor edits is pixel-identical to what gets finalized/printed.

Entry points into authoring:
1. Open an active consultation from Today's Queue (auto-transitions it to `in_consultation`).
2. "Quick create" — pick any patient in the doctor's authorized scope directly from the Create Prescription page (with the same duplicate/draft guards as §4) — for walk-ins not pre-registered as a queued consultation.
3. Resume a saved draft (from the Draft Prescriptions list or a URL/deeplink).

Clinical sections captured (all editable inline, richly structured, not just free text):
- **Chief Complaint(s)** — list of complaint entries.
- **History** (History of Present Illness) — list, auto-preloaded from the patient's Saved History on a brand-new prescription only (never overwrites a resumed draft's own content).
- **Examination / Vitals** — structured fields: BP, Pulse, Temperature, Respiratory Rate, SpO2, Weight, Height, Blood Sugar, Pain Score, Heart Rate.
- **Diagnosis(es)** — list of diagnosis entries.
- **Investigation(s)** — list of investigation/test entries.
- **Medicines (Rx)** — structured line items: medicine name (autocomplete against the Medicine Catalog), strength, dosage, frequency, duration, instructions. Includes:
  - **Duplicate-medicine guard**: adding/editing a line that matches an existing line's medicine+strength is blocked outright (not just warned) and the existing row is highlighted so the doctor edits it instead of creating a second entry for the same drug.
  - Dosage/Frequency/Duration/Instructions each have an autocomplete/datalist of standard bilingual presets, and also support Bangla phonetic ("Avro"-style) typing when the prescription language is Bangla.
- **Advice** — list of advice entries.
- **Follow-Up** — free text (e.g. "See again in 7 days").
- Every list-type section supports **Quick Add** shortcuts (see §7) that insert a doctor's pre-configured, frequently used entries with one click, alongside manual entry.
- **Bilingual authoring:** a per-prescription English/বাংলা language toggle in the authoring header. Switching languages re-renders all standard labels and preset dropdown options in that language; it's seeded from the doctor's last-used language and remembered for next time; switching it also updates the medicine field values already on the form to the matching preset text in the new language where those values came from a preset (not free-typed text).

### Finalization rules
- **Validation before finalize:** at least one diagnosis, at least one medicine, the doctor must have an uploaded signature on file, and the doctor must have a preferred template selected — finalize is blocked with a clear checklist of what's missing until all are satisfied.
- **Save as Draft:** persists the in-progress prescription (status `draft`), stamps a "last saved" timestamp, and takes the consultation out of the active queue (resumable later from Draft Prescriptions).
- **Finalize:** stamps a `finalizedAt` timestamp, marks the prescription `finalized` (permanently read-only from then on), marks the linked consultation `completed`, and overwrites the patient's Saved History with whatever is currently in the History field (verbatim carry-forward for next visit).
- A finalized prescription is never editable again through the authoring UI — it's shown read-only.

---

## 6. Prescription Lifecycle: Lists, Viewing, Printing, Verification

**Status:** `Prototype`, fully working.

- **Draft Prescriptions** list (doctor-scoped): all of a doctor's in-progress drafts, resumable.
- **My Finalized Prescriptions** list (doctor-scoped): all of a doctor's completed prescriptions, with a "just finalized" highlight when arriving right after finalizing one.
- **Prescription View** page (`/prescriptions/view/:id`, doctor/staff/admin): read-only rendering of any single prescription in its original template, for review/audit — not just the authoring doctor.
- **Patient's prescription history** panel shown alongside the live authoring view, so a doctor can see a patient's past prescriptions while writing a new one.
- **PDF export**: client-side generation of a print-accurate PDF of the finalized (or in-progress) prescription document.
- **Real A4 print pagination**: the document is paginated to true A4 page boundaries (not just "print whatever fits"), with a continuation header (hospital/doctor/patient name + "Page X of Y") on every page after the first, and content (e.g. a long medicine list) correctly split/continued across pages without being cut mid-line.
- **QR code**: every finalized prescription carries a QR code encoding a link to the public verification page for that specific prescription ID.
- **Public Verification page** (`/verify?id=...`, no login required): scanning the QR (or visiting the link directly) shows a read-only rendering of that one prescription, so a pharmacy/patient/third party can confirm authenticity without any account.

---

## 7. Medicine Catalog & Prescribing Analytics

**Status:** `Prototype` — catalog is `Read-only in prototype` (no add/edit/delete UI yet — new for real build); analytics is fully working, including a role-based scoping fix delivered in this session.

- Medicine master catalog: brand name, generic name, strength, manufacturer, dosage form, category.
- Search medicines by brand or generic name (used both on the standalone Medicines page and inside prescription authoring's medicine autocomplete).
- **`New for real build`: Medicine Catalog CRUD** — the prototype has no way to add, edit, or deactivate a medicine; the real build needs an admin-facing management UI + API for this (create/edit/soft-delete a medicine, prevent duplicate brand+strength entries, etc.).
- **Role-based "Total Prescribed" analytics column** on the Medicines page:
  - **Admin** sees the total number of *finalized* prescriptions containing each medicine, across **all doctors**.
  - **Doctor** sees the same count scoped to **only their own** finalized prescriptions.
  - **Staff** does **not** see this column or any prescription-count analytics at all — the Medicines page for staff is the plain read-only catalog only.
  - The medicine list is sorted by this "Total Prescribed" count, descending, for whichever role sees it (unsorted/catalog-order for staff, since they have no count to sort by).

---

## 8. Quick Add Presets (Doctor Productivity)

**Status:** `Prototype`, fully working.

Each doctor maintains their own personal, editable preset lists (search, add, edit, delete — a shared generic manager component/UX pattern across all five):
1. **Quick Add Medicine** — medicine + strength + default dosage/frequency/duration/instructions (referencing the shared bilingual preset dictionaries by id, not literal text, so the same saved preset renders correctly in whichever language a given prescription uses).
2. **Quick Add Diagnosis**
3. **Quick Add Investigation**
4. **Quick Add Advice** — includes **auto-translate**: typing/editing the English text automatically fills the Bangla text from a known-phrase dictionary (built from the standard preset dictionary plus the doctor's own previously-entered pairs) — only when the Bangla field is empty or hasn't been manually edited, and always asks for confirmation before overwriting a manually-entered Bangla translation.
5. **Quick Add Follow-Up** — same auto-translate behavior as Advice.
- New doctors are seeded with a small starter set for all five lists on first use, so Quick Actions are useful immediately without setup.
- Inside prescription authoring, every relevant section offers a "Quick Add [X]" dropdown that inserts the selected preset with one click (medicine presets go through the same duplicate-guard as manual entry).

---

## 9. Prescription Template Engine & Hospital Branding

**Status:** `Prototype`, fully working — this is a significant differentiator vs. a typical simple prescription app.

- Three built-in template *types* admins can create instances of:
  1. **Classic** — doctor info left / hospital info right header, two-column clinical layout, traditional typography.
  2. **Corporate** — full-width hospital branding banner, single-column, rounded modern sections.
  3. **Government** — black & white, minimal, compact, maximum writing space.
- Admin **Template Management** page: list, create, duplicate, enable/disable (a disabled template's doctors silently fall back to the classic default), delete (with a warning that assigned doctors fall back), each with a live preview card using real hospital branding.
- **Template Editor**, tabbed configuration per template:
  - **Header** — background color, height, logo size, hospital-name font, border style.
  - **Footer** — background color, height, QR verification message text (English + Bangla), border style. (Footer tagline itself is always pulled live from Hospital Settings' slogan, not stored per-template.)
  - **Style** — section spacing, border radius, divider style, accent color, font family (incl. a dedicated Bangla font), base font size, medicine-table style (plain/striped/bordered), enabled toggle.
  - **Visibility** — toggle cosmetic elements only (logo, slogan, footer, watermark text) — all clinically mandatory sections (patient info, doctor info, chief complaint, history, examination, diagnosis, investigation, Rx/medicines, advice, follow-up, QR, signature) are always shown and cannot be hidden.
  - **Labels** — every printed label/heading in the template is editable text, auto-filled from a language-appropriate default set (English or Bangla) and independently resettable to that language's defaults.
  - **Language** — sets which default label set (English/Bangla) a template starts from; changing it doesn't blindly overwrite labels the admin has already customized away from the previous language's defaults.
  - **Print settings** — page size (A4), orientation, margin.
  - Live preview panel using real Hospital Settings branding with placeholder clinical content.
- **Patient Information block** (Name/Age/Sex/Phone/Blood Group/Allergies/Date/Rx No.) has a carefully engineered proportional layout (not equal-width columns) so long values (long names, multi-item allergy lists) wrap onto additional lines instead of being clipped/truncated with ellipsis, consistently across Create/Preview/Finalized/Print, in both English and Bangla.
- **Hospital Settings** (admin, single record): hospital name, logo, address, phone, emergency number, email, website, slogan (English + Bangla), license number, seal image — used automatically across every template with no per-template duplication.

---

## 10. Doctor Management (Admin)

**Status:** `Prototype`, fully working.

- Admin CRUD for doctor accounts: name, qualification, department, BMDC registration/license number, specialization, years of experience, gender, joining date, status (active/inactive), phone (validated), email, profile photo.
- Doctor list: searchable (name/specialization/license), filterable by department and status, with summary stat tiles (total doctors, active doctors, total prescriptions, total medicine entries).
- **Doctor Details** page (admin, per-doctor drill-down): performance stats (patients consulted, prescriptions created, avg. Rx/consultation, avg. medicines/Rx), activity trend chart, busiest-hours chart, top medicines prescribed, recent prescriptions, today's/this-month prescription counters.
- Delete a doctor account (with confirmation).

---

## 11. Staff Management

**Status:** `Prototype`, fully working.

- Admin CRUD for staff accounts: name, username, email, phone, department, and a **many-to-many assignment to one or more doctors** (a staff member can support multiple doctors; a doctor can have multiple staff).
- Admin's Staff page: full list, searchable, showing each staff member's assigned doctor(s) as chips.
- A **doctor's own Staff page** is read-only: shows only staff assigned to them, labeled "Assigned to you" — no add/edit/delete controls.

---

## 12. Doctor Profile & Personal Settings

**Status:** `Prototype`, fully working.

- Edit own professional info (name, qualification, department, license) and contact info (phone — validated, email).
- Upload a profile photo.
- **Digital signature capture**: upload any photo of a signature; background is automatically removed client-side (AI-based) to produce a transparent-background PNG used on every finalized prescription. Includes processing/error/retry states and rejects invalid uploads with a clear reason.
- Pick a **preferred prescription template** from the enabled template list (via a picker modal) — every new prescription snapshots this template at creation time, so changing the preference later never alters prescriptions already in progress or finalized.
- Preferred prescription **language** (English/বাংলা) is remembered from whatever was last chosen while authoring (see §5) and shown as a read-only indicator here.
- A dashboard banner nudges a doctor who has never chosen a template (or whose chosen template was later disabled/deleted) to pick one, before finalize is even attempted.

---

## 13. Bilingual Support (English / বাংলা)

**Status:** `Prototype`, fully working, delivered most recently — treat as a first-class cross-cutting capability, not an afterthought, in the real build.

- Every standard prescription label (Name/Age/Sex/..., section headings, etc.) has an editable English and Bangla version per template (§9).
- Per-prescription language toggle while authoring (§5), remembered per doctor.
- Bangla digit formatting (converts Arabic numerals to Bangla numerals, e.g. dates) wherever numbers are shown in a Bangla-language prescription.
- Gender labels localized (Male/Female/Other ⇄ পুরুষ/মহিলা/...).
- Allergy master list, dosage/frequency/duration/advice presets, Quick Add Advice/Follow-Up, and QR verification message all carry parallel English + Bangla text, resolved at render time by the active language — never a live machine-translation call (**this is a deliberate scope boundary**: the system only ever shows a translation it already "knows" from its static dictionaries; anything the dictionary doesn't recognize is left for the doctor to type manually in both languages). A real build should preserve this boundary explicitly (documented, not silently attempted) rather than bolting on an actual translation API unless specifically requested later.
- **Bangla phonetic ("Avro"-style) text input** helper available on relevant free-text prescription fields when the active language is Bangla, so a doctor can type phonetically and have it composed into Bangla script.
- The Patient Information block's layout (§9) must remain visually correct and non-overlapping in **both** languages, since Bangla labels/values often render at different widths than their English equivalents.

---

## 14. Analytics & Reporting Dashboard

**Status:** `Prototype`, fully working.

- **Admin dashboard** = a tabbed analytics suite (role-gated to admin) covering:
  - **Medicine/Prescription Analytics** — top prescribed medicines, category breakdown, rarely-used medicines, co-prescribed medicine pairs, prescription volume trend.
  - **Doctor Performance** — per-doctor patients consulted, prescriptions created, medicines prescribed, avg. Rx/consultation, avg. meds/Rx, activity trend, busiest consultation hours; a hospital-wide doctor leaderboard.
  - **Prescription Volume & Trends** — with Day/Week/Month granularity toggles.
  - **Patient Analytics** — new vs. returning patients, new-registration trend, average visits per patient, top diagnoses, chronic/repeat-diagnosis patterns.
  - **Executive Summary** — headline KPIs (total patients/prescriptions/medicines/doctors), month-over-month prescription and new-patient trends with % change, top 5 medicines, top 5 diagnoses, top 5 most-active doctors.
- **Doctor dashboard**: personal stat cards (my patients, draft/finalized counts, my staff, patients consulted), a personal top-medicines card, and Today's Queue — all scoped to that doctor only (never another doctor's data).
- **Staff dashboard**: their own patient-registration count, their own today's queue view, and a card showing which doctor(s) they're assigned to.
- All of the above is pure read/aggregation over existing entities — no separate "reporting" data model, but in the real build these aggregations should move to efficient backend queries (the prototype recomputes them client-side over the full in-memory dataset on every load, which will not scale against a real multi-tenant database).

---

## 15. Cross-Cutting / Non-Functional Requirements for the Real Build

These are not "features" a user sees directly, but are required to turn this prototype into a real product and must be planned alongside the feature work above:

1. **Real persistence & multi-user backend** — replace `localStorage` + in-memory arrays with a real database (relational recommended, given the clearly relational entity model: Users, Patients, Consultations, Prescriptions, Medicines, Templates, HospitalSettings, plus each doctor's Quick Add lists) accessed through a real API. Concurrent multi-user access (multiple staff/doctors using the system at the same time) must work correctly — this is *not* true of the prototype today.
2. **Atomic ID/token generation** — the prototype derives prescription IDs (`RX-2026-####`), consultation IDs (`CN-2026-####`), and daily per-doctor token numbers (`T-01`, `T-02`, ...) by counting existing records client-side. In a real multi-client system this must become a server-side atomic sequence/counter (e.g. a DB sequence or transactional increment) to avoid collisions when two staff members register patients for the same doctor at the same moment.
3. **Real authentication & authorization** — see §2. Every role-based visibility rule described throughout this document (staff-sees-own-patients, doctor-sees-assigned-staff's-patients, admin-sees-all, staff-never-sees-prescription-analytics, etc.) must be enforced by the API, not only hidden in the UI.
4. **File/image storage** — logos, seals, avatars, and signature images are currently stored as base64 data URLs inline in JSON. The real build should use a proper file storage layer (e.g. object storage / CDN) and store references, not inline blobs.
5. **Background removal for signatures** — currently done client-side in-browser (`@imgly/background-removal`). Decide whether to keep this client-side (works offline, no server cost) or move it server-side (more consistent, but adds infra) — either is viable; document the choice.
6. **PDF generation** — currently 100% client-side (`html2pdf.js`). Consider whether official/legal documents warrant server-side generation for consistency and to enable server-side audit logging of every PDF ever produced.
7. **Data migrations** — the prototype has an elaborate ad-hoc client-side migration system for evolving the shape of seeded/legacy data (allergy ids, quick-add field shapes, frequency/advice preset re-splits, etc.). A real backend needs a proper schema-migration tool (e.g. Prisma/TypeORM/Knex migrations, or Alembic/Django migrations) instead — this ad-hoc pattern should not be carried over as-is.
8. **Audit trail** — who registered which patient, who finalized which prescription, when a template/hospital-setting was changed, etc. is implicit today (a plain `registeredBy` foreign key) but there is no history/audit log. Consider whether the real build needs one (e.g. for compliance).
9. **Automated testing** — the prototype has zero automated tests (only a manual QA checklist, `docs/QA.md`). The real build must have a real test strategy (unit tests for backend business rules — especially the RBAC/visibility rules and the finalize-validation rules — plus end-to-end tests for the core clinical workflow).
10. **i18n architecture** — keep the existing "static bilingual dictionary, no live translation" design intent (see §13) as an explicit, documented product decision for the real build, not something to silently "upgrade" to real machine translation without a separate decision to do so.

---

## 16. Explicitly Out of Scope (Not in the Prototype, Not Assumed for the Real Build)

Carried forward from the prototype's known gaps — call these out explicitly during real-build planning so nobody assumes they're already covered:

- **Patient self-service portal/login** — original planning docs mention a "patient" role, but it was never designed or built; there is no patient-facing account, booking, or record-access feature today. Treat as a separate future initiative, not part of this feature set, unless the stakeholders decide otherwise.
- **Inventory / pharmacy stock management.**
- **Billing / payments / insurance.**
- **Drug interaction checking or dosage-safety validation** — the medicine catalog is just a static reference list today; no clinical safety rules are implemented.
- **Appointment scheduling / booking in advance** — today's model is a same-day walk-in queue/token system only, not a future-dated appointment calendar.
- **Notifications** (SMS/email/push) — nothing in the prototype sends any notification to anyone.
- **Multi-branch / multi-hospital-tenant support** — Hospital Settings is a single global record; there's no concept of multiple hospitals/branches in one deployment today.

---

## Cross-reference

See [user-stories.md](user_stories.md) for every feature above broken into user stories with concrete acceptance criteria, organized into the same epics.
