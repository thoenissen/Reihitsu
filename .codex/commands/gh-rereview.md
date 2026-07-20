# GitHub PR Re-Review

Re-review GitHub Pull Request **#$ARGUMENTS** after the author addressed a previous `gh-review` pass.

`$ARGUMENTS` is optional. When it is empty, reuse the PR most recently handled by `gh-review` or `gh-rereview` in the current Codex task. Ask only when no PR can be identified.

Invoke the `gh-rereview` skill and follow it exactly. It supports Linux cloud and local Windows. Use authenticated `gh` for every GitHub operation:

1. Confirm the reviewer identity, then rebuild the baseline from that reviewer's GitHub inline/general comments, GraphQL thread state, and any findings table still in the task.
2. Read `.codex/skills/gh-review/SKILL.md` and re-run its complete review methodology against the current metadata, diff, changed files, linked issue, and counterpart files. The diff is not the review boundary.
3. Reconcile every prior finding against current code as `resolved` or `open`; do not trust author claims or resolved flags without verification. Treat unmatched current findings as `new`.
4. Reply to and resolve verified fixed threads, reply on still-open threads, reopen prematurely resolved threads, and post new high-confidence findings under `gh-review`'s rules.
5. Prefer static tracing. If a targeted run changes a classification, confirm the preinstalled SDK with `dotnet --list-sdks` and run only the filtered test or formatter double-run needed. Never install an SDK or modify `PATH`.
6. Report only the re-review block: the complete 19-item Checklist, Prior findings, New findings, Verification, and Hints.

A non-empty argument from which no valid PR number or URL can be extracted is an error. Ask rather than guess.
