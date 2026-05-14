# 008 — Group detail view (post-creation) — design brief for Claude Design

> **Status**: deferred. Generated during planning of `006-dashboard-and-logout`. This file is a **design brief** intended to be pasted into Claude Design to generate the mockup. Once the mockup exists, run `html-extractor` against it and proceed through the FE agent pipeline.
>
> **Trigger to execute**: `006-dashboard-and-logout` is shipped. The "Create new group" modal is empty (placeholder) — this spec turns it into a working form and adds the post-creation view.

---

## Context

In iteration `006` the dashboard ships with a "Create new group" primary button that opens an empty modal (close-only). This iteration:
1. Fills the modal with a real form (Name + Description).
2. Wires the form to `POST /management-groups`.
3. Builds a post-creation view `/groups/{id}` showing a simplified list of the group's resources + a button leading to a (future) "Schema view".

The backend is already in place: all needed endpoints exist (`POST /management-groups`, `GET /management-groups/{id}`, `GET /resources/instances?groupId={id}`, `GET /resources/types?groupId={id}`).

---

## Part 1 — Create-group form (inside the existing modal)

### Data required to create a group

| Field | Type | Constraints | UI |
|---|---|---|---|
| `name` | string | required, 1–100 chars, unique per `ownerId` (async server-side check) | Single-line text input |
| `description` | string \| null | optional, max 500 chars | Multi-line textarea (4 rows visible, autosize up to 8) |

Backend automatically attaches `ownerId` from JWT. Response:

```json
{
  "id": "uuid",
  "name": "string",
  "description": "string|null",
  "createdAt": "iso8601",
  "ownerId": "uuid"
}
```

### Form behavior (Angular Reactive Forms — per `angular-forms-pattern.md`)

- Typed `FormControl<string>` and `FormControl<string|null>`, `nonNullable: true` for `name`.
- Validators:
  - `name`: `Validators.required`, `Validators.minLength(1)`, `Validators.maxLength(100)`, async uniqueness validator using `timer(300) + first()` debounce hitting `GET /management-groups?name={name}` (need new BE query param or new endpoint — clarify in spec).
  - `description`: `Validators.maxLength(500)`.
- Submit disabled while form is `invalid`, `pending`, or `submitting`.

### Modal layout (in Claude Design — please mock this)

- **Width**: 480 px (already set in 006).
- **Header**: title "Create new group" + close (X) button top-right.
- **Body padding**: 28 px.
- **Form fields** (vertical stack, gap 16 px):
  - Field 1 — label "Group name *" above input. Input style matches existing `tb-text-field` from auth feature (surface bg, focus ring `--shadow-input-focus`). Helper text below input: "1–100 characters". Error message in `--color-danger-text` when invalid (after blur or after first submit attempt).
  - Field 2 — label "Description" above textarea. Textarea with same styling as input but `min-height: 96px`. Character counter bottom-right: `{n}/500` (color shifts to `--color-warning` at 450+, `--color-danger-text` at 500).
- **Footer** (24 px top margin, gap 12 px, right-aligned):
  - Ghost button "Cancel" (closes modal, discards form).
  - Primary button "Create group" (gradient `--grad-btn`, disabled when invalid). Shows spinner inside when submitting.
- **Success**: form submits → modal closes → router navigates to `/groups/{response.id}` with a toast "Group created" (use existing `tb-status-message` from auth).
- **Failure**: form stays open, generic error banner above footer in `--color-danger-bg` with `--color-danger-text`.

---

## Part 2 — Post-creation view `/groups/{id}`

### Route
- Path: `/groups/:id` (lazy-loaded under `features/groups/groups.routes.ts`).
- Guard: `authGuard` (existing).
- Resolver (optional): `groupResolver` fetching `GET /management-groups/{id}` so the page loads with data ready.

### Page structure (in Claude Design — please mock this)

**Layout container**: same animated background as dashboard, max-width 1100 px, padding `88px 56px 56px`.

**1. Breadcrumb** (top, font-size sm, color `--color-text-secondary`):
- "Dashboard" (link, hover → primary text color) / "{Group name}" (current, no link).

**2. Group header card** (`panel` style — glassmorphic):
- Left side:
  - H1 with group name in gradient text (same as welcome bar — `--color-accent-primary` → `--color-accent-secondary`, `--font-family-display`, weight 700).
  - Description below in `--color-text-secondary`, max 2 lines (truncated with ellipsis if longer).
  - Metadata row below: "Created {relative date}" · "{N} members" · "{N} resources" — chips with subtle borders.
