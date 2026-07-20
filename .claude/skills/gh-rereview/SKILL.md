---
name: gh-rereview
description: Re-review a GitHub Pull Request for the Reihitsu repository after the author has addressed a previous review. Triggers on "re-review PR", "rereview", "review PR again", "re-check PR #", "the review was addressed, review again", or any prompt that asks to repeat a review on a PR that already received one. Runs in a Linux Claude Code Cloud Agent environment. All GitHub interaction goes through the GitHub MCP server (`mcp__github__*`) — the `gh` CLI is not installed. It rebuilds the prior finding set from the reviewer's earlier GitHub comments (and any findings table still in the chat), re-runs the full gh-review pass on the current PR state, then reconciles: each prior finding becomes resolved or open, and anything new since the last pass becomes a new finding. It resolves threads for verified fixes, replies on still-open threads, submits every new confirmed finding in one GitHub review, and never searches for or creates follow-up issues. Re-posts the same 19-item checklist plus prior/new findings tables in chat. No praise, no chit-chat, no LGTM.
---

# Reihitsu GitHub PR Re-Review

You re-review a Pull Request that already went through a `gh-review` pass, now that the author says they addressed the findings. The job is a **delta**: confirm which reported points are fixed, which are still open, and whether the author's changes introduced new points — then re-post the same checklist.

This skill **builds on `gh-review`**. The review methodology — the 19-item checklist semantics, the adversarial input corpus, the test expectations, the severity model, the verification discipline (static-first, targeted execution only when a suspicion is checkable), the comment-body rules, and the finding-containment workflow — is defined in `.claude/skills/gh-review/SKILL.md`. Read it and apply it unchanged. This skill only adds the reconcile-and-re-post workflow on top.

You are running inside a **Linux** Claude Code Cloud Agent environment. The repository checkout is present; the .NET SDK and the `gh` CLI are not.

## Inputs

The PR identifier is **optional**. Resolve it in this order:

1. An explicit id in the invoking prompt or `$ARGUMENTS` (`123`, `#123`, or a PR URL) — always wins.
2. Otherwise, the PR already under discussion **in this chat**: the one `gh-review` or an earlier `gh-rereview` ran on in this session. This is the normal case — the user re-runs the review in the same chat after the author pushed fixes, so the number does not need repeating. If exactly one PR was reviewed in the chat, use it; if several were, use the most recently reviewed one, and name it in your first line of output so the user can correct you.

Only when both are empty — no id given and no PR identifiable from the chat — stop and ask. Never guess a number.

## GitHub access — MCP only, no `gh` CLI

Every GitHub interaction goes through the **GitHub MCP server** (`mcp__github__*`). If the tools are not loaded, surface them with `ToolSearch` first (e.g. `github pull request review`, `github add reply`). Never shell out to `gh` or `curl` the REST API by hand.

| Purpose | MCP tool |
|---|---|
| Confirm reviewer identity (to find *your own* prior comments) | `mcp__github__get_me` |
| PR metadata (title, body, base/head, current head SHA) | `mcp__github__pull_request_read` (get) |
| Current PR diff | `mcp__github__pull_request_read` (get_diff) |
| Current changed files | `mcp__github__pull_request_read` (get_files) |
| Prior inline review comments (the baseline finding set) | `mcp__github__pull_request_read` (get_review_comments) |
| Prior general PR comments | `mcp__github__pull_request_read` (get_comments) |
| Commits pushed since the last review (the delta) | `mcp__github__list_commits` |
| Linked issue (`Closes/Fixes/Resolves #N`) | `mcp__github__issue_read` |
| Reply to an existing review thread | `mcp__github__add_reply_to_pull_request_comment` |
| Resolve / reopen a review thread | `mcp__github__resolve_review_thread` / `mcp__github__unresolve_review_thread` |
| Post new findings (summary + inline) | `mcp__github__pull_request_review_write` + `mcp__github__add_comment_to_pending_review` |
| Update a legacy general finding | `mcp__github__add_issue_comment` |

## Workflow

### 1. Rebuild the prior finding set (the baseline)

The prior findings are the source of truth for "the reported points". Reconstruct them from GitHub, because a fresh session no longer has the earlier chat:

1. `mcp__github__get_me` to learn the reviewer login, so you can pick out the comments **you** posted.
2. Read the reviewer's inline review comments (`get_review_comments`) and general PR comments (`get_comments`). Each is one prior finding: capture its severity (inferred from wording), file, line, thread id, resolved-state, and the change it demanded.
3. If a `gh-review` findings table for this PR is still present in the current chat, merge it in. Merge uncertain observations from its separate Hints section too; GitHub comments remain authoritative for anything posted.

If no prior review comments exist and no prior table is in chat, this PR has not been reviewed yet — stop and tell the user to run `gh-review` first instead of guessing a baseline.

### 2. Re-run the full review on the current PR state

Fetch the current metadata, diff, changed files, and counterpart files, then walk the **complete gh-review checklist** against the current head — exactly as a first review would. Use `list_commits` to see what the author pushed since the last review and focus the adversarial tracing there, but do not treat the new commits as the only boundary: a fix in one file can break an untouched counterpart. This pass produces the **current finding set**.

### 3. Reconcile prior findings

Match each prior finding to the current code — never to the author's claim or to the thread's resolved flag alone. A thread the author resolved is a claim to verify, not evidence.

Classify each prior finding:

- **resolved** — the reported defect is genuinely gone in the current code. Confirm the fix addresses the *defect class* the original finding named, not just the one line: a finding about a missing `#if` guard is not resolved by handling comments only. A finding fixed by deleting the code, or by a change that silently reintroduces it elsewhere, is not resolved — trace it.
- **open** — not addressed, or the attempt is inadequate (wrong shape, partial, moved the bug). If the author changed the code but the defect survives, keep it `open` and say precisely what still fails.

A prior finding whose surrounding code no longer exists is `resolved` only if the concern it raised cannot recur there; otherwise re-express it against the new code as `open`.

### 4. Detect new findings

Any finding in the current set with no matching prior finding is **new** — most often something the author's own fix introduced (a regression, a new trivia gap, a convergence break). Apply the full gh-review severity model to new findings.

### 5. Act on GitHub

Reuse gh-review's posting rules (high-confidence only, English, concise, state problem + fix, no softening, no praise, dedupe against existing comments). On top of that:

- **resolved** prior finding on an inline thread → post a one-line reply confirming it is fixed, then resolve the thread with `mcp__github__resolve_review_thread`. Do not resolve a thread you have not verified.
- **open** prior finding → reply on the thread stating concisely what still fails (only when the author changed something and missed — a short reply is warranted because they believe it is done). Leave the thread unresolved; reopen it with `mcp__github__unresolve_review_thread` if the author resolved it prematurely.
- **new** finding → submit every new confirmed finding in **one** pending review (`pull_request_review_write` create → `add_comment_to_pending_review` for valid changed-line anchors → `submit_pending` once with event `COMMENT`). Put non-line, systemic, pre-existing, and out-of-scope findings in that review's summary body. Never use `add_issue_comment` for a new finding.
- Never search for or create a follow-up issue. Keep every confirmed finding in the current GitHub review, and do not demote it to a hint because of scope.

### 6. Verification

Same discipline as gh-review: static tracing by default; CI already runs the full suite. Reach for execution only when a specific reconcile question is checkable and changes a classification — e.g. "does the author's fix actually converge now?" or "is the formatter idempotent on the reshaped construct?". Then install the .NET 10 SDK via `dotnet-install.sh` and run **only** the targeted, filtered tests (or a formatter double-run) that settle it. Never run the whole suite to fill the block.

## What to write back in chat

**Only** the following block, nothing else. No preamble, no closing summary, no "Done." Render every heading even when a section is empty (`_None._`). Re-post the **same 19-item checklist** so the user sees the full picture again; mark each item against the current state.

````markdown
## Checklist
- [x] Correctness
- [x] Trivia preservation
- [x] Semantics & compilability
- [ ] Fix convergence — N/A (no code fix touched)
- [x] Idempotency & termination
- [x] Analyzer/formatter/fix parity
- [x] Defect-class closure
- [x] SRP / concern leakage
- [x] Duplication
- [ ] Other SOLID — N/A (no inheritance touched)
- [x] Coupling / cohesion
- [ ] Security — N/A (no boundary code touched)
- [x] Error handling
- [x] Tests
- [ ] Performance — N/A
- [x] Repo conventions
- [x] Naming & docs
- [x] Scope discipline
- [x] Issue coverage

## Prior findings
| # | Severity | Location | Status | GitHub | Notes |
|---|----------|----------|--------|--------|-------|
| 1 | high   | Reihitsu.Formatter/Pipeline/Foo.cs:42 | resolved | thread resolved | `#endif` now preserved; double-run clean |
| 2 | medium | Reihitsu.Analyzer/Rules/RH3204/Bar.cs:88 | open | replied | Parsing still inline in the diagnostic method — not split |

## New findings
| # | Severity | Location | Posted | Summary |
|---|----------|----------|--------|---------|
| 1 | high | Reihitsu.Analyzer.CodeFixes/RH3204Fix.cs:57 | yes | The new guard raises RH6001 in the edited span — fix no longer converges |

## Verification
- Static tracing for the RH3204 split question — CI runs the full suite.
- Installed SDK, ran `Reihitsu.Formatter.Test` filtered to `RH5xxx` to confirm Foo.cs idempotency after the fix: second pass clean.

## Hints (not posted)
_None._
````

Rules for the chat block:

- **Prior findings** lists every baseline finding with its reconciled `Status` (`resolved` / `open`). `GitHub` records the action taken (`thread resolved`, `replied`, `—`). Keep `Notes` to one sentence.
- **New findings** contains confirmed findings only. Every row must be part of the submitted GitHub review and read `yes` in `Posted`; never put hints in this table.
- Uncertain observations belong only under **Hints**, never in a table cell.
- If every prior finding is `resolved` and there are no new findings, still render all sections — Prior findings with the resolved rows, New findings as `_None._`. That is the success state, and it is the deliverable, not a no-op to skip.
- After the block, write **nothing**.

## Hard containment rules

- Never call `mcp__github__issue_write`, `mcp__github__search_issues`, `mcp__github__list_issues`, or an equivalent issue-search or issue-creation tool during re-review.
- Reading an issue explicitly linked by the PR is allowed only for issue-coverage verification; do not mutate it.
- Never move, copy, redirect, omit, or demote a confirmed finding because it is systemic, pre-existing, or broader than the diff. Keep it in the current GitHub review.
- Never post a new confirmed finding outside the submitted GitHub review; use its inline comments or summary body.
