# 007 — Backend handoff: User profile fields (FirstName / LastName)

> **Status**: deferred. Generated during planning of `006-dashboard-and-logout`. Execute this in a **separate iteration** after the dashboard is live.
>
> **Trigger to execute**: dashboard is shipped, FE shows display name derived from email — we want real first/last name.

---

## Context

The dashboard from Claude Design (`ThingsBooksy Dashboard.html`) shows the user as **"John Smith"** with initials **"JS"** in the welcome bar and user-menu avatar. The current `User` entity has only `Email`, `JobTitle`, `RoleName` — no first/last name.

Iteration `006` ships the dashboard with display name **derived from email** (`john.smith@x.com` → `John Smith` via `_.startCase(emailLocalPart.replace('.', ' '))`). This is a visual placeholder, not a real model.

This handoff describes adding real fields so the FE can read them directly.

---

## Goal

Extend `User` entity with `FirstName` and `LastName`, propagate through sign-up + `/users/me` + persistence, and let FE drop the email-derived fallback.

---

## Scope

### Domain
File: `backend/src/Modules/Users/Users.Core/Entities/User.cs`
- Add `FirstName: string` and `LastName: string` properties (encapsulated: `private set`, set via factory).
- Update factory method (`Create`) to accept both. Stay within the 4-parameter limit (`domain-entity-design.md`) — if exceeded, accept a `UserSignUpData` record.
- Validation in entity invariants:
  - `FirstName` / `LastName`: required, non-empty after trim, max 100 chars each, regex `^[\p{L}\s\-']+$` (Unicode letters + spaces + hyphens + apostrophes — supports `O'Brien`, `Anne-Marie`, Polish chars).

### Persistence
File: `backend/src/Modules/Users/Users.Core/EF/Configurations/UserConfiguration.cs`
- `Property(u => u.FirstName).IsRequired().HasMaxLength(100)`.
- `Property(u => u.LastName).IsRequired().HasMaxLength(100)`.

### Migration
Project: `backend/src/Modules/Users/Users.Migrations`
- Name: `AddUserNameFields`.
- Add columns `first_name` and `last_name` to `users.users`.
- **Backfill** existing rows: `UPDATE users.users SET first_name = INITCAP(SPLIT_PART(email, '@', 1)), last_name = '-' WHERE first_name IS NULL` (PostgreSQL).
- Apply NOT NULL constraint after backfill.

### Sign-up command + endpoint
File: `backend/src/Modules/Users/Users.Core/SignUps/SignUpCommand.cs` + handler:
- Add `FirstName` and `LastName` to command record.

File: `backend/src/Modules/Users/Users.Api/Requests/SignUpRequest.cs`:
- Add `FirstName` and `LastName` fields with `[Required]` data annotations.

File: `backend/src/Modules/Users/Users.Api/UsersModule.cs`:
- Endpoint `POST /users/sign-up`: pass new fields from request → command construction (in endpoint, not bound from body — per `command-construction-in-endpoints.md`).

### Read endpoint
File: `backend/src/Modules/Users/Users.Core/GetAccount/GetAccountQuery.cs` handler + DTO:
- Return `FirstName`, `LastName` in DTO.
- Optional: return computed `DisplayName` (= `FirstName + ' ' + LastName`) and `Initials` (= first char of each, uppercase) — preferred so FE doesn't duplicate logic.

### Tests
Integration tests in `backend/tests/Modules/Users/Users.IntegrationTests/`:
- `SignUp_WithMissingFirstName_ReturnsBadRequest`.
- `SignUp_WithValidFields_PersistsFirstAndLastName`.
- `GetMe_ReturnsFirstNameAndLastName`.
- `GetMe_ReturnsComputedDisplayNameAndInitials`.

---

## FE delta (after BE is live)

1. Remove email-derived helper from `AuthService` (`displayName`, `initials` computed signals using email).
2. Replace with direct read from `currentUser().firstName` / `lastName` (or `displayName` / `initials` if BE returns them).
3. Update `SignUpForm` component to include First Name + Last Name fields (Reactive Forms, typed, validators matching BE regex/length).
4. Regenerate API client via `fe-api-client-writer`.

---

## Out of scope (separate handoff)

- Settings page where user can edit their name → future iteration.
- Avatar upload → future iteration.
- Display name preferences (e.g., "show initials only", "show full name") → future iteration.

---

## Workflow when executed

`/speckit-specify "Extend User entity with FirstName and LastName (read specs/007-backend-user-profile-fields.md for full spec)"` → `/speckit-plan` → `/speckit-tasks` → `plan-validator` → `module-writer` (Users) → `migration-agent` → `quality-reviewer` → `integration-test-writer` → `fe-api-client-writer` → FE component updates (sign-up form + auth service).
