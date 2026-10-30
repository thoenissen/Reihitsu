Role: test implementer
Commit: 44718c9
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue's restructuring items are implementation and are not tested here, except where noted under *Not covered*.

## How the expected values were derived

- **Spec examples.** The examples of SHARED-BLANK-008, FMT-INDENT-002, FMT-DOC-003, FMT-SPACE-001, FMT-INDENT-015, and FMT-INDENT-006 are copied as written. The one exception is the end-of-file comment; see *Findings*.
- **"The column it gets behind a `//` comment"** (change doc, *Outputs that change*). For every line kind I formatted the `//` counterpart of the fixture with today's `scripts/format.sh --force` (scratchpad only). The expected output is that result with `// x` written back as `/// x`. The `//` behavior is not part of this change, so it defines the target instead of coming from a patched copy (FC-035). The analyzer tests carry `//` controls that pass today, which pins the counterpart.
- **Zero-width tokens.** Block column 8 (FMT-INDENT-015, and the change doc's "as `int[2,` / `3] a` does today"). Two controls with an array size that has text, which pass today, pin that column.
- **Line terminators.** The column the same fixture gets with `\n` inside the comment or disabled text (FMT-INDENT-006). A `\n` row of each data-driven test is the control.

## Tests

"red" means the test fails today on its first-pass output (the formatter) or on the diagnostics or fix output (the analyzer), and I watched the failure. "green" means the test passes today and must keep passing.

Formatter: `src/Reihitsu.Formatter.Test/`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Regression/Indentation/StrandedDocumentationLineStartTests.ClosingBraceOfTypeBehindStrayDocumentationCommentTakesTypeColumn` | FMT-INDENT-002, SHARED-BLANK-008 | `AssertRuleResult` | red |
| `…StrandedDocumentationLineStartTests.ContinuationLineOfStrayDocumentationCommentKeepsItsColumn` | FMT-INDENT-002 | `AssertRuleResult` | green |
| `…StrandedDocumentationLineStartTests.ClosingBraceBehindTwoLineStrayDocumentationCommentTakesTypeColumn` | FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationLineStartTests.ClosingBraceBehindLastLineOfMultiLineCommentIsNotPlaced` | SHARED-BLANK-008 | `AssertRuleResult` | green |
| `…StrandedDocumentationLineStartTests.StrayDocumentationCommentBeforeChainLinkStaysAndLinksAreAligned` | FMT-DOC-003, FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationLineStartTests.ClosingBraceOf{Block,AccessorList,Namespace,SwitchStatement}…TakesBlock/Property/Namespace/SwitchColumn` (4 tests) | FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationLineStartTests.ClosingBraceOf{ObjectInitializer,SwitchExpression}…IsAlignedWithItsOpeningBrace` (2) | FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationLineStartTests.ReturnTerminatorBehindStrayDocumentationCommentTakesStatementColumn` | FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationLineStartTests.{ClosingParenthesis,ClosingBracket,ClosingAngleBracket,LeadingComma}BehindStrayDocumentationCommentIsAlignedAsBehindSingleLineComment` (4) | FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationLineStartTests.Wrapped{Addition,LogicalAnd,Coalesce,Is}OperatorBehindStray…IsAlignedAsBehindSingleLineComment` (4) | FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationLineStartTests.Wrapped{LambdaArrow,AssignmentOperator}BehindStray…IsPlacedAsBehindSingleLineComment` (2) | FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationLineStartTests.SingleConditionalAccessBehindStray…`, `…MemberAccessDotBehindStray…`, `…FirstChainLinkBehindStray…IsAlignedAsBehindSingleLineComment` (3) | FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationLineStartTests.StrayDocumentationCommentBefore{ConditionalAccessLink,ConditionalQuestionMark,ConditionalColon,DoWhileCondition}Stays…` (4) | FMT-DOC-003, FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationLineStartTests.ClosingBraceBehindStrayDocumentationCommentAndBlankLineTakesTypeColumn` | FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationLineStartTests.TerminatorBehindStrayDocumentationCommentAnd{Directive,DisabledText}TakesStatementColumn` (2) | FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationLineStartTests.OwnLineCommentAndTerminatorBehindStrayDocumentationCommentTakeStatementColumn` | FMT-INDENT-002 | `AssertRuleResult` | green |
| `…StrandedDocumentationLineStartTests.StrayDocumentationCommentBefore{Else,Catch,Argument,SwitchLabel,Statement}IsStillRelocated` (5) | — (must-stay characterization; FMT-DOC-001 leaves these shapes undecided) | `AssertRuleResult` | green |
| `Regression/Indentation/ZeroWidthTokenLineStartTests.ClosingAngleBracketBehindOmittedTypeArgumentTakesBlockColumn` | SHARED-BLANK-008, FMT-INDENT-015 | `AssertRuleResult` | red |
| `…ZeroWidthTokenLineStartTests.ClosingAngleBracketOfUnboundGenericInColumnZeroTakesBlockColumn` (the issue's example 2) | FMT-INDENT-015 | `AssertRuleResult` | red |
| `…ZeroWidthTokenLineStartTests.{SeparatorBehindOmittedTypeArgument, ClosingAngleBracketOfSingleOmittedTypeArgument, ClosingAngleBracketBehindThreeOmittedTypeArguments, SeparatorAndClosingAngleBracketBehindOmittedTypeArguments, OverIndentedClosingAngleBracketBehindOmittedTypeArgument, ClosingAngleBracketBehindOmittedTypeArgumentAndTrailingComment}…` (6) | FMT-INDENT-015 | `AssertRuleResult` | red |
| `…ZeroWidthTokenLineStartTests.OwnLineCommentAnd{ClosingAngleBracket,Separator}BehindOmittedTypeArgumentTakeBlockColumn` (2) | FMT-INDENT-015 | `AssertRuleResult` | red |
| `…ZeroWidthTokenLineStartTests.{ClosingBracketBehindOmittedArraySize, SeparatorBehindOmittedArraySize, ClosingBracketOfArrayCreationBehindOmittedArraySize, ClosingBracketOfSecondRankBehindOmittedArraySize}TakesBlockColumn` (4) | SHARED-BLANK-008 | `AssertRuleResult` | red |
| `…ZeroWidthTokenLineStartTests.ArraySizeWithTextOnWrappedLineTakesBlockColumn`, `…ClosingBracketBehindArraySizeWithTextTakesBlockColumn` | — (control) | `AssertRuleResult` | green |
| `…ZeroWidthTokenLineStartTests.CommentBehindLastClosingBraceOfFileTakesColumnZero` | SHARED-BLANK-008 | `AssertRuleResult` | green |
| `…ZeroWidthTokenLineStartTests.EndRegionBehindLastClosingBraceOfFileEndingWithLineBreakTakesColumnZero` | SHARED-BLANK-008 | `AssertRuleResult` | green |
| `…ZeroWidthTokenLineStartTests.EndRegionBehindLastClosingBraceOfFileTakesColumnZero` | SHARED-BLANK-008 | `AssertRuleResult` | **red, also on `main`**; see *Findings* |
| `…ZeroWidthTokenLineStartTests.FirstTokenOfFileBehind{DocumentationComment,Directive}TakesColumnZero` (2) | SHARED-BLANK-008 | `AssertRuleResult` | green |
| `Regression/Indentation/CommentLineTerminatorIndentationTests.BlockCommentHoldingLineTerminatorTakesColumnOfFollowingCode` (rows `\n`, `\r`, U+0085, U+2028, U+2029) | FMT-INDENT-006 | own `AssertFormats` (LF and CRLF, `FormatSyntaxTree`, second pass) | red except `\n` |
| `…CommentLineTerminatorIndentationTests.OwnLineCommentAndBlockCommentHoldingLineTerminatorTakeColumnOfFollowingCode` (same rows) | FMT-INDENT-006 | `AssertFormats` | red except `\n` |
| `…CommentLineTerminatorIndentationTests.OwnLineCommentBeforeDisabledTextHoldingLineTerminatorTakesColumnOfFollowingCode` (same rows) | FMT-INDENT-006 | `AssertFormats` | red except `\n` |
| `…CommentLineTerminatorIndentationTests.OwnLineCommentBehindDisabledTextHoldingLineTerminatorTakesColumnOfFollowingCode` (same rows) | FMT-INDENT-006 | `AssertFormats` | green (holding cell) |
| `…CommentLineTerminatorIndentationTests.CommentsBeforeClosingBraceWithBlockCommentHoldingLineTerminatorTakeStatementColumn` (same rows) | FMT-INDENT-006 | `AssertFormats` | red except `\n` |
| `…CommentLineTerminatorIndentationTests.BlockCommentInLoneCarriageReturnFileTakesColumnOfFollowingCode` (the issue's example 3 as a whole lone-`\r` file) | FMT-INDENT-006 | `FormatSyntaxTree`, two passes | red |
| `Regression/Spacing/StrandedDocumentationTokenSpacingTests.KeepsWhitespaceInFrontOfAndBehindStrayDocumentationComment` (new FMT-SPACE-001 example) | FMT-SPACE-001 | `AssertRuleResult` | green |
| `…StrandedDocumentationTokenSpacingTests.KeepsTerminatorIndentationBelowStrayDocumentationCommentBehindReturnExpression`, `…KeepsFieldTerminatorIndentation…`, `…KeepsTerminatorIndentationBelowTwoLineStray…` (retagged from FMT-SPACE-015) | FMT-SPACE-001 | `AssertRuleResult` | green |
| `…StrandedDocumentationTokenSpacingTests.ClosingBracketBelowStrayDocumentationCommentBehindAttributeArgumentsIsPlacedAsBelowSingleLineComment` (was `KeepsClosingBracketIndentation…`, `]` 4 → 5) | FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationTokenSpacingTests.OverIndentedTerminatorBelowStrayDocumentationCommentTakesStatementColumn` (was `KeepsAuthoredColumnOfOverIndentedTerminator…`, `;` 16 → 8) | FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationTokenSpacingTests.CommaBelowStrayDocumentationCommentIsAlignedWithFirstArgument`, `…ClosingParenthesisBelowStray…AlignedWithFirstArgument` (were `Keeps…Indentation…`, 12 → 24) | FMT-INDENT-002 | `AssertRuleResult` | red |
| `…StrandedDocumentationTokenSpacingTests.MemberAccessDotBelowStrayDocumentationCommentIsAlignedBehindReceiver` (was `KeepsMemberAccessDotIndentation…`, 12 → 15) | FMT-INDENT-002 | `AssertRuleResult` | red |
| `Unit/HorizontalSpacing/HorizontalSpacingRewriterTests.KeepsIndentationOfTokenFollowingSingleLineDocumentationComment` (retagged from FMT-SPACE-015) | FMT-SPACE-001 | `AssertHorizontalSpacing` | green |
| `…HorizontalSpacingRewriterTests.SpacesGapAroundMultiLineCommentBeforeBinaryOperator`, `…RemovesGapBetweenMultiLineCommentAndComma` (the other side: an interior line break is not a gap line break) | FMT-SPACE-001 | `AssertHorizontalSpacing` | green |
| `Unit/DocumentationComments/DocumentationCommentRequirementTests.StrandedDocumentationBeforeSemicolonStaysOnItsLineAndSemicolonTakesDeclarationColumn` (FMT-DOC-003 second example, `;` 8 → 4) | FMT-DOC-003, FMT-INDENT-002 | `AssertRuleResult` | red |
| `Regression/FullPipeline/StrandedDocumentationCommentTextTests.OneLineSummaryBeforeBinaryOperatorKeepsItsText` (`+` 8 → 12), `…OneLineSummaryBeforeMemberAccessDotKeepsItsText` (`.` 8 → 15) | FMT-DOC-003, FMT-INDENT-002 | `AssertRuleResult` | red |
| `Regression/LineBreaks/WrappedAngleBracketListTests.UnboundGenericWithZeroWidthAnchorIsNotAlignedToADegenerateColumn` (16 → 8, per the issue's *Done when*) | FMT-INDENT-015 | `AssertRuleResult` | red |
| `Unit/ReihitsuFormatterNodeColumnTests.BlockTargetPlacesClosingBraceBehindStrayDocumentationComment` | FMT-INDENT-002 (FC-028) | `AssertFormatsTarget` (`FormatNodeInDocumentAsync`) + `AssertRuleResult` | red |
| `…ReihitsuFormatterNodeColumnTests.DetachedTypePlacesClosingBraceBehindStrayDocumentationComment` | FMT-INDENT-002 (FC-028) | `FormatNode`, detached, LF and CRLF, second pass | red |

Analyzer: `src/Reihitsu.Analyzer.Test/`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Formatting/RH5204…AnalyzerTests.VerifyClosingBraceBehindStrayDocumentationCommentIsDetectedAndFixed` | RH5204-001, SHARED-BLANK-008 | `Verify(source, fixed, …)` | red (no diagnostic) |
| `…RH5204…AnalyzerTests.VerifyClosingBraceBehindSingleLineCommentIsDetectedAndFixed` | — (control) | `Verify(source, fixed, …)` | green |
| `…RH5204…AnalyzerTests.VerifyClosingBraceBehindLastLineOfMultiLineCommentIsNotReported` | SHARED-BLANK-008 | `Verify(source)` | green |
| `…RH5204…AnalyzerTests.VerifyFirstTokenOfFileBehindDocumentationCommentIsDetected` | RH5204-001, SHARED-BLANK-008 | `Verify(source, diag)`, analyzer only (the fix does not converge here today, a follow-up outside this run) | green |
| `…RH5204…AnalyzerTests.VerifyClosingBraceBehindStrayDocumentationCommentAndBlankLineIsDetectedAndFixed` | RH5204-001 | `Verify(source, fixed, …)` | red (the fix only removes the blank line) |
| `…RH5204…AnalyzerTests.VerifyStatementBehindStrayDocumentationCommentIsDetectedAndFixedInOnePass` | RH5204-001, RH5204-006 (FC-007) | `Verify(source, fixed, …)` (iterative and Fix All) | red (no diagnostic) |
| `…RH5204…AnalyzerTests.VerifyIfStatementBehindStrayDocumentationCommentIsDetectedAndFixed` | RH5204-001, RH5204-006 (FC-007, FC-008) | `Verify(source, fixed, 3 diags)` | red (2 of 3 reported) |
| `Formatter/Formatting/RH5204…FormatterTests.VerifyFormatterFixesClosingBraceBehindStrayDocumentationComment` | SHARED-BLANK-008 (FC-013) | `VerifyFormatter(source, fixed, diag)` | red |
| `Formatting/RH5203…AnalyzerTests.VerifyArgumentBehindStrayDocumentationCommentIsDetectedAndFixed` | RH5203-001, SHARED-BLANK-008 | `Verify(source, fixed, …)` | red (no diagnostic) |
| `…RH5203…AnalyzerTests.VerifyArgumentBehindSingleLineCommentIsDetectedAndFixed` | — (control) | `Verify(source, fixed, …)` | green |
| `Formatting/RH6002…AnalyzerTests.VerifyCommasAroundStrayDocumentationCommentAreNotReported` (spec example) | RH6002-001, RH6002-002, SHARED-BLANK-008 | `Verify(source)` | red (2 reported) |
| `…RH6002…AnalyzerTests.VerifyCommaStartingItsLineBehindStrayDocumentationCommentIsNotReported` | RH6002-001 | `Verify(source)` | red |
| `…RH6002…AnalyzerTests.VerifyCommaInFrontOfStrayDocumentationCommentIsNotReported` | RH6002-002 | `Verify(source)` | red |
| `Formatting/RH5201…AnalyzerTests.VerifyLaterLinksBehindStrayDocumentationCommentAreDetectedAndAligned` (FMT-DOC-003 chain example) | RH5201-002, SHARED-CHAIN-002 | `Verify(source, fixed, 2 diags)` | red (the fix moves the comment) |
| `…RH5201…AnalyzerTests.VerifyConditionalAccessLinksBehindStrayDocumentationCommentAreDetectedAndAligned` | RH5201-002, SHARED-CHAIN-002 | `Verify(source, fixed, 2 diags)` | red (the fix moves the comment) |
| `…RH5201…AnalyzerTests.VerifyLinksBehindStrayDocumentationCommentInFrontOfFirstLinkAreDetectedAndAligned` | RH5201-002, SHARED-CHAIN-002 | `Verify(source, fixed, 2 diags)` | red (no diagnostic) |
| `…RH5201…AnalyzerTests.Verify{LaterLinks,ConditionalAccessLinks,LinksBehindSingleLineCommentInFrontOfFirstLink}…SingleLineComment…` (3 controls) | — | `Verify(source, fixed, 2 diags)` | green |
| `Formatting/RH5112…AnalyzerTests.VerifyFirstLinkBehindStrayDocumentationCommentIsNotReported` | RH5112-001 | `Verify(source)` | green |
| `Formatting/RH5048…AnalyzerTests.VerifyCommentBeforeFirstLinkBehindStrayDocumentationCommentIsReported` (change-doc example) | RH5048-001, SHARED-BLANK-008 | own `GetAnalyzerDiagnosticsAsync`, LF and CRLF; asserts only the `/* c */` report, as `03-spec-author.md` asks | red |
| `…RH5048…AnalyzerTests.VerifyCommentBeforeFirstLinkBehindSingleLineCommentIsReported` | RH5048-001 | `Verify(source, 2 diags)` | green |

