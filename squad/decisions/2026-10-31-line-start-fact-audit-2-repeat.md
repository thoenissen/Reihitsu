# Audit 2: RH6001 repeats the class of the analyzer-side line-break copies

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 12 |
| Run | issue #1487 |

## Question

The repair audit found that RH6001 decides the line break behind a keyword by `EndOfLineTrivia` alone, so it reports and rewrites the formatter's stable output `if/// x` / `(c)` for nine keywords — the same class as the first audit's RH6021/RH6005 finding, which one enumeration already missed. (A fix RH6001 narrowly and draft the remaining sweep as a follow-up; B change the structure first: enumerate every analyzer and code fix that decides "a line break separates two tokens" by trivia kind, reproduce each against formatter output with a `///` comment, and route every failing one through the shared fact in this run; C stop the run)

## Answer

B — applied by the lead under the maintainer's standing instruction for this run to take the lead's recommendation. The structural change does not use up a repair round. Any change-doc addition it needs (rules whose diagnostics change) is provisionally accepted under the same instruction.

## Reason

Delegated; the recommendation's reason: repairing one member per audit only finds the next, and the class is bounded by a mechanical search.
