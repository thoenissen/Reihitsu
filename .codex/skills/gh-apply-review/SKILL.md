---
name: gh-apply-review
description: Apply review feedback to a Reihitsu GitHub Pull Request in the PR author's Codex task after another party reviewed it. Use for requests such as "apply the review", "address the review comments", "work through the PR feedback", or "fix the review findings". Build one complete worklist from open reviewer findings, review summaries, user-authored PR hints, a pasted gh-review Copy block, chat context, and prior preflight findings before editing anything; implement the accepted items as one cohesive repair cycle under AGENTS.md; close the complete defect class; run a local self-review; synchronize origin/main; spend at most two official gh-preflight attempts; run the full validation once; push the existing PR branch; reply to addressed threads without resolving them. This is the fix step between gh-review and gh-rereview and supports Codex on Linux cloud and local Windows with the authenticated gh CLI and preinstalled .NET SDK.
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
3. Classify every item exactly once: fix, skip with reason, or needs decision.
4. Implement all `fix` items as one cohesive repair cycle.
5. Run the **local self-review**.
6. Synchronize with current `origin/main`.
7. Run the **official preflight** on that exact synchronized head, inside the 1 + 1 budget.
8. Run the complete **full validation** once.
9. Push the final non-`[skip ci]` CI trigger, then reply to the addressed threads without resolving them.

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
6. **Prior preflight findings** — confirmed findings from an earlier official preflight in this task that are not yet fixed.

Direct user guidance wins when it conflicts with a reviewer finding.

If there is no review and no hint, stop and report that there is nothing to apply.

## Classify every item exactly once

Assign each worklist item exactly one class:

- **fix**: actionable and unambiguous; implement it in this cycle.
- **skip**: incorrect, already handled, or out of scope; record the evidence and reason.
- **needs decision**: ambiguous, contested, architecturally significant, public-API-changing, or dependency-changing. Ask the user directly and pause that item until they decide. Do not silently choose an interpretation.

When a review item introduces a materially ambiguous **behavior** change — a different output for the same input, an anchor moved, a rule's meaning widened — the `gh-rubber-duck` workflow is the right tool to settle it before editing. Run it or recommend it; it is read-only and costs one pass. It is optional here: only `gh-implement` runs it automatically as a mandatory gate.

## Implement as one cohesive repair cycle

Implement all `fix` items together, then validate once. Keep changes limited to accepted review items. Group commits by concern and stage explicit paths only; never use `git add -A` blindly.

Before editing each accepted finding, state its general defect class and inspect sibling syntax shapes, wrappers, nested scopes, repeated-token cases, and shared helpers that can carry the same hazard. The requested counterexample is the minimum reproduction, not the implementation boundary. Regression coverage must close the relevant defect class without expanding into unrelated cleanup.

Apply the repository workflow from `AGENTS.md`:

- For analyzer or formatter bug fixes, add the reproducing regression test first and confirm it fails before changing production code.
- For formatter behavior, add the requested idempotency, CRLF, and combined-pipeline coverage when applicable.
- For code fixes, add convergence and relevant FixAll coverage. Deliver a comprehensive code fix or omit it.
- Format all changed paths before tests:

  ```shell
  dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]
  ```