Test infrastructure: I deleted the `PhaseReversalExceptionList` entry for `WrappedAngleBracketListTests.UnboundGenericWithZeroWidthAnchorIsNotAlignedToADegenerateColumn`. Its output hash belongs to the old expected output (column 16), so with the new expectation the entry can never be reached. The list can only shrink. If the line-break phase still strips the `>` line once the expectation holds, the phase-level check fails that test and the new `ZeroWidthTokenLineStartTests`. That is the ping-pong `01-spec-author.md` reported for the issue's example 2, and the check is meant to catch it.

Whole run on 44718c9:

- Formatter project: 74 failed, all of them tests listed above as red.
- Analyzer project: 13 failed, all listed above as red.
- Architecture project: 89 passed, so no test still tags the retired FMT-SPACE-015 and SHARED-BLANK-008 is covered.

## Findings

1. **SHARED-BLANK-008's end-of-file example does not match the formatter's whole output.** The spec shows `}` followed directly by `// end`. The full pipeline also inserts a blank line in front of a trailing comment at the end of the file (`TrailingCommentGainsBlankLineAtEndOfFileTests`, unchanged on `main`). Today's output for the spec's input is `}` / blank line / `// end`.
   - My test `CommentBehindLastClosingBraceOfFileTakesColumnZero` starts from an input that already has the blank line, so it asserts only the column the requirement decides.
   - The spec example should show the blank line, or say that only the column is shown. That is a spec-author repair, not a change of intended behavior.