- Right side (top-right):
  - Ghost button "Edit" (pencil icon + label) — opens edit modal (out of scope, placeholder).
  - **Primary gradient button "View schema"** with right arrow → navigates to `/groups/{id}/schema` (future view).
  - Danger ghost button "Delete" (trash icon, opens confirm modal — out of scope).

**3. Resources section** (`panel` glassmorphic):
- Header: "Resources" title + count chip + ghost button "Browse all".
- **Simplified list** (NOT a full table — that's for resources types page):
  - Each row: 64 px height, padding `12px 16px`, border-radius 10 px, hover bg `--color-surface-raised`.
  - Layout: icon (resource type's icon, 32×32 px, accent-colored circular bg) → name (primary text, weight 600) + sub-line (resource type name in secondary text) → spacer → optional status badge → arrow icon "→" right-aligned.
  - Click row → navigates to resource detail (future view).
- **Limit**: show first 6 resources; if more, show "View all {N} resources" link below.
- **Empty state** (when group has zero resources):
  - Centered illustration (simple SVG — e.g., empty box icon, 96×96 px, accent-tinted).
  - Heading "No resources yet" (display font, lg).
  - Subtext "Add your first resource to start booking" (secondary).
  - Primary button "Add resource" (opens add-resource flow — out of scope, placeholder).

**4. Members section** (optional in this iteration, can be a stub):
- Header: "Members" + count + ghost button "Manage".
- List of avatars (initials) + name + role badge.
- "Add member" ghost button at bottom.

---

## Data sources

- `GET /management-groups/{id}` → group meta + member list (already in API).
- `GET /resources/instances?groupId={id}` → resource list (already in API).
- `GET /resources/types?groupId={id}` → for icon/name mapping (already in API).

All endpoints exist. No new BE work needed.

---

## Design tokens (use ONLY these — same as dashboard)

Refer to `frontend/src/styles/_tokens.scss`. Specifically:
- Colors: `--color-bg`, `--color-overlay-card`, `--color-text-primary/secondary/muted`, `--color-accent-primary/secondary`, `--color-danger-*`, `--color-warning`, `--color-success-*`.
- Gradients: `--grad-brand` (for headings), `--grad-btn` (for primary buttons).
- Radius: `--radius-2xl` for panels, `--radius-xl` for buttons, `--radius-full` for chips/badges.
- Shadows: `--shadow-card-lg`, `--shadow-card-inset`, `--shadow-cta-hover` for buttons.
- Blur: `--blur-card` (44 px) for panels.
- Spacing: 4-point scale (`--space-1` … `--space-18`).
- Fonts: `--font-family-display` (Outfit) for headings, `--font-family-base` (DM Sans) for body.

---

## Responsive breakpoints (same as dashboard)

- `@media (max-width: 1024px)`: padding reduces to `88px 28px 48px`.
- `@media (max-width: 860px)`: group header layout stacks (actions move below text); resource rows stay full-width.
- `@media (max-width: 500px)`: actions row wraps; "View schema" button stays prominent.

---

## Accessibility

- All buttons / interactive elements have `aria-label` when icon-only.
- Modal traps focus (use Angular CDK `FocusTrap` or manual implementation).
- Toast messages are `role="status"` with `aria-live="polite"`.
- Keyboard: Tab cycles modal inputs → primary button → cancel → close (X). ESC closes modal.

---

## Workflow when executed

1. **User**: paste this file into Claude Design → generate mockup → save HTML to `C:\Users\dsieczka\Desktop\github\ThingsBooksy - Design\ThingsBooksy - group-detail\`.
2. **Claude Code**: `html-extractor` on that HTML → `fe-plan-validator` → `fe-api-client-writer` (no BE changes but safe) → `fe-component-writer` (×N components) → `fe-route-writer` (`groups.routes.ts` + register in `app.routes.ts`).
3. **Manual verification**: side-by-side compare with Claude Design mockup at all breakpoints.

---

## Out of scope (still further iterations)

- Schema view `/groups/{id}/schema` — separate handoff after this one.
- Edit-group flow + Delete-group flow (confirmation modal).
- Add-resource flow.
- Manage-members flow.
- Soft delete vs. hard delete UX clarification.
