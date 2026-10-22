# How to cover the Playground requirements that have no test seam

| | |
|---|---|
| Date | 2026-10-22 |
| Escalation | 5 |
| Run | issue #939 |

## Question

PLAY-001, PLAY-002, PLAY-003, PLAY-004, PLAY-007 and the message of PLAY-006 have no test seam: `Reihitsu.Playground.Test` references only MSTest, the members of `Home` are private, and the texts and attributes live in `.razor` markup. Options: add the bUnit component-test package to `Reihitsu.Playground.Test` and render the components; test the `.razor` markup as text plus reflection on private members; or tag only PLAY-005/PLAY-006 and leave `playground/` unenforced with a follow-up issue.

A second question: whether the formatter test implementers may start while the Playground question is open.

## Answer

Add bUnit to `Reihitsu.Playground.Test` only (a test-only dependency, no production change) and cover the requirements by rendering the components. The formatter tagging may proceed in parallel.

## Reason

None given.
