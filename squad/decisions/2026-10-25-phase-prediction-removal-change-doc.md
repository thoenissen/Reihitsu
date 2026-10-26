# Approve the change doc that removes column predictions between formatter phases

| | |
|---|---|
| Date | 2026-10-25 |
| Escalation | 2 |
| Run | issue #1463 |

## Question

1. Approve What and Why of `sdd/changes/2026-10/phase-prediction-removal.md`? (A approve, B approve with changes, C reject)
2. Where does whitespace-tab replacement live? (A a phase of its own, first in the pipeline; B a step inside `StructuralTransformPhase`)
3. How do raw-string alignment and indentation get one owner of a line's final column? (A fold raw-string alignment into `IndentationPhase`; B run the raw-string phase before anchors are measured)
4. Does `RootListPosition`, which anticipates a blank-line decision of the line-break phase, get a follow-up issue? (A draft one; B no, keep it in the inventory only)

## Answer

1A, 2A, 3A, 4B. For 3A the maintainer asked whether the larger indentation phase brings new problems of its own; the run's skeptic checks that concern before tests are written.

## Reason

None given beyond accepting the recommendations.
