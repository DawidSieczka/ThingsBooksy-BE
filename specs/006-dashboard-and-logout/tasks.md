# Tasks: Dashboard + Logout + Create-group modal

**Source**: `specs/006-dashboard-and-logout/spec.md` + `specs/006-dashboard-and-logout/plan.md`
**Branch**: `006-dashboard-and-logout`

## Format: `[ID] [P?] [US?] Description`

- **[P]** — parallelizable with other [P] tasks in the same phase (different files, no dependency)
- **[US1–US6]** — related user story from spec.md
- Each phase ends with a **Checkpoint** that can be validated independently

---

## Phase 1 (Wave 1, BE): JWT pipeline + TokenRevocation

**Goal**: backend has a working `/users/logout` endpoint that revokes tokens, plus middleware that rejects revoked tokens.

### JWT manager update

- [ ] **T001** [Shared] Update `JsonWebTokenManager.CreateToken(...)` in `backend/src/Shared/ThingsBooksy.Shared.Infrastructure/Auth/JWT/` to emit a unique `jti` claim per token (use `Guid.CreateVersion7().ToString()`). Token payload must include `jti` and `exp`.

### Domain + persistence

- [ ] **T002** [P] [Users] [US3] Create `backend/src/Modules/Users/ThingsBooksy.Modules.Users.Core/Entities/TokenRevocation.cs` with `Id`, `Jti`, `UserId`, `RevokedAt`, `ExpiresAt` (all `private set`), `private TokenRevocation()` ctor, `public static TokenRevocation Create(string jti, Guid userId, DateTime expiresAt, DateTime now)`.
- [ ] **T003** [P] [Users] [US3] Create `backend/src/Modules/Users/ThingsBooksy.Modules.Users.Core/DAL/Configurations/TokenRevocationConfiguration.cs` — `ToTable("token_revocations")`, PK `Id`, unique index on `Jti`, composite index `(UserId, RevokedAt)`.
- [ ] **T004** [Users] [US3] Edit `backend/src/Modules/Users/ThingsBooksy.Modules.Users.Core/DAL/UsersDbContext.cs` — add `DbSet<TokenRevocation> TokenRevocations`.

### Logout feature

- [ ] **T005** [P] [Users] [US3] Create `backend/src/Modules/Users/ThingsBooksy.Modules.Users.Core/Features/Logout/LogoutCommand.cs` — record `LogoutCommand(string Jti, Guid UserId, DateTime ExpiresAt) : ICommand`.
- [ ] **T006** [Users] [US3] Create `backend/src/Modules/Users/ThingsBooksy.Modules.Users.Core/Features/Logout/DataProviders/ILogoutCommandDataProvider.cs` + concrete `LogoutCommandDataProvider.cs` — method `Task RevokeAsync(string jti, Guid userId, DateTime expiresAt, CancellationToken ct)` creates `TokenRevocation` via factory, calls `_dbContext.TokenRevocations.AddAsync(...)`, `SaveChangesAsync(ct)`. Register in `AddDataProviders(...)` extension.
- [ ] **T007** [Users] [US3] Create `backend/src/Modules/Users/ThingsBooksy.Modules.Users.Core/Features/Logout/LogoutHandler.cs` — `ICommandHandler<LogoutCommand>`. Injects `ILogoutCommandDataProvider` + `IClock`/`DateTime.UtcNow`. Calls `RevokeAsync(...)`.
- [ ] **T008** [Users] [US3] Register `LogoutHandler` in `AddUsersCore(...)` (or wherever handlers are scanned/registered).

### Revoked-token checker

