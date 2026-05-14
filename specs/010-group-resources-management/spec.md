# Feature Specification: Group Detail & Schema Designer & Group/Resource Modals

**Feature Branch**: `010-group-resources-management`
**Created**: 2026-05-14
**Status**: Draft
**Input**: User description: "Group Detail (Admin Panel) + Schema Designer + Create/Edit Group Modal + Notifications — translate Polish Claude Design package to English Angular implementation, end-to-end flow: user creates a group → adds resource schemas → creates resource instances per schema, with all decisions captured in 16 final-decisions plan."

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Create a group and land on its detail page (Priority: P1)

A signed-in user opens the dashboard, clicks **Create new group**, fills the modal with a name (and optionally a description), submits, and is taken directly to the new group's detail page where they immediately see the four panels: header, schemas (empty), resources (empty), members (just themselves). A success toast confirms creation.

**Why this priority**: Without this flow the user can never reach any of the new views. It is the entry point that proves the whole feature works end-to-end with the minimum number of moving parts.

**Independent Test**: Sign in → click *Create new group* → enter a unique name and submit → assert that the modal closes, a success toast appears, and the page navigates to `/groups/{id}` rendering all four panels with the new group's data.

**Acceptance Scenarios**:

1. **Given** the user is on the dashboard with no groups, **When** they click *Create new group*, enter "Marketing Team", and submit, **Then** the modal closes with a slide+fade animation, a toast "Group created" appears, and the browser navigates to the group detail page showing "Marketing Team" in the header.
2. **Given** the user already owns a group named "Marketing Team", **When** they type "Marketing Team" in the name field of the modal, **Then** within ~500 ms an error message "This name is already taken" appears under the field and the *Create group* button becomes disabled.
3. **Given** the user types more than 100 characters in the name field, **When** they reach the limit, **Then** input is truncated/blocked at 100 and a hint message indicates "1–100 characters".
4. **Given** the user enters 480 characters in the description, **When** they type one more character, **Then** the counter switches color to warning and continues counting toward 500; at 500 it turns to error and further input is blocked.
5. **Given** the user begins filling the modal then presses Escape or clicks the overlay, **When** the close is triggered, **Then** the modal closes without confirmation (the small-cost-of-loss decision applies to this modal).

---

### User Story 2 — Design a resource schema for a group (Priority: P1)

The group owner opens the group detail page, clicks **Add schema** in the Schemas panel, lands on a full-page Schema Designer with two columns: a form on the left and a live form preview on the right. They give the schema a name (e.g. "Camera"), optional description, add fields one at a time (each defaulting to Number), can change a field's type (Text / Number / Yes-No), mark it required, drag fields up and down to reorder, and finally save. They return to the group detail page with the new schema listed.

**Why this priority**: Schemas are the prerequisite for creating resources. Without this story the user can create a group but cannot meaningfully populate it.

**Independent Test**: From a group detail page → click *Add schema* → enter "Camera", add three fields ("Serial number" Text, "Year" Number required, "In stock" Yes-No), drag "Year" to the top, click Save → assert that the page returns to the group detail and "Camera" appears in the Schemas panel.

**Acceptance Scenarios**:

1. **Given** the user is on a group's detail page, **When** they click *Add schema* in the Schemas panel, **Then** the browser navigates to a new Schema Designer page in create mode with empty form, empty preview, and a draft badge reading "No changes".
2. **Given** the Schema Designer is open, **When** the user clicks *Add field*, **Then** a new field row appears with default type Number, empty name, required = false, and the live preview shows a numeric input placeholder for it.
3. **Given** the user has added three fields, **When** they drag the third field above the first, **Then** the field order in the form and in the live preview both reflect the new order without losing any entered data.
4. **Given** the user has typed a schema name that already exists in this group, **When** they finish typing, **Then** an inline error message appears under the name field stating "A schema with this name already exists" and Save is disabled.
5. **Given** the user has made changes and clicks *Save schema* with a valid form, **When** the save succeeds, **Then** the draft badge changes to "Saved ✓" briefly, a toast "Schema saved" appears, and the user is returned to the group detail page where the new schema is listed.
6. **Given** the user has unsaved changes, **When** they click Cancel, the navbar logo, the breadcrumb, or refresh the page, **Then** a confirmation dialog asks "You have unsaved changes — discard them?" and only on confirmation does navigation proceed.
7. **Given** the user clicks the trash icon on a field row, **When** the click registers, **Then** the field is removed from the form and the live preview both, immediately, and the schema is marked dirty.

