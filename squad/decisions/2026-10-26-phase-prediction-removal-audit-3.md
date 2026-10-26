# Context entry point after the third audit (repeated class, repair rounds exhausted)

| | |
|---|---|
| Date | 2026-10-26 |
| Escalation | 7, 12 |
| Run | issue #1463 |

## Question

Audit finding #11 (`37-auditor.md`) repeats the class of #2 and #8 after the third and last repair round: the #8 repair of `FormatNodeInDocumentWithContextAsync` (writing back the phases' leading trivia with the document's blank lines) inserts whitespace in the middle of a line when the target carries a `///` written behind a token of its context (RH5111, RH5201), because "does this part of the leading trivia start its line" is decided in four places. Options: A — a further repair round that consolidates that decision on the shared predicate and fixes #11; B — revert the context entry point to its behavior before the #8 repair (the original leading trivia is written back, as on `main`), so no regression against `main` remains, narrow FMT-PIPE-017 to exempt a context target that starts a later line of its context as a recorded gap, keep the other repairs (#9 and everything before), and present the restructuring of the context entry point as a follow-up together with the `IndentationPhase` review; C — stop the run and leave the draft PR as it is.

## Answer

B — taken by the lead as its recommended option under the maintainer's standing instruction of 2026-10-25, which asked the lead to continue autonomously, take its recommendation on every question, and accept a rollback in the worst case. The revert and the narrowing run as one final round authorized by this decision; a further block stops the run (option C) and is reported to the maintainer. Checkpoint before this decision: 20251e9.

## Reason

The same class failed in three consecutive audits, each repair exposing the next member; repairing once more (A) repeats the pattern the squad's rule on repeated classes warns against. B leaves the context entry point exactly as `main` has it, so the PR introduces no regression there, keeps every other gain of the run, and moves the structural question to a reviewed follow-up. It also removes the tab-indentation hint of the third audit, which follows from the #8 repair.
