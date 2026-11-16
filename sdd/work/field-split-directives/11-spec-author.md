Role: spec author
Commit: ad3f067
Verdict: text written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Repair round 1, on 09-auditor.md.

## Changed requirements
- FMT-UNSUP-009 — Comments and directives in front of a comma on a later line are unsupported — clarified: the second tie bullet ("Every other comma in the span") now states that a directive or disabled text in front of such a comma in a field declaration with several declarators is refused under FMT-UNSUP-010 as well, starting at the same directive, and gives FMT-UNSUP-010's reason. This restates what FMT-UNSUP-010's last paragraph already decides ("Every other directive in the declaration gives this requirement's reason"), and what the code does today (auditor's CLI runs on both ties); no behavior changes. Taken from the auditor's non-blocking hint.

Auditor finding #2 (#1728, low): `AGENTS.md`, *Formatter phase ownership*, `StructuralTransformPhase` row now names `FieldDeclarationSplitTransform.HasDisabledTextInLeadingTrivia` among the per-rewriter guards. Text only.

## Examples for the tests
None new. Finding #1's tie tests (FMT-UNSUP-010 over FMT-UNSUP-009 at a non-declarator comma, and over FMT-UNSUP-008) derive from FMT-UNSUP-010's last paragraph and the clarified FMT-UNSUP-009 bullet; the test implementer owns them in this round.

## Output questions
_None._

## Internal choices
- The FMT-UNSUP-009 clarification names only the reason, as FMT-UNSUP-010's own tie paragraph does; the location is the shared starting directive either way, so no location sentence was added. Alternative: leave the bullet as is, since the two requirements already read consistently.

## Follow-up candidates
_None._