2. **`#endregion` behind the last closing brace, with no line break at the end of the file, is not placed: today, and also on `main`.**
   - Input: `#region A` / `class C` / `{` / `    int _a;` / `}` / `    #endregion`, with no final line break.
   - Output: `#endregion // A` stays in column 4.
   - The same input with a final line break puts it in column 0 (`EndRegionBehindLastClosingBraceOfFileEndingWithLineBreakTakesColumnZero`, green).
   - A `// end` comment in the same position without a final line break is placed (green).
   - SHARED-BLANK-008 names "an `#endregion` behind the last closing brace" among the end-of-file trivia that starts its line, so `EndRegionBehindLastClosingBraceOfFileTakesColumnZero` is red today. This is an FC-001/FC-002 member of the end-of-file token, and it exists on `main` independent of the change (escalation 9). The lead decides whether the run fixes it or whether the test goes to the maintainer.

## Scenarios (bug fix)

Not applicable as a reproduction lane. The two bugs that ride in this run reproduce on their reported expected-versus-actual difference:

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `typeof(Dictionary<,` / `>)` (FMT-INDENT-015) | LF, CRLF | formatter | reproduced | `>` in 16 vs 8 |
| `/* a\r b */` own line (FMT-INDENT-006) | LF, CRLF, whole lone-`\r` file | formatter | reproduced | comment in 12 vs 8; same for U+0085, U+2028, U+2029 |

