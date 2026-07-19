---
name: gh-apply-review
description: Work through the review on a Reihitsu GitHub Pull Request — implement the reviewer's findings and any extra hints the user posted, then push. Triggers on "apply the review", "address the review comments", "work through the PR feedback", "fix the review findings", or any prompt asking to act on review comments left on a PR. Runs in a Linux Claude Code Cloud Agent environment. GitHub access goes through the GitHub MCP server (`mcp__github__*`); local git handles branch/commit/push; the `gh` CLI is not installed. It builds a worklist from the reviewer's open inline and general findings plus extra hints the user posted (as PR comments, as a gh-review Copy block, or in chat), implements each actionable fix following the repository workflow in CLAUDE.md, installs the .NET 10 SDK and runs the four test projects, keeps CI quiet with [skip ci] until the final trigger commit, and replies once per addressed thread without resolving it (resolution belongs to gh-rereview). Ambiguous or architecturally significant findings are confirmed with the user before editing. Reports a compact Applied / Skipped / Needs-decision worklist in chat. This is the middle step of the loop: gh-review -> gh-apply-review -> gh-rereview.
---

# Reihitsu GitHub PR Apply Review

You take a Pull Request that has a `gh-review` pass on it and **do the work**: implement the reviewer's findings and the user's extra hints, validate, and push. This is the middle step of the loop:

```
gh-review  →  gh-apply-review  →  gh-rereview
 (find)          (fix)              (re-check)
```

You own the implementation and the branch. You do **not** own the verdict on whether a finding is resolved — that is `gh-rereview`'s job. So you fix and reply; you never resolve a thread yourself.

You are running inside a **Linux** Claude Code Cloud Agent environment. The repository checkout is present; the .NET SDK and the `gh` CLI are not.

## Inputs

The PR identifier is **optional**. Resolve it in this order:

1. An explicit id in the invoking prompt or `$ARGUMENTS` (`123`, `#123`, or a PR URL) — always wins.
2. Otherwise, the PR already under discussion **in this chat** (the one `gh-review` / `gh-rereview` ran on). This is the normal case. If several were reviewed, use the most recent and name it in your first line of output.

Only when both are empty, stop and ask. Never guess a number.

## GitHub access — MCP only, no `gh` CLI

GitHub platform calls go through the **GitHub MCP server** (`mcp__github__*`); if the tools are not loaded, surface them with `ToolSearch` first (e.g. `github pull request review`, `github add reply`). Local `git` still handles branch/commit/push. Never shell out to `gh` or `curl` the REST API.

| Purpose | MCP tool |
|---|---|
| Confirm the reviewer identity (to tell reviewer findings from user hints) | `mcp__github__get_me` |
| PR metadata (base/head branch, current head SHA) | `mcp__github__pull_request_read` (get) |
| Reviewer inline findings (the worklist core) | `mcp__github__pull_request_read` (get_review_comments) |
| General PR comments — reviewer findings **and** user hints | `mcp__github__pull_request_read` (get_comments) |
| Current diff / changed files (context for each fix) | `mcp__github__pull_request_read` (get_diff / get_files) |
| Linked issue (`Closes/Fixes/Resolves #N`) | `mcp__github__issue_read` |
| Reply on a thread after addressing it | `mcp__github__add_reply_to_pull_request_comment` |
| General reply (non-line hint) | `mcp__github__add_issue_comment` |

## Build environment

The sandbox does not ship the .NET SDK. Before any `dotnet` command, install the latest .NET 10 SDK (the repo targets `net10.0`, no `global.json`):

```bash
dotnet --list-sdks   # probe first
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --channel 10.0 --install-dir "$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"
dotnet --list-sdks
```

Keep `$HOME/.dotnet` on `PATH` for every later `dotnet` call. If the script cannot be reached (no egress), stop and report it — a green run without the SDK is meaningless.

## Workflow

### 1. Build the worklist

Gather every open item that this PR is expected to act on, from three sources, and dedupe them:

1. **Reviewer findings** — the inline review comments and general PR comments posted by the reviewer identity (`get_me`). Skip any whose thread is already resolved *and* whose fix is already in the head — those are done. Each surviving finding is a worklist item with its file, line, severity, and the change it demands.
2. **User hints** — extra guidance the user added: PR comments the user authored (not the reviewer), a `gh-review` **Copy block** pasted anywhere, and hints passed directly in this chat. Treat these as first-class worklist items. A user hint that contradicts a reviewer finding wins — the user is steering.
3. **In-chat `hint` rows** — any `gh-review` hints still in the current chat that were never posted to GitHub.

If the worklist is empty (no open findings, no hints), stop and say so — there is nothing to apply. Point the user at `gh-review` if the PR was never reviewed.

### 2. Triage before editing

For each item decide: **fix**, **skip**, or **needs decision**.

- **fix** — actionable and unambiguous. Proceed.
- **skip** — not actually a defect, out of scope, or already handled. Record the reason; do not edit.
- **needs decision** — the finding is ambiguous (could be read several ways), contested (you believe it is wrong), or architecturally significant (a refactor, a public-API change, a new dependency). Do **not** guess. Use `AskUserQuestion` with enough context that the user can answer without scrolling, then act on the answer. Never silently pick an interpretation for a significant change.

