# Fix the end-of-file #endregion without a final line break in the line-start run

| | |
|---|---|
| Date | 2026-10-30 |
| Escalation | 9 |
| Run | issue #1487 |

## Question

An `#endregion` behind the last `}` of a file without a final line break keeps its column on `main` as well, although SHARED-BLANK-008 says the trivia after the last `}` is placed. (A fix it in this run; B weaken the test to the final-line-break variant and draft a follow-up)

## Answer

A — the run fixes it as a member of the end-of-file class SHARED-BLANK-008 names.

## Reason

None given beyond accepting the recommendation.
