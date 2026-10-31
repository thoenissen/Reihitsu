# How to treat the node-in-document outputs that change because decisions read outside a detached root

| | |
|---|---|
| Date | 2026-10-31 |
| Escalation | 5 |
| Run | issue #1489 |

## Question

The skeptic found that detaching every node-level root changes 334 `FormatNodeInDocumentAsync` outputs, contrary to the approved change doc: line-break decisions that read the target's chain, one-line accessor list, enclosing list element, owning `else` clause, or a zero-offset depth get no fact under FMT-PIPE-024, and the `else if (y) // note` target loses its comment. (A proceed and list the outputs as changed; B amend the change doc so FMT-PIPE-024 carries plain-value facts for these contexts; C narrow the run to `FormatNode` with `Refs`; D stop and fix the detached-path defects first)

## Answer

B — amend the change doc so FMT-PIPE-024 also carries plain-value facts for the decisions that read outside the root; the amended change doc returns for approval.

## Reason

None given beyond accepting the recommendation.
