Role: test implementer
Commit: 49ac8ed4
Verdict: tests written

My prompt named the change (lead decision 16) and carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from FMT-STRUCT-026 as changed on this branch (`git diff origin/main -- sdd/specs/formatter/structural-transforms.md`): its three field-split examples, and its normative sentences ("exactly one space in front of each comment that trailed a separating comma, whatever the author wrote there", "a line break behind each comment it moves onto its own line in front of a later field", "MUST NOT write indentation in front of a field it creates or of a comment it moves", "MUST NOT write a line break in front of a field it creates other than these separators"), plus the separator bullets of the same requirement. The one document-level test pins today's output on `main` (decision 5: document-level output stays unchanged). Nothing comes from a patched copy of the code (FC-035).

## Compile state and how the failures were checked

`Reihitsu.Formatter.Test` still does not compile on the branch until the implementer adds `ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync` and `ReihitsuFormatter.SplitsFieldDeclaration` (06). As 06 did, I ran the tests in a scratch worktree of 49ac8ed4 (deleted since) with two throwaway stubs in `ReihitsuFormatter`, Debug build (the Release build rejects the stubs through the repository's analyzers). No production code was touched on the branch.

Result there for `StructuralTransformSeparatorTests` and `FieldDeclarationSplitTests`: 11 failed, 101 passed. The 11 are exactly the new phase-output tests. Each fails on the field split's layout only: today's phase output writes a line break and the member indentation in front of every created field, and the member indentation in front of every moved comment, for example

```
expected: "class C\n{\n    private int _x;private int _y;\n}"
actual:   "class C\n{\n    private int _x;\n    private int _y;\n}"

expected: "class C\n{\n    private int _x;\n/* v */\nprivate int _y;\n}"
actual:   "class C\n{\n    private int _x;\n    /* v */\n    private int _y;\n}"
```

The parts the requirement keeps (one space in front of a comment that trailed a comma, the line break behind `// h` and behind a moved comment, no second line break behind a `///` comment) already match today; they are asserted in the same string, so a change that drops them fails these tests too.

## Tests

`src/Reihitsu.Formatter.Test/Unit/StructuralTransforms/StructuralTransformSeparatorTests.cs` (changed). Each new test puts the members in `class C`, takes the structural-transform phase's output under LF and CRLF (`ForEachLineEnding`), asserts its whole text, and asserts that it parses back to its tree (`AssertSameTree`, FMT-PIPE-021). New helper `AssertSplitFields`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| FieldCreatedWithoutACommentGetsNoLineBreakAndNoIndentation (spec example 1: `private int _x;private int _y;`) | FMT-STRUCT-026 | `AssertSplitFields` | fails |
| EveryFieldCreatedWithoutACommentGetsNoLineBreakAndNoIndentation (three declarators, initializers) | FMT-STRUCT-026 | same | fails |
| FieldCreatedBehindADeclarationWithACommentOnItsLineGetsNoIndentation (`/* note */ private int _x, _y;`) | FMT-STRUCT-026 | same | fails |
| FieldCreatedBehindADeclarationWithACommentOnTheLineInFrontGetsNoIndentation | FMT-STRUCT-026 | same | fails |
| FieldCreatedBehindADocumentedDeclarationGetsNoIndentation (`///` leads the declaration) | FMT-STRUCT-026 | same | fails |
| SingleLineCommentThatTrailedACommaWithoutASpaceGetsOneSpaceAndTheLineBreakBehindIt (spec example 2) | FMT-STRUCT-026 | same | fails |
| SingleLineCommentThatTrailedACommaBehindSeveralSpacesGetsOneSpace (`_x,    // h`) | FMT-STRUCT-026 | same | fails |
| BlockCommentThatTrailedACommaGetsOneSpaceAndNoLineBreakBehindIt (`_x,/* h */ _y;` → `_x; /* h */private int _y;`) | FMT-STRUCT-026 | same | fails |
| BlockCommentMovedInFrontOfACreatedFieldGetsALineBreakBehindItAndNoIndentation (spec example 3) | FMT-STRUCT-026 | same | fails |
| SingleLineCommentMovedInFrontOfACreatedFieldGetsALineBreakBehindItAndNoIndentation (`// v` on its own line) | FMT-STRUCT-026 | same | fails |
| DocumentationCommentMovedInFrontOfACreatedFieldGetsNoSecondLineBreakAndNoIndentation (`///` in front of `_y`, which ends its own line) | FMT-STRUCT-026 | same | fails |

`src/Reihitsu.Formatter.Test/Regression/Structural/FieldDeclarationSplitTests.cs` (changed)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| TrailingBlockCommentAfterSeparatorOnTheDeclaratorsLineIsPreserved (`private int _a,/* first */ _b;` → `private int _a; /* first */` and `private int _b;` on the next line) | FMT-STRUCT-022 | `AssertRuleResult` (LF, CRLF, second pass, phase stability) | passes; must still pass. It pins the document-level output for the one phase-output case above with no line break behind the carried comment, where the line-break phase now has to start `_b`'s field on the next line behind a comment that trails `_a;` |

The sides of each condition the split's layout depends on: comment trailing a comma vs. none (tests 1–2 vs. 6–8); single-line vs. block comment trailing a comma (line break behind vs. none, 6 vs. 8); comment moved in front of a later declarator that does not end its line vs. one that does (`/* v */`, `// v` vs. `///`, 9–10 vs. 11); declaration whose leading trivia ends its line vs. shares it with a comment (4–5 vs. 3), which are the two branches of today's `GetMemberIndentationTrivia`. LF and CRLF run in every test.

## Not covered, and why

- **A declaration wrapped between declarators with no comment** (`private int _x,` followed by a line `_y;`). FMT-STRUCT-026 does not say whether the author's line break behind the comma is trivia the split carries over or a line break it writes in front of the created field; its examples all have a comment in that gap. I did not pick a reading; the lead can ask the spec author if it matters. The document-level output is decided by the line-break phase either way.
- **Attributes repeated on created fields** (`[A]` on a line of its own). The trivia inside the repeated attribute list and modifiers is carried over from the author, not written in front of the created field; FMT-STRUCT-026 decides only the field's leading layout, and whether the inner line break and indentation count as "in front of a field it creates" is not stated. Left out for the same reason.
- **Comment and directive in the gap** (rule touching token gaps): the comment arms are the tests above; a directive blocks the split (FMT-STRUCT-023), already covered by existing tests and by 06's `SplitsFieldDeclaration` tests.
- **Second-pass idempotency / combined pipeline**: the phase-output tests assert one phase; `AssertRuleResult` in the regression test and the existing `FieldDeclarationSplitTests` (all `AssertRuleResult`) cover the whole pipeline, second pass and phase stability.
- **RH7101 fix output**: covered by 06's analyzer tests (FMT-PIPE-029, RH7101-004); this run's fix formats through the pipeline, so its output is the document-level layout, not the phase output.
- Failure classes: FC-035 (expected values from a patched copy) does not apply, values come from the spec text; code-fix, naming, and analyzer classes do not reach a phase-output test.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.StructuralTransforms.StructuralTransformSeparatorTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Structural.FieldDeclarationSplitTests"
```
