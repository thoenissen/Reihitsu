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

## Lane widening

Widened from tests to tooling after the skeptic: the escalation-12 answer is recorded in `squad/decisions/`, and `scripts/lane-check.sh tests --head worktree` reports `LANE CHECK: mismatch — claimed tests, diff needs tooling` because the lane classifier counts every `squad/` path as tooling. The widening happened before any implementer started. The tooling lane adds the implementer and the auditor. Follow-up candidate: the lane classifier puts a run's own `squad/decisions/` entry in the tooling lane, although the playbook requires every run to commit one.

## Roles

- Spec author — skipped: the issue asks for no spec; the run adds a test-side check of existing behavior, not a spec of a surface. Re-evaluated if the skeptic or test implementer finds a spec gap.
- Skeptic — runs: premise check of the issue's statements about today's code against `main`, including the three listed reversal causes.
- Test implementer — runs: extends the helpers and writes the exception list.
- Implementer — skipped: the test implementer (`02-test-implementer.md`) delivered the whole change test-side and changed no production or tooling file, so no failing test was left to make pass.
- Cleaner — runs.
- Spec checker — runs.
- Auditor — runs after the widening to tooling.
- Validator — runs.

## Maintainer clarifications

- Escalation 12 (skeptic architecture findings: three pre-existing two-phase reversals): the maintainer chose to keep the run narrow, list the reversing fixtures as exceptions with their causes, and link the existing structural packages under thoenissen/Reihitsu#1482 as follow-up work instead of filing new issues.
- Escalation 7 (repair round 1, test identity): the maintainer chose to declare `TestContext` on `FormatterTestsBase`, remove it from the derived formatter test classes, and drop the call-stack walk.
- Escalation 12 (repair-round audit finding 10 repeats finding 2, colliding data-row keys): the maintainer chose the narrow fix — an injective row text for supported arguments and a clear failure at test start for unsupported ones — in repair round 2, together with findings 11 and 12.
