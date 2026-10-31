# Raw-string move: indentation in front of a hole that starts a content line

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 12 |
| Run | issue #1488 |

## Question

The second audit found that the token-editing raw-string move, for an interpolated raw string whose closing marker is in column 0, writes a moved content line's indentation as whitespace trivia in front of a hole's `{` when the line starts with that hole; parsed again, the indentation is an interpolated-string text token, so the phase tree breaks FMT-PIPE-021. The final text equals `main`'s. It shares its mechanism and input axis with the first audit's wrong-token-value finding on the same move. Options: A repair it in round 2 after the test implementer enumerates the whole class (line-start kind, `$`/`$$`, closing-marker column, move direction, line ending), keeping the token-editing move; B reparse the moved literal and copy annotations, amending the change doc; C stop the run.

## Answer

A — repaired in round 2, with the whole class enumerated first and fresh role agents for the round. The `///` separator wording and direct Core tests for the would-merge helper stay out of this round as follow-up candidates.

## Reason

Cost: a focused round with fresh agents is cheaper than a separate run, and the visible output (the hole's `{` at the column text would take) is unchanged.
