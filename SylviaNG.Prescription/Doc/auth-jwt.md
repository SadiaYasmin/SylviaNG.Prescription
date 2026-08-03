# auth-jwt — Real Authentication (Epic A)

## What it is

Replaces the prototype's mock, client-trusted login (any username/password, role picked on the form) with real
server-issued authentication for Doctor/Admin/Staff, backed by **Keycloak** as the identity store and issued as
JWTs the frontend attaches to every API call.

- **Backend** (`SylviaNG.Prescription`): ROPC (Resource Owner Password Credentials) login against Keycloak,
  refresh token, logout (server-side revocation), admin-created accounts, password reset by an admin. Role is
  read from the token's realm-role claim — never selectable by the client. Every endpoint enforces role via
  `[Authorize(Roles = ...)]`, not just the frontend route guard.
- **Frontend** (`sylviang.adminui.prescription`): login page, `AuthService` (token storage), functional `authGuard`
  (`CanActivateFn`), `AuthInterceptor` (attaches `Authorization: Bearer`), `ErrorHandlerInterceptor` (401 → redirect
  to `/login`), header shows the logged-in username + role with a logout action.

## Why it was built

`feature.md` §2 and §15.3 require real credential storage, server-issued sessions, and server-side role
enforcement — the prototype's client-trusted role selector is explicitly called out as not acceptable once
there's a real API. Keycloak was chosen over hand-rolled identity so password storage, token issuance, and
role/claim management are handled by a proven identity provider rather than reimplemented.

## Non-obvious Keycloak gotchas (all resolved, worth knowing before touching this area again)

- **Escaped dots in the role-claim protocol mapper.** The realm-export's role-claim mapper name must escape dots
  (`http://schemas\\.microsoft\\.com/...`) — Keycloak treats unescaped dots in a claim name as JSON-nesting
  separators, which silently breaks `RoleClaimType = ClaimTypes.Role` matching on the backend.
- **Service account roles.** The backend's Keycloak service account needs both `manage-users` **and**
  `view-realm` (realm-management client roles) — `view-realm` is required to look up a realm role by name before
  it can be assigned to a newly created user.
