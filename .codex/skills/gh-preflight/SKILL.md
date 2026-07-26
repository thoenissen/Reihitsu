---
name: gh-preflight
description: Run a read-only, local preflight review of a Reihitsu pull request before external review or re-review. Use for `/gh-preflight`, "preflight this PR", "review before review", or as the mandatory quality gate inside `gh-implement` and `gh-apply-review` before their final validation and CI-trigger commit. Apply the complete repository `gh-review` checklist, adversarial corpus, test expectations, counterpart tracing, and defect-class closure without posting to GitHub or changing code. Return a blocking gate when any confirmed finding remains.
---

# Reihitsu GitHub PR Preflight

Audit the current PR as an independent reviewer before the author declares it ready. Find the issues that would otherwise create another `gh-review` / `gh-apply-review` round.

Preflight is read-only:

- Do not edit source or test files.
- Do not create commits, push, change branches, or change PR state.
- Do not post reviews, comments, replies, or thread mutations.
- Targeted tests may create normal build artifacts when they settle a specific finding.

## Position in the workflow

```text
gh-implement
  -> gh-preflight
  -> author fixes every confirmed finding
  -> gh-preflight again until PASS
  -> full validation
  -> final CI-trigger commit

gh-apply-review
  -> gh-preflight
  -> author closes the complete defect class
  -> gh-preflight again until PASS
  -> full validation
  -> final CI-trigger commit
```

When a parent workflow invokes this skill, return the gate result to that workflow and let it continue. Use the strict chat output below only when `/gh-preflight` is invoked directly.

## Reviewer isolation

When `gh-implement` or `gh-apply-review` invokes preflight and subagents are available, run the audit in exactly one fresh read-only subagent with no author transcript. Give it only the repository root, PR identifier, current head SHA, and this skill path. Do not pass the author's conclusions, suspected findings, or intended fixes. The parent remains the only writer and consumes the subagent's gate report.

If subagents are unavailable, perform the audit locally from GitHub and filesystem evidence. A direct `/gh-preflight` invocation already acts as the reviewer and does not need another agent.

## Resolve the PR

Resolve the PR in this order:

1. Use an explicit number, `#number`, or PR URL from the prompt or `$ARGUMENTS`.
2. Otherwise, reuse the PR created or updated by the current author task.
3. Otherwise, use the PR associated with the current branch when `gh pr view` identifies exactly one.
4. If no PR can be identified, stop. Never guess.

For a repository other than the current `origin`, pass `--repo <owner>/<repo>` to every `gh` command.

Read current state without mutating GitHub:

```shell
gh auth status
gh pr view <N> --json number,title,body,author,baseRefName,headRefName,headRefOid,url,files,commits
gh pr diff <N>
git status --short
git rev-parse HEAD
git log --oneline origin/<base-branch>..HEAD
```

Use `gh issue view` only for an issue linked by `Closes`, `Fixes`, or `Resolves` in the PR body.

The gate requires the local checkout to match the PR head SHA and all intended scoped changes to be committed and pushed. If it does not, return `BLOCKED — state mismatch` with the exact mismatch. Do not switch branches or repair the state from this skill.

## Apply the review methodology

Read `.codex/skills/gh-review/SKILL.md` completely. Apply its complete methodology to the current PR:

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

Default to static tracing. Run only targeted tests or formatter double-runs that resolve a concrete suspicion. Before execution, confirm the preinstalled SDK with:

```shell
dotnet --list-sdks
```

Do not install an SDK or modify `PATH`. Do not run the full solution test suite; the parent workflow owns full validation after this gate passes.

## Gate decision

- `PASS`: no confirmed `high`, `medium`, or `low` finding remains.
- `BLOCKED — findings`: at least one confirmed finding remains.
- `BLOCKED — state mismatch`: the checkout, PR head, or intended committed scope cannot be reviewed reliably.
- Hints do not block the gate.

When invoked by `gh-implement` or `gh-apply-review`:

1. The parent fixes every in-scope confirmed finding.
2. It formats changed paths, runs focused tests, commits with `[skip ci]`, pushes, and updates the PR body when needed.
3. It asks the user about architecturally significant, public-API-changing, dependency-changing, contested, or out-of-scope findings.
4. It reruns preflight from current GitHub and filesystem state.
5. It proceeds to full validation only after `PASS`.

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
- Never mark `PASS` with a confirmed finding.
- Never review only the diff when a counterpart or pipeline neighbor is relevant.
- Never accept a test-only or paper fix when the defect class remains open.
- Never run the full validation suite from preflight.
- Never create or search for follow-up issues.