---

### User Story 3 — Create a resource instance based on an existing schema (Priority: P1)

From the group detail page, the user clicks **Add resource** (either the global button in the Resources panel or the per-schema "+" action in the Schemas panel). A modal opens showing the schema selector at the top — a dropdown when entry was global, or a read-only chip when entry was per-schema — followed by name, optional description, and a dynamically generated set of fields matching the chosen schema's property definitions. The user fills in the required fields, submits, and the new resource appears in the Resources panel.

**Why this priority**: Closes the end-to-end loop ("group → schema → resource"). Without this the user can build schemas but never store actual resources.

**Independent Test**: From a group with at least one schema "Camera" containing fields *Serial number* (Text, required) and *Year* (Number) → click *Add resource* in the Resources panel → pick "Camera" → fill in name, serial number, year → submit → assert the new resource appears in the Resources list with `Type = Camera` and status badge "Available (mocked)".

**Acceptance Scenarios**:

1. **Given** the group has two schemas "Camera" and "Laptop", **When** the user clicks the global *Add resource* button, **Then** a modal opens with a schema dropdown listing both schemas, name and description fields, and no property fields visible until a schema is picked.
2. **Given** the user picks "Camera" in the dropdown, **When** the selection changes, **Then** the property fields for "Camera" appear (Serial number text input, Year number input), each labeled with required indicator where applicable.
3. **Given** the user clicked the per-schema "+" on "Laptop", **When** the modal opens, **Then** "Laptop" is shown as a read-only chip at the top (no dropdown) and the property fields for "Laptop" are already rendered.
4. **Given** the user submits the form with a required field empty, **When** they click Create, **Then** the empty field shows an inline error, the submit is blocked, and no request is sent.
5. **Given** the user submits a valid form, **When** the request succeeds, **Then** the modal closes, a toast "Resource created" appears, and the Resources panel reflects the new row (without a full page reload).

---

### User Story 4 — Browse a group's resources with infinite scroll (Priority: P2)

A user with access to a group with many resources opens the group detail page. The Resources panel shows the first batch of resources and, as the user scrolls toward the bottom, the next batch is fetched and appended automatically without pagination clicks. The same behaviour applies to the Members panel.

**Why this priority**: Required for groups that grow past the first page. Independent of P1 stories — even before infinite scroll works, the first page is visible — but it is critical for usability.

**Independent Test**: Seed a group with 50 resources → open group detail → assert the first 20 are visible → scroll until near the bottom → assert another batch is fetched and rendered, total 40 visible → scroll again until all 50 are visible → assert no more fetches happen.

**Acceptance Scenarios**:

1. **Given** the group has 50 resources, **When** the user opens the group detail page, **Then** the Resources panel renders the first 20 entries and shows a small loading indicator near the bottom only while loading.
2. **Given** the user scrolls so that the last visible row approaches the viewport bottom, **When** the sentinel intersects, **Then** a second batch of 20 entries is fetched and appended below the first batch.
3. **Given** all entries have been loaded, **When** the user scrolls past the last entry, **Then** no further requests are made and an end-of-list state is visible (no spinner, no "load more" button).
4. **Given** the Members panel contains more than the page size, **When** the user scrolls the panel (or page) toward its bottom, **Then** the same infinite-scroll behaviour appends more members.
5. **Given** the user's `prefers-reduced-motion` is enabled, **When** new entries are appended, **Then** they appear without an entry animation (or with a near-instant one).

---

### User Story 5 — Edit a group's name and description (Priority: P2)

A group owner clicks **Edit** in the group header, sees a modal pre-filled with the current name and description, makes changes (including possibly a new name with re-checked uniqueness), submits, and the header reflects the new values with a success toast.

**Why this priority**: The Edit button is in the header design and the backend already exposes the update endpoint. It reuses the Create modal component, so it is cheap to add and high-value for correctness ("you can't fix a typo without it").

