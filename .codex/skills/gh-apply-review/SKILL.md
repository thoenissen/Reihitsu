---
name: gh-apply-review
description: Apply review feedback to a Reihitsu GitHub Pull Request in the PR author's Codex task, or publish follow-up drafts the user approved in that task. Use for "apply the review", "address the review comments", "fix the review findings", or "approve follow-up F1". Build one complete worklist before editing, freeze the PR's mechanism or requirement scope, fix confirmed same-scope and PR-introduced bugs in one repair cycle, and preserve every other confirmed item as an English copy-ready issue draft in final chat plus an ignored recovery cache. Require explicit approval before `gh issue create`; never invoke the local issue-upload script. Run local self-review, synchronize origin/main, use at most two preflight attempts, validate once, push the existing branch, and reply without resolving threads. Supports Linux and Windows with authenticated `gh` and the preinstalled .NET SDK.
---

# Reihitsu GitHub PR Apply Review

Implement the feedback on an existing PR, validate it, push it, and reply to each addressed item. Run this in the PR author's task, normally the same task that ran `gh-implement`:

```text
gh-review -> gh-apply-review -> gh-rereview
   find             fix            re-check
reviewer task     author task     reviewer task
```

Own the implementation and branch, but not the verdict. Fix and reply; never resolve a review thread. Verification and resolution belong to `gh-rereview`.

Support Linux cloud and local Windows. Use the repository checkout, authenticated `gh` CLI, local `git`, and preinstalled .NET SDK. Follow `AGENTS.md` throughout.

## Run order

1. Resolve the PR and read its current state.
2. Build **one** complete worklist from every source, before editing anything.
3. State the PR's defect mechanism or accepted requirement boundary and its shipped-surface boundary, classify every item exactly once, and freeze that scope.
4. Preserve each confirmed out-of-scope item as a reviewable follow-up draft; do not publish it yet.
5. Implement all `fix here` items as one cohesive repair cycle.
6. Run the **local self-review**.
7. Synchronize with current `origin/main`.
8. Run the **official preflight** on that exact synchronized head, inside the 1 + 1 budget.
9. Run the complete **full validation** once.
10. Push the final non-`[skip ci]` CI trigger, reply to fixed items, and return every pending follow-up as a copy-ready chat block for user approval.
11. On a later approval turn, publish only the approved drafts, update `Follow-up work`, and reply to their review threads without changing the audited tree or resolving threads.

## Resolve the PR

Resolve the PR in this order:

1. Use an explicit number, `#number`, or PR URL from the prompt or `$ARGUMENTS`.
2. Otherwise, reuse the PR created by `gh-implement` in the current task. If several were created, use the most recent and state the chosen PR before making changes.
3. If no PR can be identified, ask the user for it. Never guess.

If the prompt names a repository other than the current `origin`, pass `--repo <owner>/<repo>` to every `gh` PR command and use that owner/repository in API paths.

Read the PR before editing, batching the independent queries:

```shell
gh auth status
gh api user --jq .login
gh pr view <N> --json number,title,body,author,baseRefName,headRefName,headRefOid,url,comments,reviews
gh pr diff <N>
gh pr view <N> --json files,commits
gh api repos/{owner}/{repo}/pulls/<N>/comments
```

Use `gh api graphql` to read review threads with their `id`, `isResolved`, and comments. Paginate when the PR has more than 100 threads or comments. Use `gh pr view` and `gh api repos/{owner}/{repo}/issues/<N>/comments` for general PR comments.

Verify that the checkout is on the PR head branch and current head SHA before editing. If switching is required, run `git worktree list` first and do not take a branch already checked out elsewhere.

## Build the complete worklist before editing

Read **all** the feedback first. Starting to edit after the first thread is what produces half-addressed reviews, contradictory fixes, and an extra review round: the second thread often changes how the first one should be fixed.

Call `gh api user --jq .login` first and compare it with the PR author so the identities are explicit. Gather and deduplicate from every source:

1. **Unresolved review threads** — inline findings authored by accounts other than the PR author. Capture file, line, severity when supplied, requested change, comment database id, and thread id. Skip a resolved thread only after confirming its fix is present at the current head.
2. **Review summary bodies** — the top-level body of each review, which frequently carries the findings that had no line anchor.
3. **User-authored PR hints** — comments the PR author's own account posted on the PR.
4. **Pasted review blocks** — every line of a `gh-review` Copy block pasted into the task.
5. **Conversation clarifications** — relevant guidance given in chat that never became a GitHub comment.
6. **Prior preflight findings and scope hints** — confirmed findings and confirmed unrelated pre-existing concerns from an earlier official preflight in this task that are not yet fixed or preserved as follow-up drafts. Keep uncertain hints identified as uncertain; do not convert them into issues without confirmation.

