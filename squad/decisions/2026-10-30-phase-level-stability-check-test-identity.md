# How the phase-level stability check identifies the running test

| | |
|---|---|
| Date | 2026-10-30 |
| Escalation | 7 |
| Run | issue #1483 |

## Question

The first audit found that the check identifies the running test by walking the call stack, which fails when tiered compilation is off (the test frame disappears behind a tail call) and cannot tell data rows apart. Options: A — declare `TestContext` once on `FormatterTestsBase`, remove the duplicate from the 14 derived classes, and key each test by class, method, and data row; B — use MSTest's experimental static `TestContext.Current`, which needs a suppression; C — keep the stack walk and document its dependence on tiered compilation.

## Answer

A. The base class declares `TestContext`, the derived formatter test classes use the inherited property, and the stack walk is removed.

## Reason

None given; the lead's recommendation was that A takes the identity from MSTest without an experimental API or a suppression and also fixes data-driven tests.