**Independent Test**: From a group's detail page → click Edit in the header → in the modal change the name to a new unique name and clear the description → submit → assert the header updates and a toast appears.

**Acceptance Scenarios**:

1. **Given** the user is the group owner viewing a group, **When** they click *Edit*, **Then** the same modal as for create opens, but the title reads "Edit group" and the fields are pre-filled with current values.
2. **Given** the user changes the name to one that already exists among their groups (excluding the current one), **When** the availability check completes, **Then** the inline error "This name is already taken" appears and Save is disabled.
3. **Given** the user submits unchanged content, **When** they click Save, **Then** the modal closes silently with no toast (no-op) — or a toast "No changes" — and no network noise occurs beyond the eager availability check.
4. **Given** the user is not the owner, **When** they view the group, **Then** the Edit button is hidden (it is owner-only).

---

### User Story 6 — Delete a group with cascading data cleanup (Priority: P2)

The group owner clicks **Delete** in the group header, sees a confirmation dialog that explicitly tells them how many schemas and resources will be removed alongside the group, confirms, and is returned to the dashboard where the group no longer appears in their list.

**Why this priority**: Destructive, requires user awareness. The flow itself is short, but the cascade behaviour must be communicated clearly so the user is not surprised.

**Independent Test**: Create a group with 2 schemas and 5 resources → click Delete in header → confirm the dialog → assert navigation to dashboard, group missing from list, and (server-side) all 5 resources soft-deleted, 2 schemas hard-deleted.

**Acceptance Scenarios**:

1. **Given** the group has 2 schemas and 5 resources, **When** the owner clicks *Delete*, **Then** a dialog appears with text including "Delete this group? It will also delete 2 schemas and 5 resources."
2. **Given** the confirmation dialog is open, **When** the user clicks "Cancel" or presses Escape, **Then** the dialog closes and no deletion occurs.
3. **Given** the user clicks "Delete" in the dialog, **When** the request succeeds, **Then** the browser navigates to the dashboard, a toast "Group deleted" appears, and the group does not appear in the dashboard list.
4. **Given** another user is a member (not the owner) of the group, **When** they view the group, **Then** the Delete button is hidden (it is owner-only).

---

### User Story 7 — Delete a schema with cascading data cleanup (Priority: P3)

From within the Schema Designer (or via a per-row delete action in the Schemas panel), the owner deletes a schema; a confirmation dialog states how many resources will be removed; on confirmation the schema disappears and its resources are soft-deleted on the server.

**Why this priority**: Symmetric to group deletion, but less frequent. Lower priority because day-to-day users rarely delete schemas and the data-loss exposure is smaller.

**Independent Test**: Create a schema "Camera" with 3 resources → invoke Delete schema → confirm → assert the schema is gone from the Schemas panel and the 3 resources are gone from the Resources panel.

**Acceptance Scenarios**:

1. **Given** a schema "Camera" has 3 resources, **When** the owner triggers Delete on it, **Then** a dialog appears with text "Delete schema 'Camera'? 3 resources of this type will be deleted."
2. **Given** the user confirms, **When** the request succeeds, **Then** the schema disappears from the Schemas panel and its 3 resources disappear from the Resources panel without a full reload.

---

### User Story 8 — Errors and notifications surface consistently (Priority: P2)

Across all the above flows, transient errors (network failure, unexpected 4xx/5xx) and success events both surface through a single toast system in the corner of the screen. Toasts stack, auto-dismiss after a few seconds, and respect reduced-motion.

**Why this priority**: Affects every other story. Without consistent feedback, users won't know whether their actions succeeded.

**Independent Test**: Force a 500 from any endpoint while attempting an action → assert a single error toast appears with a meaningful message → wait 5 s → assert it auto-dismisses → trigger two successes in quick succession → assert toasts stack and dismiss in order.

**Acceptance Scenarios**:

1. **Given** the server returns 4xx or 5xx for any user action, **When** the response is received, **Then** an error toast appears with a user-readable message.
2. **Given** a user successfully creates a group, schema, or resource, **When** the action completes, **Then** a success toast appears matching the action (e.g. "Group created", "Schema saved", "Resource created").
3. **Given** several toasts are visible at once, **When** more are triggered, **Then** they stack visually (up to a sensible limit) and dismiss in FIFO order; user can dismiss manually.

