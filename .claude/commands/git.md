---
model: claude-sonnet-4-6
---

Scan current branch changes, propose a commit message, then commit and optionally push.

---

**Step 1 — Gather changes**

Run in parallel:
- `git status` — list all changed/staged/untracked files
- `git diff HEAD` — full diff of staged and unstaged changes
- `git log main..HEAD --oneline` — commits ahead of main (context for what's already committed)
- `git log --oneline -5` — recent commit history (style reference)

If `git status` shows no changes at all, inform the user and stop.

---

**Step 2 — Propose commit message**

Analyze the diff thoroughly. Focus on:
- **What** changed functionally (not just file names)
- **Type**: `feat`, `fix`, `refactor`, `docs`, `chore`, `test`, `style`, `ci`
- **Why** — if inferable from code context

Propose a commit message in conventional commits format:
- **Subject line**: `<type>: <summary>` — max 72 chars, imperative mood, no trailing period
- **Body** (optional): 2–3 lines max, only when the "why" isn't obvious from the subject

Present the proposed message clearly in a code block. Invite the user to approve or suggest corrections.

---

**Step 3 — Iterate on feedback**

Accept corrections and re-present the updated message. Repeat until the user explicitly approves.

---

**Step 4 — Format and commit**

1. If any `.cs` files changed, run `dotnet format` first (project rule).
2. Stage only the files shown in `git status` as modified/new — prefer explicit file names over `git add -A`.
3. Commit with the approved message, always appending:
   `Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>`
4. Run `git status` to confirm the commit succeeded.

---

**Step 5 — Push?**

Ask the user whether to push: `git push`
- **Yes** → run `git push`, confirm result
- **No** → end
