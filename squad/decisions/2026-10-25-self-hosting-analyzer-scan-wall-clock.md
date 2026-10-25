# The analyzer self-hosting scan is not faster after the split into rows

| | |
|---|---|
| Date | 2026-10-25 |
| Escalation | 7 |
| Run | issue #1467 |

## Question

The auditor measured, on a 4-core sandbox, 15 m 13 s for the old single analyzer self-hosting test and 16 m 01 s for the new 340 per-analyzer rows, so the issue's "wall-clock drops measurably" condition did not hold for the analyzer scan; the formatter scans dropped from 4 m 41 s to 2 m 29 s. The lead reported this together with the escalation-12 question and planned a repair round for it.

## Answer

The performance goal of the issue is mainly about what the default local test run executes, and that run no longer contains the scans, so it is faster. If parallel rows cannot gain more in the sandbox, that is accepted; no repair for the analyzer scan's wall-clock time.

## Reason

The default run is what matters for local test time.