---

### Edge Cases

- **Network hiccup mid-save**: schema save fails on transient network error → dirty state remains, toast surfaces a retryable error, no data loss.
- **Concurrent owner edits**: two browser tabs editing the same schema → last write wins; the silent loser sees no warning in this iteration (acceptable for owner-only operations; documented as known gap).
- **Empty group**: detail page renders all four panels even when there are zero schemas / resources / members other than the owner; each panel shows a friendly empty state.
- **Schema with zero fields**: user can save a schema with no fields (used as a placeholder) — preview shows the empty-state hint.
- **Resource creation against a deleted schema**: race between two tabs — one deletes the schema while the other has its create modal open. On submit, server returns an error; toast explains the schema no longer exists; modal closes.
- **Browser back from Schema Designer with dirty state**: triggers the same confirmation as Cancel.
- **Refresh (F5) on a `/groups/:id` URL**: page reloads, fetches the group again, all panels re-hydrate (no stale state).
- **Resource list reorder during scroll**: if a new resource is created elsewhere while infinite-scroll is running, cursor-based paging guarantees no duplicates and no skipped rows.
- **prefers-reduced-motion**: every animation (modal enter, fadeUp panels, draftPulse, toast slide) is reduced to ~0ms duration so the UI still renders correctly.
- **Owner-only buttons hidden for members**: Edit, Delete (group), Add schema, Add resource, Add member — none appear for non-owner users.
- **Member list excludes the owner duplication**: the owner appears at the top of the members list with an "Admin" badge; rest sorted by join date.

## Requirements *(mandatory)*

### Functional Requirements

**Group lifecycle and listing**

- **FR-001**: The system MUST allow a signed-in user to create a new group by providing a name (1–100 characters, required) and optionally a description (0–500 characters), and MUST automatically associate the new group with the calling user as its owner.
- **FR-002**: The system MUST prevent the same user from owning two groups with the same name (case-insensitive trimmed comparison) and MUST surface this conflict both as an inline error in the create / edit modal and as a 409-equivalent error on the API.
- **FR-003**: The system MUST allow the group owner to update the name and description of a group, applying the same validation rules as creation.
- **FR-004**: The system MUST allow the group owner to delete a group; deletion is a soft-delete on the group itself, soft-delete on all of its resources, and hard-delete on all of its schemas (the schema definition is recoverable by recreation).
- **FR-005**: The system MUST navigate the user to the new group's detail page immediately after successful creation, and MUST surface a success notification on arrival.
- **FR-006**: The system MUST make group rows on the dashboard clickable, navigating to the corresponding group detail page.

**Group detail page composition**

- **FR-007**: The group detail page MUST present four stacked panels in this order: (1) group header with avatar, name, description, meta chips (creation date, member count, resource count), and owner-only Edit / Delete buttons; (2) Schemas panel with a list of schemas and an owner-only "Add schema" button; (3) Resources panel with a list of resource instances and an owner-only "Add resource" button; (4) Members panel listing owner first then members.
- **FR-008**: The group's avatar color MUST be deterministic based on the group's identifier (so the same group renders the same color everywhere).
- **FR-009**: Owner-only buttons (Edit, Delete, Add schema, Add resource) MUST be hidden when the viewer is not the owner of the group.
- **FR-010**: The Add-member affordance MUST be visible in this iteration but disabled with an explanatory tooltip ("Coming soon").

**Schema design**

