# GitHub PR Apply Review

Apply the review on GitHub Pull Request **#$ARGUMENTS**: implement the reviewer's findings and the user's extra hints, validate, and push.

Run this in the PR author's Codex task, normally the same task that ran `gh-implement`. This is the fix step; `gh-review` and `gh-rereview` run in the reviewer's task.

`$ARGUMENTS` is optional. When it is empty, reuse the PR created by `gh-implement` in the current task. Ask only when no PR can be identified.

Invoke the `gh-apply-review` skill and follow it exactly. It supports Linux cloud and local Windows. Use authenticated `gh` for GitHub operations and local `git` for the branch:

1. Confirm identity and fetch PR metadata, GraphQL review-thread state, inline comments, general comments, diff, files, and commits. Build a deduplicated worklist from unresolved reviewer findings, user-authored PR comments, a pasted `gh-review` Copy block, and current task context.
2. Classify every item as fix, skip, or needs decision. Ask the user before acting on ambiguous, contested, architecturally significant, public-API-changing, or dependency-changing feedback. Never move an item to a new issue; keep it attached to the current PR.
3. Implement accepted items on the existing PR head branch under `AGENTS.md`: reproduce analyzer/formatter bugs with a failing test first, add required idempotency/convergence coverage, and format changed paths with `Reihitsu.Cli`.
4. Run `dotnet --list-sdks` to confirm the preinstalled .NET 10 SDK; never install one or modify `PATH`. Build the solution and run all four test projects.
5. Keep CI quiet with `[skip ci]` on every fix commit. After validation is green, push one final trigger commit without the marker.
6. Reply once to every addressed thread with the change and commit SHA. Do not resolve threads; verification and resolution belong to `gh-rereview`.
7. Report only the Applied / Skipped / Needs decision / Validation / Pushed block defined by the skill.

A non-empty argument from which no valid PR number or URL can be extracted is an error. Ask rather than guess.
