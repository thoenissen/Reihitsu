# Final layout of comments at the arrow of an expression-bodied member

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 1 |
| Run | issue #1488 |

## Question

For an expression-bodied member with a comment directly at the arrow (`int Get() => // c` / `/* d */ 42;`, or `/* c */ =>` on its own line), the final output on `main` and the head is `{// c`, `return` alone, a blank line, then `/* d */ 42;`, and `/* c */ {` behind the signature. FMT-STRUCT-006 leaves these places unspecified. Options: A keep it unspecified and file a follow-up; B move the leading comment behind `return` now; C lead the created statement with own-line comments, changing FMT-STRUCT-006.

## Answer

A — unspecified in this run; a follow-up issue decides the layout.

## Reason

None given beyond accepting the recommendation.
