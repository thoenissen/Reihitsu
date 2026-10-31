# Reach of FMT-BREAK-029 beyond the conditional trim

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 5 |
| Run | issue #1488 |

## Question

After the conditional trim in `MoveTokenToNewLine` was removed, `TokenGapNormalizer.NormalizeGapBeforeToken`/`NormalizeGapBeforeOwnedToken` (a shared helper with about 25 call sites) and `LineBreakInitializerRewriter.CleanupTrailingWhitespaceBeforeToken` still remove whitespace in front of a line break by rewriting the previous token's trailing trivia. Options: A keep them, state in FMT-BREAK-029 that it covers only line breaks written into the moved token's leading trivia, and draft a follow-up; B remove them in this run too.

## Answer

A — they stay; FMT-BREAK-029 states its narrower reach and the rest is a follow-up candidate.

## Reason

None given beyond accepting the recommendation.
