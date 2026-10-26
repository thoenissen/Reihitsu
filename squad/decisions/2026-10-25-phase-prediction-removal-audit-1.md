# Node-level column step and comment lines in interpolation holes after the first audit

| | |
|---|---|
| Date | 2026-10-25 |
| Escalation | 12, 1 |
| Run | issue #1463 |

## Question

1. Audit finding 1 (architecture): the node-level step after the pipeline (`ComputeTokenColumn` / `AdjustNodeIndentation`) and the indentation phase both decide the final column of a line that starts inside a multi-line comment and of every line aligned to an anchor on it; the narrow RH5303-006/RH5304-005 guard covers only one member, and RH5301, RH5303, and RH5304 fixes with object creations, call arguments, `&&` operands, and arrow-expression targets misplace lines. Options: A — widen the guard to every fix that formats through the node-level path; B — change the structure first: the node-level path formats the target at its document column so no shift follows; C — exclude comment-tail anchors from FMT-INDENT-030; D — stop the run.
2. Audit finding 3: a comment line above a comment-led line inside an interpolation hole is now aligned to the leading comment (as on ordinary lines) instead of the token behind it. Options: A — keep the new output and align FMT-RAW-007, the change doc, and a test to it; B — restore the old output.

## Answer

1B, 2A. The maintainer also gave a standing instruction for the rest of the run: continue autonomously; for any further question take the lead's recommended option; commit a checkpoint before acting on each such decision so it can be rolled back; present every decision taken when the run is finished. The checkpoint before this decision is commit 724ce92.

## Reason

None given beyond accepting the recommendations; a rollback is acceptable in the worst case.
