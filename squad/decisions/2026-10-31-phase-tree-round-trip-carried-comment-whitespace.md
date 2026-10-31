# Whitespace around comments a structural transform carries over

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 1 |
| Run | issue #1488 |

## Question

The brace and expression-body transforms write one space in front of carried trailing comments and around a block comment moved behind `return`, a space no parse needs and the spacing phase does not restore. Options: A keep the author's whitespace there, writing and dropping none; B decide today's single space as a separator; C drop it.

## Answer

A — the transforms keep the author's whitespace in front of and between the comments they carry over. Final outputs whose trailing-comment gap is not one space change, and the change doc lists them.

## Reason

None given beyond accepting the recommendation.
