Role: spec author
Commit: 13f643b
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore; it routed maintainer answer 26 and decision 5, which are recorded decisions, not hopes.

## Changed requirements

- RH2009-003 — Cognitive complexity of a body — changed (clarified per maintainer answer 26): recursion is an invocation that binds to the member's own symbol on any receiver or none — `M(…)`, `this.M(…)`, `C.M(…)`, and `other.M(…)` such as `Left.Depth(…)`. The earlier "written as `M(…)`, `this.M(…)`, or `C.M(…)`" list is gone. New example: `Node.Depth` calling `Left.Depth()` and `Right.Depth()`, limit 3, scores 4 and is reported; without the recursion point it would score 3.
- `documentation/cognitive-complexity.md`, *Recursion*: every call of the method itself counts, on any instance; the example gains a `Node.Depth` tree walk (`if` 1, `||` 1, recursion 1, total 3).
- `sdd/changes/2026-11/method-complexity-rule.md`, *Decisions / What scores, and how*: new bullet recording answer 26 as a maintainer decision. Status stays Accepted.
- `documentation/rules/RH2009.md`: decision 5 — the Violation and Correction examples are wrapped in `public class Scorer { … }`; content, comment, and scores (16 vs limit 15) unchanged. `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RuleDocumentationExampleTests"` — Passed 2, Failed 0.

Check: the new spec example and the guide's example were run through the built analyzer (scratch project with `maximumMethodComplexity: 3`): `Node.Depth` reports complexity 4; a guide variant with one extra nested `if` reports 5, confirming the guide's 3. No requirement ID, `Uses` row, or code owner was added or removed, so the architecture tests were not rerun.

## Examples for the tests

RH2009-003 — the new `Node.Depth` example (recursion on another instance). Implementer open point 1 is closed by it; open point 2 (receiver list) is answered: the walker's current behavior is the specified one, no receiver check.

## Output questions

_None._

## Internal choices

- The recursion clause names the receiver forms as examples of "binds to the member's own symbol" rather than as an exhaustive list, so the rule is stated by its cause (the bound symbol), not by syntax forms. Alternative: an exhaustive list of receiver shapes.

## Follow-up candidates

_None._