Direct user guidance wins when it conflicts with a reviewer finding.

If there is no review and no hint, stop and report that there is nothing to apply.

## Freeze scope and classify every item exactly once

Before classifying findings, record the **scope ledger**:

- the PR's original defect mechanism or accepted feature requirement, phrased so membership is decidable;
- the issue requirements and shipped surfaces the PR already changes;
- defects introduced by the current PR diff, which remain the PR's responsibility even when their mechanism differs from the original bug;
- each worklist item's mechanism, origin, change type, shipped-surface impact, disposition, and evidence.

Use the linked issue and Behavior Contract when available. Otherwise derive the original mechanism from the issue, current diff, and dispatch code. If the mechanism is materially ambiguous, use `gh-rubber-duck` or classify the item `needs decision`; do not invent a broad umbrella mechanism merely to keep work in the PR.

Assign each worklist item exactly one class:

- **fix here**: a confirmed actionable bug for which `(same mechanism/accepted requirement as the PR OR introduced by the PR diff)` is true, the change restores intended behavior rather than adding new behavior, and it changes no shipped diagnostic, public API, or dependency. Reverting an unintended PR-local surface change to the base behavior is restoration, not a new shipped change.
- **follow-up draft**: a confirmed actionable item that fails any `fix here` condition. New behavior, a new mechanism, and diagnostic/API/dependency changes belong here even when severe.
- **dismissed**: demonstrably incorrect, duplicate, already fixed, or no longer applicable. Out-of-scope is not a dismissal reason. Record the evidence.
- **needs decision**: ambiguous, contested, or impossible to classify without a material user choice. Ask the user directly and pause that item until they decide. Do not silently choose an interpretation.

Every confirmed actionable item is therefore either `fix here` or `follow-up draft`; it is never dropped. High severity changes follow-up priority, not PR scope.

Freeze the ledger after this first complete classification. Later review or preflight evidence may add another candidate of an already accepted mechanism or identify a PR-introduced defect, but it may not admit a new pre-existing mechanism into this PR. Apply the same criterion to every later finding and preserve new mechanisms as follow-up drafts.

When a review item introduces a materially ambiguous **behavior** change — a different output for the same input, an anchor moved, a rule's meaning widened — the `gh-rubber-duck` workflow is the right tool to settle it before editing. Run it or recommend it; it is read-only and costs one pass. It is optional here: only `gh-implement` runs it automatically as a mandatory gate.

## Implement as one cohesive repair cycle

Implement all `fix here` items together, then validate once. Keep changes limited to accepted review items. Group commits by concern and stage explicit paths only; never use `git add -A` blindly.

Before editing each accepted finding, state its general defect class and inspect sibling syntax shapes, wrappers, nested scopes, repeated-token cases, and shared helpers that can carry the same hazard. The requested counterexample is the minimum reproduction, not the implementation boundary. Regression coverage must close the relevant defect class without expanding into unrelated cleanup.

Apply the repository workflow from `AGENTS.md`:

- For analyzer or formatter bug fixes, add the reproducing regression test first and confirm it fails before changing production code.
- For formatter behavior, add the requested idempotency, CRLF, and combined-pipeline coverage when applicable. Use the existing helpers rather than new ones: `VerifyFormatterFixAndIdempotency` (second pass plus LF/CRLF) for layout changes, `VerifyFormatterFix` for plain parity, `VerifyFormatterStability` for code that must stay untouched, and `AssertRuleResult(input, expected, endOfLine)` for formatter phases. `VerifyFormatterFix` alone is not idempotency coverage.
- For code fixes, add convergence and relevant FixAll coverage. Deliver a comprehensive code fix or omit it.
- Format all changed paths before tests:

  ```shell
  dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
  ```

- Run the focused tests for the touched rule or phase as you go, not the full suite.
- Avoid unrelated cleanup. Preserve broader confirmed concerns through the follow-up-draft workflow below; do not edit the PR to implement them.

## Preserve follow-up work for user review

For each `follow-up draft` item, first search open and closed GitHub issues and the current task's existing draft IDs for a duplicate. Reuse an exact existing issue instead of creating another draft. Combine several review items only when they share one decidable mechanism and one acceptance boundary.

