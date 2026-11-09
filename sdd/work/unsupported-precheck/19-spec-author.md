Role: spec author
Commit: 4369feb9
Verdict: text written

Repair round 2 (Decision 18), the auditor's hint on the `AGENTS.md` phase table (`16-auditor.md`, *Hints*). The prompt carried no intended fix or hoped-for verdict beyond naming the hint.

## Changed requirements
_None._ No spec or change-doc text changes. `AGENTS.md`, the `PrecheckPhase` row of *Formatter phase ownership*, is reworded:

- Before: "A shape declared unsupported needs no guard in any later phase, and no phase produces one".
- After: "A later phase needs no guard for a shape declared unsupported in an input that holds the shape; a node-level input that holds only part of it, such as an equals-value clause without its declaration, is not refused and still reaches the phases (FMT-UNSUP-003), so a guard that serves such inputs or other constructs stays. No phase produces an unsupported shape (FMT-UNSUP-005)".

This states what the precheck guarantees and matches the change doc's *Checked and kept* list, which keeps `LineBreakAssignmentRewriter`'s guard because an equals-value clause target with a comment still reaches the phases (FMT-UNSUP-003), and keeps the other guards because they serve other constructs. The spec's own wording (FMT-UNSUP-001 Why: "a phase that never sees it needs no guard for it") is already exact and stays.

## Examples for the tests
_None._ Text only; no requirement changed.

## Output questions
_None._

## Internal choices
- Named the equals-value clause as the example of a partial node-level input and cited FMT-UNSUP-003 and FMT-UNSUP-005, rather than the bare clause "for an input that holds the shape" the auditor suggested; the alternative was the shorter clause, which would not say why kept guards stay.

## Follow-up candidates
_None._
