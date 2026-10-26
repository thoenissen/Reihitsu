# Complex element behind the last line of a multi-line comment in the node-level path

| | |
|---|---|
| Date | 2026-10-25 |
| Escalation | 5, 12 |
| Run | issue #1463 |

## Question

The RH5303/RH5304 code fixes format the `EqualsValueClauseSyntax` through `FormatNodeInDocumentAsync`. For a complex element whose brace line starts inside the last line of a multi-line comment, the node-level shift after the pipeline (`AdjustNodeIndentation`) cannot move that line, while the indentation phase already measured from the brace's real column, so the element ends six columns too far right and the fix does not converge (RH5303-005). The node-level shift and the indentation phase disagree on a line's final column (architecture finding); the change doc lists the node-level column steps as not changed. Options: A — narrow: the RH5303/RH5304 fixes withhold themselves for this shape, the gap is recorded in the rule specs, the node-level tests that assert the excluded behavior are replaced, and the node-level column ownership becomes a follow-up; B — change the node-level path first so the target is formatted at its document column; C — stop the run.

## Answer

A.

## Reason

None given; the lead's recommendation was that the shape is rare, disappears with the line break decided in issue #1382, and that the node-level path deserves its own reviewed restructuring.