- [ ] **T009** [P] [Shared] [US3] Create `backend/src/Shared/ThingsBooksy.Shared.Abstractions/Auth/IRevokedTokenChecker.cs` — interface with `Task<bool> IsRevokedAsync(string jti, CancellationToken ct)`. **Lives in Shared.Abstractions** so the JWT pipeline in Shared.Infrastructure can depend on it without referencing Users.Core (preserves modular boundaries).
- [ ] **T010** [Users] [US3] Create `backend/src/Modules/Users/ThingsBooksy.Modules.Users.Core/Services/RevokedTokenChecker.cs` — implements `IRevokedTokenChecker` (from `Shared.Abstractions`). Uses `IMemoryCache` (key: `revoked:{jti}`) with `AbsoluteExpiration = ExpiresAt`. On cache miss, queries `UsersDbContext.TokenRevocations.AsNoTracking().AnyAsync(x => x.Jti == jti, ct)`.
- [ ] **T011** [Users] [US3] Register `IRevokedTokenChecker` → `RevokedTokenChecker` and `IMemoryCache` (if not already) in `AddUsersCore(...)`.

### JWT pipeline wiring

- [ ] **T012** [Shared] [US3] In `backend/src/Shared/ThingsBooksy.Shared.Infrastructure/Auth/Extensions.cs` (or wherever `AddJwt(...)` lives) add `JwtBearerEvents.OnTokenValidated`:
  - Extract `jti` claim from `context.Principal`.
  - Resolve `IRevokedTokenChecker` (from `Shared.Abstractions`) via `context.HttpContext.RequestServices`.
  - If `await checker.IsRevokedAsync(jti, ct)` → `context.Fail("Token revoked.")`.
  - **No new project reference required** — `Shared.Infrastructure` already references `Shared.Abstractions`.
- [ ] **T013** [Users] [US3] Add endpoint `POST /users/logout` to `backend/src/Modules/Users/ThingsBooksy.Modules.Users.Api/UsersModule.cs`:
  - `.RequireAuthorization()`.
  - Extracts `jti` claim, `exp` claim (Unix seconds → `DateTimeOffset.FromUnixTimeSeconds(...).UtcDateTime`), `sub` claim (user id).
  - Builds `LogoutCommand`, dispatches via `IDispatcher.SendAsync`.
  - Returns `Results.Ok()`.
  - WithTags `"Account"`, WithName `"Sign out"`.

### Build + format

- [ ] **T014** [Solution-wide] Run `dotnet format backend/ThingsBooksy.slnx`.
- [ ] **T015** [Solution-wide] Run `dotnet build backend/ThingsBooksy.slnx` — must pass.

**Checkpoint Wave 1**: `dotnet build` passes, `/users/logout` callable via Swagger, manual test: sign-in → logout → /users/me with same token returns 401.

---

## Phase 2 (Wave 1, BE): Migration

- [ ] **T016** [Users] Run `migration-agent` with module `Users` and Schema changes `New entity TokenRevocation in users schema`. Migration name: `AddTokenRevocations`. Migration must create `token_revocations` table with unique index on `jti`.

**Checkpoint**: migration generated, designer/snapshot updated, `dotnet build` still passes.

---

## Phase 3 (Wave 1, BE): Quality review

- [ ] **T017** [Users] Run `quality-reviewer` agent on the changes T001–T015 (module: `Users`, task IDs T001–T015). Address any unchallenged BLOCKERs via repair loop (max 2 iterations).

---

## Phase 4 (Wave 1, BE): Integration tests

- [ ] **T018** [Users] [US3] Create `backend/src/Modules/Users/ThingsBooksy.Modules.Users.IntegrationTests/Users/LogoutEndpointTests.cs`:
  - Test class part of `IntegrationTestCollection`, derives from `TestClient` per pattern.
  - `Logout_WithValidToken_Returns200_AndPersistsRevocation`
  - `GetMe_AfterLogout_WithSameToken_Returns401`
  - `Logout_TwiceWithSameToken_FirstReturns200_SecondReturns401`
  - `Logout_WithoutToken_Returns401`
- [ ] **T019** [Users] Run `dotnet test backend/src/Modules/Users/ThingsBooksy.Modules.Users.IntegrationTests/` — all new + existing tests pass.

**Checkpoint Wave 1**: BE complete — endpoint, revocation, tests all green.

---

## Phase 5 (Wave 2, FE foundation): Design tokens + auth glue

