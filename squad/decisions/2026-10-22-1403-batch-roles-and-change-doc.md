# Which roles does a run of several issues spawn, and is the squad change doc amended?

| | |
|---|---|
| Date | 2026-10-22 |
| Escalation | none of 1–10; the auditor's finding needed a design choice the earlier entry did not make |
| Run | issue #1403 |

## Question

1. A run of several issues claims the narrowest lane that admits them all and runs only that lane's roles, so an issue can lose a role its own lane would run — for example a rule-page text fix together with a `scripts/` change claims tooling, which runs no spec author. Options: run the union of the claimed lane's roles and each issue's own-lane roles, giving each issue the duties of its own lane; or run the claimed lane's roles only and escalate when an issue's work does not fit them.
2. `sdd/changes/2026-10/establish-squad.md` still lists escalations 1–10 and the single-issue invocation. Options: leave it as history; or amend it in place.

## Answer

1. Run the union: every role of the claimed lane plus every role each issue's own lane runs, and each issue receives the duties of its own lane (for example a bug in the batch is still reproduced first).
2. Leave the change doc unchanged.

## Reason

none given
