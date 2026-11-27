Role: test implementer
Commit: 69c5e19
Verdict: #1568: tests written; #1698: tests written; #1567: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples (FMT-INDENT-031, FMT-INDENT-006, FMT-BREAK-045, copied byte-exact from `sdd/specs/formatter/*.md` at 69c5e19) or are hand-derived from the requirements; for #1568, hand-derived columns were cross-checked against today's output of the block-lambda counterpart in the same position (FMT-INDENT-031 names FMT-INDENT-028's anchor as its own), never against a patched formatter. Every spec example was first run through today's formatter (`scripts/format.sh`): the outputs differ from the spec outputs only in the lines the change moves.

"Expected now" means on 69c5e19 (production unchanged). Every test marked *fails* was run and fails on exactly the lines the change moves (block lines of an anonymous method; the comment line in front of a hole's `}`; the joined return-type line) — checked per test from the assertion diff, LF and CRLF; code-fix tests fail in "Iterative code fix application" after their diagnostic markup matched.

## thoenissen/Reihitsu#1568

### Tests

New file `tests/Reihitsu.Formatter.Test/Regression/Indentation/AnonymousMethodAlignmentTests.cs` (helper `AssertRuleResult`: LF and CRLF, second pass, phase stability):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleArgumentModifiersParameterListAndValuesAlignUnderTheAnchor` | FMT-INDENT-031 | AssertRuleResult | fails |
| `SpecExampleFieldInitializerBranchArmElementCompoundAssignmentChainAndNestedAlignUnderDelegate` | FMT-INDENT-031 | AssertRuleResult | fails |
| `SpecExampleAnonymousMethodInRegularStringKeepsItsLineInCSharp10` (C# 10; brace does not start a line, FC-003 other side) | FMT-INDENT-031 | AssertRuleResult | passes |
| `AnonymousMethodArgumentFollowedByAnotherArgumentAlignsUnderDelegate` | FMT-INDENT-031 | AssertRuleResult | fails |
| `StaticAnonymousMethodAlignsUnderDelegate` | FMT-INDENT-031 | AssertRuleResult | fails |
| `AsyncAnonymousMethodAlignsUnderAsync` | FMT-INDENT-031 | AssertRuleResult | fails |
| `AnonymousMethodInObjectInitializerAlignsUnderDelegate` | FMT-INDENT-031 | AssertRuleResult | fails |
| `EmptyAnonymousMethodAlignsUnderDelegate` | FMT-INDENT-031 | AssertRuleResult | fails |
| `AnonymousMethodWithWrappedParameterListAlignsUnderDelegate` | FMT-INDENT-031 | AssertRuleResult | fails |
| `AnonymousMethodAsExpressionBodyOfPropertyAlignsUnderDelegate` (FC-027 arrow scope) | FMT-INDENT-031 | AssertRuleResult | fails |
| `BlockContentMovesWithTheBlockWhileLiteralLinesAndDisabledTextKeepTheirColumns` (FC-020, FC-021, FC-022: verbatim continuation, raw literal, `#if NEVER` disabled text, leading and trailing comments) | FMT-INDENT-031 | AssertRuleResult | fails |
| `AnonymousMethodAlreadyAlignedUnderItsAnchorIsLeftUnchanged` (stable valid code) | FMT-INDENT-031 | AssertRuleResult | fails (today realigns to scope depth) |
| `BlockBehindCommentOnTheDelegateLineAlignsUnderDelegate` (FC-019/FC-021) | FMT-INDENT-031 | AssertRuleResult | fails |
| `CommentLineInFrontOfTheBlockTakesTheColumnOfTheAlignedBrace` (FC-021) | FMT-INDENT-031, FMT-INDENT-006 | AssertRuleResult | fails |
| `BlockBehindConditionalDirectivesAlignsUnderTheActiveDelegate` (FC-019/FC-020) | FMT-INDENT-031 | AssertRuleResult | fails |

Added in `tests/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeColumnTests.cs` (tab-width matrix cell "node-level target with the tab in the whitespace in front of it", anchored; `FormatNodeInDocumentAsync`, LF/CRLF, second pass):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `AnonymousMethodInStatementTargetBehindTabsAlignsUnderDelegate` | FMT-INDENT-031, FMT-PIPE-017 | AssertFormatsTarget | fails |
| `BlockLambdaInStatementTargetBehindTabsAlignsUnderItsParenthesis` (sibling, FC-016; validates the hand-derived column 11) | FMT-INDENT-028, FMT-PIPE-017 | AssertFormatsTarget | passes |

Code fixes (FC-028 node-level path; `VerifyCodeFixWithEndOfLine`, `[DataRow]` LF and CRLF, Roslyn one-pass convergence and Fix All):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5101FirstArgumentShouldBeOnSameLineAnalyzerTests.VerifyFixAlignsTheBlockOfAnAnonymousMethodArgumentUnderDelegate` | FMT-INDENT-031 | VerifyCodeFixWithEndOfLine | fails (block 16 → 17) |
| `RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests.VerifyFixAlignsTheBlockOfAnAnonymousMethodArgumentUnderDelegate` | FMT-INDENT-031 | VerifyCodeFixWithEndOfLine | fails (16 → 17) |
| `RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.VerifyFixAlignsTheBlockOfAnAssignedAnonymousMethodUnderDelegate` | FMT-INDENT-031 | VerifyCodeFixWithEndOfLine | fails (8 → 12) |
| `RH5111…VerifyFixAlignsTheBlockOfAnAnonymousMethodInAnObjectInitializerUnderDelegate` | FMT-INDENT-031 | VerifyCodeFixWithEndOfLine | fails (25 → 29) |
| `RH5111…VerifyFixAllAlignsTheBlockOfEveryAssignedAnonymousMethodUnderDelegate` (two diagnostics, Fix All) | FMT-INDENT-031 | VerifyCodeFixWithEndOfLine | fails |
| `RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzerTests.VerifyFixAlignsTheBlockOfAnAnonymousMethodInTheLambdaBodyUnderDelegate` | FMT-INDENT-031 | VerifyCodeFixWithEndOfLine | fails |

Analyzer ↔ formatter parity (FC-013, SHARED-DIAG-010; `FormatterTestsBase<TAnalyzer>.VerifyFormatter(source)`: no diagnostic, formatter leaves it, LF and CRLF):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5204IndentationMustUseFourSpacesPerScopeLevelFormatterTests.VerifyAnonymousMethodBlocksAlignedUnderTheirAnchorAreNotReportedAndStayAsWritten` (second spec example's output) | FMT-INDENT-031 | VerifyFormatter | fails at the formatter step (analyzer step passes: RH5204 reports nothing) |
| `RH5203MultiLineArgumentsShouldBeAlignedFormatterTests.VerifyAnonymousMethodArgumentsAlignedUnderDelegateAreNotReportedAndStayAsWritten` (first spec example's output) | FMT-INDENT-031 | VerifyFormatter | fails at the formatter step |
| `RH5205StatementLambdaOpeningBraceShouldBeAlignedFormatterTests.VerifyAnonymousMethodInLambdaBodyAlignedUnderDelegateIsNotReportedAndStaysAsWritten` | FMT-INDENT-031 | VerifyFormatter | fails at the formatter step |

Existing tests whose asserted output the change moves — each rewritten to the aligned layout (the `{`…`}` block shifted under its anchor, nothing else), now failing on exactly those lines:

| Test | Change |
|---|---|
| `AnonymousMethodHoleAlignmentTests.SpecExampleAnonymousMethodInsideBlockLambdaBodyInHoleAndOutsideHoleAlignsUnderDelegate` (was `…OfTheSameHoleAndOutsideHoleKeepScopeDepth`) | input and output replaced by the changed spec example |
| `…SpecExampleAnonymousMethodInSingleLineRawStringNoMultiLineHoleEnclosesAlignsUnderDelegate` (was `…KeepsScopeDepth`) | changed spec example |
| `…SpecExampleAnonymousMethodInRegularStringInsideBlockLambdaBodyInHoleAlignsUnderDelegate` (was `…KeepsScopeDepth`) | changed spec example |
| `…AnonymousMethodInsideAnonymousMethodOfTheSameHoleAlignsUnderItsDelegate`, `…AnonymousMethodInHoleOfSingleLineLiteralAlignsUnderDelegate`, `…AnonymousMethodInRegularStringNoMultiLineHoleEnclosesAlignsUnderDelegate`, `…AnonymousMethodInVerbatimStringNoMultiLineHoleEnclosesAlignsUnderDelegate`, `…AnonymousMethodInSingleLineRawStringInsideBlockLambdaBodyInHoleAlignsUnderDelegate`, `…AnonymousMethodInRegularStringInsideAnonymousMethodOfTheSameHoleAlignsUnderItsDelegate` (all renamed from `…TakesScopeDepth…`/`…KeepsScopeDepth`) | expected realigned; summaries rewritten |
| `ContinuationJoinTests.EachGapIsJoined` row "Modifier of an anonymous method"; `ContinuationJoinTests.SpecExampleJoinedConstructsAreLaidOutAsWhenWrittenOnOneLine` (FMT-BREAK-041) | block under `delegate` (29) |
| `ParenthesizedLambdaLineStartTests.SpecExampleAnonymousMethodParenthesisJoinsWhileLambdaArgumentKeepsItsLine` (FMT-BREAK-011) | block under `delegate` (32) |
| `NonVerbatimInterpolationLineBreakTests` constants `SpecCSharp10Expected`, `SpecCSharp11Expected` (used by the two FMT-BREAK-044 spec tests and `FormatDocumentAsync…`/`FormatNodeInDocumentAsyncKeepsInterpolationOnOneLineInCSharp10Project`), and `AtDollarVerbatimInterpolationIsBrokenInCSharp10` | blocks under `delegate` |
| `ListElementInteriorJoinTests.AnonymousMethodArgumentParameterListIsCollapsedOntoTheDelegateKeyword` | block under `delegate` (10) |
| `DeclaredNameSpacingTests.ParameterNamesOfEveryParameterListGetOneSpaceBehindTheirType` | block under `delegate` (35) |
| `ThrowOperandPrecedenceExpressionBodyTests.AnonymousMethodGetterOperandStaysBare` | braces under `delegate` (26) |
| `ReihitsuFormatterNodeColumnTests.ChainTargetStartingItsStatement{OffItsDepth,InTwoSpaceIndentedFile,AtItsDepth}AlignsTheBlockUnderDelegate` (were `…ShiftsTheBlockByTheContextsOffset` / `…PlacesTheBlockByTheDepth`) | block under `delegate`; summaries rewritten; tag FMT-INDENT-031 added next to FMT-PIPE-017 |
| `RH5201MethodChainsShouldBeAlignedAnalyzerTests`: const `ChainOfStatementOffItsDepthResultData` (used by `VerifyFixOfChainOfStatementOffItsDepthChangesOnlyTheChain` and its CRLF twin), `VerifyFixOfChainOfStatementInTwoSpaceIndentedFileChangesOnlyTheChain`, `VerifyFixOfChainOfStatementAtItsDepthChangesOnlyTheChain` | block under `delegate`; two summaries reworded ("whose block the depth places") |
| `RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests`: const `ChainOfStatementInTwoSpaceIndentedFileResultData` (and its CRLF test), `VerifyFixOfChainOfStatementOffItsDepthChangesOnlyTheChain` | block under `delegate`; one summary reworded |
| `RH5417FunctionAndAccessorBodyBracesMustNotShareLineFormatterTests.VerifyFormatterFixesLambdaAndAnonymousMethodBodiesAndIsIdempotent` | block under `delegate` (38) |
| `Reihitsu.Tooling.Test` `CodeFixRunCommandLanguageVersionTests.ExecuteAsyncWithoutLangVersionFormatsTheFixResultForTheNewestVersion` (RH3202 fix result) | block under `delegate` (22) |

Found by scanning every file under `tests/` for a line ending in `delegate`/`async delegate`/`delegate (…)` whose next line is a `{` in another column (scanner script, 52 hits), plus `rg 'delegate\\n'` for escaped strings; each hit classified by hand. Left unchanged, and why: RH3004 tests (anonymous methods only in inputs; outputs are lambdas), RH3106 and RH6024 tests (fixes do not format), RH2002/RH2008/RH2009 (diagnostics only), `ReihitsuFormatterHiddenCodeTests` (document returned unchanged, FMT-PIPE-033), `RawStringInterpolationHoleTests` lines 983/1034 (inputs; outputs already aligned in multi-line raw holes), `CliLanguageVersionEndToEndTests` (C# 10, stays on one line). No source under `src/` holds the shape.

### Not covered, and why

- FC-023/024/025/026/029/036, FC-018, FC-007–009 beyond the Roslyn verifier: no rename, no member kind, no new fix; the fixes are unchanged and only their formatted output moves.
- FC-038–043: no shape is declared unsupported in this run.
- FC-006: the change only moves whitespace in front of lines; the block-content test covers literals and disabled text that must not move.
- FC-012: no neighbor phase interacts beyond the blank line FMT-BLANK-011 writes, which `CommentLineInFrontOfTheBlockTakesTheColumnOfTheAlignedBrace` pins.
- A detached `FormatNode` root: the node-level path is covered through `FormatNodeInDocumentAsync` (tab test) and the four code fixes; the anchor and the block are always inside the same node.
- Tab-width matrix, other cells (shift of a raw literal; tab inside the target): the hole cells are pinned by the existing `HoleAnchor*` and `AnonymousMethodHoleAlignmentTests`; FMT-TAB-001 replaces a tab inside the target before indentation, so it reaches the same call site as the whole-document tests.

## thoenissen/Reihitsu#1698

### Tests

New file `tests/Reihitsu.Formatter.Test/Regression/Indentation/InterpolationClosingBraceCommentTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleCommentInFrontOfClosingBraceOfNonRawInterpolationTakesTheBraceColumn` (t, u, v, s, n, and the unchanged raw `r`; blank lines pinned next to the column) | FMT-INDENT-006 | AssertRuleResult | fails |
| `CommentInFrontOfClosingBraceOfRegularInterpolationTakesTheBraceColumn` | FMT-INDENT-006 | AssertRuleResult | fails |
| `CommentWrittenOneLevelDeeperThanClosingBraceOfRegularInterpolationMovesToTheBraceColumn` | FMT-INDENT-006 | AssertRuleResult | fails |
| `DelimitedCommentInFrontOfClosingBraceOfRegularInterpolationTakesTheBraceColumn` (FC-021) | FMT-INDENT-006 | AssertRuleResult | fails |
| `TwoCommentsInFrontOfClosingBracesOfTwoInterpolationsTakeTheirBracesColumns` | FMT-INDENT-006 | AssertRuleResult | fails |
| `CommentsInFrontOfClosingBracesOfNestedRegularInterpolationsTakeTheirBracesColumns` (FC-027) | FMT-INDENT-006 | AssertRuleResult | fails |
| `CommentInFrontOfRegularBraceTakesItsColumnWhileCommentInNestedRawHoleStaysOneLevelDeeper` (FC-003 both sides in one input) | FMT-INDENT-006 | AssertRuleResult | fails (only the regular brace's comment line differs) |
| `CommentBehindWrappedArgumentInRegularInterpolationTakesTheBraceColumn` (content column ≠ brace column) | FMT-INDENT-006 | AssertRuleResult | fails |
| `CommentInFrontOfClosingBraceOfRegularInterpolationInBlockLambdaTakesTheBraceColumn` (FC-027) | FMT-INDENT-006 | AssertRuleResult | fails |
| `CommentAlreadyInTheBraceColumnOfRegularInterpolationIsLeftUnchanged` (stable valid code) | FMT-INDENT-006 | AssertRuleResult | fails |
| `RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzerTests.VerifyFixPutsACommentInFrontOfTheClosingBraceOfARegularInterpolationInTheBraceColumn` (code-fix path, LF/CRLF) | FMT-INDENT-006 | VerifyCodeFixWithEndOfLine | fails |
| `RH5204IndentationMustUseFourSpacesPerScopeLevelFormatterTests.VerifyCommentsInTheBraceColumnOfInterpolationsAreNotReportedAndStayAsWritten` (FC-013; the spec output without `r`, because the markup parser reads `$$` as a position marker) | FMT-INDENT-006 | VerifyFormatter | fails at the formatter step (RH5204 reports nothing) |

Other side already pinned (no new test): the multi-line raw literal's own hole keeps the comment one level deeper — `RawStringInterpolationHoleTests.CommentInFrontOfTheClosingBraceOfAHoleIsIndentedOneLevelDeeperThanTheBrace` and `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndClosingBraceOfRawStringInterpolation` (both pass, unchanged). No existing test asserts the old column for a non-raw hole: `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndClosingBraceOfRegularStringInterpolation` asserts blank lines only and stays valid (scanned every test for a comment line one level deeper than a following `}` that continues a string).

### Not covered, and why

- FC-019/FC-020 (directive in front of the hole's `}`): FMT-INDENT-006 decides only a comment's column; the change adds no condition on directives.
- The alignment clause on its own line in a hole (`$"{⏎a,⏎5⏎}"`): dropped by the lead (decision 4); not pinned so as not to lock in an undecided column.
- FC-038–043, FC-006: no shape declared, whitespace only.

## thoenissen/Reihitsu#1567

### Tests

New file `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/LambdaReturnTypeJoinTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleReturnTypeJoinsTheParameterListInEveryPosition` | FMT-BREAK-045 | AssertRuleResult | fails |
| `SpecExampleCommentInTheGapKeepsTheLineBreakAndLambdaWithoutReturnTypeIsUnchanged` (other side) | FMT-BREAK-045 | AssertRuleResult | passes |
| `GenericReturnTypeJoinsTheParameterList`, `TupleReturnTypeJoinsTheParameterList`, `ArrayReturnTypeJoinsTheParameterList`, `NullableReturnTypeJoinsTheParameterList`, `RefReturnTypeJoinsTheParameterList`, `ReturnTypeOfAsyncLambdaJoinsTheParameterList` (every return-type kind the skeptic listed) | FMT-BREAK-045 | AssertRuleResult | fail |
| `ModifierReturnTypeAndParameterListOnThreeLinesJoinIntoOne` (FMT-BREAK-041 gap next to this one) | FMT-BREAK-045 | AssertRuleResult | fails |
| `ReturnTypeOfAssignedLambdaJoinsTheParameterList`, `ReturnTypeOfLambdaInExpressionBodyOfLambdaJoinsTheParameterList`, `ReturnTypeOfPropertyExpressionBodyJoinsTheParameterList` (FC-027) | FMT-BREAK-045 | AssertRuleResult | fail |
| `DelimitedCommentInTheGapKeepsTheLineBreak` (FC-019/FC-021) | FMT-BREAK-045 | AssertRuleResult | passes |
| `CommentLineInTheGapKeepsTheLineBreaks` (FC-019; output with the FMT-BLANK-011 blank line) | FMT-BREAK-045 | AssertRuleResult | passes |
| `DirectiveInTheGapKeepsTheLineBreaks` (FC-019/FC-020) | FMT-BREAK-045 | AssertRuleResult | passes |
| `DisabledTextInTheGapKeepsTheLineBreaks` (FC-020) | FMT-BREAK-045 | AssertRuleResult | passes |
| `JoinAppliesInANodeLevelRoot` (`FormatNode`, LF/CRLF, FC-028) | FMT-BREAK-045 | direct | fails |
| `RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.VerifyFixJoinsTheReturnTypeOfAnAssignedLambdaWithItsParameterList` (code-fix path, LF/CRLF) | FMT-BREAK-045 | VerifyCodeFixWithEndOfLine | fails |

Existing tests: none asserts a kept return-type line break (scanned every test for a type-ending line followed by a `(… ) =>` line, and escaped strings). The list-element case is already joined and pinned by `LambdaReturnTypeSpacingTests` (unchanged, passes).

### Not covered, and why

- A lambda attribute list in front of the return type (`[A] int⏎(int x) => x`): the attribute's own line break is not specified for lambdas (lead decision 4); not pinned.
- A line break inside a `ref` return type: not FMT-BREAK-045's gap and its layout is unspecified (lead decision 4).
- FC-038: the join is withheld whenever trivia other than whitespace and line breaks is in the gap (four "keeps" tests), so no comment moves.

## Whole run

### Spec gaps (found while writing the tests; for the lead to route)

FMT-INDENT-031 now aligns every anonymous method's block, but three examples of `sdd/specs/formatter/line-breaks.md` still show the scope-depth block in their formatted output. I rewrote the tests of these examples to FMT-INDENT-031's layout (covered by the maintainer's answer B), so each test now differs from the literal text of the example it is tagged with until the spec is amended:
- FMT-BREAK-011, example "the opening parenthesis of an anonymous method moves onto the `delegate` line" (output around line 705): block should be in column 32.
- FMT-BREAK-041, example "after the join, the requirements that lay out the construct…" (output around line 4056): block in column 29.
- FMT-BREAK-044, both C# 10 and C# 11 outputs (around lines 4379–4425).

Change-doc completeness: `anonymous-method-aligns-everywhere.md` *Code-fix results* lists RH5101, RH5102, RH5111, RH5205; the fixes of RH5201 and RH5112 (chain with an anonymous-method argument), RH3202 (expression body to block body, the tooling test), and the formatter parity of RH5417 move the block the same way (tests above).

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~AnonymousMethodAlignmentTests|FullyQualifiedName~InterpolationClosingBraceCommentTests|FullyQualifiedName~LambdaReturnTypeJoinTests|FullyQualifiedName~AnonymousMethodHoleAlignmentTests|FullyQualifiedName~ContinuationJoinTests|FullyQualifiedName~ListElementInteriorJoinTests|FullyQualifiedName~NonVerbatimInterpolationLineBreakTests|FullyQualifiedName~ParenthesizedLambdaLineStartTests|FullyQualifiedName~DeclaredNameSpacingTests|FullyQualifiedName~ThrowOperandPrecedenceExpressionBodyTests|FullyQualifiedName~ReihitsuFormatterNodeColumnTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5101FirstArgumentShouldBeOnSameLineAnalyzerTests|FullyQualifiedName~RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests|FullyQualifiedName~RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests|FullyQualifiedName~RH5205StatementLambdaOpeningBraceShouldBeAligned|FullyQualifiedName~RH5201MethodChainsShouldBeAlignedAnalyzerTests|FullyQualifiedName~RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests|FullyQualifiedName~RH5417FunctionAndAccessorBodyBracesMustNotShareLineFormatterTests|FullyQualifiedName~RH5204IndentationMustUseFourSpacesPerScopeLevelFormatterTests|FullyQualifiedName~RH5203MultiLineArgumentsShouldBeAlignedFormatterTests"
scripts/test.sh --project tooling --filter "FullyQualifiedName~CodeFixRunCommandLanguageVersionTests"
scripts/test.sh --project architecture
```

Results on 69c5e19 with these tests: formatter filter 391 tests, 60 fail (all on the moved lines, listed above); analyzer filter 417 test runs, 28 fail (the tests listed above as failing); tooling 1 fails (`ExecuteAsyncWithoutLangVersionFormatsTheFixResultForTheNewestVersion`); architecture 198 passed (FMT-BREAK-045 is now covered).

### Follow-up candidates

_None_ beyond the spec gaps above, which are within the run.
