# A block comment at column 0 moved behind a created `return`

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 1 |
| Run | issue #1488 |

## Question

Under the carried-comment-whitespace answer (keep the author's whitespace), `int Get() =>` followed by `/* c */ 42;` starting in column 0 becomes `{ return/* c */ 42; }` instead of today's `{ return /* c */ 42; }`. Options: A accept it as a consequence of that answer; B have the transform write exactly one space between a created `return` and a block comment it moves behind it when no whitespace stood there.

## Answer

A — accepted for now as a consequence of keeping the author's whitespace.

## Reason

Acceptable for the time being; no further reason given.