Assign stable IDs `F1`, `F2`, and so on within the PR. Write each new draft in English using the matching template from `.codex/commands/draft-issue.md`. Include the source PR and review item, defect mechanism, scope-split reason, acceptance criteria, and non-goals in the body while retaining every required template heading.

Store two identical representations before the turn ends:

1. **Canonical review copy:** the complete frontmatter and body in a copy-ready fenced Markdown block under `Follow-up drafts` in the final chat response. Commentary is not sufficient because it is collapsed after the turn.
2. **Recovery cache:** the same Markdown in `plans/issues/pr-<PR>/F<n>-<slug>.md`. `plans/` is ignored and the cache never enters the PR diff. It is redundancy for context compaction, not the source of truth and not proof that an issue exists.

Never invoke `scripts/upload-issues.ps1`; it is a user-owned local upload tool. Never call `gh issue create` before the user explicitly approves the specific draft ID and content. Until then, report `awaiting approval`, do not add a fictitious issue reference to the PR body, and do not tell the reviewer the item has a durable GitHub home.

Continue the in-scope repair, validation, and push while drafts await approval. Follow-up publication changes GitHub metadata but not the audited tree, so it is a later handoff rather than a reason to grow or revalidate the PR.

## Publish only approved follow-ups

When the user later approves, edits, combines, or rejects a draft in this author task, treat that instruction as authoritative. Use the approved chat block as the canonical content; use the ignored cache only to recover text lost to context compaction.

For each approved draft:

1. Search open and closed issues again for the title, mechanism, and distinctive terms. Reuse a matching issue URL when one already exists.
2. Otherwise write the approved body, without YAML frontmatter, to a temporary file outside the repository and create the issue directly with authenticated `gh issue create --title ... --body-file ...`, applying only labels that exist.
3. Capture the returned URL, update the PR body's `Follow-up work` section with the issue link and one-line scope rationale, and reply to the source review thread with the same link and rationale.
4. Leave the thread unresolved for `gh-rereview`. Verify the tracked tree is unchanged; do not rerun preflight or .NET validation for metadata-only publication.

If the user explicitly rejects a draft, record that decision against the draft ID. Silence, a missing local cache, or an unapproved draft never counts as rejection or publication.

## Commit and keep CI quiet

End every fix commit subject with `[skip ci]` so intermediate pushes do not run CI:

```text
Address review: preserve directives in parameter joins (#<PR>) [skip ci]
```

Push to the PR's existing head branch only. Do not open another PR or change draft/ready state. Retry transient push failures with bounded 2s/4s/8s/16s backoff.

The single non-`[skip ci]` trigger commit comes at the very end, after preflight and full validation are green:

```shell
git push
git commit --allow-empty -m "Ready for CI (#<PR>)"
git diff --exit-code <audited-sha> HEAD
git push
```

The trigger commit is empty, so it carries the audited tree under a new SHA. `git diff --exit-code` must print nothing; if it prints a diff, the audit no longer covers what you are about to push.

Do not create the trigger commit when no change was applied.

## Local self-review (before the official preflight)

The official preflight is a final quality gate, not a discovery loop, and only two attempts are available. Walk your own change first, locally, in this agent, without another agent and without the full suite:

- **every worklist row** — for each accepted item, name the change and the test that proves it;
- **scope ledger** — every confirmed actionable row satisfies `fix here` or has a complete follow-up draft, and no new pre-existing mechanism entered the PR after the freeze;
- **follow-up preservation** — every pending draft has an identical final-chat copy and ignored recovery cache, with no unapproved GitHub issue or PR-body claim;
- **counterpart parity** — formatter output is not flagged by the analyzer, analyzer-clean code is formatter-stable;
- **defect-class closure** — sibling shapes and private copies of the changed policy carry no residual hazard;
- **convergence** — the code fix silences its own diagnostic in one pass and raises no new RH diagnostic;
- **idempotency** — a second formatter pass over the output is a no-op, on LF and CRLF;
- **comments and directives** — the relevant trivia shapes survive at sensible positions, or the edit is refused;
- **documentation** — the rule doc under `documentation/rules/` matches the shipped behavior;
- **changed-path formatting** — every changed C# path went through `Reihitsu.Cli`;
- **focused tests** — the tests for the touched rule/phase pass at the current working tree.

