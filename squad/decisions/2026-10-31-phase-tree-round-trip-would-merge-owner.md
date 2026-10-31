# Two owners of the would-merge check for adjacent tokens

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 12 |
| Run | issue #1488 |

## Question

The formatter's `TokenGapUtilities.WouldMergeWhenTight` (a character heuristic, now also behind the created `return` separator) and the RH3002 code fix's private `WouldMergeIntoOneToken` (lexer-based) both decide whether two tokens written without a space read as one, and they disagree (`return` + `a` fuses into `returna`; `return` + `@a` gets an unneeded space). Options: A fix narrowly — the `return` separator asks the lexer — and file a follow-up; B make one lexer-based owner in a shared place used by the list join, the `return` separator, and the RH3002 fix, coming back if an existing test output changes; C stop the run.

## Answer

B — one lexer-based owner for the formatter's list join, the `return` separator, and the RH3002 fix; a changed existing test output comes back to the maintainer.

## Reason

None given beyond accepting the recommendation.
