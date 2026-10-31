Role: test implementer
Commit: 1d8778f
Verdict: tests written

Inputs: thoenissen/Reihitsu#1488, `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md`, `sdd/changes/2026-11/phase-tree-round-trip.md`, the spec diff against `origin/main`, and the eight decisions `squad/decisions/2026-10-31-phase-tree-round-trip-*.md`. The decisions override the issue where they differ: the third-case test asserts `2` in column 25, not under `1` in column 20. My prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

Expected values come from the spec blocks. A script compared every raw-string `input`/`expected` taken from a spec example with the spec's code blocks, and all of them match character for character. Final outputs that the change doc leaves unchanged and that no spec block shows were checked with `scripts/format.sh` at 4c9e416. These are the braced form with a blank line between two leading comments, the compact accessors, the top-level `if`, and the two trailing-comma inputs at their second pass.

## Per-phase reparse over every formatter fixture (Done-when 1)

`FormatterTestsBase.ApplyRule` (`src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs`) now runs every pass of every `AssertRuleResult` / `AssertSecondPassIsStable` fixture through `PhaseTreeRoundTrip.ExecuteAndCompare` (`src/Reihitsu.Formatter.Test/Helpers/PhaseTreeRoundTrip.cs`). It does this under LF and CRLF, with the fixture's parse options. It fails for every phase whose tree differs from the tree its text gives when parsed again.

The comparison follows FMT-PIPE-021:

- **Structure:** a pre-order walk of nodes and tokens (kind, span, child count, token text, missing). A syntax error in the reparsed text fails.
- **Owned trivia:** for each token, the non-whitespace, non-line-break trivia of each side, by kind and text.
- **Not compared:** the owner of whitespace and line breaks.

Fixtures whose input has a syntax error skip the check, because the formatter never passes such a tree (FMT-PIPE-021 covers `FormatDocumentAsync`/`FormatSyntaxTree` only).

At 4c9e416, before my new test classes existed, this hook alone failed **64 existing tests**. Their outputs are otherwise unchanged. In every one, the first phase that fails is `StructuralTransformPhase`:

- 60 by `return` fused with the next token (`return42`, `returnawait`, `returnref`, `returnp with`, …);
- 4 by the inserted `}` written inside a trailing `//` comment.

Later phases in the same messages fail only because they inherit that text. Whether a later phase breaks the round trip on its own will show once the structural phase is fixed. The failing tests by class:

- `ArgumentFormattingFullPipelineTests` (4), `AsyncUnknownTaskTypeExpressionBodyTests` (3), `CollectionExpressionAlignmentTests` (1), `ControlFlowBraceCommentTests` (1), `EmptyTypeDeclarationSemicolonLanguageVersionAfterEarlierTransformTests` (1)
- `ExpressionBodiedConstructorTests` (1), `ExpressionBodiedLocalFunctionTests` (5), `ExpressionBodiedMethodFullPipelineTests` (1), `ExpressionBodiedMethodTests` (8), `ExpressionBodiedOperatorTests` (1)
- `ExpressionBodyArrowCommentTests` (13), `ExpressionBodyDirectiveGuardTests` (3), `ExpressionBodyNextLineExpressionTests` (12)
- `GetOnlyMemberExpressionBodyTests` (2), `PipelineRequirementTests` (4), `StructuralTransformRequirementTests` (3), `WithInitializerLayoutTests` (1)

