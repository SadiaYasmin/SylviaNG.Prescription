# Local Dev Infrastructure — Postgres + Keycloak (Docker Compose)

## Purpose

Before this change, the backend could not start usefully for local development: `Database:ConnectionString` and every `Keycloak:*` setting in `appsettings.json` were blank placeholders, and `Infrastructure/Extensions/DependencyInjection.cs` throws immediately on an empty connection string. Epic A (real authentication, next feature) needs a real Postgres database and a real Keycloak identity provider to build and test against.

This change adds **zero business logic** — no domain entities, no EF migrations, no controllers. It is purely local-dev plumbing: containers + config + docs. The actual `User` entity, login flow, and RBAC wiring are built in the next branch, `feature/auth-jwt`.

## What's included

| Container | Image | Port | Purpose |
|---|---|---|---|
| `sylviang-prescription-postgres` | `postgres:16-alpine` | 5432 | Hosts two databases: `prescriptionms` (the app) and `keycloak` (Keycloak's own storage) |
| `sylviang-prescription-keycloak` | `quay.io/keycloak/keycloak:26.0` | 8080 | Identity provider, realm auto-imported on first start |

Keycloak realm `prescriptionms` (auto-imported from `docker/keycloak/realm-export.json`):
- Client `prescriptionms-backend` (confidential, dev-only fixed secret, direct-grant enabled for manual token testing)
- Realm roles: `Admin`, `Doctor`, `Staff`
- **No users are pre-seeded via realm import anymore.** The backend seeds exactly one Admin account on its own startup (username/password from `AdminSeed:Username`/`AdminSeed:Password` config — never hardcoded here). See `Doc/auth-jwt.md` for how and why.

## Prerequisites
- Docker Desktop running.
- Ports `5432` and `8080` free locally.

## One-time setup
```bash
cp .env.example .env
cp SylviaNG.Prescription/appsettings.Development.json.example SylviaNG.Prescription/appsettings.Development.json
```
Both `.env` and `appsettings.Development.json` are gitignored — customize them locally if you need different ports/credentials, but keep the `Database`/`Keycloak` values in `appsettings.Development.json` consistent with whatever's in `.env` and `docker/keycloak/realm-export.json` (realm name, client id/secret, db name).

## Bringing the stack up
```bash
docker compose up -d
docker compose ps   # both containers should show healthy/running
```

## Verifying it works
1. Confirm both databases exist:
   ```bash
   docker exec -it sylviang-prescription-postgres psql -U postgres -c "\l"
   ```
   Expect both `prescriptionms` and `keycloak` listed.
2. Open `http://localhost:8080`, log into the admin console with `KEYCLOAK_ADMIN_USER`/`KEYCLOAK_ADMIN_PASSWORD` from `.env` — confirm realm `prescriptionms`, client `prescriptionms-backend`, and roles `Admin`/`Doctor`/`Staff` exist. No app users exist yet at this point — the backend seeds its one Admin account the first time it starts (see `Doc/auth-jwt.md`).
3. After running the API once (step 4 below) so the Admin account gets seeded, request a real token via Keycloak's direct-grant endpoint:
   ```bash
   curl -X POST http://localhost:8080/realms/prescriptionms/protocol/openid-connect/token \
     -d grant_type=password \
     -d client_id=prescriptionms-backend \
     -d client_secret=dev-only-secret-change-me \
     -d username=admin \
     -d password=admin123
   ```
   (username/password are whatever `AdminSeed:Username`/`AdminSeed:Password` are set to locally.) Expect a JSON response containing a signed `access_token` (decode at jwt.io to see `iss`, `aud`, and the `Admin` role claim).
4. Run the API:
   ```bash
   cd SylviaNG.Prescription
   dotnet run
   ```
   Expect it to start cleanly (no more empty-connection-string exception) with Swagger at `http://localhost:5208/swagger`.
5. **Stretch check** (proves the Keycloak↔JWT wiring end-to-end): paste the `access_token` from step 3 into Swagger's Authorize button and call an authenticated endpoint. **Superseded**: at the time this doc was written, no real endpoints or migrations existed yet, so this step just proved authentication reached a (deliberately failing) Postgres query. Both now exist for real — just use the app's own `POST prescription/auth/login` (Epic A) instead of this Keycloak-direct-grant workaround for anything beyond debugging the raw JWT wiring.
   - This is *not* the app's own login flow (that's Epic A) — just borrowing Keycloak's direct-grant endpoint as a test-token source.
   - **Realm-export note**: the client needs an `oidc-audience-mapper` protocol mapper (included in `docker/keycloak/realm-export.json`) so issued tokens carry an `aud` claim matching `prescriptionms-backend` — without it, `AuthenticationExtensions.cs`'s `ValidAudience` check rejects every token with a 401 even though the token is otherwise valid. Discovered and fixed during verification of this branch.

## Resetting / tearing down
- `docker compose down` — stops containers, keeps data (Postgres volume + Keycloak's realm data survive).
- `docker compose down -v` — also removes the `prescriptionms_postgres_data` volume, wiping both databases; the next `docker compose up -d` re-runs the Postgres init script and Keycloak's realm import from scratch.

## Troubleshooting
- **Port already in use**: change the host-side port mapping in `docker-compose.yml` (or stop whatever's using 5432/8080 locally).
- **Keycloak realm missing after `docker compose up`**: check `docker compose logs keycloak` — the import only runs once per fresh Postgres volume; if you edited `realm-export.json` after the first run, you'll need `docker compose down -v` to force a re-import.
- **App throws "connection refused"**: confirm both containers report healthy (`docker compose ps`) before running `dotnet run`.

## Out of scope
No `User` entity, no EF migrations for app tables, no login controller/endpoint, no RBAC/authorization policy wiring. All of that is the next branch, `feature/auth-jwt`.
