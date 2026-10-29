# Pre-existing phase reversals that the phase-level stability check exposes

| | |
|---|---|
| Date | 2026-10-29 |
| Escalation | 12 |
| Run | issue #1483 |

## Question

The skeptic confirmed three pre-existing structural defects that the new per-phase check exposes, each a decision owned by two phases: the blank line above a comment that leads a using directive of the same group (`UsingDirectiveOrderingPhase` and `BlankLinePhase`), the blank line next to `{` or `}` (`BlankLinePhase` and `LineBreakPhase`), and a line's leading whitespace (`LineBreakPhase` and `IndentationPhase`). Each already has a structural package under #1482. Options: A — keep the run narrow, put the reversing fixtures on the exception list with their causes, and link the existing packages as follow-up work without filing new issues; B — change the structure first inside this run, widening it to a behavior change that pulls in the structural packages; C — stop the run.

## Answer

A. The run keeps the scope of #1483; the structural fixes stay with their existing packages under #1482, which delete their exception entries when they land.

## Reason

None given; the lead's recommendation was that #1482 orders this check as the safety net before the structural packages.
