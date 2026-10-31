# Phase reversal on a comment in front of a converted member body

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 4 |
| Run | issue #1488 |

## Question

`StructuralTransformSeparatorFinalOutputTests.MembersWithACommentAtTheArrowReachAStableOutput` fails its phase-stability check: on the final `/* c */ {` of a member whose comment stood in front of the arrow, the blank-line phase inserts a blank line and the line-break phase removes it, as it does on `main`. Options: A record the reversal in `PhaseReversalExceptionList`, removed by the follow-up on the arrow-comment layout; B remove the shape from the test input; C fix the reversal in this run.

## Answer

A — record the reversal in `PhaseReversalExceptionList`; the follow-up on the arrow-comment layout removes the entry.

## Reason

None given beyond accepting the recommendation.