These tests carry no new tag. They verify FMT-PIPE-021 only through the shared helper.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `PhaseTreeRoundTripTests.EveryPhaseOfTheConvertedAndBracedClassParsesBackToItsTree` | FMT-PIPE-021 | `PhaseTreeRoundTrip.ExecuteAndCompare` | fail (`else` cannot start a statement) |
| `PhaseTreeRoundTripTests.StructuralTransformPhaseOutputReadsTheCreatedReturnAndTheLiteralAsTwoTokens` | FMT-PIPE-021 | `ObservedPipeline.GetPhaseOutput` | fail (`return42`) |
| `PhaseTreeRoundTripTests.StructuralTransformPhaseOutputPutsTheInsertedBraceOutsideTheTrailingComment` | FMT-PIPE-021 | `ObservedPipeline.GetPhaseOutput` | fail (`// trailing}`) |
| `PhaseTreeRoundTripTests.LineBreakWrittenInFrontOfTheMovedBraceLeavesTheSameTree` | FMT-PIPE-021 | `ExecuteAndCompare`, `FindDifference` | pass |
| `PhaseTreeRoundTripTests.LeadingCommentBehindABraceOnTheBracesLineIsNotTheSameTree` | FMT-PIPE-021 | `FindDifference` | pass |
| `PhaseTreeRoundTripTests.PhaseOutputIsParsedAgainWithThePreprocessorSymbolsOfTheGivenTree` | FMT-PIPE-021 | `ExecuteAndCompare` with `DEBUG` | fail (`return42` in the active `#if DEBUG` code) |
| `PhaseTreeRoundTripTests.PhaseOutputOfAPreviewTreeIsParsedAgainWithThePreviewLanguageVersion` | FMT-PIPE-021 | `ExecuteAndCompare` with `Preview` | fail (`return42`) |
| `StructuralTransformSeparatorTests.CreatedReturnInFrontOfALiteralGetsOneSpace` | FMT-STRUCT-026 | phase output, gap | fail (gap `""`) |
| `StructuralTransformSeparatorTests.CreatedReturnInFrontOfAParenthesisGetsNoSpace` | FMT-STRUCT-026 | phase output, gap | pass |
| `StructuralTransformSeparatorTests.CreatedReturnInFrontOfACommentAtColumnZeroGetsNoSpace` | FMT-STRUCT-026 | phase output | fail (`return /* c */`) |
| `StructuralTransformSeparatorTests.CreatedReturnInFrontOfAnIndentedCommentKeepsTheAuthorsWhitespace` | FMT-STRUCT-026 | phase output | fail (one space instead of eight) |
| `StructuralTransformSeparatorTests.CreatedCloseBraceOfAConvertedBodyTakesTheSemicolonsTrailingTrivia` | FMT-STRUCT-026 | phase output | pass |
| `StructuralTransformSeparatorTests.OwnLineCommentMovedInFrontOfTheCreatedReturnKeepsLeadingIt` | FMT-STRUCT-026 | phase output, gap, `FindDifference` | fail (`return42`) |
| `StructuralTransformSeparatorTests.TrailingSingleLineCommentWithoutLineBreakGetsOneInFrontOfTheInsertedBrace` | FMT-STRUCT-026 | phase output, gap (top-level `if` at end of file) | fail (no line break) |
| `StructuralTransformSeparatorTests.TrailingBlockCommentOnTheIfLineGetsNoLineBreakAroundTheInsertedBrace` | FMT-STRUCT-026 | phase output, gap | fail (the space behind `/* x */` is dropped) |
| `StructuralTransformSeparatorTests.InsertedOpenBraceWithoutLeadingCommentIsFollowedByNoLineBreak` | FMT-STRUCT-026 | phase output, gap | pass |
| `StructuralTransformSeparatorTests.InsertedOpenBraceInFrontOfALeadingCommentIsFollowedByALineBreak` | FMT-STRUCT-026 | phase output, gap | pass |
| `StructuralTransformSeparatorTests.TwoLeadingBlockCommentsStayOnTheStatementsLine` | FMT-STRUCT-026 | phase output, gap | fail |
| `StructuralTransformSeparatorTests.BlankLineBetweenTwoLeadingCommentsIsKept` | FMT-STRUCT-026 | phase output | fail |
| `StructuralTransformSeparatorTests.InsertedCloseBraceInFrontOfADirectiveIsFollowedByALineBreak` | FMT-STRUCT-026 | phase output, without and with `DEBUG` | pass |
| `StructuralTransformSeparatorTests.InsertedCloseBraceInFrontOfAStatementIsFollowedByNoLineBreak` | FMT-STRUCT-026 | phase output, gap | fail (line break moved behind `}`) |
| `StructuralTransformSeparatorTests.InsertedCloseBraceInFrontOfACommentLeadingTheNextStatementIsFollowedByALineBreak` | FMT-STRUCT-026 | phase output, gap | pass |
| `StructuralTransformSeparatorTests.InsertedCloseBraceInFrontOfACommentLeadingElseIsFollowedByALineBreak` | FMT-STRUCT-026 | phase output, gap | pass |
| `StructuralTransformSeparatorTests.InsertedCloseBraceInFrontOfADocumentationCommentIsFollowedByALineBreak` | FMT-STRUCT-026 | phase output, gap | pass |
| `StructuralTransformSeparatorTests.InsertedCloseBracesInFrontOfElseAndOfTheEnclosingBraceAreFollowedByNoLineBreak` | FMT-STRUCT-026 | phase output, gap | fail |
| `StructuralTransformSeparatorTests.GapsInFrontOfCarriedTrailingCommentsStayAsWritten` | FMT-STRUCT-026 | phase output, gap | fail |
| `StructuralTransformSeparatorTests.AccessorTransformWritesNoSpaceAroundTheCreatedArrow` | FMT-STRUCT-026 | phase output, gap | fail (`get =>`) |
| `StructuralTransformSeparatorTests.GetOnlyMemberTransformWritesNoSpaceAroundTheCreatedArrow` | FMT-STRUCT-026 | phase output, gap | fail (`P =>`) |
| `ControlFlowBraceTransformTests.TrailingCommentOnIfBodyIsPreserved` (changed: brace outside the comment, no syntax error) | FMT-STRUCT-026 | `FormatterPhaseTestsBase.ApplyPhase` | fail (`// note}`) |
| `StructuralTransformSeparatorFinalOutputTests.ConvertedAndBracedClassKeepsItsFinalOutput` | FMT-STRUCT-026 | `AssertRuleResult` | fail (reparse hook only) |
| `StructuralTransformSeparatorFinalOutputTests.BracedBodiesKeepTheirCommentsAndTheDirectiveBehindThemWhereTheAuthorPutThem` | FMT-STRUCT-026, FMT-PIPE-013 | `AssertRuleResult` | fail (`/* x */ /* y */` split) |
| `StructuralTransformSeparatorFinalOutputTests.TwoLeadingBlockCommentsOfABracedBodyStayOnTheStatementsLine` | FMT-PIPE-013 | `AssertRuleResult` | fail |
| `StructuralTransformSeparatorFinalOutputTests.BlankLineBetweenTwoLeadingCommentsOfABracedBodyIsKept` | FMT-PIPE-013 | `AssertRuleResult` | fail (blank line dropped) |
| `StructuralTransformSeparatorFinalOutputTests.GapsInFrontOfCarriedTrailingCommentsStayAsWritten` | FMT-STRUCT-026, FMT-PIPE-013 | `AssertRuleResult` | fail |
| `StructuralTransformSeparatorFinalOutputTests.SpaceBetweenTwoTrailingCommentsOfABracedBodyIsKept` | FMT-PIPE-013 | `AssertRuleResult` | fail |
| `StructuralTransformSeparatorFinalOutputTests.IndentedCommentMovedBehindReturnEndsUpOneSpaceBehindIt` | FMT-STRUCT-026 | `AssertRuleResult` | pass |
| `StructuralTransformSeparatorFinalOutputTests.CommentAtColumnZeroMovedBehindReturnFollowsItWithoutASpace` | FMT-STRUCT-026 | `AssertRuleResult` | fail (`return /* c */`) |
| `StructuralTransformSeparatorFinalOutputTests.CompactAccessorsAndGetOnlyPropertyKeepTheirFinalOutput` | FMT-STRUCT-026 | `AssertRuleResult` | pass |
| `StructuralTransformSeparatorFinalOutputTests.TrailingCommentAtTheEndOfTheFileStaysBehindTheBracedStatement` | FMT-STRUCT-026 | `AssertRuleResult` | fail (reparse hook: `}` in the comment) |
| `TrailingCommaCommentOwnerTests.CommentsOfRemovedTrailingCommasBelowTheLastElementLeadTheClosingBrace` | FMT-STRUCT-027 | `AssertRuleResult` | fail |
| `TrailingCommaCommentOwnerTests.TrailingCommentOfAnEnumCommaOnTheNextLineLeadsTheClosingBrace` | FMT-STRUCT-027 | `AssertRuleResult` | fail |
| `TrailingCommaCommentOwnerTests.OwnLineCommentInFrontOfAnInitializerCommaLeadsTheClosingBrace` | FMT-STRUCT-027 | `AssertRuleResult` | fail |
| `TrailingCommaCommentOwnerTests.RemovedEnumCommasCommentLeadsTheClosingBraceInThePhaseTree` | FMT-STRUCT-027 | phase output | fail |
| `TrailingCommaCommentOwnerTests.RemovedInitializerCommasCommentLeadsTheClosingBraceInThePhaseTree` | FMT-STRUCT-027 | phase output | fail |
| `TrailingCommaCommentOwnerTests.CommentOfARemovedCommaOnTheLastMembersLineStaysBehindIt` | FMT-STRUCT-027 | `AssertRuleResult` | pass |
| `LineBreakResidueWhitespaceTests.LineBreakPhaseKeepsTheSpacesInFrontOfTheLineBreaksItInserts` | FMT-BREAK-029 | `ObservedPipeline.GetPhaseOutput("LineBreakPhase")` | fail |
| `LineBreakResidueWhitespaceTests.FinalOutputHoldsNoWhitespaceInFrontOfTheInsertedLineBreaks` | FMT-BREAK-029 | `AssertRuleResult` | pass |
| `PhaseAnnotationTests.AnnotationsOnCallAndOnTheContentTextTokenSurviveTheRawStringMove` | FMT-PIPE-022 | pipeline observer, every phase | fail (`IndentationPhase`) |
| `PhaseAnnotationTests.AnnotationsOnEveryElementOfAMovedInterpolatedRawStringLiteralSurviveEveryPhase` | FMT-PIPE-022 | pipeline observer | fail (`IndentationPhase`) |
| `PhaseAnnotationTests.AnnotationOnAMovedRawStringLiteralTokenSurvivesEveryPhase` | FMT-PIPE-022 | pipeline observer | fail (`IndentationPhase`) |
| `PhaseAnnotationTests.AnnotationsInsideARawStringLiteralNestedInAHoleSurviveEveryPhase` | FMT-PIPE-022 | pipeline observer | fail (`IndentationPhase`) |
| `PhaseAnnotationTests.AnnotationsOnAnAlignedInterpolatedRawStringLiteralSurviveEveryPhase` | FMT-PIPE-022 | pipeline observer | pass |
| `PhaseAnnotationTests.ConvertedExpressionBodyKeepsTheAnnotationsOfItsKeptElementsAndNotThoseOfTheReplacedSemicolon` | FMT-PIPE-022 | `StructuralTransformPhase` alone | pass |
| `PhaseAnnotationTests.BracedStatementKeepsItsAnnotations` | FMT-PIPE-022 | pipeline observer | pass |
| `ReihitsuFormatterNodeColumnTests.InvocationTargetInsideAMovingRawStringLiteralTakesThePhasesColumnOnItsSecondLine` | FMT-PIPE-017, FMT-PIPE-022 | `AssertFormatsTargetWithContext` | fail (document unchanged) |
| `ReihitsuFormatterNodeColumnTests.RawStringLiteralTargetWhoseLinesMoveIsFormattedUnderItsOpeningQuotes` | FMT-PIPE-022 | `AssertFormatsTargetWithContext` | fail (document unchanged) |
| `ReihitsuFormatterNodeColumnTests.InvocationTargetInsideAnAlignedRawStringLiteralPlacesItsSecondLineUnderTheFirstArgument` | FMT-PIPE-017 | `AssertFormatsTargetWithContext` | pass |

