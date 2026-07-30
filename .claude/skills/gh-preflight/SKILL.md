---
name: gh-preflight
description: Run a read-only, local preflight review of a Reihitsu pull request before external review or re-review in a Linux Claude Code Cloud Agent. Use for `/gh-preflight`, "preflight this PR", "review before review", or as the mandatory final quality gate inside `gh-implement` and `gh-apply-review` — invoked once their implementation, local self-review, and `origin/main` synchronization are done, and before their single full validation and CI-trigger commit. Apply the complete repository `gh-review` checklist, adversarial corpus, test expectations, counterpart tracing, and defect-class closure without posting through GitHub MCP or changing code. Report every confirmed finding in one pass, because the parent gets one consolidated repair cycle and at most one retry attempt. Return a blocking gate when any confirmed finding remains.
---

# Reihitsu GitHub PR Preflight

Audit the current PR as an independent reviewer before the author declares it ready. Find the issues that would otherwise create another `gh-review` / `gh-apply-review` round.

Preflight is the **final quality gate**, not a discovery or debugging loop. The parent workflow is expected to arrive here with its Behavior Contract satisfied, its local self-review done, and current `origin/main` already merged, so this audit confirms a finished change instead of driving it. That is also why the parent may spend at most two official attempts on it: one, plus one retry after a single consolidated repair cycle.

Preflight is read-only:

- Do not edit source or test files.
- Do not create commits, push, change branches, or change PR state.
- Do not post reviews, comments, replies, or thread mutations through GitHub MCP.
- Targeted tests may create normal build artifacts when they settle a specific finding.

## Position in the workflow

```text
gh-implement / gh-apply-review
  -> Behavior Contract (gh-implement) or complete review worklist (gh-apply-review)
  -> implementation + focused tests
  -> local self-review (parent, not official)
  -> merge current origin/main, format, focused tests, push
  -> gh-preflight  ........................ official attempt 1
       PASS    -> full validation (once) -> final CI-trigger commit
       BLOCKED -> one consolidated worklist -> one repair cycle
                  -> local self-review
                  -> gh-preflight  ......... official attempt 2 (the retry, fresh agent, new head)
                       PASS    -> full validation (once) -> final CI-trigger commit
                       BLOCKED -> stop and report; no third attempt without the user
```

When a parent workflow invokes this skill, return the gate result to that workflow and let it continue. Use the strict chat output below only when `/gh-preflight` is invoked directly.

## Calling conditions

A parent should invoke preflight only when all of the following hold. Preflighting earlier burns one of the two attempts on a state that was going to change anyway:

- the intended implementation or review repair cycle is complete and committed;
- the parent's local self-review has run and its findings are fixed;
- current `origin/main` is merged into the branch and any conflict resolution is formatted and focused-tested;
- the head is pushed and the local checkout matches it.

## Reviewer isolation

When `gh-implement` or `gh-apply-review` invokes preflight and a subagent facility is available, run the audit in exactly one fresh read-only subagent with no author transcript. Give it only the repository root, PR identifier, current head SHA, and this skill path. Do not pass the author's conclusions, suspected findings, or intended fixes. The parent remains the only writer and consumes the subagent's gate report.

The retry attempt gets its own fresh subagent on the exact new head — never a continuation of the first one, which would carry its earlier conclusions into a review that is supposed to be independent.

If subagents are unavailable, perform the audit locally from GitHub and filesystem evidence. A direct `/gh-preflight` invocation already acts as the reviewer and does not need another agent.

## Resolve the PR

Resolve the PR in this order:

1. Use an explicit number, `#number`, or PR URL from the prompt or `$ARGUMENTS`.
2. Otherwise, reuse the PR created or updated by the current author chat.
3. If no PR can be identified, stop. Never guess.

The sandbox has no `gh` CLI. Use the GitHub MCP server for read-only GitHub access; use `ToolSearch` if the tools are not loaded:

| Purpose | MCP tool |
|---|---|
| Identity | `mcp__github__get_me` |
| PR metadata and head SHA | `mcp__github__pull_request_read` (get) |
| PR diff | `mcp__github__pull_request_read` (get_diff) |
| Changed files | `mcp__github__pull_request_read` (get_files) |
| Linked issue | `mcp__github__issue_read` |

Read a linked issue only when the PR body contains `Closes`, `Fixes`, or `Resolves`.

Also run:

```bash
git status --short
git rev-parse HEAD
git log --oneline origin/<base-branch>..HEAD
```

The gate requires the local checkout to match the PR head SHA and all intended scoped changes to be committed and pushed. If it does not, return `BLOCKED — state mismatch` with the exact mismatch. Do not switch branches or repair the state from this skill.

## Apply the review methodology

