Role: lead
Commit: 9368956
Verdict: lane claimed

## Run

- Issue: thoenissen/Reihitsu#1483 — formatter tests check that no single phase changes the final output (package A2 of thoenissen/Reihitsu#1482).
- Slug: `phase-level-stability-check`
- Branch: `claude/admiring-fermat-6asu0n` (the session's designated branch, used instead of `claude/issue-1483-<slug>` because the session may push to no other branch). Claim: draft PR thoenissen/Reihitsu#1504.
- Base: `origin/main` at `735d0db`, the commit the issue was written against.

## Lane

Tests. The issue changes the formatter test helpers under `src/Reihitsu.Formatter.Test/Helpers/` and adds an exception list next to them; it asks for no change to shipped source and no new behavior. The issue itself names this lane.

## Roles

- Spec author — skipped: the issue asks for no spec; the run adds a test-side check of existing behavior, not a spec of a surface. Re-evaluated if the skeptic or test implementer finds a spec gap.
- Skeptic — runs: premise check of the issue's statements about today's code against `main`, including the three listed reversal causes.
- Test implementer — runs: extends the helpers and writes the exception list.
- Implementer — skipped: not in the tests lane.
- Cleaner — runs.
- Spec checker — runs.
- Auditor — skipped: not in the tests lane.
- Validator — runs.

## Maintainer clarifications

None.
