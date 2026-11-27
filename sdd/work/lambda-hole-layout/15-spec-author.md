Role: spec author
Commit: 6d465ff (derived from the implementer's 8fcadf5, `13-implementer.md`)
Verdict: spec written

Repair round 1, follow-up to auditor finding #2 (#1567): the specs now match the single owner the implementer gave to the gap between a lambda's return type and its `(`. No visible output changes.

## Changed requirements

- FMT-BREAK-015 — Wraps inside a list element are joined — changed. In the list of gaps other requirements own, "the opening parenthesis of a parameter list other than a lambda's" now reads "other than that of a parenthesized lambda without an explicit return type (behind a lambda's return type, FMT-BREAK-045 joins it)".
- FMT-BREAK-045 — A lambda's explicit return type joins its parameter list — clarified. The parenthetical "(where FMT-BREAK-015 joins the same gap)" is gone. The requirement now says it is the one owner of the gap and that FMT-BREAK-015 leaves the gap to it inside a list element. This is the same wording FMT-BREAK-039 and FMT-BREAK-040 use. Its Why no longer says FMT-BREAK-015 joins the gap; it now says one owner keeps one guard and one writer.
- Change doc `sdd/changes/2026-11/lambda-return-type-join.md`: Affected requirements adds `FMT-BREAK-015 (changed)`. A new decision, "One owner for the gap, also inside a list element", states the ownership and why the output stays the same: both joins refuse the same trivia and write the same minimal separator, which the implementer confirmed with a byte-identical fixture comparison. Its Why (line 15) and code-fix paragraph (line 141) still say "joined inside a list element (FMT-BREAK-015)". I left them because they describe the base (fe7b8f8), where that was true.

## Other places checked

- FMT-BREAK-011 (line 590) already gives the gap behind a return type to FMT-BREAK-045 and the gap in front of the lambda to FMT-BREAK-015 / -012 / -016. It is consistent, so I left it unchanged.
- No other spec mentions FMT-BREAK-015 together with a lambda or a return type (`rg`).
- `scripts/test.sh --project architecture`: 198 passed.

## Examples for the tests
None new. The output is unchanged and already pinned by `LambdaReturnTypeJoinTests` and `ListElementInteriorJoinTests`.

## Output questions
_None._

## Internal choices
- I left the change doc's description of the base state (FMT-BREAK-015 joined the gap inside a list element) as it was and added a decision for the new ownership. The alternative was to rewrite those sentences, which would misstate the base.

## Follow-up candidates
_None._
