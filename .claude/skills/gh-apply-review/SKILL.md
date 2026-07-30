---
name: gh-apply-review
description: >-
  Apply review feedback to a Reihitsu GitHub Pull Request in the PR author's Claude chat after another party reviewed it. Use for requests such as "apply the review", "address the review comments", "work through the PR feedback", or "fix the review findings". Build one complete worklist from open reviewer findings, review summary bodies, user-authored PR hints, a pasted gh-review Copy block, chat context, and prior preflight findings before editing anything; implement the accepted items as one cohesive repair cycle under CLAUDE.md; close the complete defect class; run a local self-review; synchronize origin/main; spend at most two official gh-preflight attempts; install the .NET 10 SDK; run the full validation once; push the existing PR branch; and reply to addressed threads without resolving them. Ask before acting on ambiguous or architecturally significant feedback. Never search for or create follow-up issues; every review item remains attached to the current PR. This is the fix step between gh-review and gh-rereview and runs in a Linux Claude Code Cloud Agent through the GitHub MCP server.
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

## Run order

1. Resolve the PR and read its current state.
2. Build **one** complete worklist from every source, before editing anything.
3. Classify every item exactly once: fix, skip with reason, or needs decision.
4. Implement all `fix` items as one cohesive repair cycle.
5. Run the **local self-review**.
6. Synchronize with current `origin/main`.
7. Run the **official preflight** on that exact synchronized head, inside the 1 + 1 budget.
8. Run the complete **full validation** once.
9. Push the final non-`[skip ci]` CI trigger, then reply to the addressed threads without resolving them.

## Inputs

The PR identifier is **optional**. Resolve it in this order:

1. An explicit id in the invoking prompt or `$ARGUMENTS` (`123`, `#123`, or a PR URL) — always wins.
2. Otherwise, the PR **this chat is building** — the draft PR `gh-implement` created earlier in this session. This is the normal case: the author runs `gh-apply-review` in the same chat that produced the PR, so the number does not need repeating. If the chat produced several PRs, use the most recent and name it in your first line of output so the user can correct you.

Only when both are empty — no id given and no PR from this chat's `gh-implement` run — stop and ask. Never guess a number.

## GitHub access — MCP only, no `gh` CLI

GitHub platform calls go through the **GitHub MCP server** (`mcp__github__*`); if the tools are not loaded, surface them with `ToolSearch` first (e.g. `github pull request review`, `github add reply`). Local `git` still handles branch/commit/push. Never shell out to `gh` or `curl` the REST API. Batch the independent read-only calls in one step.

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

### 1. Build the complete worklist before editing

Read **all** the feedback first. Starting to edit after the first thread is what produces half-addressed reviews, contradictory fixes, and an extra review round: the second thread often changes how the first one should be fixed.

Call `get_me` first so you know which account is *you* (the author): review findings come from **other** accounts, and the user's own PR comments are hints, not findings. Gather and dedupe from every source:

1. **Unresolved review threads** — inline findings (`get_review_comments`) authored by an account other than `get_me`. Skip a resolved thread only after confirming its fix is present at the current head. Each surviving finding carries its file, line, severity, and the change it demands.
2. **Review summary bodies** — the top-level body of each review, which often carries the findings that had no line anchor.
3. **User-authored PR hints** — PR comments the `get_me` account posted itself.
4. **Pasted review blocks** — every line of a `gh-review` **Copy block** pasted into this chat.
5. **In-chat clarifications** — guidance the user gave here that never became a GitHub comment.
6. **Prior preflight findings** — confirmed findings from an earlier official preflight in this chat that are not yet fixed.

A user hint that contradicts a reviewer finding wins — the user is steering.

If the worklist is empty (no reviewer findings, no hints), stop and say so — there is nothing to apply. If the PR has no review at all yet, say that too rather than inventing work.

### 2. Classify every item exactly once

Assign each item exactly one class: **fix**, **skip**, or **needs decision**.

- **fix** — actionable and unambiguous. It goes into this repair cycle.
- **skip** — not actually a defect, out of scope, or already handled. Record the reason; do not edit.
- **needs decision** — ambiguous (could be read several ways), contested (you believe it is wrong), or architecturally significant (a refactor, a public-API change, a new dependency). Do **not** guess. Use `AskUserQuestion` with enough context that the user can answer without scrolling, then act on the answer.

When a review item introduces a materially ambiguous **behavior** change — a different output for the same input, an anchor moved, a rule's meaning widened — the `gh-rubber-duck` workflow is the right tool to settle it before editing. Run it or recommend it; it is read-only and costs one pass. It is optional here: only `gh-implement` runs it automatically as a mandatory gate.

### 3. Implement as one cohesive repair cycle, following `CLAUDE.md`

Work on the PR's head branch and implement all `fix` items together, then validate once. Keep commits focused (group by file/concern; a bug fix and its regression test in one commit).