- **FR-011**: The system MUST allow the group owner to create a new schema for a group by providing a name (required, unique within the group, case-insensitive trimmed) and optional description, then defining zero or more property fields where each field has a name, a type (Text, Number, or Yes/No), and a required flag.
- **FR-012**: The system MUST allow the group owner to update an existing schema (name, description, property fields), applying the same uniqueness constraint within the group.
- **FR-013**: The system MUST present the schema editor as a full-page route with two side-by-side panels (form on the left, live form preview on the right) that collapses to a single column on narrow viewports.
- **FR-014**: The system MUST update the live preview in real time as the form is edited, including when fields are added, removed, renamed, retyped, toggled required, or reordered.
- **FR-015**: The system MUST support drag-and-drop reordering of property fields in the schema editor and reflect that order in both the form and the live preview.
- **FR-016**: The system MUST create new fields with type Number by default (no type-picker on the "Add field" button); type changes happen on the existing field row.
- **FR-017**: The system MUST surface unsaved changes via a draft badge in the editor (states: "No changes", "Unsaved changes" with a pulsing indicator, "Saved" briefly after success).
- **FR-018**: The system MUST prompt the user to confirm discarding unsaved changes when they try to leave the Schema Designer (Cancel button, in-app navigation, browser back, or page refresh).
- **FR-019**: The system MUST allow the group owner to delete a schema; deletion is cascading: the schema is hard-deleted and all of its resources are soft-deleted. The confirmation dialog MUST state the number of affected resources.

**Resource lifecycle (this iteration)**

- **FR-020**: The system MUST allow the group owner to create a new resource instance bound to one of the group's schemas, with a name, optional description, and one value per property defined by the schema; required fields MUST be validated client-side and server-side.
- **FR-021**: The system MUST offer two ways to start resource creation: (a) a global "Add resource" button in the Resources panel which opens the modal with a schema selector at the top; (b) a per-schema action in the Schemas panel which opens the same modal with the schema pre-selected and shown as a read-only chip.
- **FR-022**: The system MUST display a resource's owning schema name (the "Type") as a column in the Resources list.
- **FR-023**: The system MUST display a status indicator on each resource row labeled "Available (mocked)" in this iteration; no underlying status data is persisted.
- **FR-024**: Edit, Delete, and Detail views for an individual resource are OUT of scope for this iteration.

**Listing, pagination, and infinite scroll**

- **FR-025**: The system MUST paginate the Resources list and the Members list using cursor-based pagination (a cursor + a page size), suitable for forward-only infinite scroll.
- **FR-026**: The system MUST automatically request the next page when the user scrolls near the bottom of the list, and MUST stop requesting once the server reports no more items.
- **FR-027**: The Schemas list is small by nature and MUST be fetched in a single request (no pagination required); however the response MUST be capped at a reasonable maximum to protect the client.
- **FR-028**: The pagination response MUST be stable in the face of concurrent inserts and deletes: scrolling MUST NOT show duplicates or skip rows.

**Notifications**

- **FR-029**: The system MUST provide a single notification mechanism (toast) that supports success, error, and info variants; toasts auto-dismiss after roughly five seconds and stack visually.
- **FR-030**: All server errors (any non-success HTTP status) MUST surface through the toast mechanism with a user-readable message; if the server provides a specific message, it MUST be used; otherwise a generic fallback per status family is acceptable.
- **FR-031**: All success outcomes for the flows covered by this spec (group / schema / resource create, update, delete) MUST surface a confirming toast.
- **FR-032**: Toasts MUST respect `prefers-reduced-motion` and degrade to near-instant transitions in that mode.

**Localization and animations**

- **FR-033**: All user-facing strings introduced by this feature MUST be in English. (The source design uses Polish; copy MUST be translated.)
- **FR-034**: The system MUST honor the motion guidelines from the design package (consistent easing, staggered fade-up entries for the four panels, modal slide-and-scale enter, draft-pulse for unsaved badge), and MUST collapse all animations to near-zero duration when `prefers-reduced-motion: reduce` is active.

**Authorization and visibility**

- **FR-035**: All endpoints introduced by this feature MUST require an authenticated user; the system MUST reject unauthenticated requests with the standard 401-equivalent.
- **FR-036**: All mutating endpoints (create / update / delete on groups, schemas, resources) MUST authorize the caller as the group owner; non-owner attempts MUST be rejected with the standard 403-equivalent.
- **FR-037**: All read endpoints (list / get group, schemas, resources, members) MUST require the caller to be either the group owner or a member of the group.

### Key Entities

