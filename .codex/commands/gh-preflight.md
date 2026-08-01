# GitHub PR Preflight

Run a read-only quality gate on GitHub Pull Request **#$ARGUMENTS** before external review or re-review.

This is the **final** gate, not a discovery loop: the implementation or repair cycle is finished, the parent's local self-review has run, and current `origin/main` is already merged into the branch. `gh-implement` and `gh-apply-review` may spend at most two official attempts on it — one, plus one retry after a single consolidated repair cycle.

`$ARGUMENTS` is optional. When it is empty, reuse the PR created or updated in the current author task, or the single PR associated with the current branch. Ask only when no PR can be identified.

Invoke the `gh-preflight` skill and follow it exactly:

1. Confirm GitHub identity and read the PR metadata, diff, changed files, commits, and linked issue through authenticated `gh`. Do not fetch prior review comments merely to copy another review.
2. Require the local checkout to match the PR head and all intended scoped changes to be committed and pushed. Report a blocking state mismatch instead of switching or repairing branches.
3. Read `.codex/skills/gh-review/SKILL.md` and apply its complete 19-item checklist, adversarial corpus, test expectations, counterpart tracing, defect-class closure, severity model, and static-first verification.
4. Build the required three-axis audit. For every changed predicate, guard, or policy, name its exact inspected span, the existing counterpart predicate for the same decision, and whether their boundaries agree. Enumerate every policy owner by `rg`; multiple owners are a finding unless the diff justifies them. For every new or materially changed test, name its invariant, the observation that would falsify it, and why the assertion/helper is strong enough. Missing rows or weaker assertions block `PASS` even when tests are green.
5. Post nothing to GitHub and do not edit files, commit, push, change branches, or change PR state.
6. Return `PASS` only when no confirmed high, medium, or low finding remains and the three-axis audit is complete. Render an explicit N/A reason for an axis with no applicable row. Hints are non-blocking, but every confirmed unrelated pre-existing concern must appear there with its defect mechanism and `new mechanism` relation so the author workflow can preserve it. Report every blocking finding in this one pass with its scope relation because the parent gets a single consolidated repair cycle and one retry.
7. When a targeted test is needed to settle a concrete suspicion, confirm the preinstalled SDK with `dotnet --list-sdks`; never install one, modify `PATH`, or run the full suite.

A non-empty argument from which no valid PR number or URL can be extracted is an error. Ask rather than guess.
