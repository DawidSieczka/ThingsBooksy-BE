# Feature Specification: Dashboard + functional logout + Create-group modal

**Branch**: `006-dashboard-and-logout`
**Created**: 2026-05-14
**Status**: Draft
**Plan reference**: `C:\Users\dsieczka\.claude\plans\w-claude-design-przygotowa-em-peppy-abelson.md`
**Design source**: `C:\Users\dsieczka\Desktop\github\ThingsBooksy - Design\ThingsBooksy - dashboard\ThingsBooksy Dashboard.html`

---

## Context

After signing in, the user currently sees a "post-login confirmation" placeholder on the same auth page. The product needs a real dashboard. A Claude Design mockup is already prepared — this spec turns it into a working Angular feature with full visual parity (colors, layout, typography, animations) and English copy. The backend needs a real `/users/logout` endpoint so the dropdown action genuinely revokes the session.

Three explicit gaps are accepted as known short-term debt and captured in handoff files:
- Real first/last name on the User entity → `specs/007-backend-user-profile-fields.md` (FE shows email-derived placeholder for now).
- Create-group form fields + post-creation `/groups/{id}` view → `specs/008-group-detail-view.md` (modal ships empty in this iteration).
- Bookings/Reservations module — does not exist; history panel ships with hardcoded mock data.

---

## User Stories

### Story 1 — Sign in lands on a real dashboard (Priority: P1)

As a signed-in user I want to be redirected to a dedicated `/dashboard` route after successful sign-in so that I get a productive landing page instead of a confirmation screen.

**Why this priority**: foundation — every other story depends on having a dashboard route.

**Independent test**: open `/`, sign in with valid credentials, observe automatic navigation to `/dashboard` and rendering of dashboard panels.

**Acceptance scenarios**:
1. Given a user with valid credentials, when they submit the sign-in form, then the app navigates to `/dashboard` and renders header + welcome bar + history panel + admin panel.
2. Given an unauthenticated user, when they visit `/dashboard` directly, then they are redirected to `/`.
3. Given an authenticated user, when they refresh `/dashboard`, then `/users/me` is called on bootstrap and the dashboard loads without flash-of-login.

---

### Story 2 — Dashboard layout matches Claude Design (Priority: P1)

As a stakeholder I want the dashboard to visually replicate the Claude Design mockup so that brand & UX consistency is preserved.

**Why this priority**: same iteration cost regardless of fidelity, but loss of fidelity is hard to claw back later.

**Independent test**: open `/dashboard` side-by-side with `ThingsBooksy Dashboard.html` in browser at viewport widths 1280 / 1024 / 860 / 500 px. Visual diff manual check.

**Acceptance scenarios**:
1. Given the dashboard is rendered at 1280 px, when compared to the mockup, then header height is exactly 64 px, page max-width 1300 px, panels grid `1fr 336px` with 20 px gap.
2. Given any panel, when inspected, then `background`, `border-radius` (22 px), `backdrop-filter: blur(44px)`, and `box-shadow` match the design tokens defined in `_tokens.scss`.
3. Given the viewport shrinks below 860 px, when observed, then panels stack to single column and the table's `Time` column is hidden.
4. Given the viewport shrinks below 500 px, when observed, then the table's `Amount` and `Status` columns hide, leaving only resource + time.
5. Given a fresh load, when the dashboard mounts, then welcome / history / admin animations play with 0.1 s / 0.2 s / 0.3 s stagger.
6. Given the user inspects all text content, when scanning, then no Polish characters appear (full translation per `Translations` table in the plan).

---

### Story 3 — Functional logout (Priority: P1)

As a signed-in user I want clicking "Log out" in the user dropdown to actually invalidate my session so that re-using my old token after logout is rejected by the server.

**Why this priority**: security — without server-side revocation, "logout" is theatre.

**Independent test**:
- Sign in → call `POST /users/logout` with the token → call `GET /users/me` with the same token → expect 401.
- In the UI: click Log out → redirected to `/` → reload → still on `/` (no auto-login).

**Acceptance scenarios**:
1. Given a valid JWT, when `POST /users/logout` is called with `Authorization: Bearer <token>`, then the response is 200 and the token's `jti` is recorded in `users.token_revocations`.
2. Given a token whose `jti` is in `users.token_revocations`, when any authenticated endpoint is called with that token, then the response is 401.
3. Given a user clicks "Log out" in the dropdown, when the request resolves, then `localStorage` is cleared, the signal state is reset, and the app navigates to `/`.
4. Given the logout endpoint returns a non-2xx (e.g., network error), when the FE handles it, then the user is still logged out client-side and shown a toast warning ("Signed out locally; the server may still hold a session").

---

