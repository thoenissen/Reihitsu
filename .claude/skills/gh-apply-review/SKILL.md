---
name: gh-apply-review
description: >-
  Apply review feedback to a Reihitsu GitHub Pull Request in the PR author's Claude chat, or publish follow-up drafts the user approved in that chat. Use for "apply the review", "address the review comments", "fix the review findings", or "approve follow-up F1". Build one complete worklist before editing, freeze the PR's mechanism or requirement scope, fix confirmed same-scope and PR-introduced bugs in one repair cycle, and preserve every other confirmed item as an English copy-ready issue draft in the final chat response plus an ignored recovery cache. Require explicit approval before creating a GitHub issue; never invoke the local issue-upload script. Run a local self-review under CLAUDE.md and record its admission artifact, synchronize origin/main, spend at most two official gh-preflight attempts when the trigger list requires an audit, re-audit each repair against the guard-delta and predicate-boundary tables, prepare the toolchain through the repository scripts, validate once, push the existing PR branch, and reply without resolving threads. This is the fix step between gh-review and gh-rereview and runs in a Linux Claude Code Cloud Agent through the GitHub MCP server.
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
3. State the PR's defect mechanism or accepted requirement boundary and its shipped-surface boundary, classify every item exactly once, and freeze that scope.
4. Preserve each confirmed out-of-scope item as a reviewable follow-up draft; do not publish it yet.
5. Implement all `fix here` items as one cohesive repair cycle.
6. Run the **local self-review** and record its **admission artifact**.
7. Synchronize with current `origin/main`.
8. Decide from `gh-preflight`'s trigger list whether an audit is required, and run it on that exact synchronized head inside the 1 + 1 budget.
9. Run the complete **full validation** once.
10. Push the final non-`[skip ci]` CI trigger, reply to fixed items, and return every pending follow-up as a copy-ready chat block for user approval.
11. On a later approval turn, publish only the approved drafts, update `Follow-up work`, and reply to their review threads without changing the audited tree or resolving threads.

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

The sandbox does not ship the .NET SDK. Prepare the toolchain through the repository script before any `dotnet` command (the repo targets `net10.0`, no `global.json`):

```bash
scripts/prepare.sh
```

It probes `dotnet --list-sdks` and installs .NET 10 into `$HOME/.dotnet` only when no `10.*` SDK is present. `scripts/build.sh`, `scripts/test.sh`, `scripts/format.sh`, and `scripts/verify-text-only.sh` resolve the SDK the same way, so use them instead of hand-written `dotnet` invocations. If the SDK cannot be installed (no egress), stop and report it — a green run without the SDK is meaningless.

## Workflow

### 1. Build the complete worklist before editing

Read **all** the feedback first. Starting to edit after the first thread is what produces half-addressed reviews, contradictory fixes, and an extra review round: the second thread often changes how the first one should be fixed.

Call `get_me` first so you know which account is *you* (the author): review findings come from **other** accounts, and the user's own PR comments are hints, not findings. Gather and dedupe from every source:

1. **Unresolved review threads** — inline findings (`get_review_comments`) authored by an account other than `get_me`. Skip a resolved thread only after confirming its fix is present at the current head. Each surviving finding carries its file, line, severity, and the change it demands.
2. **Review summary bodies** — the top-level body of each review, which often carries the findings that had no line anchor.
3. **User-authored PR hints** — PR comments the `get_me` account posted itself.
4. **Pasted review blocks** — every line of a `gh-review` **Copy block** pasted into this chat.
5. **In-chat clarifications** — guidance the user gave here that never became a GitHub comment.
6. **Prior preflight findings and scope hints** — confirmed findings and confirmed unrelated pre-existing concerns from an earlier official preflight in this chat that are not yet fixed or preserved as follow-up drafts. Keep uncertain hints identified as uncertain; do not convert them into issues without confirmation.

A user hint that contradicts a reviewer finding wins — the user is steering.

If the worklist is empty (no reviewer findings, no hints), stop and say so — there is nothing to apply. If the PR has no review at all yet, say that too rather than inventing work.

### 2. Freeze scope and classify every item exactly once