- [ ] **T020** [P] [FE-styles] Edit `frontend/src/styles/_tokens.scss` — add `--gradient-brand`, `--gradient-btn`, `--gradient-hover`, composite `--shadow-card`, `--color-backdrop`.
- [ ] **T021** [P] [FE-core] [US6] Create `frontend/src/app/core/guards/auth.guard.ts` — `CanActivateFn`, injects `AuthService` + `Router`. If `currentUser()` is null → `router.parseUrl('/')`; else `true`.
- [ ] **T022** [FE-core] [US6] Re-export `authGuard` from `frontend/src/app/core/index.ts`.
- [ ] **T023** [FE-api] Run `fe-api-client-writer` to regenerate API client (so `Users.ts` has `logout()`). Swagger source: `http://localhost:8080/swagger/v1/swagger.json` (start backend if needed).
- [ ] **T024** [FE-auth] [US3] Edit `frontend/src/app/features/auth/auth.service.ts`:
  - `signIn(...)` — after success, `router.navigate(['/dashboard'])`.
  - `signOut()` — `httpClient.post('/users/logout', null).pipe(catchError(() => of(null))).subscribe(() => { /* clear state + navigate */ })`.
  - Add `displayName` and `initials` computed signals from `currentUser().email`.
- [ ] **T025** [FE-auth] [US1] Edit `frontend/src/app/features/auth/auth-page/auth-page.component.ts` — `ngOnInit`: if `isAuthenticated()` after `loadCurrentUser()` → `router.navigate(['/dashboard'])`.
- [ ] **T026** [FE-app] [US1] [US6] Edit `frontend/src/app/app.routes.ts` — add `dashboard` route with `loadChildren` + `canActivate: [authGuard]`.

**Checkpoint**: signing in redirects to `/dashboard` (page does not exist yet — will 404 in router-outlet until Phase 7 — acceptable for now). Auth guard works (manual: open `/dashboard` without token → redirect to `/`).

---

## Phase 6 (Wave 2, FE foundation): Shared atoms

Run via `html-extractor` → `fe-plan-validator` → `fe-component-writer` × N (parallel).

- [ ] **T027** [FE-extract] Run `html-extractor` with path `C:\Users\dsieczka\Desktop\github\ThingsBooksy - Design\ThingsBooksy - dashboard\ThingsBooksy Dashboard.html`. Output: HTML-EXTRACTOR COMPLETE block.
- [ ] **T028** [FE-extract] Run `fe-plan-validator` with the HTML-EXTRACTOR COMPLETE block. Resolve any NO-GO blockers and re-extract if needed.
- [ ] **T029** [P] [FE-shared] [US2] `fe-component-writer` — `AnimatedBackgroundComponent` (`tb-animated-background`) extracted from auth-page (refactor existing markup into shared component).
- [ ] **T030** [P] [FE-shared] [US2] `fe-component-writer` — `IconSettingsComponent`, `IconLogoutComponent`, `IconChevronComponent`, `IconPlusComponent`, `IconCloseComponent` (`tb-icon-*`) — inline SVG, `size` input default 16.
- [ ] **T031** [P] [FE-shared] [US5] `fe-component-writer` — `UserMenuComponent` (`tb-user-menu`) — dropdown with Settings + Logout; `name`, `initials` inputs; `settings`, `logout` outputs; closes on outside click.
- [ ] **T032** [P] [FE-shared] [US2] `fe-component-writer` — `StatusBadgeComponent` (`tb-status-badge`) — `status` input mapped to design tokens.
- [ ] **T033** [P] [FE-shared] [US2] `fe-component-writer` — `CountChipComponent` (`tb-count-chip`) — `count` input.
- [ ] **T034** [P] [FE-shared] [US4] `fe-component-writer` — `ModalComponent` (`tb-modal`) — native `<dialog>`; `open` input toggles `.showModal()` / `.close()`; ESC + backdrop click close; emits `close` output; focus-trap.

**Checkpoint**: all shared atoms have unit specs and `npm run build` passes.

---

