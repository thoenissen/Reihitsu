---
name: gh-apply-review
description: >-
  Apply review feedback to a Reihitsu GitHub Pull Request in the PR author's Claude chat after another party reviewed it. Use for requests such as "apply the review", "address the review comments", "work through the PR feedback", or "fix the review findings". Build a worklist from open reviewer findings, user-authored PR hints, a pasted gh-review Copy block, and chat context; implement actionable fixes under CLAUDE.md; install the .NET 10 SDK; run the required validation; push the existing PR branch; and reply to addressed threads without resolving them. Ask before acting on ambiguous or architecturally significant feedback. Never search for or create follow-up issues; every review item remains attached to the current PR. This is the fix step between gh-review and gh-rereview and runs in a Linux Claude Code Cloud Agent through the GitHub MCP server.
---

# Reihitsu GitHub PR Apply Review

You take a Pull Request that has a review on it and **do the work**: implement the reviewer's findings and the user's extra hints, validate, and push. You are the **fix** step of the loop, and you run in the **PR author's chat** — the same session that ran `gh-implement` to build this PR:

```
gh-review   →   gh-apply-review   →   gh-rereview
 (find)            (fix)                (re-check)
reviewer's chat    THIS chat            reviewer's chat
                 (gh-implement author)
```

The review, the re-review, and the finding verdicts happen in the **reviewer's** chat. You are on the author's side: this chat created the PR (via `gh-implement`), the reviewer is a **different party**, and you are acting on the comments they left. You own the implementation and the branch. You do **not** own the verdict on whether a finding is resolved — that is `gh-rereview`'s job in the reviewer's chat. So you fix and reply; you never resolve a thread yourself.

You are running inside a **Linux** Claude Code Cloud Agent environment. The repository checkout is present; the .NET SDK and the `gh` CLI are not.

## Inputs

The PR identifier is **optional**. Resolve it in this order:

1. An explicit id in the invoking prompt or `$ARGUMENTS` (`123`, `#123`, or a PR URL) — always wins.
2. Otherwise, the PR **this chat is building** — the draft PR `gh-implement` created earlier in this session. This is the normal case: the author runs `gh-apply-review` in the same chat that produced the PR, so the number does not need repeating. If the chat produced several PRs, use the most recent and name it in your first line of output so the user can correct you.

Only when both are empty — no id given and no PR from this chat's `gh-implement` run — stop and ask. Never guess a number.

## GitHub access — MCP only, no `gh` CLI

GitHub platform calls go through the **GitHub MCP server** (`mcp__github__*`); if the tools are not loaded, surface them with `ToolSearch` first (e.g. `github pull request review`, `github add reply`). Local `git` still handles branch/commit/push. Never shell out to `gh` or `curl` the REST API.

| Purpose | MCP tool |
|---|---|
| Confirm identity — this chat's account authored the PR; the reviewer is someone else | `mcp__github__get_me` |
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

Gather every open item that this PR is expected to act on, from three sources, and dedupe them. Call `get_me` first so you know which account is *you* (the author): review findings come from **other** accounts, and the user's own PR comments are hints, not findings.

1. **Reviewer findings** — the review left on the PR by the other party: the inline review-thread comments (`get_review_comments`) and any review-summary comments authored by an account **other than** `get_me`. Skip any whose thread is already resolved *and* whose fix is already in the head — those are done. Each surviving finding is a worklist item with its file, line, severity, and the change it demands.
2. **User hints** — extra guidance from the user driving this chat: hints passed directly in the chat, a `gh-review` **Copy block** pasted in, and PR comments the user authored themselves (the `get_me` account). Treat these as first-class worklist items. A user hint that contradicts a reviewer finding wins — the user is steering.
3. **In-chat context** — any hints or `gh-review` findings the user pasted into this chat that never became GitHub comments.

If the worklist is empty (no reviewer findings, no hints), stop and say so — there is nothing to apply. If the PR has no review at all yet, say that too rather than inventing work.

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
- Stay in scope. A finding is a licence to fix *that* problem, not to refactor around it. Keep out-of-scope concerns in the current PR worklist and report; never move them to a new issue.

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
| 1 | reviewer | Reihitsu.Cli/Program.cs:120 | Out of scope — remains recorded on this PR |

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
- **Never** search for or create a follow-up issue. Every review item remains attached to the current PR.
- **Never** reach for the `gh` CLI or a raw GitHub API call — use the GitHub MCP server.
