# Data-row keys that collide after the first repair round

| | |
|---|---|
| Date | 2026-10-30 |
| Escalation | 12 |
| Run | issue #1483 |

## Question

The repair-round audit found that a data-driven test's row is keyed by the text of its arguments, which for arrays, objects, and strings containing quoted separators is not unique, so two rows share one key; it repeats the class of the first audit's data-row finding. Options: A — fix narrowly: make the row text unique for every argument it can represent (arrays element by element, escaped quotes) and fail with a clear message at test start for an argument it cannot represent, pinning each case with a test, together with the round's two other low findings; B — change the structure first and key rows by something MSTest guarantees unique, likely needing an experimental API or row-index bookkeeping; C — stop the run.

## Answer

A. The row key is made injective for the arguments it supports, unsupported arguments fail at test start, and the remaining low findings of the audit are repaired in the same round.

## Reason

None given; the lead's recommendation was that A covers every realistic row shape and fails loudly on the rest without a structural change.
