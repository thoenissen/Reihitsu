# The RH5405 code fix drops the comment of `else if (y) // note` on main

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 9 |
| Run | issue #1489 |

## Question

`scripts/apply-fix.sh RH5405` on `else if (y) // note` followed by an unbraced statement drops `// note` and places the braces in column 13, on `main` and independently of the run. (A separate follow-up bug, closed by this run if its facts happen to fix it; B fix it in this run)

## Answer

A — a separate follow-up bug, whose text the maintainer approves before it is created.

## Reason

None given beyond accepting the recommendation.
