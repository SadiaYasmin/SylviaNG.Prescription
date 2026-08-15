# Analytics & Reporting Dashboard (Epic M, US-072–079)

Phase 6, the final epic. Aggregates data produced by every prior epic (Patient, Consultation, PrescriptionRecord, Medicine, Doctor, Staff) into read-only dashboards for Admin/Doctor/Staff. No new entities, no migration — everything here is computed on the fly from existing tables.

## What it is

- **Admin** (`/analytics`, new route, Admin-only): a single page with 5 tabs — Executive Summary, Medicine & Prescription, Doctor Performance, Prescription Trends, Patient Analytics. Matches the reference prototype's UX (one page, tab state, lazy-fetch per tab on first activation, cached for the session).
- **Doctor's own dashboard** (existing `/dashboard`): gained an own-scoped stats section (own patients, patients consulted, draft/finalized counts, assigned staff, personal top medicines) above the existing Today's Queue.
- **Staff's own dashboard**: gained a patients-registered-by-me tile and an assigned-doctors list above the existing My Queue.
- **Doctor Details drill-down** (`/doctors/doctor-details/:id`, existing Epic I page): the `DoctorPerformanceStats` payload this page already fetched was a real, stably-shaped **zero stub** since Epic I (Consultation/Prescription didn't exist yet) — this epic fills it in for real and adds a new `BusiestHours` field, plus renders the two fields (`activityTrend`, `busiestHours`) that existed on the DTO but were never drawn as charts.

## Why the backend design looks the way it does

Prescription line items (medicines, diagnoses) are free-typed JSON text columns with **no FK back to the `Medicine` catalog** (see `PrescriptionRecord`'s doc comment) — this was already true before this epic and is why `GetMedicineCatalogHandler`'s "Total Prescribed" count is an in-memory aggregate over deserialized JSON, not a SQL `GROUP BY` join. Every medicine/diagnosis aggregation in this epic follows that same established pattern: SQL-filter as tightly as possible (`Status == Finalized`, `+DoctorId` when scoped) before pulling rows into memory, then aggregate with plain `Dictionary`s. This is the realistic scalability bar for this schema (US-079's actual ask, relative to the prototype's "recompute everything client-side over the full dataset on every page load") — not a claim that every query here is a single indexed SQL aggregate.

New shared helpers, `Application/Features/Analytics/`:
- `AnalyticsDateBucketing` — server port of the reference prototype's `dateBuckets.js`. Day/Week/Month bucket keys as plain strings (`yyyy-MM-dd`, Monday-of-week `yyyy-MM-dd`, `yyyy-MM`), all three lexicographically sortable.
- `AnalyticsMath` — `PercentChange` (null = no baseline, not 0%) and `SafeDivide` (0 on divide-by-zero), both exact ports of the prototype's edge-case handling.
- `MedicinePrescribingAggregator` — the one place every medicine-identity aggregation (top prescribed, category breakdown, rarely-used, co-prescribed pairs) shares. Groups by **generic name, falling back to brand name**, via `MedicineDuplicateGuard.NormalizeKey(name, string.Empty)` — a deliberately different call-site than `GetMedicineCatalogHandler`'s brand+strength SKU key (same shared primitive, different key shape for a different purpose: two strengths of the same medicine must count as one medicine here).
- `AnalyticsDiagnosisAggregator` — shared `TopDiagnoses`/`ChronicPatterns`, case-insensitive dedup with first-seen-casing display labels. "Chronic" = same diagnosis text on **>1** prescriptions for one patient (2+, not 3+) — matches the prototype exactly.

New query slices (`Application/Features/Analytics/Queries/`), one per story: `GetMedicineAnalytics` (US-072), `GetDoctorLeaderboard` (US-073 leaderboard half), `GetPrescriptionVolumeTrend` (US-074, also reused by US-072's embedded Day-trend chart — one handler, not two), `GetPatientAnalytics` (US-075), `GetExecutiveSummary` (US-076), `GetMyDoctorAnalytics` (US-077), `GetMyStaffAnalytics` (US-078). All exposed on a new `Controllers/AnalyticsController.cs` (`prescription/analytics/*`), per-action `[Authorize]` since it mixes Admin/Doctor/Staff roles.

`GetDoctorDetailsHandler` (US-073's per-doctor detail half) was rewritten in place — same route/DTO shape as before, real logic instead of the zero stub. Reuses the same `MedicinePrescribingAggregator`/`AnalyticsDateBucketing`/`AnalyticsMath` building blocks as everything else in this epic, so there's exactly one implementation of "how do we compute avg Rx/consultation" etc., not two.

US-077's own-patient count reuses `PatientVisibilityScope.ApplyAsync` verbatim rather than re-deriving the Staff→Doctor visibility join — same reasoning every other feature in this codebase follows for that scope.

## Known, deliberate deviations from the prototype (flagged during planning, not discovered late)

- **"Busiest Consultation Hours" is bucketed in UTC**, not hospital-local time. `HospitalSettings` has no timezone field and everything in this codebase is stored UTC (`UtcDateTimeInterceptor`). The prototype used the browser's local hour; this is a real, documented behavioral difference, not a bug. Adding a timezone setting is out of scope for this epic (no new entities).
- **Busiest hours uses `Consultation.CheckInAt`, never `Audit.CreatedAt`** — `CreatedAt` is a confirmed dead column in this codebase (nothing populates it anywhere), a known fact carried forward from earlier epics, not rediscovered here.
- **`ExecutiveSummaryResponse.TotalMedicines`** = active catalog row count, not "distinct medicines ever prescribed" — consistent with the catalog-count convention already used elsewhere in the app. A one-line handler change either way if that reading turns out to be wrong.
- **Medicine/diagnosis grouping is case-insensitive with first-seen-casing display labels** for every widget in this epic (not just diagnoses, where the prototype states this explicitly) — a deliberate consistency choice on top of "follow the codebase's own established convention," not a byte-for-byte prototype port.

## Verified

Backend: `dotnet test` — 392 total (34 new for this epic), 389 pass; the 3 failures are the same pre-existing `SequenceGeneratorTests` needing a live Postgres connection, unrelated to this feature (and in fact passed when run against the live local stack during this session's manual verification — see below). Frontend: `ng test` — 325/325 pass (36 new), `ng build` clean.

**Full manual browser verification was possible this session** (Docker was reachable) — logged in as the seeded Admin account against the live local Postgres+Keycloak stack and walked through all 5 Admin analytics tabs (Executive Summary, Medicine & Prescription, Doctor Performance leaderboard, Prescription Trends with the Day/Week/Month toggle, Patient Analytics) plus the Doctor Details drill-down page, all rendering real non-zero numbers computed from the seeded dev data with no console errors. The Doctor/Staff own-dashboard scoped-stats sections (US-077/078) were verified only via the automated Karma test suite this session, not a live click-through as a non-admin user (no doctor/staff test credentials were readily available) — worth a quick live check next session if convenient, though the wiring is the same proven `AnalyticsService`/`ApiResponse` pattern already confirmed working live for the Admin tabs.
