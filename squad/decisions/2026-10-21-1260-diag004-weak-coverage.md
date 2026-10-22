# SHARED-DIAG-004 without a test that can fail (#1260)

| | |
|---|---|
| Date | 2026-10-21 |
| Escalation | 1 |
| Run | issue #1260 |

## Question

SHARED-DIAG-004 (P15) has no conforming shape a test could fail on: the RH2102 and RH2004 examples pass for reasons other than the principle, and every shape that separates a judgement on the symbol from a per-part check is reported per part today (#1287). Options: write it and tag the weak shapes as covering tests; or defer DIAG-004 to #1287.

## Answer

Write SHARED-DIAG-004 and tag the RH2102/RH2004 shapes as its covering tests; the run report states that they cannot fail and that real coverage comes with #1287.

## Reason

none given.