### Story 4 — Create-group modal opens and closes (Priority: P2)

As a user I want clicking "Create new group" in the admin panel to open a centered modal that I can dismiss without committing to anything so that I can explore the action without fear.

**Why this priority**: groundwork for `specs/008-group-detail-view.md` — the modal shell needs to exist before the form can be built.

**Independent test**: click "Create new group" → modal opens centered with backdrop blur → press ESC → modal closes. Repeat with backdrop click and X button.

**Acceptance scenarios**:
1. Given the dashboard is loaded, when the user clicks "Create new group", then a modal opens centered on the viewport with a glassmorphic backdrop.
2. Given the modal is open, when the user presses ESC, the body is scroll-locked, and pressing ESC closes it.
3. Given the modal is open, when the user clicks the X button or the backdrop (not the modal body), then the modal closes.
4. Given the modal is open, when the user tabs through, then focus is trapped inside the modal (does not escape to background content).
5. Given the modal is closed, when reopened, then it shows a placeholder text "Form coming soon" — no form yet.

---

### Story 5 — User dropdown with Settings + Log out (Priority: P2)

As a user I want a discoverable menu in the top-right showing my identity + Settings + Log out so that I can manage my account.

**Why this priority**: needed to trigger Story 3's logout.

**Independent test**: click avatar pill → dropdown appears with two items → click "Log out" → triggers logout flow.

**Acceptance scenarios**:
1. Given the dashboard is loaded, when the user clicks the avatar pill, then a dropdown opens with min-width 192 px, glassmorphic styling, `fadeDown 0.18s` animation.
2. Given the dropdown is open, when the user clicks outside of it, then it closes.
3. Given the dropdown is open, when the user hovers over "Log out", then the row turns red (`--color-danger-*`).
4. Given the user is logged in and the email is `john.smith@example.com`, when the dropdown is shown, then the avatar text is "JS" and the welcome bar shows "Hi, John Smith" (email-derived placeholder per `specs/007-backend-user-profile-fields.md`).
5. Given the user clicks "Settings", when handled, then a placeholder action runs (e.g., toast "Settings coming soon" — no settings page yet).

---

### Story 6 — Auth guard prevents unauthenticated dashboard access (Priority: P2)

As an operator I want any attempt to load `/dashboard` without a valid session to be redirected to the sign-in screen so that no protected UI ever flashes without authentication.

**Why this priority**: security + UX hygiene.

**Acceptance scenarios**:
1. Given no token in localStorage, when navigating to `/dashboard`, then the user is redirected to `/`.
2. Given a token that fails `/users/me` validation, when bootstrapping, then `signOut()` runs and the user lands on `/`.
3. Given a valid token, when navigating to `/dashboard`, then the page renders without redirect.

---

## Functional Requirements

### Backend (Users module)

- **FR-BE-1**: Add `Jti` claim to JWT tokens issued by `JsonWebTokenManager.CreateToken()`.
- **FR-BE-2**: Add `TokenRevocation` entity to `Users.Core/Entities/` with `Id`, `Jti`, `UserId`, `RevokedAt`, `ExpiresAt`. Schema `users`.
- **FR-BE-3**: Migration `AddTokenRevocations` adds table `users.token_revocations` with index on `Jti` (unique).
- **FR-BE-4**: New command `LogoutCommand(Jti, UserId, ExpiresAt)` + handler that creates the revocation record via `IUsersDataProvider`.
- **FR-BE-5**: New endpoint `POST /users/logout` requiring `[Authorize]`, building the command from `HttpContext.User` claims, returning 200 OK on success.
- **FR-BE-6**: Extend JWT pipeline (`JwtBearerEvents.OnTokenValidated`) to reject tokens whose `Jti` is in `users.token_revocations`. Use in-memory cache (TTL = remaining lifetime) with DB fallback on miss.
- **FR-BE-7**: Integration tests: `Logout_WithValidToken_Returns200_AndRevokes`, `Logout_TwiceWithSameToken_SecondCallReturns401`, `GetMe_AfterLogout_Returns401`, `Logout_WithExpiredToken_Returns401`.

### Frontend (Auth + Dashboard)