- **Group**: A named container owned by exactly one user, with an optional description, a creation timestamp, a soft-delete timestamp, a set of members, a set of schemas, and a set of resources. Has a deterministic display color derived from its identifier. Name is unique per owner.
- **Group Member**: A user-to-group association recording join date. The owner is implicitly a member with the "Admin" role; explicit non-owner members are plain members.
- **Schema (Resource Type)**: A named template within a group, with optional description and an ordered list of property definitions. Name is unique within its group. Hard-deleted together with all its resources when the group or the schema itself is deleted.
- **Property Definition**: One field of a schema, identified by name, type (Text / Number / Yes-No), required flag, and order.
- **Resource (Resource Instance)**: A concrete item belonging to one schema within one group, with a name, optional description, and one stored value per property definition of its schema. Soft-deleted when its schema or its group is deleted. Has a presentation-only "Available (mocked)" status in this iteration.
- **Notification (Toast)**: A transient client-side message with a kind (success / error / info), a body string, and an auto-dismiss timer.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A signed-in user can go from clicking "Create new group" on the dashboard to seeing the new group's detail page in **under 5 seconds** on a typical broadband connection, with no manual page reloads.
- **SC-002**: A user can complete the full happy path (create group → add a schema with three fields → create a resource of that schema) in **under 90 seconds** during usability testing with no prior demo, demonstrating that the flow is self-explanatory.
- **SC-003**: At least **95 % of resource lists with more than one page** scroll seamlessly: the next page of results appears before the user reaches the end of the visible list, with no spinner-blocking interaction.
- **SC-004**: Attempting to create or rename a group or schema with a duplicate name produces a visible, accurate error within **one second** of the user pausing typing, in **100 % of attempts**.
- **SC-005**: Deleting a group containing N schemas and M resources removes them from the user-facing lists in a single action with a single confirmation; **0 orphaned schemas or resources** remain visible to any user.
- **SC-006**: Every failed action surfaces exactly one notification (no duplicates, no silent failures); audited across all flows defined by this spec.
- **SC-007**: A user running with `prefers-reduced-motion: reduce` can complete every flow with no visually distracting animation, and visual layout remains identical (no flicker, no missing entries).
- **SC-008**: Translation completeness: zero Polish strings appear anywhere in the new views, verified by a sweep of all introduced templates.
- **SC-009**: A non-owner viewing a group sees **zero** mutating buttons (Edit, Delete, Add schema, Add resource); audited via UI snapshots of one member-only view.
- **SC-010**: Refreshing the page on `/groups/:id` or on the Schema Designer route restores the same view with the same data, with **no broken-state screen** in any tested case.

## Assumptions

- The user is already signed in. Sign-in / sign-up flows, password resets, and account settings remain unchanged.
- The user's browser is a modern evergreen browser supporting `IntersectionObserver`, CSS custom properties, modern flex/grid, and `backdrop-filter` (the visual treatment can degrade gracefully on browsers without backdrop-filter but layout MUST remain correct).
- Mobile support follows the same responsive breakpoints as the design package; tablet/desktop are the primary target.
- The existing dashboard layout, animated background, and authentication infrastructure are reused; this feature does not modify any auth code.
- The existing `GroupCreated`, `GroupMemberAdded`, `GroupMemberRemoved`, `GroupDeleted` events in the shared abstractions are sufficient; no new cross-module events are required.
- "Available (mocked)" status is a UI-only label this iteration; no schema/value persistence is required. A future iteration will introduce real resource status.
- Adding members to a group, editing or deleting individual resources, browsing a single resource's details, and restoring a deleted group are explicitly out of scope.
- Notification messages can be hard-coded (no i18n catalogue) — translations beyond English are out of scope.
- The system's existing notion of "group owner" is treated as "admin" for the purposes of these UI flows; no separate roles system is introduced.
- Performance budgets follow the project's existing frontend bundle budgets and backend integration test timing — no specific new SLOs are introduced.

## Out of Scope

- Adding new members to a group via UI (button is visible but disabled with "Coming soon").
- Editing or soft-deleting an individual resource instance.
- A dedicated resource detail page.
- A real resource status field and lifecycle (booked / under maintenance / available).
- Restoring a soft-deleted group from the UI.
- Filtering / sorting / searching within the resources or members panels.
- Role-based access control beyond owner-vs-member.
- Bulk operations on resources or members.
- Internationalization beyond English.
