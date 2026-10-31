# How to handle a block target whose opening brace stays on the declaration's line

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 1 |
| Run | issue #1489 |

## Question

`FormatNodeInDocumentAsync` with the body of `void M() { Foo(1, 2); }` as target leaves `{` on the declaration's line, because FMT-PIPE-010 keeps the leading trivia of a target that does not start its line; this conflicts with FMT-BREAK-002. The run does not change it. (2A follow-up that clarifies FMT-BREAK-002 to today's output; 2B follow-up behavior change that writes back the line break in front of `{`; 2C leave it unspecified)

## Answer

2B — a follow-up issue outside this run, proposing that the entry point write back the line break in front of `{`.

## Reason

None given beyond accepting the recommendation.