Before classifying findings, record the **scope ledger**:

- the PR's original defect mechanism or accepted feature requirement, phrased so membership is decidable;
- the issue requirements and shipped surfaces the PR already changes;
- defects introduced by the current PR diff, which remain the PR's responsibility even when their mechanism differs from the original bug;
- each worklist item's mechanism, origin, change type, shipped-surface impact, disposition, and evidence.

Use the linked issue and Behavior Contract when available. Otherwise derive the original mechanism from the issue, current diff, and dispatch code. If the mechanism is materially ambiguous, use `gh-rubber-duck` or classify the item `needs decision`; do not invent a broad umbrella mechanism merely to keep work in the PR.

Assign each item exactly one class:

- **fix here** — a confirmed actionable bug for which `(same mechanism/accepted requirement as the PR OR introduced by the PR diff)` is true, the change restores intended behavior rather than adding new behavior, and it changes no shipped diagnostic, public API, or dependency. Reverting an unintended PR-local surface change to the base behavior is restoration, not a new shipped change.
- **follow-up draft** — a confirmed actionable item that fails any `fix here` condition. New behavior, a new mechanism, and diagnostic/API/dependency changes belong here even when severe.
- **dismissed** — demonstrably incorrect, duplicate, already fixed, or no longer applicable. Out-of-scope is not a dismissal reason. Record the evidence.
- **needs decision** — ambiguous (could be read several ways), contested (you believe it is wrong), or impossible to classify without a material user choice. Do **not** guess. Use `AskUserQuestion` with enough context that the user can answer without scrolling, then act on the answer.

Every confirmed actionable item is therefore either `fix here` or `follow-up draft`; it is never dropped. High severity changes follow-up priority, not PR scope.

Freeze the ledger after this first complete classification. Later review or preflight evidence may add another candidate of an already accepted mechanism or identify a PR-introduced defect, but it may not admit a new pre-existing mechanism into this PR. Apply the same criterion to every later finding and preserve new mechanisms as follow-up drafts.

When a review item introduces a materially ambiguous **behavior** change — a different output for the same input, an anchor moved, a rule's meaning widened — the `gh-rubber-duck` workflow is the right tool to settle it before editing. Run it — in a `reihitsu-rubber-duck` subagent, spawned by `subagent_type` with no model argument — or recommend it; it is read-only and costs one pass. It is optional here: only `gh-implement` runs it automatically as a mandatory gate.

### 3. Implement as one cohesive repair cycle, following `CLAUDE.md`

Work on the PR's head branch and implement all `fix here` items together, then validate once. Keep commits focused (group by file/concern; a bug fix and its regression test in one commit).

Before editing each accepted finding, state its general defect class and inspect sibling syntax shapes, wrappers, nested scopes, repeated-token cases, and shared helpers that can carry the same hazard. The requested counterexample is the minimum reproduction, not the implementation boundary. Regression coverage must close the relevant defect class without expanding into unrelated cleanup.

A finding's `Required change` — from a reviewer or from preflight — is a **suggestion, not a specification**. It is written from one reader's view of one symptom, and implementing it literally is how an over-broad repair creates the next round's finding. Whenever a repair moves a guard, predicate, or exemption, re-derive its scope with the guard-delta and predicate-boundary tables from `gh-rubber-duck` — against the guard *as repaired* — and add a test on **each** side of every boundary the repair moves. "No trivia at all" and "trivia on one line" are two tests; shipping only the first leaves the real boundary untested.

Honor the repository workflow — the review found these problems *because* the workflow was skipped, so do not skip it again:

- **Analyzer or formatter bug fix** → write the failing regression/repro test **first**, watch it fail, then fix. Analyzer tests are many small focused tests, not one large multi-case test.
- **New/changed formatter behavior** → add the idempotency (double-run) and CRLF assertions the reviewer asked for, through the existing helpers: `VerifyFormatterFixAndIdempotency` (second pass plus LF/CRLF), `VerifyFormatterStability` for code that must stay untouched, `AssertRuleResult(input, expected, endOfLine)` for formatter phases. `VerifyFormatterFix` alone checks neither the second pass nor CRLF.
- **New/changed code fix** → add the convergence (and FixAll where relevant) test; ship a comprehensive code fix or none.
- **Format the changed files** through the CLI before running tests:

  ```bash
  scripts/format.sh <changed-path-1> [<changed-path-2> ...]
  ```