- Run the focused tests for the touched rule or phase as you go, not the full suite.
- Avoid unrelated cleanup. Keep broader concerns in the current PR worklist and report; never move them to a new issue.

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
git push
```

Do not create the trigger commit when no change was applied.

## Local self-review (before the official preflight)

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

## Synchronize with `origin/main` before the official gate

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

## Official preflight gate — hard 1 + 1 budget

After the accepted fixes are committed and pushed with `[skip ci]`, the local self-review is done, and `main` is synchronized, read `.codex/skills/gh-preflight/SKILL.md` completely and apply it as an internal, read-only gate against the current PR head. Do not post preflight findings to GitHub. Run it in a fresh, independent read-only subagent when subagents are available.

A **routine round** — one whose diff contains no production code at all (documentation, repository instructions, or workflow files only) and whose accepted findings touch no analyzer, formatter, code-fix, or `Reihitsu.Core` behavior — may skip the official preflight and go straight to full validation. Record the skip and its reason in the Validation block. Every other round spends at least attempt 1.

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

## Full validation — run it once

Before the first build or test, confirm the preinstalled SDK:

```shell
dotnet --list-sdks
```

Do not install an SDK or modify `PATH`. Focused tests run throughout the repair cycle; the complete suite runs **once**, after the fixes are in, `main` is synchronized, the official preflight has passed, and the worktree matches the audited head:

```shell
dotnet build Reihitsu.sln -c Release --verbosity minimal
dotnet test Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj -c Release --no-build --verbosity minimal
dotnet test Reihitsu.Formatter.Test/Reihitsu.Formatter.Test.csproj -c Release --no-build --verbosity minimal
dotnet test Reihitsu.Core.Test/Reihitsu.Core.Test.csproj -c Release --no-build --verbosity minimal
dotnet test Reihitsu.Cli.Test/Reihitsu.Cli.Test.csproj -c Release --no-build --verbosity minimal
```

`--no-build` is valid only because the Release build immediately above covered this exact tree; drop it and rebuild if any file changed since. All relevant tests must pass. Fix regressions caused by the review changes in focused `[skip ci]` commits and rerun the focused tests plus the failing project — not the whole suite. Never silence, ignore, or delete a test to obtain a green run. If the SDK is absent or the base branch has an independent failure, stop and report the evidence.

If the user explicitly asks to skip repeated local validation and rely on CI, obey that instruction and report exactly which local checks ran and which did not.

## Reply without resolving

After the commits are pushed, reply once to every addressed inline comment using the review comment's database id:

```shell
gh api --method POST repos/{owner}/{repo}/pulls/<N>/comments \
  -F in_reply_to=<comment-id> -f body='Addressed: <change> (<sha>).'
```

Use `gh pr comment <N> --body '<message>'` for a non-line hint. Keep replies concise and in English. Do not resolve any thread; its open state is the handshake for `gh-rereview`.

## Chat output

After completion, write only this structure, rendering `_None._` under empty sections:

```markdown
## Applied
| # | Source | Location | Commit | Change |
|---|--------|----------|--------|--------|
| 1 | reviewer | Reihitsu.Formatter/Pipeline/Foo.cs:42 | a1b2c3d | Preserve `#endif`; add a regression test |

## Skipped
| # | Source | Location | Reason |
|---|--------|----------|--------|
| 1 | reviewer | Reihitsu.Cli/Program.cs:120 | Pre-existing and outside this PR's scope |

## Needs decision
_None._

## Validation
- Local self-review: every worklist row checked; parity, convergence, idempotency, directives re-checked.
- Base sync: merged `origin/main` at `<sha>`; conflicts formatted and focused-tested.
- Official preflight: 1 attempt used, PASS; budget not exhausted. (State a skip and its reason here instead when the round changed no production code.)
- Build: green.
- Analyzer / Formatter / Core / CLI tests: green (one full run).

## Pushed
- PR #123, branch `codex/...`: two `[skip ci]` fix commits and trigger commit `Ready for CI (#123)`.
- Replied on threads #1 and #2; left them unresolved for `gh-rereview`.
```

List every item once. Give a reason for every skipped item. Include a `preflight` source row under Applied for each confirmed preflight finding that the parent fixed; these have no reviewer thread to reply to. Move answered decisions into Applied or Skipped; list only deferred decisions under Needs decision. The Validation block must state how many official preflight attempts were used, the result of each, and whether the budget was exhausted. If validation or push failed, state the exact failure instead of claiming success. Add no preamble or closing text.

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
- Never skip the test-first, idempotency, convergence, formatting, or validation requirements in `AGENTS.md`.
- Never start a third official preflight automatically; the budget is one attempt plus one retry.
- Never split one preflight worklist into several fix/preflight loops, and never run a preflight after every individual fix.
- Never run the official preflight on a knowingly stale or conflicting branch and merge `main` afterwards — synchronize first.
- Never start full validation or create the final CI-trigger commit until `gh-preflight` returns `PASS` for the current PR head — the only exception is a round with no production code in the diff, which records the skip. If the budget is exhausted without a `PASS`, stop and report; that is not a licence to proceed.
- Never install an SDK or modify `PATH`.
- Never push a non-`[skip ci]` commit before validation is green.
- Never stage unrelated paths, open another PR, or change the PR's draft state.
- Never search for or create a follow-up issue. Every review item remains attached to the current PR.
- Use authenticated `gh` for GitHub operations; do not use raw unauthenticated HTTP calls.
