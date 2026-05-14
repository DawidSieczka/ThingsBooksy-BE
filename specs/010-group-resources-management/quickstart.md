# Quickstart — feature 010

End-to-end manual verification script after implementation. Each step lists the user action, the expected UI / network behaviour, and the implicit acceptance criteria.

## Prerequisites

- Backend running locally (`dotnet run --project backend/src/Bootstrapper/ThingsBooksy.Bootstrapper`) or via `wsl docker compose up --build`.
- Frontend running locally (`cd frontend && npm start` at `localhost:4200`).
- A test user account; sign up if needed.

## Happy path

### 1. Sign in and reach the dashboard
- Action: open `localhost:4200`, sign in.
- Expected: redirected to `/dashboard`. Admin Panel section visible with a primary "Create new group" button and (initially) no groups listed.

### 2. Open the Create Group modal
- Action: click *Create new group*.
- Expected: modal opens with `modalEnter` animation (overlay fades, card slides up and scales). Focus is in the name field. Footer has *Cancel* and *Create group* buttons (Create disabled while form invalid).

### 3. Async name availability
- Action: type "Marketing Team".
- Expected after ~500 ms: a green check + "Name is available" under the field. The FE called `GET /management-groups/name-available?name=Marketing%20Team` exactly once (debounce). Type more characters → another call is fired after typing pauses.

### 4. Submit the form
- Action: click *Create group*.
- Expected: spinner inside the button briefly, modal closes, toast "Group created" appears bottom-right, URL navigates to `/groups/{newGroupId}`. The group detail page renders four panels in stagger: header (with the new group's name, owner-only Edit/Delete), Schemas (empty state), Resources (empty state), Members (owner row with "Admin" badge).

### 5. Add a schema
- Action: in the Schemas panel click *Add schema*.
- Expected: full-page navigation to `/groups/{id}/schemas/new`. Schema Designer renders two columns; left is the form with name field focused, right is an empty Live preview ("Add the first field to see the preview"). Draft badge reads "No changes".

### 6. Build a schema
- Action: type schema name "Camera"; click *Add field*; rename "Field 1" to "Serial number"; change type from Number to Text; tick Required; click *Add field*; rename to "Year"; leave as Number; click *Add field*; rename to "In stock"; switch type to Yes/No.
- Expected: Live preview shows three input rows in order, with required markers and the correct input types. Draft badge changes to "Unsaved changes" with pulsing dot after the first edit.

### 7. Reorder fields
- Action: drag the third row ("In stock") above the first.
- Expected: rows reorder both on the form and on the live preview. Drag handle accepts keyboard reordering (`Tab` to it, `Space` to lift, arrow keys to move). Reduced-motion users see no flicker.

### 8. Try a duplicate name
- Action: change the name to "Marketing Team" (or to a literal repeated name like keeping "Camera" then attempting Add schema with the same name later — for this step, just verify the local check by triggering the spec scenario).
- Expected: client-side inline error "A schema with this name already exists" (synchronous comparison against schemas already in the active group context — for the *first* schema, this won't fire; trigger by adding a second schema in step 11).

### 9. Save the schema
- Action: click *Save schema* (bottom-right of the page).
- Expected: button shows a spinner; on 200/201 the draft badge changes to "Saved ✓" briefly, toast "Schema saved" appears, route navigates back to `/groups/{id}`. The new "Camera" schema now appears in the Schemas panel.

### 10. Discard-unsaved-changes guard
- Action: re-open the schema in edit mode, change a field name, then click the navbar logo *or* press the browser back button.
- Expected: `ConfirmDialog` appears: "You have unsaved changes. Discard them?". Cancel keeps you on the page; OK navigates and discards.

### 11. Add a resource via global button
- Action: back on `/groups/{id}`, click *Add resource* in the Resources panel header.
- Expected: modal opens with a "Resource type" dropdown listing "Camera". After choosing "Camera", three dynamic fields appear (Serial number text required, Year number, In stock yes/no). Fill in valid values; submit. Toast "Resource created"; modal closes; the row appears in the Resources panel with `Type: Camera`, status badge "Available (mocked)".

### 12. Add a resource via per-schema action
- Action: hover over the "Camera" row in the Schemas panel; a small "+" icon appears. Click it.
- Expected: same modal opens but the schema picker is a read-only chip "Camera" (no dropdown), fields ready below.

### 13. Infinite scroll
- Action: seed 25 resources (re-execute step 11 / 12 — or use a DB seed helper), then scroll the Resources panel.
- Expected: first 20 are visible from the start, second 5 fetched and appended as you scroll near the bottom. Network tab shows two calls: one without `afterId`, one with `afterId=<last Id from page 1>`. No spinner blocks the UI.

### 14. Edit the group
- Action: click *Edit* in the group header.
- Expected: same Create-style modal opens with title "Edit group" and fields pre-filled. Change the description, save. Toast "Group updated"; header reflects the change.

### 15. Delete the group with cascade
- Action: click *Delete* in the header. Confirmation dialog: "Delete this group? It will also delete 1 schema and 5 resources." Click *Delete*.
- Expected: toast "Group deleted"; redirect to `/dashboard`; the group no longer appears in the Admin Panel.

## Negative path quick checks

- **Duplicate group name** (two browser tabs): submit the same name in two tabs near-simultaneously. The slower one shows toast "You already own a group with this name." (HTTP 409, error envelope's `message` field surfaced).
- **Schema duplicate name**: in step 11, name another schema "Camera" — inline error before submit.
- **Resource creation against deleted schema**: in one tab open the Add Resource modal preselected with "Camera"; in another tab, delete the "Camera" schema. Submit in the first tab. Expected: toast "This schema no longer exists" (or generic "Server rejected the request"), modal closes.
- **Server down**: stop the BE during step 4; expected: toast "Something went wrong" (generic 5xx fallback message from interceptor), modal stays open so the user can retry.
- **prefers-reduced-motion**: in browser dev-tools toggle the simulation, repeat steps 4 / 5 / 9. Expected: no animation jitter; modal/page transitions are near-instant; draft badge does not pulse.
- **Non-owner viewer**: log in as a different user that's a member of the group, navigate to `/groups/{id}`. Expected: Edit / Delete (header), Add schema, Add resource buttons are all hidden. Add member button is hidden. Lists are readable.

## Backend integration test seeds

The `integration-test-writer` agent will produce (at minimum) these tests:

1. `CreateManagementGroup_DuplicateNameSameOwner_Returns409`
2. `UpdateManagementGroup_DuplicateNameAmongOthers_Returns409`
3. `CreateManagementGroup_DuplicateNameDifferentOwner_Succeeds`
4. `IsGroupNameAvailable_AvailableName_ReturnsTrue`
5. `IsGroupNameAvailable_TakenName_ReturnsFalse`
6. `IsGroupNameAvailable_NameOfSoftDeletedGroup_ReturnsTrue`
7. `GetGroupMembers_FirstPage_IncludesOwnerFirst`
8. `GetGroupMembers_CursorPagination_NoDuplicatesAcrossPages`
9. `GetGroupMembers_NonMemberCaller_Returns403`
10. `GetResourceInstances_CursorPagination_StableUnderConcurrentInserts`
11. `CreateResourceType_DuplicateNameInGroup_Returns409`
12. `CreateResourceType_SameNameDifferentGroup_Succeeds`
13. `DeleteManagementGroup_CascadesToResourceInstancesAndTypes`
14. `GroupDeletedHandler_RedeliveredEvent_IsNoOp`
