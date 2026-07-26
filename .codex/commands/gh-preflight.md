# GitHub PR Preflight

Run a read-only quality gate on GitHub Pull Request **#$ARGUMENTS** before external review or re-review.

`$ARGUMENTS` is optional. When it is empty, reuse the PR created or updated in the current author task, or the single PR associated with the current branch. Ask only when no PR can be identified.

Invoke the `gh-preflight` skill and follow it exactly:

1. Confirm GitHub identity and read the PR metadata, diff, changed files, commits, and linked issue through authenticated `gh`. Do not fetch prior review comments merely to copy another review.
2. Require the local checkout to match the PR head and all intended scoped changes to be committed and pushed. Report a blocking state mismatch instead of switching or repairing branches.
3. Read `.codex/skills/gh-review/SKILL.md` and apply its complete 19-item checklist, adversarial corpus, test expectations, counterpart tracing, defect-class closure, severity model, and static-first verification.
4. Post nothing to GitHub and do not edit files, commit, push, change branches, or change PR state.
5. Return `PASS` only when no confirmed high, medium, or low finding remains. Hints are non-blocking.
6. When a targeted test is needed to settle a concrete suspicion, confirm the preinstalled SDK with `dotnet --list-sdks`; never install one, modify `PATH`, or run the full suite.

A non-empty argument from which no valid PR number or URL can be extracted is an error. Ask rather than guess.
