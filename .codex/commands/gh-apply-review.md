# GitHub PR Apply Review

Apply the review on GitHub Pull Request **#$ARGUMENTS**: implement the reviewer's findings and the user's extra hints, validate, and push.

Run this in the PR author's Codex task, normally the same task that ran `gh-implement`. This is the fix step; `gh-review` and `gh-rereview` run in the reviewer's task.

`$ARGUMENTS` is optional. When it is empty, reuse the PR created by `gh-implement` in the current task. Ask only when no PR can be identified.

Invoke the `gh-apply-review` skill and follow it exactly. It supports Linux cloud and local Windows. Use authenticated `gh` for GitHub operations and local `git` for the branch:

1. Confirm identity and fetch PR metadata, GraphQL review-thread state, inline comments, general comments, diff, files, and commits in one batch. Build **one complete, deduplicated worklist before editing anything**: unresolved reviewer threads, review summary bodies, user-authored PR hints, every line of a pasted `gh-review` Copy block, relevant task clarifications, and any unfixed findings from a previous official preflight. Do not start editing after reading only the first thread.
2. Classify every item exactly once as fix, skip (with reason), or needs decision. Ask the user before acting on ambiguous, contested, architecturally significant, public-API-changing, or dependency-changing feedback. When a review item introduces a materially ambiguous behavior change, `gh-rubber-duck` is the read-only way to settle it first — recommended, not mandatory, here. Never move an item to a new issue; keep it attached to the current PR.
3. Implement all `fix` items as **one cohesive repair cycle** on the existing PR head branch under `AGENTS.md`: generalize each finding to its defect class, inspect sibling shapes and shared helpers, reproduce analyzer/formatter bugs with a failing test first, add required idempotency/convergence coverage, format changed paths with `Reihitsu.Cli`, and run the focused `--filter` tests as you go.
4. Commit and push the fixes with `[skip ci]`, then run the **local self-review**: every worklist row, counterpart parity, defect-class closure, convergence, idempotency, comments and directives, documentation, changed-path formatting, focused tests. It is not an official preflight.
5. **Synchronize the base before the gate**: `git fetch origin main`, check worktree and branch state, merge current `origin/main` when the branch is behind, resolve conflicts so both sides' behavior survives, format and focused-test the resolutions, and push the synchronized head with `[skip ci]`.
6. Run the read-only `gh-preflight` skill against that exact head as a fresh independent subagent — **official attempt 1**. A round with no compiled file in the diff and no accepted finding touching analyzer, formatter, code-fix, or Core behavior skips this gate and the full validation alike, and records both skips. On `BLOCKED — findings`, merge every finding into **one** worklist with anything still open, fix it in **one** repair cycle, redo the local self-review, then spend the single **preflight retry** on the new head. If the retry blocks too, stop and report; never start a third official preflight automatically. Do not post preflight findings to GitHub.
7. Run `dotnet --list-sdks` to confirm the preinstalled .NET 10 SDK; never install one or modify `PATH`. After `PASS`, build the solution and run all four test projects **once** (`--no-build` is fine because the Release build covered the same tree).
8. After preflight and validation are green, push one final trigger commit without `[skip ci]`.
9. Reply once to every addressed thread with the change and commit SHA. Do not resolve threads; verification and resolution belong to `gh-rereview`.
10. Report only the Applied / Skipped / Needs decision / Validation / Pushed block defined by the skill. The Validation block states the local self-review, the base sync, the official preflight attempts used with each result and whether the budget was exhausted, and the single full-validation run.

A non-empty argument from which no valid PR number or URL can be extracted is an error. Ask rather than guess.