Fix what you find now. This is not an official preflight, does not consume a preflight attempt, and is not reported as one.

## Synchronize with `origin/main` before the official gate

The audited tree must be the tree that will merge:

1. `git fetch origin main`.
2. Check worktree and branch state — clean of unintended changes, on the PR head branch.
3. Merge current `origin/main` into the PR branch when it is behind.
4. Resolve conflicts so that **both** the branch behavior and the `main` behavior survive.
5. Run `Reihitsu.Cli` over every conflict-resolved and changed C# path.
6. Run the focused tests affected by the merge.
7. Commit and push the synchronized head with `[skip ci]`.
8. Run the official preflight against that exact head.

If `origin/main` moves again after a passing preflight, do not enter an unlimited re-merge/re-preflight loop. Check whether another merge is actually required, state that merging again changes the audited tree, and follow the user's explicit direction — including their decision to rely on CI without another preflight attempt.

## Official preflight gate — hard 1 + 1 budget

After the accepted fixes are committed and pushed with `[skip ci]`, the local self-review is done, and `main` is synchronized, read `.codex/skills/gh-preflight/SKILL.md` completely and apply it as an internal, read-only gate against the current PR head. Do not post preflight findings to GitHub. Run it in a fresh, independent read-only subagent when subagents are available.

A **routine round** — no compiled file anywhere in the diff, and no accepted finding touching analyzer, formatter, code-fix, or `Reihitsu.Core` behavior — skips the official preflight and the full validation alike; the PR review is the gate for text-only changes. Record the skip and its reason in the Validation block. Every other round spends at least attempt 1.

The budget is fixed:

1. **Attempt 1** runs automatically on the synchronized head.
2. On `PASS`, continue to full validation.
3. On `BLOCKED — findings`, merge **every** finding into **one** consolidated worklist — together with anything still open from the review worklist — and classify it against the frozen scope ledger. Do not fix before the worklist is complete, and do not run a preflight in between.
4. Fix every `fix here` row in **one** repair cycle and preserve every `follow-up draft` row without changing the PR for it: close each in-scope finding's full defect class, format the changed paths, run the focused tests, redo the local self-review, then commit and push with `[skip ci]`.
5. **Attempt 2** — the preflight retry — then runs **once**, as a fresh, independent, read-only subagent against the exact new head.
6. If the retry also blocks, **stop**. Report the remaining findings to the user and let them decide. Never start a third official preflight automatically.

On `BLOCKED — state mismatch`, reconcile the checkout, commits, and PR head before rerunning; a state mismatch is a setup error, not a review result, so it does not consume an attempt.

Classify architecturally significant, public-API-changing, dependency-changing, contested, or unrelated preflight findings against the frozen ledger. Use `needs decision` when the classification itself is ambiguous; otherwise preserve them as follow-up drafts rather than expanding the PR. Do not create the final CI-trigger commit until both preflight and full validation are green.

A tracked-file change made after a passing preflight means the audited tree is no longer the tree that will merge:

- the change touches no compiled file → note it in the report and continue;
- it touches compiled files and an attempt is unspent → spend the retry on the new tree;
- it touches compiled files and the budget is exhausted → stop and report. The user decides whether to ship a tree that no audit covered; you do not decide it silently.

## Full validation — run it once

Before the first build or test, confirm the preinstalled SDK:

```shell
dotnet --list-sdks
```

Do not install an SDK or modify `PATH`. Focused tests run throughout the repair cycle; the complete suite runs **once**, after the fixes are in, `main` is synchronized, the official preflight has passed, and the worktree matches the audited tree:

```shell
dotnet build Reihitsu.sln -c Release --verbosity minimal
dotnet test Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj -c Release --no-build --verbosity minimal
dotnet test Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj -c Release --no-build --verbosity minimal
dotnet test Reihitsu.Core.Test/Reihitsu.Core.Test.csproj -c Release --no-build --verbosity minimal
dotnet test Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj -c Release --no-build --verbosity minimal
```

`--no-build` is valid only because the Release build immediately above covered this exact tree; drop it and rebuild if any file changed since. All relevant tests must pass. Fix regressions caused by the review changes in focused `[skip ci]` commits. **A change to any compiled file invalidates the build, every project result gathered before it, and the preflight** — those green runs proved the previous tree. Re-run the build and all four test projects on the repaired tree; in this repository a formatter fix really can flip analyzer results, because `Reihitsu.Analyzer.CodeFixes` depends on the formatter and the analyzer tests drive it through `FormatterTestsBase<TAnalyzer>`. Never silence, ignore, or delete a test to obtain a green run. If the SDK is absent or the base branch has an independent failure, stop and report the evidence.

