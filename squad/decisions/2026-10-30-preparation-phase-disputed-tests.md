# Stability-check phase names and the documentation-comment line-ending fallback

| | |
|---|---|
| Date | 2026-10-30 |
| Escalation | 4 |
| Run | issue #1485 |

## Question

1. `PhaseStabilityCheckTests.CheckRunsTheProductionPhasesInPipelineOrder` compares observed phase names (`PreparationPhase/TabReplacement`, `PreparationPhase/LineEndings`, …) with production phase type names (`PreparationPhase`, …), which FMT-PIPE-001 makes unequal. (A the stability check runs and names the sub-phases under their reported names, the test keeps its assertion; B the test collapses observed names to `PreparationPhase`)
2. `DocCommentElementNormalizer.GetLineBreak` falls back to `Environment.NewLine`; with normalization moved to the front, a CRLF file ending in a documentation comment without a final line break gets `\n` written into it and changes on a second pass. Fixing it changes the signature four tests call. (A the test implementer adds a regression test and updates the four calls, the implementer then uses the detected line ending; B leave it for a follow-up)

## Answer

1A, 2A. Neither answer changes the spec.

## Reason

None given beyond accepting the recommendations.
