# Remove the copy of the tab rule in the node-level column measurement

| | |
|---|---|
| Date | 2026-10-30 |
| Escalation | 12 |
| Run | issue #1485 |

## Question

1. The rule that a tab in a node-level target's own leading whitespace counts as four columns is applied by `TabReplacementPhase` and predicted beforehand by `ReihitsuFormatterHelpers.ComputeDocumentColumn`/`IsInOwnLeadingWhitespace`. How does the run handle the duplicate? (A fix narrowly and file the structural change as a follow-up; B change the structure first in this run — measure the column from the preparation phase's result and remove the copy, with the FMT-PIPE-017 tests passing unchanged; C stop the run)
2. Line breaks kept inside multi-line comments and disabled text (lone `\r`, U+0085, U+2028, U+2029) already change layout today. Draft a follow-up issue? (A yes, shown before creation; B only mention it in the PR)

## Answer

1B, 2A.

## Reason

None given beyond accepting the recommendations.
