# Withheld fixes: the lane check rejects the run's decisions entry

| | |
|---|---|
| Date | 2026-10-27 |
| Escalation | 10 |
| Run | issues #1136, #1134, #1135, #1262, #1332, #1336, #1340 |

## Question

`scripts/lane-check.sh bug-fix` classified the run's decisions entry under `squad/decisions/` as tooling, so the claimed bug-fix lane failed, and shipped source next to tooling without a change doc is admitted by no lane. Options: A — widen the run to the behavior-change lane with a change doc, approved by the maintainer (escalation 2); B — keep the decisions entries off the branch and land them in a separate pull request.

## Answer

A — the maintainer reclassifies the run as a behavior change and allows change docs for it, overriding the earlier triage notes on #1262 and #1340 that ruled a change doc out. The widening happens before the implementer has started.

## Reason

None given; the maintainer did not want to change the workflow, and the run removes diagnostics (RH7003, RH7304) and withholds fixes that were offered before.
