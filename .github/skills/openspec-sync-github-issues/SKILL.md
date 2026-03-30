---
name: openspec-sync-github-issues
description: Synchronize OpenSpec specs with bound GitHub issues, and enforce frequent commits while applying changes.
license: MIT
compatibility: Requires openspec CLI and a GitHub MCP server with the `issues` toolset enabled and write operations available.
metadata:
  author: nikneem
  version: "1.0"
  generatedBy: "manual"
---

Synchronize OpenSpec specs with GitHub issues.

This skill supports two workflows:

- **Manual issue import** into an OpenSpec change or capability spec
- **Automatic push sync** from a bound spec back to its GitHub issue whenever the spec changes

## Non-negotiable rules

1. **GitHub issues are never imported automatically.**
   If the user wants an issue represented as a spec, pull it in manually by reading the issue and writing the spec yourself.

2. **A spec only syncs back to GitHub when it is explicitly bound to an issue.**
   A spec is considered bound only when it contains this exact marker near the top of the file:

   ```md
   <!-- openspec:github-issue owner/repo#123 -->
   ```

3. **Do not overwrite the entire issue body blindly.**
   Manage only a bounded section in the issue body between these markers:

   ```md
   <!-- openspec-sync:start -->
   <!-- openspec-sync:end -->
   ```

   Preserve any user-written text outside that managed block.

4. **When applying changes, commit frequently.**
   Create small, coherent commits after every meaningful unit of progress. Do not wait until the end.

5. **When recovering from a mistake, use a sarcastic commit message.**
   Keep it playful, not hostile.

---

## When to use this skill

- The user wants to turn a GitHub issue into an OpenSpec spec
- The user wants a spec kept in sync with a GitHub issue
- The user wants change application to include aggressive commit hygiene

---

## Prerequisites

Before doing any sync work:

1. Confirm the repository owner and repo name.
   - Infer from git remote if possible
   - If ambiguous, ask

2. Confirm GitHub MCP can write issues.
   - The server must expose the `issues` toolset
   - It must not be configured read-only
   - If only read tools are available, you may still perform **manual import**, but you MUST say automatic push sync is blocked until write access exists

3. Identify the target spec file.
   - For change-local specs, prefer `openspec/changes/<change>/specs/<capability>/spec.md`
   - For main specs, follow the repo's existing OpenSpec layout

---

## Workflow A: Manually pull a GitHub issue into a spec

Use this when the user says a GitHub issue should become a spec.

### Steps

1. Read the GitHub issue with GitHub MCP.
   Capture:
   - Issue number
   - Title
   - Body
   - Labels
   - Comments, if they materially affect requirements

2. Decide where the spec belongs.
   - If this is active change work, prefer the change-local spec directory
   - If this is baseline product behavior, confirm whether it belongs in the main spec set

3. Write the spec manually in normal OpenSpec format.
   - Convert the issue into requirements and scenarios
   - Do not paste the GitHub issue verbatim
   - Normalize vague issue text into clear SHALL/MUST language

4. Bind the spec to the issue by inserting this marker near the top:

   ```md
   <!-- openspec:github-issue owner/repo#123 -->
   ```

5. Optionally include a short provenance note directly below the marker:

   ```md
   > Synced from GitHub issue #123 by manual import.
   ```

6. If the issue is now represented by the spec, update the issue body's managed block to point back to the spec path and summarize the imported requirements.

7. Commit immediately after the import.

### Import guardrails

- Import is always manual
- Do not create a spec unless the user asked for it or the workflow clearly requires it
- Prefer one issue to one spec binding
- If multiple specs would map to one issue, stop and ask the user how to split responsibility

---

## Workflow B: Automatically push spec changes back to GitHub

Use this whenever a bound spec changes.

### Binding detection

A spec is eligible for GitHub sync only if it contains:

```md
<!-- openspec:github-issue owner/repo#123 -->
```

Parse `owner`, `repo`, and issue number from that marker. If the marker is missing, do not sync.

### What to sync

