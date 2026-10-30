# Implementer's architecture findings: spacing rule across phase folders, omitted type arguments, analyzer-side copies

| | |
|---|---|
| Date | 2026-10-30 |
| Escalation | 12 |
| Run | issue #1487 |

## Question

1. `ListElementInteriorJoinRewriter` (line-break phase) uses the horizontal-spacing phase's `SpacingPolicy` and unary-operator guard. (A leave it and draft a follow-up; B move both to `Pipeline/Core` as a neutral rule both phases call; C stop the run)
2. FMT-BREAK-014 requires a wrapped type-argument list to be joined, while FMT-INDENT-015, the issue, and the approved change doc keep `typeof(Dictionary<,` / `>)` wrapped with `>` at the block column. (A keep it wrapped and add an exception to FMT-BREAK-014; B join it to `typeof(Dictionary<,>)`, amending the change doc)
3. `AttributeTargetUtilities.StartsLine` and the analyzer-side terminator sets of RH5202, RH5401, RH5603, RH8402, and the copyright-header helpers keep their own copies. (A leave them out of this run and draft follow-ups; B bring them into the run with a change-doc amendment)

## Answer

1B, 2B, 3A. The spacing rule moves to `Pipeline/Core`. A wrapped type-argument list of omitted type arguments is joined like any other (FMT-BREAK-014 applies; the change doc, FMT-INDENT-015's example, and the zero-width tests are amended accordingly). The analyzer-side copies become follow-up drafts.

## Reason

None given.
