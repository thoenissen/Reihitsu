# Elements covered by FMT-PIPE-022

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 1 |
| Run | issue #1488 |

## Question

FMT-PIPE-022 as drafted requires phases to keep annotations on nodes, tokens, and trivia, which every whitespace-writing phase, the documentation phase, tab replacement, and region formatting break today. Options: A nodes and tokens of the code only; B also nodes and tokens inside documentation comments and directives; C as written, trivia included.

## Answer

A — FMT-PIPE-022 covers nodes and tokens of the code only, not structured trivia and not trivia.

## Reason

None given beyond accepting the recommendation.