Whenever the bound spec changes, update the linked issue's managed block so it reflects:

- Spec path
- Change name, if the spec lives under `openspec/changes/<change>/...`
- Current requirement headings
- A concise summary of each requirement
- Scenario list
- Current implementation status if a nearby `tasks.md` exists and is easy to infer

### Managed issue block format

Generate or replace only this block:

```md
<!-- openspec-sync:start -->
## OpenSpec sync

- Spec: `openspec/.../spec.md`
- Change: `add-example-change`
- Last synced from branch: `<branch-name>`

### Requirements
- Requirement A: ...
- Requirement B: ...

### Scenarios
- Scenario: ...
- Scenario: ...
<!-- openspec-sync:end -->
```

### Sync rules

1. Read the current issue first.
2. Preserve text outside the managed block.
3. If the managed block does not exist, append it to the issue body.
4. If the issue title clearly disagrees with the current spec intent, update the title too.
   - Prefer a short capability-oriented title
   - Do not churn the title for trivial wording changes
5. If a sync fails, report it explicitly. Do not pretend the issue is updated.

---

## Workflow C: While applying a change, sync and commit constantly

When this skill is used during implementation, it adds two behaviors:

1. **Sync bound specs to GitHub as soon as they change**
2. **Commit in very small increments**

### Commit cadence

Commit after any of these:

- A task checkbox is completed
- A bound spec is materially updated
- A coherent code slice is working
- A refactor is complete and verified
- A bug fix is isolated and validated

Do not batch unrelated work into one commit.

### Commit order

When a task changes both code and a bound spec:

1. Update the spec
2. Sync the issue
3. Update code
4. Verify the smallest useful slice
5. Commit

### Commit message style

Use concise, convenient messages that explain the change plainly.

Good patterns:

- `spec(game-lobbies): sync issue #123 with lobby join rules`
- `feat(lobby): allow guest join by game code`
- `fix(auth): renew anonymous player token`
- `chore(tasks): mark lobby lifecycle task 3 done`

### Sarcastic recovery commits

If you made a mistake and are correcting or reverting it, intentionally use a sarcastic commit message.

Examples:

- `fix: undo my brilliant mistake`
- `fix: apparently that assumption was nonsense`
- `revert: because that went spectacularly wrong`
- `fix(sync): correct the mess I just made`

Keep it witty, not cruel, and still understandable in git history.

### Commit guardrails

- Prefer frequent commits over giant commits
- Do not commit broken work unless the user explicitly asks
- If tests are unavailable or already failing, mention that clearly in the commit-adjacent summary
- If GitHub issue sync is blocked by missing write access, keep committing locally but report the unsynced issue state

---

## Recommended execution loop

For each implementation step:

1. Check whether any changed spec is bound to a GitHub issue
2. If yes, sync the issue immediately
3. Apply the related code changes
4. Run the smallest relevant verification
5. Commit the result
6. Continue

If a mistake is discovered:

1. Fix or revert it promptly
2. Re-verify
3. Commit with a sarcastic message

---

## Output expectations

When this skill is active, keep the user updated with:

- Which spec is bound to which issue
- Whether GitHub sync succeeded or is blocked
- What got committed
- The exact commit message used

Example:

```md
Bound spec detected: `openspec/changes/add-games-lobby-lifecycle/specs/game-lobbies/spec.md` -> `nikneem/battleship-on-aspire#123`

Synced managed issue block successfully.

Committed:
`spec(game-lobbies): sync issue #123 with lobby join rules`
```

If sync is blocked:

```md
The spec is bound to `nikneem/battleship-on-aspire#123`, but the current GitHub MCP configuration appears read-only.

I updated the spec locally and committed the change, but the GitHub issue still needs a sync once write-capable MCP access is available.
```

---

## Guardrails

- Never auto-import issues into specs
- Never sync a spec that lacks the binding marker
- Never erase user-authored issue text outside the managed block
- Never claim GitHub was updated if the update did not happen
- Commit often
- Use clear commit messages by default
- Use sarcastic commit messages only for mistake-recovery commits