### 3. Implement, following `CLAUDE.md`

Work on the PR's head branch. Fix items in an order that keeps commits focused (group by file/concern; a bug fix and its regression test in one commit).

Honor the repository workflow — the review found these problems *because* the workflow was skipped, so do not skip it again:

- **Analyzer or formatter bug fix** → write the failing regression/repro test **first**, watch it fail, then fix. Analyzer tests are many small focused tests, not one large multi-case test.
- **New/changed formatter behavior** → add the idempotency (double-run) and CRLF assertions the reviewer asked for.
- **New/changed code fix** → add the convergence (and FixAll where relevant) test; ship a comprehensive code fix or none.
- **Format the changed files** through the CLI before running tests:

  ```bash
  dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
  ```

- Stage only the files that belong to the review items. Never `git add -A` blindly — the sandbox may hold SDK install artifacts.
- Stay in scope. A finding is a licence to fix *that* problem, not to refactor around it. Out-of-scope cleanups the review surfaced go to a follow-up note, not this branch.

### 4. Keep CI quiet, commit, push

The repo's `SonarCloud.yml` runs on every push to a PR. To collapse the many fix commits into one CI run, end **every** commit subject with `[skip ci]` **except the final trigger commit**:

```text
Address review: preserve #endif when joining parameters (#<PR>) [skip ci]
```

After all fixes are in and validation is green (next step), push a single non-`[skip ci]` trigger commit so CI runs once:

```bash
git push
git commit --allow-empty -m "Ready for CI (#<PR>)"
git push
```

Push to the PR's existing head branch with `git push -u origin <head-branch>` (retry on network error with 2s/4s/8s/16s backoff). Do not open a new PR and do not change the PR's draft/ready state.

### 5. Validate (always, never skip)

With the SDK on `PATH`, from the repo root:

```bash
dotnet build Reihitsu.sln -c Release --verbosity minimal
dotnet test Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Core.Test/Reihitsu.Core.Test.csproj -c Release --verbosity minimal
dotnet test Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj -c Release --verbosity minimal
```

All four test projects must pass. Fix regressions your change caused (commit with `[skip ci]`). Never silence or `[Ignore]` a test to go green. If a failure is pre-existing on the base branch and independent of the review items, record it in the report and stop rather than build on a broken baseline.

### 6. Reply, do not resolve

For each **fixed** item on an inline thread, post one concise reply with `mcp__github__add_reply_to_pull_request_comment` stating what changed and the commit sha (`Addressed: guard now preserves `#endif`; regression test added (<sha>).`). For a non-line hint, reply via `mcp__github__add_issue_comment`.

**Do not resolve any thread.** Resolution is `gh-rereview`'s verified step — leaving threads open is the handshake that tells the re-review what to check. English only, concise, state what changed, no praise.

## What to write back in chat

**Only** the following block, nothing else. No preamble, no closing summary. Render every heading even when empty (`_None._`). If you resolved the PR from chat context rather than an explicit id, name it in the first line.

````markdown
## Applied
| # | Source | Location | Commit | Change |
|---|--------|----------|--------|--------|
| 1 | reviewer | Reihitsu.Formatter/Pipeline/Foo.cs:42 | a1b2c3d | Preserve `#endif` when joining parameters; regression test added |
| 2 | user hint | Reihitsu.Analyzer/Rules/RH3204/Bar.cs:88 | e4f5a6b | Split parsing out of the diagnostic method |

## Skipped
| # | Source | Location | Reason |
|---|--------|----------|--------|
| 1 | reviewer | Reihitsu.Cli/Program.cs:120 | Out of scope — pre-existing, filed follow-up |

## Needs decision
_None._

## Validation
- Build: green.
- Analyzer / Formatter / Core / Cli tests: green (SDK installed via dotnet-install.sh).

## Pushed
- Branch `claude/...`: 2 fix commits (`[skip ci]`) + trigger commit `Ready for CI (#<PR>)`.
- Replied on threads #1, #2. Threads left unresolved for gh-rereview.
````

Rules for the block:

- **Applied** lists each fixed item with the commit that carried it and a one-sentence change note.
- **Skipped** always carries a reason. Never skip silently.
- **Needs decision** lists items you raised with the user via `AskUserQuestion` and are still waiting on (or that the user deferred). If you asked and got an answer mid-run, the item moves to Applied or Skipped instead.
- **Validation** is one line per project group; state that the SDK was installed. If validation could not run, say why.
- **Pushed** names the commits and the threads you replied on, and states the threads were left unresolved.
- After the block, write **nothing**.

## Hard rules

- **Never** resolve a review thread — that is `gh-rereview`'s verified step.
- **Never** guess on an ambiguous, contested, or architecturally significant finding — use `AskUserQuestion` first.
- **Never** skip the regression-test-first / idempotency / convergence discipline in `CLAUDE.md`; the review exists because it was skipped once.
- **Never** silence, `[Ignore]`, or delete a test to make validation green.
- **Never** push a non-`[skip ci]` commit before validation is green — the empty trigger commit is the only exception.
- **Never** `git add -A` blindly, and never edit files outside the review items' scope.
- **Never** open a new PR or flip the PR's draft/ready state.
- **Never** reach for the `gh` CLI or a raw GitHub API call — use the GitHub MCP server.