Before editing each accepted finding, state its general defect class and inspect sibling syntax shapes, wrappers, nested scopes, repeated-token cases, and shared helpers that can carry the same hazard. The requested counterexample is the minimum reproduction, not the implementation boundary. Regression coverage must close the relevant defect class without expanding into unrelated cleanup.

Honor the repository workflow — the review found these problems *because* the workflow was skipped, so do not skip it again:

- **Analyzer or formatter bug fix** → write the failing regression/repro test **first**, watch it fail, then fix. Analyzer tests are many small focused tests, not one large multi-case test.
- **New/changed formatter behavior** → add the idempotency (double-run) and CRLF assertions the reviewer asked for.
- **New/changed code fix** → add the convergence (and FixAll where relevant) test; ship a comprehensive code fix or none.
- **Format the changed files** through the CLI before running tests:

  ```bash
  dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
  ```

- Run the focused tests for the touched rule or phase as you go, not the full suite.
- Stage only the files that belong to the review items. Never `git add -A` blindly — the sandbox may hold SDK install artifacts.
- Stay in scope. A finding is a licence to fix *that* problem, not to refactor around it. Keep out-of-scope concerns in the current PR worklist and report; never move them to a new issue.

### 4. Keep CI quiet, commit, push

The repo's `SonarCloud.yml` runs on every push to a PR. To collapse the many fix commits into one CI run, end **every** commit subject with `[skip ci]` **except the final trigger commit**:

```text
Address review: preserve #endif when joining parameters (#<PR>) [skip ci]
```

Push to the PR's existing head branch with `git push -u origin <head-branch>` (retry on network error with 2s/4s/8s/16s backoff). Do not open a new PR and do not change the PR's draft/ready state.

The single non-`[skip ci]` trigger commit comes at the very end, after preflight and full validation are green:

```bash
git push
git commit --allow-empty -m "Ready for CI (#<PR>)"
git push
```

### 5. Local self-review

The official preflight is a final quality gate, not a discovery loop, and only two attempts are available. Walk your own change first, locally, in this agent, without another agent and without the full suite:

- **every worklist row** — for each accepted item, name the change and the test that proves it;
- **counterpart parity** — formatter output is not flagged by the analyzer, analyzer-clean code is formatter-stable;
- **defect-class closure** — sibling shapes and private copies of the changed policy carry no residual hazard;
- **convergence** — the code fix silences its own diagnostic in one pass and raises no new RH diagnostic;
- **idempotency** — a second formatter pass over the output is a no-op, on LF and CRLF;
- **comments and directives** — the relevant trivia shapes survive at sensible positions, or the edit is refused;
- **documentation** — the rule doc under `documentation/rules/` matches the shipped behavior;
- **changed-path formatting** — every changed C# path went through `Reihitsu.Cli`;
- **focused tests** — the tests for the touched rule/phase pass at the current working tree.

Fix what you find now. This is not an official preflight, does not consume a preflight attempt, and is not reported as one.

### 6. Synchronize with `origin/main` before the official gate

The audited head must be the head that will merge:

1. `git fetch origin main`.
2. Check worktree and branch state — clean of unintended changes, on the PR head branch.
3. Merge current `origin/main` into the PR branch when it is behind.
4. Resolve conflicts so that **both** the branch behavior and the `main` behavior survive.
5. Run `Reihitsu.Cli` over every conflict-resolved and changed C# path.
6. Run the focused tests affected by the merge.
7. Commit and push the synchronized head with `[skip ci]`.
8. Run the official preflight against that exact head.

If `origin/main` moves again after a passing preflight, do not enter an unlimited re-merge/re-preflight loop. Check whether another merge is actually required, state that merging again changes the audited head, and follow the user's explicit direction — including their decision to rely on CI without another preflight attempt.

### 7. Official preflight gate — hard 1 + 1 budget

After the accepted fixes are committed and pushed with `[skip ci]`, the local self-review is done, and `main` is synchronized, read `.claude/skills/gh-preflight/SKILL.md` completely and apply it as an internal, read-only gate against the current PR head. Do not post preflight findings through GitHub MCP. Run it in a fresh, independent read-only subagent when subagents are available.

The budget is fixed:

1. **Attempt 1** runs automatically on the synchronized head.
2. On `PASS`, continue to full validation.
3. On `BLOCKED — findings`, merge **every** finding into **one** consolidated worklist — together with anything still open from the review worklist. Do not fix before the worklist is complete, and do not run a preflight in between.
4. Fix the complete worklist in **one** repair cycle: close each finding's full defect class, format the changed paths, run the focused tests, redo the local self-review, then commit and push with `[skip ci]`.
5. **Attempt 2** — the preflight retry — then runs **once**, as a fresh, independent, read-only subagent against the exact new head.
6. If the retry also blocks, **stop**. Report the remaining findings to the user and let them decide. Never start a third official preflight automatically.

On `BLOCKED — state mismatch`, reconcile the checkout, commits, and PR head before rerunning; a state mismatch is a setup error, not a review result, so it does not consume an attempt.