- Run the focused tests for the touched rule or phase as you go, not the full suite.
- Stage only the files that belong to the review items. Never `git add -A` blindly — the sandbox may hold SDK install artifacts.
- Stay in scope. A finding is a licence to fix *that* problem, not to refactor around it. Preserve broader confirmed concerns through the follow-up-draft workflow below; do not edit the PR to implement them.

### 4. Preserve follow-up work for user review

For each `follow-up draft` item, first search open and closed GitHub issues (`mcp__github__search_issues`) and this chat's existing draft IDs for a duplicate. Reuse an exact existing issue instead of creating another draft. Combine several review items only when they share one decidable mechanism and one acceptance boundary.

Assign stable IDs `F1`, `F2`, and so on within the PR. Write each new draft in English using the matching template from `.claude/commands/draft-issue.md`. Include the source PR and review item, defect mechanism, scope-split reason, acceptance criteria, and non-goals in the body while retaining every required template heading.

Store two identical representations before the turn ends:

1. **Canonical review copy:** the complete frontmatter and body in a copy-ready fenced Markdown block under `Follow-up drafts` in the final chat response. Commentary is not sufficient because it is collapsed after the turn.
2. **Recovery cache:** the same Markdown in `plans/issues/pr-<PR>/F<n>-<slug>.md`. `plans/` is ignored and the cache never enters the PR diff. It is redundancy for context compaction, not the source of truth and not proof that an issue exists.

Never invoke `scripts/upload-issues.ps1`; it is a user-owned local upload tool. Never call `mcp__github__issue_write` before the user explicitly approves the specific draft ID and content. Until then, report `awaiting approval`, do not add a fictitious issue reference to the PR body, and do not tell the reviewer the item has a durable GitHub home.

Continue the in-scope repair, validation, and push while drafts await approval. Follow-up publication changes GitHub metadata but not the audited tree, so it is a later handoff rather than a reason to grow or revalidate the PR.

### 5. Publish only approved follow-ups

When the user later approves, edits, combines, or rejects a draft in this chat, treat that instruction as authoritative. Use the approved chat block as the canonical content; use the ignored cache only to recover text lost to context compaction.

For each approved draft:

1. Search open and closed issues again for the title, mechanism, and distinctive terms. Reuse a matching issue URL when one already exists.
2. Otherwise create the issue directly with `mcp__github__issue_write` from the approved body, without the YAML frontmatter, applying only labels that exist.
3. Capture the returned URL, update the PR body's `Follow-up work` section with the issue link and one-line scope rationale, and reply to the source review thread with the same link and rationale.
4. Leave the thread unresolved for `gh-rereview`. Verify the tracked tree is unchanged; do not rerun preflight or .NET validation for metadata-only publication.

If the user explicitly rejects a draft, record that decision against the draft ID. Silence, a missing local cache, or an unapproved draft never counts as rejection or publication.

### 6. Keep CI quiet, commit, push

The repo's `SonarCloud.yml` runs on every push to a PR. To collapse the many fix commits into one CI run, end **every** commit subject with `[skip ci]` **except the final trigger commit**:

```text
Address review: preserve #endif when joining parameters (#<PR>) [skip ci]
```

Push to the PR's existing head branch with `git push -u origin <head-branch>` (retry on network error with 2s/4s/8s/16s backoff). Do not open a new PR and do not change the PR's draft/ready state.

The single non-`[skip ci]` trigger commit comes at the very end, after preflight and full validation are green:

```bash
git push
git commit --allow-empty -m "Ready for CI (#<PR>)"
git diff --exit-code <audited-sha> HEAD
git push
```

The trigger commit is empty, so it carries the audited tree under a new SHA. `git diff --exit-code` must print nothing; if it prints a diff, the audit no longer covers what you are about to push.

### 7. Local self-review

The official preflight is a final quality gate, not a discovery loop, and only two attempts are available. Walk your own change first, locally, in this agent, without another agent and without the full suite:

- **every worklist row** — for each accepted item, name the change and the test that proves it;
- **scope ledger** — every confirmed actionable row satisfies `fix here` or has a complete follow-up draft, and no new pre-existing mechanism entered the PR after the freeze;
- **follow-up preservation** — every pending draft has an identical final-chat copy and ignored recovery cache, with no unapproved GitHub issue or PR-body claim;
- **counterpart parity** — formatter output is not flagged by the analyzer, analyzer-clean code is formatter-stable;
- **defect-class closure** — sibling shapes and private copies of the changed policy carry no residual hazard;
- **boundary closure** — every guard or predicate the repair moved has a test on each side of its new boundary;
- **convergence** — the code fix silences its own diagnostic in one pass and raises no new RH diagnostic;
- **idempotency** — a second formatter pass over the output is a no-op, on LF and CRLF;
- **comments and directives** — the relevant trivia shapes survive at sensible positions, or the edit is refused;
- **comment and documentation consistency** — for **every method whose body changed**, re-read its XML summary and inline comments and confirm they still describe the code they sit next to; a comment left describing the previous behavior is a defect in the same diff that changed it;
- **documentation** — the rule doc under `documentation/rules/` matches the shipped behavior;
- **changed-path formatting** — every changed C# path went through `scripts/format.sh`;
- **focused tests** — the tests for the touched rule/phase pass at the current working tree.

Fix what you find now. This is not an official preflight, does not consume a preflight attempt, and is not reported as one.

**Admission artifact.** A narrative self-review is not checkable — "ownership looked consistent" is what precedes an audit finding three owners. Before preflight may start, record the same falsifiable artifact `gh-implement` defines, with worklist rows in place of contract rows: each requirement qualifier and its owner/predicate, each changed predicate with the test on both sides of its boundary, the exact `rg` result for every changed policy owner, each worklist row and its regression test, each new test's invariant, falsifying observation, and helper, and the comment and documentation consistency check for every changed method. A missing row blocks admission to preflight.

### 8. Synchronize with `origin/main` before the official gate

The audited tree must be the tree that will merge:

1. `git fetch origin main`.
2. Check worktree and branch state — clean of unintended changes, on the PR head branch.
3. Merge current `origin/main` into the PR branch when it is behind.
4. Resolve conflicts so that **both** the branch behavior and the `main` behavior survive.
5. Run `Reihitsu.Cli` over every conflict-resolved and changed C# path.
6. Run the focused tests affected by the merge.
7. Commit and push the synchronized head with `[skip ci]`.
8. Run `scripts/build.sh` on that head and record the result in the evidence bundle — eleven seconds that remove a class of expensive reasoning from the audit, and a red build is never handed to a gate.
9. Run the official preflight against that exact head.

**Re-verify the remote in the spawning step.** A remote-tracking ref proves only when you last fetched, and a reviewer spawned against a stale one can run a complete audit and return `BLOCKED — state mismatch` having produced no gate result. Run `git ls-remote origin refs/heads/main` in the same step that spawns the agent — about one second — and re-merge before spawning when it names a SHA you have not merged. Record the SHA and the time in the bundle. This applies to the retry spawn too.

If `origin/main` moves again after a passing preflight, do not enter an unlimited re-merge/re-preflight loop. Check whether another merge is actually required, state that merging again changes the audited tree, and follow the user's explicit direction — including their decision to rely on CI without another preflight attempt.

### 9. Official preflight gate — hard 1 + 1 budget

After the accepted fixes are committed and pushed with `[skip ci]`, the local self-review and its admission artifact are complete, and `main` is synchronized, read `.claude/skills/gh-preflight/SKILL.md` completely and apply it as an internal, read-only gate against the current PR head. Do not post preflight findings through GitHub MCP. Run it in a fresh, independent subagent of type `reihitsu-preflight` when subagents are available, handing it the neutral evidence bundle that skill defines — the issue and PR **by number** rather than pasted in, base and head SHAs, the merge base, the remote `main` SHA with the time you read it, changed files and diff, the build result, your focused-test results, your proof that the checkout matches the head, and a per-item checklist-applicability list derived from the diff's shape — and nothing of your own reasoning. The reviewer confirms that applicability list rather than adopting it, and may overturn any row. Its model and effort come from `.claude/agents/reihitsu-preflight.md`; pass no model argument.