## Phase 7 (Wave 3, FE dashboard): Dashboard components

- [ ] **T035** [P] [FE-dashboard] [US1] [US2] `fe-component-writer` — `DashboardWelcomeBarComponent` (`tb-dashboard-welcome-bar`) — "Hi, {name}" + date (`DatePipe`, format `'MMM d, y'`).
- [ ] **T036** [P] [FE-dashboard] [US2] `fe-component-writer` — `DashboardHistoryPanelComponent` (`tb-dashboard-history-panel`) — title + count chip + ghost "View all" + table with 6 rows from `mock-data.ts`. Responsive: hide Time at 860px, Amount at 500px.
- [ ] **T037** [P] [FE-dashboard] [US2] [US4] `fe-component-writer` — `DashboardAdminPanelComponent` (`tb-dashboard-admin-panel`) — title + primary "Create new group" + 2 sections. Emits `createGroup` output captured by `DashboardPageComponent`.
- [ ] **T038** [P] [FE-dashboard] [US4] `fe-component-writer` — `CreateGroupModalComponent` (`tb-create-group-modal`) — wraps `tb-modal`, header "Create new group", body placeholder "Form coming soon", X button. `open` input, `close` output.
- [ ] **T039** [P] [FE-dashboard] [US5] `fe-component-writer` — `DashboardHeaderComponent` (`tb-dashboard-header`) — logo + nav (4 links translated) + `tb-user-menu` consuming `displayName` + `initials` from `AuthService`. Wires `(logout)` to `authService.signOut()`.
- [ ] **T040** [P] [FE-dashboard] [US1] [US2] `fe-component-writer` — `DashboardPageComponent` (`tb-dashboard-page`) — orchestrates `tb-animated-background`, `tb-dashboard-header`, `tb-dashboard-welcome-bar`, `tb-dashboard-history-panel`, `tb-dashboard-admin-panel`, `tb-create-group-modal`. Local signal `modalOpen` toggled by admin-panel output.
- [ ] **T041** [FE-dashboard] Create `frontend/src/app/features/dashboard/mock-data.ts` — exports `HISTORY_ROWS`, `MEMBER_GROUPS`, `ADMIN_GROUPS`. English text. Comment `// TODO: replace with /bookings + /management-groups API`.

**Checkpoint Phase 7**: all dashboard components compile and pass their specs.

---

## Phase 8 (Wave 3, FE): Routing

- [ ] **T042** [FE-dashboard] [US1] Run `fe-route-writer` with feature `dashboard`, route plan: `{ path: '', component: DashboardPageComponent }`. Register in `app.routes.ts` under `/dashboard` with `canActivate: [authGuard]`.

**Checkpoint**: signing in lands on `/dashboard` rendered fully; pixel-precision matches Claude Design at 1280/1024/860/500 px.

---

## Phase 9: Architecture guard

- [ ] **T043** [Solution-wide] Run `architecture-guard` listing modules touched: `Users` (BE), `shared`/`features/dashboard` (FE). Resolve any unchallenged violations via repair loop.

---

## Phase 10: Manual verification

- [ ] **T044** [Solution-wide] Smoke per spec acceptance:
  - Sign in → redirect to `/dashboard`.
  - Side-by-side visual diff with `ThingsBooksy Dashboard.html` at all breakpoints.
  - User dropdown opens; Logout calls `POST /users/logout` (verify in Network); redirect to `/`.
  - Re-using the old token returns 401.
  - Modal opens / ESC / backdrop / X close.
  - Direct `/dashboard` without token → redirect to `/`.

---

## Dependency notes

- Wave 1 (BE) is fully independent and runs first.
- T023 (`fe-api-client-writer`) depends on Wave 1 completing.
- T024–T026 depend on T023 (regen client).
- Phase 6 (shared atoms) is independent of Phase 5 — could overlap.
- Phase 7 depends on Phase 6 atoms existing.
- Phase 8 depends on Phase 7 (DashboardPageComponent must exist).
- Phase 9 runs after Phase 8.