- **FR-FE-1**: New `core/guards/auth.guard.ts` (`CanActivateFn`). Redirects to `/` when `authService.currentUser()` is null.
- **FR-FE-2**: `app.routes.ts` — root `/` keeps auth feature, new `/dashboard` lazy-loads `features/dashboard/dashboard.routes.ts` with `authGuard`. `signIn` success navigates to `/dashboard`. If already authenticated and on `/`, redirect to `/dashboard`.
- **FR-FE-3**: `AuthService.signOut()` calls `POST /users/logout` (regenerated client). On any result (success or failure), clears token + signal + localStorage, then navigates to `/`.
- **FR-FE-4**: Add design tokens to `frontend/src/styles/_tokens.scss`: `--grad-brand`, `--grad-btn`, `--grad-hover`, composite `--shadow-card`, `--color-backdrop`.
- **FR-FE-5**: Extract animated background (orbs + grid + rings) from `features/auth/auth-page` into `shared/components/animated-background/` and reuse in dashboard.
- **FR-FE-6**: Shared atoms in `shared/components/`: `user-menu` (dropdown), `status-badge`, `count-chip`, `modal` (native `<dialog>`), `icons/` (`tb-icon-settings`, `tb-icon-logout`, `tb-icon-chevron`, `tb-icon-plus`, `tb-icon-close`).
- **FR-FE-7**: Dashboard feature components in `features/dashboard/`: `dashboard-page`, `dashboard-header`, `dashboard-welcome-bar`, `dashboard-history-panel`, `dashboard-admin-panel`, `create-group-modal`. All standalone, `tb-` selector prefix, signals where state, `inject()` not constructor DI.
- **FR-FE-8**: `mock-data.ts` exports 6 history rows + 4 member groups + 2 admin groups, all English text, all dates as `Date` objects.
- **FR-FE-9**: `AuthService` exposes computed `displayName` and `initials` derived from `currentUser().email` (transformation: split on `@`, replace `.` with space, `_.startCase`-like; initials = first letter of each word, max 2).
- **FR-FE-10**: All text from `ThingsBooksy Dashboard.html` translated per the table in the plan file; no Polish characters in compiled output.

---

## Non-functional Requirements

- **NFR-1 (Design fidelity)**: every dimension, color, font size, shadow, animation duration in the dashboard must come from `_tokens.scss` (no hardcoded values — enforced by `angular-styling.md`).
- **NFR-2 (Accessibility)**: all interactive elements (dropdown items, modal close, buttons) have `aria-label` when icon-only; modal traps focus; ESC closes modal; backdrop click closes modal; `prefers-reduced-motion` disables animations (already handled in base styles).
- **NFR-3 (Performance)**: dashboard route is lazy-loaded; no synchronous network blocking initial render — `/users/me` runs on app bootstrap (already implemented).
- **NFR-4 (Security)**: token revocation check runs on every authenticated request without significant performance impact (in-memory cache + DB fallback).
- **NFR-5 (Test coverage)**: BE — every new handler and endpoint covered by an integration test (constitution: tests required for new business logic).

---

## Out of scope (explicit)

- Real form inside the Create-group modal → `specs/008-group-detail-view.md`.
- Group detail view `/groups/{id}` → `specs/008-group-detail-view.md`.
- `FirstName`/`LastName` on User entity → `specs/007-backend-user-profile-fields.md`.
- Settings page (dropdown item is a placeholder).
- Bookings/Reservations module (history panel is mock-data only).
- Refresh token mechanism (token lifetime stays at 7 days).
- Schema view (`/groups/{id}/schema`).
- Tweaks panel from the design (developer tool, not shipped to production).

---

## Dependencies

- Existing: `Users` module, `ManagementGroups` module, `Resources` module, JWT setup, auth service, generated API client.
- New BE work: only inside `Users` module (no cross-module changes).
- FE: no new npm dependencies — native `<dialog>`, Angular core only.

---

## Success criteria

- All 6 user stories pass their acceptance scenarios.
- `dotnet build` + `dotnet test` + `npm run build` + `npm test` + `npm run lint` all pass.
- Side-by-side visual diff with `ThingsBooksy Dashboard.html` shows no perceptible difference at 1280 / 1024 / 860 / 500 px breakpoints.
- `architecture-guard` reports zero unresolved violations.
- `specs/007-backend-user-profile-fields.md` and `specs/008-group-detail-view.md` exist and are referenced from this spec.

---

## Workflow

1. `/speckit-plan` — produce `plan.md` (BE module changes + FE component plan).
2. `/speckit-tasks` — produce `tasks.md` (Wave 1 BE, Wave 2 FE foundation, Wave 3 FE dashboard).
3. `plan-validator` — GO/NO-GO + EXECUTION MAP.
4. **Wave 1 (BE)**: `module-writer` (Users) → `migration-agent` → `quality-reviewer` → `integration-test-writer`.
5. **Wave 2-3 (FE)**: `fe-api-client-writer` (refresh client after BE) → `html-extractor` (input: `ThingsBooksy Dashboard.html`) → `fe-plan-validator` → `fe-component-writer` × N (parallel) → `fe-route-writer`.
6. `architecture-guard`.
7. Manual visual review per Success criteria.
