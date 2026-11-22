Role: spec author
Commit: c6c49b5
Verdict: spec written

Repair round 2, spec side of decisions 29 and 30 (`00-lead.md`) for auditor findings 1 and 2 (`22-auditor.md`). The prompt named the change asked for; it carried no suspected cause or hoped-for verdict beyond the lead's recorded decisions.

## Changed requirements

- FMT-BREAK-022 — Wrapped collection expressions and patterns get one element per line — clarified. "Introduced directly by a `case` label" is replaced by the condition the code and FMT-PIPE-024's case-label pattern fact already use: a `case` label introduces its own pattern and every pattern reached from it through the operands of `and`, `or`, and `not`, and only those (not inside a parenthesized pattern, a subpattern, or the `when` clause). There the requirement leaves the brace on the line it is written on (it does not join it; the old "stays on the label's line" read as a join). For a node-level root the case-label pattern fact decides (FMT-PIPE-024, added to `Uses`). A brace the exception leaves in place is a first token like any other, which FMT-BREAK-017/041 join. Why extended: one condition for this requirement and both joins. New example with both sides: `case P and {`, `case not {` keep the brace; `when p.B is not {` and the subpattern `B: not {` start it on its own line. No visible change: the example is today's and `main`'s output.
- FMT-BREAK-017 — Binary operators start the continuation line — clarified (output changes for the case-label input, decision 29). FMT-BREAK-022 alone decides the exception, by its own conditions: the `(` of a multi-line parenthesized pattern, and the `{` of a multi-line property pattern without a type unless a `case` label introduces the pattern. In a case-label pattern the brace joins the combinator wherever the author broke the line. New example: `case P and⏎{`, `case 1 or⏎{` join (`and {`, `or {`); `o is P and⏎{` keeps the brace below `and`. Its output equals today's output for the same input with the brace written behind the combinator.
- FMT-BREAK-041 — A continuation behind a keyword, `is`, `not`, … joins its line — clarified (output changes, decision 29). The enumerating sentence that gave the second reading for `not` ("… keep their own line below the `is`") now defers to FMT-BREAK-022's condition, behind `not` as behind `is`, and states the case-label consequence: `case not⏎{` becomes `case not {`, `o is not⏎{` keeps the brace below `not`. New example with both sides (`case not⏎{`, `case 1 or not⏎{`, `o is not⏎{`).
- FMT-UNSUP-019 — Comments and directives behind an assignment operator inside a switch arm or an anonymous-object member are unsupported — clarified (decision 30). The first condition names object-initializer, `with`-initializer, and index-initializer members as assignment expressions, with refused examples inside an arm and inside an anonymous-object member and a formatted example outside both. New refused example (location line 14, column 38); the other-side example gains `c5`–`c7` (initializer members at statement level).

Change docs:
- `sdd/changes/2026-11/continuation-join.md` — section 2 gains the case-label input with `main`'s before and the after; the decision "The multi-line pattern's opening delimiter keeps its line" states that FMT-BREAK-022's case-label exception holds for the joins (lead, standing instruction); an alternative records the rejected two-fixed-point layout. No *Special cases* change: `PatternLineStartUtilities.StartsOwnLine` is new in this run, not a special case on `main`.
- `sdd/changes/2026-11/assignment-comment-in-arm-unsupported.md` — *Visible output changes* gains the initializer-member input, `main`'s before, the CLI line; a decision sentence and a rejected alternative ("Leave initializer members out of the shape").

No rule page changes (no rule reports these shapes; rule pages never describe the formatter). No AGENTS.md change: the phase table's wording ("pattern chains") still holds.

## Examples for the tests

The test implementer derives tests from these examples (expected outputs verified as stated below):

- FMT-BREAK-017, new example "in a `case` label, the brace … joins its combinator": add to `PatternChainLayoutTests` (exact output, LF and CRLF, both break positions). Rows: `case P and⏎{⏎A: 1⏎}:` and `case 1 or⏎{⏎A: 2⏎}:` → `case P` / `and {` and `case 1` / `or {`; the same input written `case P and {` / `case 1 or {` gives the same output (already true today — a characterization row); other side `o is P and⏎{` keeps `and` / `{` (already true today). The first two rows fail on today's code.
- FMT-BREAK-041, new `not` example: rows `case not⏎{⏎A: 1⏎}:` → `case not {`, `case 1 or not⏎{⏎A: 2⏎}:` → `case 1` / `or not {`; the brace-behind-`not` forms give the same output; other side `o is not⏎{` keeps the brace below `not`. The first two fail today. Natural home: `PatternChainLayoutTests` or `ContinuationJoinTests` (other-side rows).
- FMT-BREAK-022, new case-label example: a characterization test (passes today and must keep passing after the predicate is unified): `case P and {`, `case not {` keep the brace; `case P p when p.B is not {` and `case P { B: not {` start it on its own line.
- A node-level row for the case-label fact, if the implementer's unified predicate reads `RootConstruct.CaseLabelPattern` in the join: `FormatNodeInDocumentAsync` on the pattern `not⏎{⏎A: 1⏎}` of a `case` label gives the same layout as whole-document formatting (FMT-PIPE-024's fact; the auditor's suggested change names it).
- FMT-UNSUP-019, new refused example and the extended other side: add to `UnsupportedAssignmentInArmCommentTests` refused rows for an object-initializer member, a `with`-initializer member, and an index initializer, each inside a switch arm and inside an anonymous-object member (`B = r with { A = /* c */ 3 }`), and an other-side row with the same three initializers at statement level (formatted). These pass on today's code; they pin the scope.

Checks run:
- Branch CLI (`scripts/format.sh`) over every new example: FMT-BREAK-022's example input gives its output exactly; FMT-BREAK-017's and FMT-BREAK-041's outputs equal today's output for the brace-behind-combinator/`not` form, and differ for the line-break form (the defect to fix); FMT-UNSUP-019's examples: the two refused inputs are refused at (7,30) and (14,38), the other side is formatted.
- `main`'s CLI (built from a temporary worktree of `ab547b7`, removed again) for the change docs' *before* blocks: case-label input keeps `and` / `or not` / `case not` apart from `{` (as written in the change doc); brace-behind form already gives the after layout on `main`; initializer members inside the arm are formatted on `main`.
- `scripts/test.sh --project architecture`: 198/198 after adding FMT-PIPE-024 to line-breaks.md's `Uses` row.

## Output questions

_None._ Both outputs are decided by the lead under the standing instruction (decisions 29, 30).

## Internal choices

- FMT-BREAK-022's case-label condition is stated in FMT-BREAK-022 itself, and FMT-BREAK-017/041 defer to it by reference instead of restating it; alternative: restate the condition in each join requirement (rejected: three statements of one condition is the drift finding 1 reports).
- FMT-BREAK-022 now says the exception "leaves the brace on the line it is written on" instead of "stays on the label's line", matching the code (it never joined); alternative: keep the old wording (rejected: it reads as a join and was a second reading).
- FMT-PIPE-024 (not FMT-PIPE-017) is cited for the node-level case-label fact, because it holds the facts table.

## Follow-up candidates

_None._ The `case⏎(` layout seen for `case (1 or⏎{…}):` is already in `00-lead.md`'s list, and no new example shows it.
