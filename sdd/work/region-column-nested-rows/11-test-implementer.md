Role: test implementer
Commit: 7dd086b
Verdict: tests written

I worked from `00-lead.md` (decision 7), `09-spec-author.md` (*Examples for the tests*), my earlier handover `06-test-implementer.md`, and FMT-BREAK-046 in `sdd/specs/formatter/line-breaks.md` as of 7dd086b. My prompt carried no intended fix, suspected cause, or hoped-for verdict. The production code is already implemented, so every new test is expected to pass on this head; they pin the jagged-array sentence that 09 added.

Expected values come from FMT-BREAK-046's text: the jagged-array sentence (`{ new[] {⏎1⏎}, new[] { 2 } }` keeps `}, new[] { 2 }` on one line; the row `{ {⏎1⏎}, { 2 } }` puts `{ 2 }` below `},`), the brace placement for a declaration's `=` and for `new[]`/`new T[]`, and "every other line break between the elements stays as written". The columns come from the FMT-INDENT-023 spec example (`int[][] j = {` with `new[]` one level in and its `{` under `new`), which `SpecExampleRowsAreLaidOutLikeComplexElementInitializers` already pins.

## Tests
All tests are in `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/ArrayInitializerBraceLineTests.cs`. `AssertRuleResult` runs each one under LF and CRLF, with a second pass and the per-phase stability check.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ElementBehindAMultiLineImplicitArrayCreationInAJaggedArrayStaysOnTheBracesLine` (the spec's jagged inline example, `new[]`) | FMT-BREAK-046 | `AssertRuleResult` | passes |
| `ElementBehindAMultiLineArrayCreationInAJaggedArrayStaysOnTheBracesLine` (the same with `new int[]`, the second creation form the sentence names) | FMT-BREAK-046 | `AssertRuleResult` | passes |
| `LineBreakInFrontOfTheElementBehindAMultiLineArrayCreationInAJaggedArrayStaysAsWritten` (other side of the condition: a line break the author wrote after `},` stays) | FMT-BREAK-046 | `AssertRuleResult` | passes |
| `ElementBehindAMultiLineRowStartsTheLineBelowItsClosingBrace` (the spec's row inline example `{ {⏎1⏎}, { 2 } }`) | FMT-BREAK-046 | `AssertRuleResult` | passes |

Result on 7dd086b: `ArrayInitializerBraceLineTests` 32 passed, 0 failed.

## Scenarios (bug fix)
_None._ This is a behavior change.

## Not covered, and why
- Row side, beyond the inline example: `DeclarationInitializerWithAMultiLineAndASingleLineRow` and `ArrayCreationWithASingleLineAndAMultiLineRow` already pin a row moving the element behind it. I added the row test only to state the spec's own inline example next to its jagged counterpart.
- `stackalloc` jagged forms: `stackalloc` cannot create a jagged array, because its element type must be unmanaged, so no such input exists.
- A comment or directive in the gap between `},` and the next jagged element: FMT-BREAK-046's comment paragraph and the existing `CommentBehindTheCommaBehindARowKeepsItsLine` and `ConditionalGroupBetweenRowsIsKept` cover comments and directives next to the braces. For a jagged element nothing is moved, so the jagged case adds no gap that the rule joins or breaks.
- I did not run the architecture project. These tests add no new requirement ID.

## Commands
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.LineBreaks.ArrayInitializerBraceLineTests"

## Follow-up candidates
_None._