- **"Account is not fully set up" on ROPC login.** Keycloak v26's declarative User Profile silently blocks
  direct-grant login with this error if `firstName`/`lastName`/**`email`** are missing on the user — it does not
  show up in the stored `requiredActions` list, so it's easy to misdiagnose as a credentials/config problem.
  `email` joined this list later than `firstName`/`lastName` (hit while building the admin-seeding routine below,
  where a `null` email silently produced the same failure) — check the realm's User Profile config
  (`GET admin/realms/{realm}/users/profile`) for the current `required` attribute list rather than assuming it's
  only the two originally documented fields. Fixed by always sending all three on account creation (falling back
  to `{username}@prescriptionms.local` / username / role if the caller didn't supply them) — see
  `AuthService.CreateUserAccountAsync` and `AdminAccountSeeder`.
- **Admin-created/reset passwords are not marked `temporary: true`.** Explicit decision: a temporary credential
  triggers Keycloak's interactive "update password" required action, which ROPC/direct-grant login can never
  satisfy (there's no browser flow to complete it). Known gap: a future self-service change-password feature
  would restore the "forced change on first login" property; until then, admin-set passwords are usable
  immediately as given.

## Bootstrapping the first account: config-driven admin seed, not realm-import test users

Earlier in this feature, `docker/keycloak/realm-export.json` seeded three fixed test accounts (`admin.dev`,
`doctor.dev`, `staff.dev`, all sharing one plaintext password) directly in that checked-in file. That's fine for
throwaway test data but wrong for anything meant to be a real, usable credential — the realm-export file is
tracked in git, so any password written into it is committed to source control in plaintext.

**Current approach**: `realm-export.json` seeds no users at all beyond the backend's own service account (needed
for the Keycloak Admin API calls below). Instead, `AdminAccountSeeder`
(`Application/Services/AdminAccountSeeder.cs`) runs once on every app startup (wired into `Program.cs` right after
`app.Build()`) and:
1. Reads `AdminSeed:Username` / `AdminSeed:Password` from configuration. Both are blank in the tracked
   `appsettings.json`; the real values live only in the gitignored `appsettings.Development.json` (and its
   `.example` template, since these are dev-only defaults, not a genuine secret) — the same pattern already used
   for the DB connection string and Keycloak client secret. If either is blank, seeding is skipped (safe default
   for environments that don't want auto-provisioned credentials).
2. Skips silently if a local `User` with that username already exists (`IUserRepository.ExistsByUsernameAsync`) —
   safe to run on every restart, not just the first.
3. Otherwise calls the same `IKeycloakAdminClient.CreateUserAsync` used by the real "admin creates an account"
   flow (US-004), with the `Admin` realm role, then creates the matching local `User` row.

Local dev default (`appsettings.Development.json.example`): `admin` / `admin123`. Change it there (or via
environment variable override) for any environment where that default isn't acceptable.

**Gotcha hit here**: the seeder's first version passed `email: null` (no dev-facing reason to have one for a
bootstrap admin) — this reproduced the "Account is not fully set up" ROPC failure above, because this realm's
User Profile requires `email` too. Fixed by synthesizing `{username}@prescriptionms.local` when no real email is
available, both here and in `AuthService.CreateUserAccountAsync` (the general-purpose admin-account-creation path
had the same latent gap — it forwarded `request.Email` with no fallback, unlike `FirstName`/`LastName` which
already had one).

**Async-disposal gotcha**: the first cut wrapped the seeding call in `using (app.Services.CreateScope())`, which
throws `InvalidOperationException: 'UnitOfWork' type only implements IAsyncDisposable` on scope teardown, because
`UnitOfWork` (`SharedKernel/Generic/UnitOfWork.cs`) only implements `IAsyncDisposable`, not the synchronous
`IDisposable` that `IServiceScope.Dispose()` needs. Fixed by using `await using (app.Services.CreateAsyncScope())`
instead — any other one-off startup code that resolves scoped services needs the same async-scope treatment.

## Frontend routing bug found and fixed during this feature: duplicate unguarded route

`authGuard` was correctly wired in `app.routes.ts` (`{ path: '', canActivate: [authGuard], loadChildren: ... }`)
from the start, and the compiled dev bundle always reflected that correctly — but unauthenticated users still
landed straight on `/dashboard` with the guard never executing. The cause was **not** a caching, browser, or
tooling issue (a long list of those was ruled out first): `ShellModule` (`shell.module.ts`) eagerly imported
`PagesModule` directly in its `imports` array, purely as leftover placeholder-scaffolding wiring — nothing in
`ShellComponent`/`HeaderComponent`/`SidebarComponent` actually used anything from it.

Because `ShellModule` is itself imported eagerly by `AppModule`, that eager import pulled in
`PagesRoutingModule`'s `RouterModule.forChild([Shell.childRoutes([...])])` at the **root** injector, registering
a *second* `path: ''` route (no `canActivate`) directly on the root `Router.config` — separate from, and ordered
**before**, the properly guarded lazy `path: ''` route from `app.routes.ts`. Angular's router activates the first
matching route in array order, so every navigation matched the unguarded duplicate and the real, guarded route
was never reached. This was invisible from reading `app.routes.ts` alone (which was correct); it only showed up
by inspecting the live `Router.config` array at runtime (4 entries instead of the expected 3, two of them
`path: ''`, only one with a guard) via `window.ng.getInjector(appRootEl).get(Router)` in the browser console.

**Fix:** removed the eager `PagesModule` import from `shell.module.ts`. `PagesModule` is now loaded exactly once,
lazily, through `app.routes.ts`'s `loadChildren`, and the guard runs correctly on every direct navigation.

**Lesson for future features:** if a route guard (or resolver, or any `canActivate`/`canMatch`) appears to be
"wired correctly" in source but never fires, check for **duplicate route registration** caused by a module being
imported both eagerly (directly in another eagerly-loaded module's `imports`) and lazily (via `loadChildren`)
before assuming a build/cache/browser issue — the duplicate wins silently because it has no guard to fail.
