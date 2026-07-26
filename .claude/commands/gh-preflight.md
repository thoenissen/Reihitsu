# GitHub PR Preflight

Run a read-only quality gate on GitHub Pull Request **#$ARGUMENTS** before external review or re-review.

`$ARGUMENTS` is optional. When it is empty, reuse the PR created or updated in the current author chat. Ask only when no PR can be identified.

Invoke the `gh-preflight` skill and follow it exactly:

1. Read the PR metadata, diff, changed files, and linked issue through the GitHub MCP server; use local `git log` / `git show` when commit history matters. Do not fetch prior review comments merely to copy another review, and never use `gh`.
2. Require the local checkout to match the PR head and all intended scoped changes to be committed and pushed. Report a blocking state mismatch instead of switching or repairing branches.
3. Read `.claude/skills/gh-review/SKILL.md` and apply its complete 19-item checklist, adversarial corpus, test expectations, counterpart tracing, defect-class closure, severity model, and static-first verification.
4. Post nothing to GitHub and do not edit files, commit, push, change branches, or change PR state.
5. Return `PASS` only when no confirmed high, medium, or low finding remains. Hints are non-blocking.
6. When a targeted test is needed to settle a concrete suspicion, use the review skill's .NET 10 setup only if the SDK is unavailable; never run the full suite.

A non-empty argument from which no valid PR number or URL can be extracted is an error. Ask rather than guess.