Read `.claude/skills/gh-review/SKILL.md` completely. Apply its complete methodology to the current PR:

- all 19 checklist items;
- the relevant adversarial input corpus;
- test expectations for every changed concern;
- severity and confidence rules;
- counterpart tracing beyond the diff;
- static-first verification.

Override the review skill's GitHub-posting, existing-comment deduplication, and output rules with this skill. Review the current code independently; do not fetch prior review comments merely to learn what another reviewer found.

Limit blocking findings to defects caused by the PR, missing issue requirements, incomplete tests required by the change, and pre-existing behavior that the changed code newly depends on or exposes. Record unrelated pre-existing concerns as hints rather than expanding the PR.

Complete the entire checklist and relevant adversarial corpus before returning. Report every confirmed finding in one pass; never stop after the first.

## Prove defect-class closure

For every bug fix or review fix:

1. State the general defect class, not only the reported counterexample.
2. Search for sibling syntax shapes and private copies of the same policy.
3. Trace wrappers, aliases, nested executable scopes, repeated tokens, and target re-resolution when relevant.
4. Verify the regression test reproduces the actual failure shape.
5. Verify the relevant matrix:
   - token or trivia changes: comments, directives, and disabled text;
   - formatter changes: LF, CRLF, second-pass idempotency, and neighboring phases;
   - code fixes: one-pass convergence, multiple-diagnostic Fix All, and target identity after earlier edits;
   - analyzer / formatter / fix changes: both directions of counterpart parity;
   - naming fixes: Roslyn Renamer reference retargeting.

A narrow guard for one example is not closure when sibling shapes retain the same hazard.

## Verification

Default to static tracing. Run only targeted tests or formatter double-runs that resolve a concrete suspicion. If execution is required, probe `dotnet --list-sdks` first. Install the .NET 10 SDK through the same `dotnet-install.sh` procedure defined by `.claude/skills/gh-review/SKILL.md` only when `10.*` is unavailable. Do not run the full solution test suite; the parent workflow owns full validation after this gate passes.

## Gate decision

- `PASS`: no confirmed `high`, `medium`, or `low` finding remains.
- `BLOCKED — findings`: at least one confirmed finding remains.
- `BLOCKED — state mismatch`: the checkout, PR head, or intended committed scope cannot be reviewed reliably.
- Hints do not block the gate.

Report every confirmed finding in the one gate report. The parent has a single repair cycle to work from it, so a finding withheld for a "next round" never gets one.

When invoked by `gh-implement` or `gh-apply-review`, the parent owns the budget:

1. It merges all confirmed findings into one consolidated worklist and fixes every in-scope item — including the complete defect class — in a single repair cycle, without invoking preflight in between.
2. It formats changed paths, runs focused tests, redoes its local self-review, commits with `[skip ci]`, pushes, and updates the PR body when needed.
3. It asks the user about architecturally significant, public-API-changing, dependency-changing, contested, or out-of-scope findings.
4. It spends the **preflight retry** — one fresh, independent, read-only run against the exact new head. `BLOCKED — state mismatch` does not consume an attempt; reconcile the state and rerun.
5. It proceeds to full validation only after `PASS`, and it stops and reports if the retry blocks. A third official attempt requires explicit user direction.

## Direct chat output

For direct `/gh-preflight` invocations, write only:

```markdown
## Gate
PASS

## Scope
- PR #123 at `<head-sha>`; local checkout matches.
- Reviewed changed files, linked issue requirements, and named counterpart files.

## Checklist
<all 19 gh-review checklist items>

## Findings
_None._

## Verification
- Static tracing only; no targeted execution needed.

## Hints
_None._
```

For findings, set the gate to `BLOCKED — findings` and use:

```markdown
| # | Severity | Location | Defect class | Required change |
|---|----------|----------|--------------|-----------------|
| 1 | high | Reihitsu.Formatter/Pipeline/Foo.cs:42 | Cross-scope label relocation | Model executable scopes and add the nested-scope regression |
```

Keep every confirmed finding in the table exactly once. Provide a concrete counterexample for each high finding. Do not add a preamble or closing text.

## Hard rules

- Never edit tracked files or mutate GitHub or repository history.
- Never use `gh` or raw unauthenticated GitHub API calls.
- Never mark `PASS` with a confirmed finding.
- Never review only the diff when a counterpart or pipeline neighbor is relevant.
- Never accept a test-only or paper fix when the defect class remains open.
- Never run the full validation suite from preflight.
- Never hold a confirmed finding back for a later round; the parent has one consolidated repair cycle and one retry.
- Never act as the parent's discovery loop — it arrives with its local self-review done and `origin/main` merged.
- Never create or search for follow-up issues.