Ask the user before acting on a preflight finding that is architecturally significant, public-API-changing, dependency-changing, contested, or unrelated to the accepted review work. Do not create the final CI-trigger commit until both preflight and full validation are green.

A tracked-file change made after a passing preflight leaves the final head unaudited. If an attempt is still unspent, use it on the new head. If the budget is exhausted, do not start another official preflight: run the local self-review and the focused tests over the change, and say so in the report.

### 8. Full validation — run it once

Focused tests run throughout the repair cycle. The complete suite runs **once**, with the SDK on `PATH`, after the fixes are in, `main` is synchronized, the official preflight has passed, and the worktree matches the audited head:

```bash
dotnet build Reihitsu.sln -c Release --verbosity minimal
dotnet test Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj -c Release --no-build --verbosity minimal
dotnet test Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj -c Release --no-build --verbosity minimal
dotnet test Reihitsu.Core.Test/Reihitsu.Core.Test.csproj -c Release --no-build --verbosity minimal
dotnet test Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj -c Release --no-build --verbosity minimal
```

`--no-build` is valid only because the Release build immediately above covered this exact tree; drop it and rebuild if any file changed since. All four test projects must pass. Fix regressions your change caused (commit with `[skip ci]`) and rerun the focused tests plus the failing project — not the whole suite. Never silence, `[Ignore]`, or delete a test to go green. If a failure is pre-existing on the base branch and independent of the review items, record it in the report and stop rather than build on a broken baseline.

If the user explicitly asks to skip repeated local validation and rely on CI, obey that instruction and report exactly which local checks ran and which did not.

### 9. Reply, do not resolve

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
- Local self-review: every worklist row checked; parity, convergence, idempotency, directives re-checked.
- Base sync: merged `origin/main` at `<sha>`; conflicts formatted and focused-tested.
- Official preflight: 1 attempt used, PASS; budget not exhausted.
- Build: green.
- Analyzer / Formatter / Core / Cli tests: green, one full run (SDK installed via dotnet-install.sh).

## Pushed
- Branch `claude/...`: 2 fix commits (`[skip ci]`) + trigger commit `Ready for CI (#<PR>)`.
- Replied on threads #1, #2. Threads left unresolved for gh-rereview.
````

Rules for the block:

- **Applied** lists each fixed item with the commit that carried it and a one-sentence change note. Include a `preflight` source row for each confirmed preflight finding that the parent fixed; these have no reviewer thread to reply to.
- **Skipped** always carries a reason. Never skip silently.
- **Needs decision** lists items you raised with the user via `AskUserQuestion` and are still waiting on (or that the user deferred). If you asked and got an answer mid-run, the item moves to Applied or Skipped instead.
- **Validation** states the local self-review, the base sync, how many official preflight attempts were used with each result and whether the budget was exhausted, and the single full-validation result. If validation could not run, say why.
- **Pushed** names the commits and the threads you replied on, and states the threads were left unresolved.
- After the block, write **nothing**.

## Execution economy

- Use `rg` for discovery instead of opening candidate files one by one.
- Batch the independent read-only GitHub queries when building the worklist.
- Read a large file once and work from that content; do not reload it per finding.
- Use focused `--filter` runs during the repair cycle; keep the suite for the single full validation.
- Do not rerun a passing focused test unless the head changed code it covers.
- Keep build and test verbosity minimal.
- Capture very large command output to a file and report a concise summary; on failure show the actionable error and the relevant log tail, not thousands of warning lines.
- Do not narrate unchanged state between steps.

None of this may reduce correctness or hide a failing result.

## Hard rules

- **Never** resolve a review thread — that is `gh-rereview`'s verified step.
- **Never** start editing before the complete worklist exists and every item is classified.
- **Never** guess on an ambiguous, contested, or architecturally significant finding — use `AskUserQuestion` first.
- **Never** skip the regression-test-first / idempotency / convergence discipline in `CLAUDE.md`; the review exists because it was skipped once.
- **Never** start a third official preflight automatically; the budget is one attempt plus one retry.
- **Never** split one preflight worklist into several fix/preflight loops, and never run a preflight after every individual fix.
- **Never** run the official preflight on a knowingly stale or conflicting branch and merge `main` afterwards — synchronize first.
- **Never** start full validation or create the final CI-trigger commit until `gh-preflight` returns `PASS` for the current PR head. If the budget is exhausted without a `PASS`, stop and report — that is not a licence to proceed.
- **Never** silence, `[Ignore]`, or delete a test to make validation green.
- **Never** push a non-`[skip ci]` commit before validation is green — the empty trigger commit is the only exception.
- **Never** `git add -A` blindly, and never edit files outside the review items' scope.
- **Never** open a new PR or flip the PR's draft/ready state.
- **Never** search for or create a follow-up issue. Every review item remains attached to the current PR.
- **Never** reach for the `gh` CLI or a raw GitHub API call — use the GitHub MCP server.