I watched every test that is expected to fail fail. Each fails on the requirement's own difference: the message names a fused `return`, a `}` inside the comment, the gap that differs, the comment owner, the phase that drops annotations (`IndentationPhase` in all four raw-string tests), or the unchanged document. One test first failed for my own reason, an exception before its assertion. I reordered it, and it now fails on `// trailing}`. The tests listed as passing pass at 1d8778f.

Formatter project at 1d8778f: 3326 tests, 103 failing. That is the 64 existing tests caught by the reparse hook plus 39 new or changed tests. `scripts/test.sh --project architecture`: 89/89 pass, `RequirementCoverageTests` included, now that FMT-PIPE-021, FMT-PIPE-022, FMT-STRUCT-026, FMT-STRUCT-027, and FMT-BREAK-029 have tagged tests.

Helper changes, test code only:

- `ObservedPipeline` gained `Run(input, endOfLine, parseOptions)` and `GetPhaseOutput(input, endOfLine, phaseName, parseOptions)`.
- `FormatterTestsBase.ApplyRule` runs the reparse check described above.
- `PhaseTreeRoundTrip` is new.

## Not covered, and why

- **The `return` separator in front of every token kind (identifier, keyword, string) and for each of the seven expression-bodied kinds (FC-025).** Every conversion goes through `ExpressionBodyToBlockConverter.CreateBlock`, which is the only caller of `CreateReturnStatement`. `rg -n "CreateReturnStatement|ExpressionBodyToBlockConverter\.\w+" src/Reihitsu.Formatter/Pipeline/StructuralTransforms` shows six transforms that call `CreateBlock` and one call of `CreateReturnStatement`. The literal case and the parenthesis case pin the two sides. The existing fixtures for methods, local functions, operators, conversions, `await`, `ref`, and `with` now fail through the reparse hook, so they cover the other token kinds without a new test each.
- **`else if` chains (FC-026).** Both bodies go through the same `WrapWithBraces` (`ControlFlowBraceTransform.cs`, the two calls in the `IfStatement` visitor). The existing `else` fixtures, `ElseKeywordClosingBraceAuditFanOutTests` among them, run through the reparse hook. The `}` in front of `else` is pinned on both sides: no line break in front of `else`, and a line break in front of a comment that leads `else`.
- **The field split (FMT-STRUCT-021).** It is excluded from FMT-STRUCT-026 until thoenissen/Reihitsu#1502, and every field-split fixture keeps running through the reparse hook. The modifiers it repeats are excluded from FMT-PIPE-022, so no annotation test covers it.
- **A target that starts a moved hole line, such as the argument `2` as target.** Its output is the unchanged document both today (annotation lost) and after the change (the first case of FMT-PIPE-017's gap with no further lines), so a test could not tell the two apart.
- **A nested raw literal as a context-entry-point target.** Its annotation survival is covered at document level (`AnnotationsInsideARawStringLiteralNestedInAHoleSurviveEveryPhase`). Its written-back layout is not specified beyond FMT-PIPE-017's gap.
- **Which rewriter places member-body braces.** The member-body braces decision makes this unspecified, with no change to the final output. The existing brace fixtures keep the output.
- **The node-level entry points under FMT-PIPE-021.** The requirement excludes them.
- **Trivia, structured trivia, documentation comments, and directives under FMT-PIPE-022.** The annotation-scope decision excludes them.
- **FC-013/FC-014 and the code-fix classes FC-007, FC-008, FC-009, FC-018, FC-030.** No analyzer, code fix, or rule changes. The analyzer suite's formatter-delegating fixes are not re-run here, and FC-032 applies to the validator after the implementation.
- **Self-hosting stability.** CI runs it (`FormatterSelfHostingTests`) and it is not duplicated.

## Follow-up candidates

None beyond those in `02-skeptic.md`.

## Commands

```shell
scripts/test.sh --project formatter
scripts/test.sh --project formatter --filter "FullyQualifiedName~PhaseTreeRoundTripTests|FullyQualifiedName~PhaseAnnotationTests|FullyQualifiedName~StructuralTransformSeparator|FullyQualifiedName~TrailingCommaCommentOwnerTests|FullyQualifiedName~LineBreakResidueWhitespaceTests|FullyQualifiedName~ControlFlowBraceTransformTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterNodeColumnTests"
scripts/test.sh --project formatter --filter "TestCategory=FMT-PIPE-021|TestCategory=FMT-PIPE-022|TestCategory=FMT-STRUCT-026|TestCategory=FMT-STRUCT-027|TestCategory=FMT-BREAK-029"
scripts/test.sh --project architecture
```