If the user explicitly asks to skip repeated local validation and rely on CI, obey that instruction and report exactly which local checks ran and which did not.

## Reply without resolving

After the commits are pushed, reply once to every fixed inline comment using the review comment's database id:

```shell
gh api --method POST repos/{owner}/{repo}/pulls/<N>/comments \
  -F in_reply_to=<comment-id> -f body='Addressed: <change> (<sha>).'
```

Use `gh pr comment <N> --body '<message>'` for a non-line hint. For a follow-up item, wait until the approved issue exists, then reply with its URL and scope rationale. Keep replies concise and in English. Do not resolve any thread; its open state is the handshake for `gh-rereview`.

## Chat output

After completion, write only this structure, rendering `_None._` under empty sections:

`````markdown
## Applied
| # | Source | Location | Commit | Change |
|---|--------|----------|--------|--------|
| 1 | reviewer | Reihitsu.Formatter/Pipeline/Foo.cs:42 | a1b2c3d | Preserve `#endif`; add a regression test |

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
- Local self-review: every worklist row checked; parity, convergence, idempotency, directives re-checked.
- Base sync: merged `origin/main` at `<sha>`; conflicts formatted and focused-tested.
- Official preflight: 1 attempt used, PASS on tree `<sha>`; budget not exhausted. (State the skip and its reason here instead when the round changed no compiled file.)
- Build: green.
- Analyzer / Formatter / Core / CLI tests: green (one full run).

## Pushed
- PR #123, branch `codex/...`: two `[skip ci]` fix commits and trigger commit `Ready for CI (#123)`.
- Replied on fixed thread #1; F1 awaits approval and has no follow-up thread reply yet. Left threads unresolved for `gh-rereview`.
`````

List every item once. Every confirmed actionable item appears under Applied or Follow-up drafts; Dismissed is reserved for findings disproved, duplicated, already fixed, or no longer applicable. Include a `preflight` source row under Applied or Follow-up drafts for each confirmed preflight finding or confirmed scope hint; these have no reviewer thread until an issue is approved and published. Move answered decisions into the matching table; list only unresolved decisions under Needs decision. The Validation block must state how many official preflight attempts were used, the result of each, and whether the budget was exhausted. If validation or push failed, state the exact failure instead of claiming success. Add no preamble or closing text.

For an approval-only continuation turn, return the same structure with each approved draft changed to `created: <issue-url>` or `reused: <issue-url>`, the cache path retained for traceability, the PR-body update and thread reply under Pushed, and unchanged validation explicitly carried forward because the tracked tree did not change. Omit the copy block once the issue exists.

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

- Never resolve a review thread.
- Never start editing before the complete worklist exists and every item is classified.
- Never guess on ambiguous or significant feedback.
- Never expand the frozen PR mechanism scope for a pre-existing defect, new behavior, diagnostic, public API, or dependency change; preserve it as a follow-up draft.
- Never drop a confirmed actionable item: fix it here or return a complete reviewable follow-up draft.
- Never skip the test-first, idempotency, convergence, formatting, or validation requirements in `AGENTS.md`.
- Never start a third official preflight automatically; the budget is one attempt plus one retry.
- Never split one preflight worklist into several fix/preflight loops, and never run a preflight after every individual fix.
- Never run the official preflight on a knowingly stale or conflicting branch and merge `main` afterwards — synchronize first.
- Never start full validation or create the final CI-trigger commit until `gh-preflight` returns `PASS` for the current PR tree — the one exception is a round with no compiled file in the diff, which skips both and records the skip. If the budget is exhausted without a `PASS`, stop and report; that is not a licence to proceed.
- Never treat an earlier green project result as still valid after a compiled file changed, and never claim the audit covered the final commit when only the tree matches.
- Never install an SDK or modify `PATH`.
- Never push a non-`[skip ci]` commit before validation is green.
- Never stage unrelated tracked paths, open another PR, or change the PR's draft state. Ignored `plans/issues/pr-<PR>/` files are recovery caches only.
- Never invoke `scripts/upload-issues.ps1`, publish an unapproved draft, or rely on an ignored local file as the sole copy of follow-up work.
- Use authenticated `gh` for GitHub operations; do not use raw unauthenticated HTTP calls.