## Not covered, and why

- **The one-contributor-per-line test (the issue's *Done when*).** It asserts a property of the layout model. The issue changes that model ("names, for each physical line, the token or trivia where its text starts"). Today `LayoutModel.Set` keeps no record of which contributor wrote a line, and its contract is "later calls override earlier ones". A test written now would need a probe into internals that the implementer is about to replace. It belongs with the restructured model, and the lead should route it explicitly.
- **Restructuring items** (helper moves, the `ConfigurableStructuralTransforms` move, the `ReihitsuFormatterHelpers` split, every terminator set using the shared predicate). These carry no behavior. `Reihitsu.ArchitectureTests` and the auditor check them, not unit tests. The behavior-carrying members of those terminator sets are covered through their outputs:
  - `CountEmbeddedNewLines` through the M3 tests;
  - the chain, conditional-access, conditional, and do-while line-break rewriters through the `StrayDocumentationCommentBefore…` tests, which also run the phase-level stability check.
  The `\n`-only checks in `SwitchCaseWhenLineBreakRewriter` and `FieldDeclarationSplitTransform.EndsLine` have no dedicated test. I did not construct an input whose output depends on them. After the preparation phase, every line break between tokens is the detected line ending, so only a terminator inside a comment or disabled text could reach them, and I did not probe whether one does.
- **A Core unit test of `SyntaxTokenPositionUtilities.IsFirstOnLine`.** The issue moves or replaces that predicate. Its behavior is pinned through every surface that asks it: RH5204, RH5203, RH6002, RH5201, RH5112, RH5048, and the formatter phases.
- **A multi-line `/** */` holding a foreign terminator.** The preparation phase rewrites its interior newline tokens (skeptic, M3), so it leaves the class. **Verbatim strings with `\r` or U+2028** are already covered by `ReihitsuFormatterLineEndingIndependenceTests.KeptLineBreaksInsideLiteralsCommentsAndDisabledTextAreTheSameInTheCopy`.
- **FC-019, FC-020, FC-021.** Covered by the M1 context axes: a directive, disabled text, an own-line comment, and a blank line between the `///` and the line; a two-line stranded comment; a `/* */` before the stranded comment (existing `KeepsTerminatorIndentationBelowStrayDocumentationCommentBehindTwoLineBlockComment`).
- **FC-006, FC-018, FC-022 to FC-025, FC-029, FC-036.** No rewrite of syntax, names, literals, or member kinds is involved.
- **FC-009.** No code action becomes newly offered without effect. RH6002 stops reporting; the other fixes already exist.
- **The RH5204 fix at the start of a file** (`VerifyFirstTokenOfFileBehindDocumentationCommentIsDetected` asserts only the diagnostic). The skeptic found the fix does not converge there today. The input has no `///` written behind code, so it is outside this run (follow-up candidate in `02-skeptic.md`).
- **Whether RH5048 reports the stranded `///` itself.** Deliberately not asserted (`03-spec-author.md`, follow-up candidate).

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~StrandedDocumentationLineStartTests|FullyQualifiedName~ZeroWidthTokenLineStartTests|FullyQualifiedName~CommentLineTerminatorIndentationTests|FullyQualifiedName~StrandedDocumentationTokenSpacingTests|FullyQualifiedName~StrandedDocumentationCommentTextTests|FullyQualifiedName~DocumentationCommentRequirementTests|FullyQualifiedName~WrappedAngleBracketListTests|FullyQualifiedName~HorizontalSpacingRewriterTests|FullyQualifiedName~ReihitsuFormatterNodeColumnTests"
scripts/test.sh --project analyzer --filter "Name~StrayDocumentationComment|Name~SingleLineComment|Name~MultiLineComment|Name~FirstTokenOfFileBehind|Name~StatementBehindStray|Name~CommasAroundStray|Name~CommaStartingItsLine|Name~CommaInFrontOfStray|Name~FirstLinkBehind"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"
```
