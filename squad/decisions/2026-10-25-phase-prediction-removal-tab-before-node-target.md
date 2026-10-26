# Column of a node-level target behind a whitespace tab (revisits Q2 of the third amendment)

| | |
|---|---|
| Date | 2026-10-25 |
| Escalation | 1 |
| Run | issue #1463 |

## Question

The skeptic (`20-skeptic.md`, C1) showed that counting a tab in front of a node-level target as four columns (FMT-PIPE-017, Q2 A of `2026-10-25-phase-prediction-removal-third-amendment.md`) makes the RH5303/RH5304/RH5301 fixes non-converging on tab-indented code: the analyzers count characters, so they report the fix's own output (SHARED-FIX-001, SHARED-FIX-005). Options: A — keep four columns and add an exception to SHARED-FIX-001/005; B — count characters for whatever lies outside the target, and state that SHARED-FIX-007 does not reach whitespace tabs outside the code the fix changes; C — withhold these fixes when a whitespace tab stands in front of the target's first line; D — make the alignment analyzers count a tab as four columns (wider scope).

## Answer

B — taken by the lead as its recommended option under the maintainer's standing instruction of 2026-10-25. This reverses Q2 A, which was taken before this conflict was known. Checkpoint before this decision: 0e22387.

## Reason

B keeps today's converging behavior of the fixes and the agreement between fix and analyzer without an exception to SHARED-FIX-001; the tabs outside the target are not code the fix changes, and once the formatter replaces them it lays out the whole line consistently. C would give away safe fixes, D widens the run into every alignment analyzer, and A keeps a non-converging fix.
