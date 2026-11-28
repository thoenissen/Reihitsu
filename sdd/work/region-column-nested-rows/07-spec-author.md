Role: spec author
Commit: a10309c
Verdict: spec written

Repair round. My inputs were:
- `00-lead.md`;
- my earlier handover `05-spec-author.md`;
- *Spec findings* in `06-test-implementer.md`;
- the specs at a10309c.

My prompt named the gaps to repair and carried no intended fix, suspected cause, or hoped-for verdict. Every edit is to spec or change-doc text; no requirement is added or retired.

## Changed requirements

- **FMT-BREAK-038 — A comma that starts a line in an initializer, a collection expression, a pattern, or a base list ends the element's line — clarified.** In the array-initializer example, the `int[] y` output is now `int[] y = {`, `1,` and `2` at column 22, and `};` at column 18, as FMT-BREAK-046 gives it. The example's caption now names FMT-BREAK-046. This is the output `InitializerLeadingCommaTests.SpecificationExampleArrayInitializerCommaMovesAndKeepsTheOtherLineBreaks` asserts.
- **FMT-STRUCT-027 — A removed comma's comment on a line of its own stays there — clarified.**
  - In the example output, `int[] _a = new[]` is followed by `{` under `new` at column 15, then `1,`, `2`, a blank line, `// comma`, and `};` at column 15. This is the output `TrailingCommaCommentOwnerTests.CommentsOfRemovedTrailingCommasBelowTheLastElementLeadTheClosingBrace` asserts.
  - The caption names FMT-BREAK-046.
  - `Uses` gains FMT-BREAK-046.
- **FMT-BLANK-019 — No blank line before the closing bracket of a collection expression or pattern — clarified.**
  - The sentence that kept or inserted the blank line below a region directive in front of `]` or `)` is replaced. The new sentence says that the phases never receive a region directive in the leading trivia of such a token. Such a token is never the first token of the input, of a member, or of a using directive, and never a type's or a namespace's closing brace, so FMT-UNSUP-023 refuses the input.
  - The RH5032 sentence in *Why* is removed.
  - FMT-UNSUP-023 was already in `Uses`.
- **FMT-STRUCT-007 — Directives that cannot survive keep the expression body — clarified.** The region clause stays because SHARED-BODY-001 relies on it: RH3202 and RH3203 do not report a region that the semicolon splits, and `sdd/specs/shared/expression-body-conversion.md` has an example for it. A new sentence, with a matching addition to *Why*, says three things:
  - such a region stands inside the member;
  - the formatter refuses the input under FMT-UNSUP-023 before it converts anything;
  - the clause stays for the analyzers.
- **FMT-STRUCT-013 — Directives that cannot survive keep the accessor block — clarified.** The region clause stays because SHARED-GETONLY-007 relies on it: a member whose getter block FMT-STRUCT-013 keeps is reported without a fix. The added sentence and *Why* text follow FMT-STRUCT-007's. `Uses` gains FMT-UNSUP-023 and SHARED-GETONLY-007.
- **FMT-PIPE family statement (`pipeline.md`, the paragraph on choices a construct's own requirement makes) — clarified.** "an array initializer keeps the author's layout" now says the layout is kept between its elements, and that FMT-BREAK-046 decides the lines of a multi-line initializer's braces. `Uses` gains FMT-BREAK-046.
- **Change doc `sdd/changes/2026-11/region-column-nested-rows.md` — still Accepted. No visible output change was added.**
  - Under *Special cases*, *Deleted*, the in-member bullet now also names FMT-BLANK-019's sentence.
  - Under *Checked and kept*, the predicate bullet now also says that FMT-STRUCT-007 and FMT-STRUCT-013 keep their region clauses for the same reason.

## Search for further stale examples

- **Regions.** I scripted a check of every `csharp` block in `sdd/specs/formatter/*.md` that holds a `#region`/`#endregion`, recording each directive's brace depth and the line in front of it. Apart from FMT-UNSUP-023's own refused examples, every directive stands at a position FMT-UNSUP-023 accepts:
  - between members, at any depth of nested types;
  - in front of a type's closing brace;
  - at file level.

  The prose mentions of regions in `blank-lines.md`, `indentation.md`, `pipeline.md`, `region-formatting.md`, and `structural-transforms.md` hold no other in-member claim. FMT-BLANK-013, FMT-REGION-005, and FMT-INDENT-010 already defer to FMT-UNSUP-023.
- **Array initializers.** I grepped the formatter specs for an array initializer's brace sharing a line with an element (`= { x,`, `new[] { x,`, `x };`, `}, {`). The only other hits are inputs and the FMT-BREAK-046 examples. The output of FMT-BREAK-039's `Baz(new[] { x.GetHashCode() }, 2)` is on one line after the join, which agrees with FMT-BREAK-046. The prose of FMT-BREAK-021 and FMT-BREAK-038 already cites FMT-BREAK-046.

## Not run

My prompt forbids building and running test suites while the implementer works on `src/`. So:
- I did not run `scripts/test.sh --project architecture`, although `Uses` rows changed: structural-transforms gains FMT-BREAK-046, FMT-UNSUP-023, and SHARED-GETONLY-007; pipeline gains FMT-BREAK-046. Each ID is referenced in its spec's text.
- I did not run `scripts/format.sh`. The two corrected examples equal the expected outputs of 06's tests, which derive them from FMT-BREAK-046.

The spec checker or validator should run the architecture tests.

## Examples for the tests

- FMT-BREAK-038 and FMT-STRUCT-027: the existing tests named above already assert the corrected outputs.
- FMT-STRUCT-007, FMT-STRUCT-013: no formatter test is possible for their region clauses. The analyzer tests of SHARED-BODY-001 and SHARED-GETONLY-007 cover the clauses.
- FMT-BLANK-019: no new test; its region tests are deleted (06).

## Output questions

_None._

## Internal choices

- **FMT-STRUCT-007 and FMT-STRUCT-013 keep their region clauses** with a note that the formatter refuses those positions first. Two alternatives were rejected:
  - Removing the clauses would leave SHARED-BODY-001's "keeps under FMT-STRUCT-007" and SHARED-GETONLY-007's "FMT-STRUCT-013 keeps its getter block" without a statement of the region case.
  - Moving the clauses into the shared specs would be a larger rewrite of two specs outside this repair.
- **FMT-BLANK-019's sentence is replaced, not removed.** It now states, by cause, why the phases never receive a region in front of those tokens. The alternative was to delete it silently.
- **I extended the repair to the `pipeline.md` family paragraph.** It was not named in 06. It said "an array initializer keeps the author's layout" without the FMT-BREAK-046 exception.

## Follow-up candidates

_None._ The candidates F3–F5 and F6 are unchanged from `00-lead.md`.
