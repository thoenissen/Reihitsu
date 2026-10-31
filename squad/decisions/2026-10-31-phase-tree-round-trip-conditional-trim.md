# Conditional trim of whitespace before an inserted line break

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 12 |
| Run | issue #1488 |

## Question

`LineBreakTriviaUtilities.MoveTokenToNewLine` trims the previous token's trailing whitespace only when that token lies inside the rewritten node, while the cleanup phase removes it everywhere (FMT-CLEAN-001). Options: A keep the trim with FMT-BREAK-029 as drafted and file a follow-up; B remove the conditional trim in this run so the cleanup phase is the only owner; C stop the run.

## Answer

B — the conditional trim is removed in this run; only the line-break phase's intermediate output changes.

## Reason

None given beyond accepting the recommendation.