**First decide whether an audit is required at all**, using `gh-preflight`'s trigger list rather than the fact that a file compiles. A round whose accepted findings only touch comments, documentation, Markdown, skill and command files, or templates — including inside `.cs` — records a skip instead of spending an attempt, proven with `scripts/verify-text-only.sh --base <base-sha> --head <head-sha>` and its `TEXT-ONLY PROOF: PASS …` line. Ask the user when the round fits neither list. A skipped audit still leaves the full validation in place unless the diff contains no compiled file at all.

The budget is fixed:

1. **Attempt 1** runs automatically on the synchronized head.
2. On `PASS`, continue to full validation.
3. On `PASS — non-blocking cleanup`, apply the listed comment and documentation fixes, prove them non-behavioral *and free of public API documentation changes* with `scripts/verify-text-only.sh --strict-docs --base <audited-sha> --head worktree`, and continue to full validation without spending an attempt. If the proof rejects the cleanup, treat it as a repair cycle instead.
4. On `BLOCKED — findings`, merge **every** finding into **one** consolidated worklist — together with anything still open from the review worklist — and classify it against the frozen scope ledger. Do not fix before the worklist is complete, and do not run a preflight in between.
5. Fix every `fix here` row in **one** repair cycle and preserve every `follow-up draft` row without changing the PR for it: close each in-scope finding's full defect class, re-derive the repair against the delta tables, format the changed paths, run the focused tests, redo the local self-review and admission artifact, then commit and push with `[skip ci]`.
6. **Attempt 2** — the preflight retry — then runs **once**, as a fresh, independent `reihitsu-preflight` subagent against the exact new head, carrying the repair-delta inputs: the previous report, the previously audited SHA, the repaired SHA, and the repair diff.
7. If the retry also blocks, **stop**. Report the remaining findings to the user and let them decide. Never start a third official preflight automatically.

On `BLOCKED — state mismatch`, reconcile the checkout, commits, and PR head before rerunning; a state mismatch is a setup error, not a review result, so it does not consume an attempt. Neither does a reviewer agent that returned no verdict — that costs a process start, and `gh-preflight`'s bounded restart policy applies: one start, one restart after a no-progress timeout, then the local read-only fallback.

Classify architecturally significant, public-API-changing, dependency-changing, contested, or unrelated preflight findings against the frozen ledger. Use `needs decision` when the classification itself is ambiguous; otherwise preserve them as follow-up drafts rather than expanding the PR. Do not create the final CI-trigger commit until both the preflight decision and full validation are settled and green.

A tracked-file change made after a passing preflight means the audited tree is no longer the tree that will merge:

- the change is proven text-only by `scripts/verify-text-only.sh` → note it and its proof line in the report and continue;
- it touches compiled behavior and an attempt is unspent → spend the retry on the new tree;
- it touches compiled behavior and the budget is exhausted → stop and report. The user decides whether to ship a tree that no audit covered; you do not decide it silently.

### 10. Full validation — run it once

Focused tests run throughout the repair cycle. The complete suite runs **once**, after the fixes are in, `main` is synchronized, the preflight decision is settled, and the worktree matches the audited tree. Only a round whose diff contains no compiled file at all skips it; a skipped audit does not skip validation, because the build is what catches a malformed comment or a changed documentation artifact and test runtime costs wall-clock rather than tokens:

```bash
scripts/build.sh
scripts/test.sh --no-build
```

Run this in a subagent of type `reihitsu-validate` when subagents are available, passing the tree it must validate and whether anything changed since the Release build. It runs the two scripts and returns pass/fail per step plus the failing assertions verbatim, which keeps several thousand lines of output out of this context; its model comes from `.claude/agents/reihitsu-validate.md`, so pass no model argument. It fixes nothing and diagnoses nothing — every repair is yours. Without subagents, run the two commands here and capture the output to a file instead of into the transcript.

`scripts/test.sh` runs all four test projects in order; `--no-build` is valid only because the Release build immediately above covered this exact tree; drop it and rebuild if any file changed since. All four test projects must pass. Fix regressions your change caused (commit with `[skip ci]`). **A change to any compiled file invalidates the build, every project result gathered before it, and the preflight** — those green runs proved the previous tree. Re-run the build and all four test projects on the repaired tree; in this repository a formatter fix really can flip analyzer results, because `Reihitsu.Analyzer.CodeFixes` depends on the formatter and the analyzer tests drive it through `FormatterTestsBase<TAnalyzer>`. Never silence, `[Ignore]`, or delete a test to go green. If a failure is pre-existing on the base branch and independent of the review items, record it in the report and stop rather than build on a broken baseline.

If the user explicitly asks to skip repeated local validation and rely on CI, obey that instruction and report exactly which local checks ran and which did not.

### 11. Reply, do not resolve

For each **fixed** item on an inline thread, post one concise reply with `mcp__github__add_reply_to_pull_request_comment` stating what changed and the commit sha (`Addressed: guard now preserves `#endif`; regression test added (<sha>).`). For a non-line hint, reply via `mcp__github__add_issue_comment`. For a follow-up item, wait until the approved issue exists, then reply with its URL and scope rationale.

**Do not resolve any thread.** Resolution is `gh-rereview`'s verified step — leaving threads open is the handshake that tells the re-review what to check. English only, concise, state what changed, no praise.

## What to write back in chat

**Only** the following block, nothing else. No preamble, no closing summary. Render every heading even when empty (`_None._`). If you resolved the PR from chat context rather than an explicit id, name it in the first line.

`````markdown
## Applied
| # | Source | Location | Commit | Change |
|---|--------|----------|--------|--------|
| 1 | reviewer | Reihitsu.Formatter/Pipeline/Foo.cs:42 | a1b2c3d | Preserve `#endif` when joining parameters; regression test added |
| 2 | user hint | Reihitsu.Analyzer/Rules/RH3204/Bar.cs:88 | e4f5a6b | Split parsing out of the diagnostic method |

## Follow-up drafts
| ID | Source | Location | Mechanism | Scope reason | Status | Cache |
|----|--------|----------|-----------|--------------|--------|-------|
| F1 | reviewer | Reihitsu.Formatter/Pipeline/Foo.cs:42 | Accessor-list layout policy | New formatter behavior | awaiting approval | `plans/issues/pr-123/F1-accessor-list-layout.md` |

### F1 — Copy-ready issue draft

````markdown
---
template: formatter_feature_request
title: "[FORMATTER FEATURE] Example title"
labels: enhancement, formatter
---

<complete template-compatible issue body>
````

## Dismissed
| # | Source | Location | Reason and evidence |
|---|--------|----------|---------------------|
| 1 | reviewer | Reihitsu.Cli/Program.cs:120 | Already fixed at the current head; the requested guard is present at line 118 |

## Needs decision
_None._

## Validation
- Local self-review: every worklist row checked; parity, boundaries, convergence, idempotency, directives, comment/documentation consistency re-checked. Admission artifact complete.
- Base sync: merged `origin/main` at `<sha>`; conflicts formatted and focused-tested.
- Official preflight: required by <trigger>; 1 attempt used (1 reviewer start), PASS on tree `<sha>`; budget not exhausted. (State the skip and its `TEXT-ONLY PROOF` line here instead when the trigger list did not require an audit.)
- Build: green — pre-gate run at `<sha>` and the full validation run.
- Analyzer / Formatter / Core / Cli tests: green, one full run via `scripts/test.sh`.
- Gate cost: `<gate>` — `<model>` / `<effort>`, `<tokens or "not reported">`, `<elapsed>`; one line per gate that ran.

## Pushed
- Branch `claude/...`: 2 fix commits (`[skip ci]`) + trigger commit `Ready for CI (#<PR>)`.
- Replied on fixed threads #1, #2; F1 awaits approval and has no follow-up thread reply yet. Threads left unresolved for gh-rereview.
`````

Rules for the block:

- List every item once. Every confirmed actionable item appears under **Applied** or **Follow-up drafts**; it is never dropped.
- **Applied** lists each fixed item with the commit that carried it and a one-sentence change note.
- **Follow-up drafts** carries one row per pending or published draft, each followed by its copy-ready fenced draft block. Omit the copy block once the issue exists.
- **Dismissed** is reserved for findings disproved, duplicated, already fixed, or no longer applicable, and always carries the evidence. Out-of-scope is not a dismissal reason.
- Include a `preflight` source row under **Applied** or **Follow-up drafts** for each confirmed preflight finding or confirmed scope hint; these have no reviewer thread until an issue is approved and published.
- **Needs decision** lists items you raised with the user via `AskUserQuestion` and are still waiting on (or that the user deferred). Answered decisions move into the matching table instead.
- **Validation** states the local self-review and its admission artifact, the base sync, the preflight decision with its trigger or proof line, how many official attempts and how many reviewer process starts were used with each result and whether the budget was exhausted, and the single full-validation result. It also carries one `Gate cost` line per gate — the model tier, the effort level, the token cost where the environment reports it, and the elapsed time — because a tier or scope change cannot be shown not to have regressed a verdict without a baseline to compare it against. If validation or push failed, state the exact failure instead of claiming success.
- **Pushed** names the commits and the threads you replied on, and states the threads were left unresolved.
- After the block, write **nothing**.

For an approval-only continuation turn, return the same structure with each approved draft changed to `created: <issue-url>` or `reused: <issue-url>`, the cache path retained for traceability, the PR-body update and thread reply under Pushed, and unchanged validation explicitly carried forward because the tracked tree did not change.

## Execution economy

- Check the remote with `git ls-remote origin refs/heads/main` in the step that spawns a reviewer; one second there is what stops a complete audit from returning `BLOCKED — state mismatch` with no gate result.
- Build before the gate; eleven seconds removes a class of expensive reasoning from the audit.
- Reference the issue and PR by number in the evidence bundle instead of pasting their bodies into every prompt.
- Derive the checklist applicability once and hand it over, rather than letting each reviewer rediscover the same negatives.
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
- **Never** expand the frozen PR mechanism scope for a pre-existing defect, new behavior, diagnostic, public API, or dependency change; preserve it as a follow-up draft.
- **Never** drop a confirmed actionable item: fix it here or return a complete reviewable follow-up draft.
- **Never** skip the regression-test-first / idempotency / convergence discipline in `CLAUDE.md`; the review exists because it was skipped once.
- **Never** start a third official preflight automatically; the budget is one attempt plus one retry.
- **Never** invoke preflight before the admission artifact is complete, and never claim a comment-only carve-out without the `scripts/verify-text-only.sh` proof line.
- **Never** implement a `Required change` literally without re-deriving its scope against the delta tables and adding a test on each side of every boundary the repair moved.
- **Never** split one preflight worklist into several fix/preflight loops, and never run a preflight after every individual fix.
- **Never** run the official preflight on a knowingly stale or conflicting branch and merge `main` afterwards — synchronize first.
- **Never** start full validation or create the final CI-trigger commit while an audit is required and has not returned `PASS` or `PASS — non-blocking cleanup` for the current PR tree. A recorded, proven skip from the trigger list is the only way past it, and it still leaves the full validation in place unless the diff contains no compiled file at all. If the budget is exhausted without a passing result, stop and report; that is not a licence to proceed.
- **Never** treat an earlier green project result as still valid after a compiled file changed, and never claim the audit covered the final commit when only the tree matches.
- **Never** silence, `[Ignore]`, or delete a test to make validation green.
- **Never** push a non-`[skip ci]` commit before validation is green — the empty trigger commit is the only exception.
- **Never** `git add -A` blindly, and never stage tracked files outside the review items' scope. Ignored `plans/issues/pr-<PR>/` files are recovery caches only.
- **Never** open a new PR or flip the PR's draft/ready state.
- **Never** invoke `scripts/upload-issues.ps1`, publish an unapproved draft, or rely on an ignored local file as the sole copy of follow-up work.
- **Never** reach for the `gh` CLI or a raw GitHub API call — use the GitHub MCP server.
