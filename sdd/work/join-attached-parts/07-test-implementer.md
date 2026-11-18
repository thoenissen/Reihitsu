Role: test implementer
Commit: 82f6885
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Tests are derived from FMT-BREAK-040, the FMT-BREAK-039 edits, FMT-UNSUP-014, FMT-SPACE-028, RH2001-007, RH7101-007, and the change doc `sdd/changes/2026-11/attached-part-below-element.md` (changes 1–7). Expected values come from the spec and change-doc examples. For the positions the examples do not show (multi-line receivers, switch arm, initializer value, chain in a hole, lambda, or arm, a comment behind the part), the expected value is the current formatter's output for the same input with the line break in front of the part removed by hand. That is the method the change doc states for its *after* blocks, and no patched code was involved.

## Tests

New files:

- `src/Reihitsu.Formatter.Test/Regression/LineBreaks/AttachedPartLineStartTests.cs` (FMT-BREAK-040)
- `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedAttachedPartCommentTests.cs` (FMT-UNSUP-014)

Helper: `FormatterTestsBase.AttachedPartCommentReason` (the FMT-UNSUP-014 reason text).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| AttachedPartLineStartTests.RequirementExampleJoinsEveryAttachedPart | FMT-BREAK-040 | AssertRuleResult | fail (`arr⏎[0]`, `Get()⏎!`, `s⏎⏎!` stay split) |
| …RequirementExampleJudgesListAndConditionalAsTheLineTheyBecomeAndKeepsCollectionPerElement | FMT-BREAK-040 | AssertRuleResult | fail |
| …RequirementExampleKeepsBracketsThatStartNoAttachedPart | FMT-BREAK-040 | AssertRuleResult (unchanged) | pass |
| …ChangeExampleJoinsElementAccessesAndNullForgivingOperatorsOutsideChains (change 1) | FMT-BREAK-040 | AssertRuleResult | fail |
| …ElementAccessInLambdaAndInterpolationHoleJoins | FMT-BREAK-040 | AssertRuleResult | fail |
| …LineBreakBetweenQuestionMarkAndBracketIsRemoved | FMT-BREAK-040 | AssertRuleResult | pass (`main` already joins) |
| …ConditionalExpressionIsJudgedAsTheLineItBecomes (change 2) | FMT-BREAK-040 | AssertRuleResult | fail |
| …ListIsJudgedAsTheLineItBecomes (change 3, all seven lines) | FMT-BREAK-040 | AssertRuleResult | fail |
| …ListWithLineBreakBetweenArgumentsKeepsOneArgumentPerLineAroundJoinedPart (other side of the list condition) | FMT-BREAK-040 | AssertRuleResult | pass |
| …CollectionExpressionKeepsOneElementPerLine (change 4) | FMT-BREAK-040 | AssertRuleResult | fail |
| …CollectionExpressionInListElementIsJudgedAsWrittenAroundJoinedElementAccess | FMT-BREAK-040 | AssertRuleResult | fail |
| …NullForgivingOperatorJoinsWhateverFollowsIt (`![0]`, `!?[0]`, `!(1)`, `!)`, `!⏎!` → `s!!`) | FMT-BREAK-040 | AssertRuleResult | fail |
| …NullForgivingOperatorThatStartsALinkOperatorIsLaidOutAsALink (`!.`, `!?.`) | FMT-BREAK-040 | AssertRuleResult | pass |
| …MultiLineElementAccessJoinsItsElement | FMT-BREAK-040 | AssertRuleResult | fail |
| …ElementAccessJoinsTheLastLineOfAMultiLineElement (raw string, initializer, switch) | FMT-BREAK-040 | AssertRuleResult | fail |
| …ElementAccessJoinsInEveryPosition (arrow member, pointer, `^1`, `1..`, switch arm, initializer value) | FMT-BREAK-040 | AssertRuleResult | fail |
| …CommentBehindThePartStaysBehindItAfterTheJoin | FMT-BREAK-040 | AssertRuleResult | fail |
| …ConditionalWithCollectionExpressionBranchJoinsOnlyTheElementAccess | FMT-BREAK-040 | AssertRuleResult | fail |
| …ElementAccessInAChainJoinsInEveryPosition (chain in a hole, a lambda, a switch arm) | FMT-BREAK-040 | AssertRuleResult | pass |
| …AttachedPartsAlreadyOnTheirElementsLineStayUnchanged | FMT-BREAK-040 | AssertRuleResult (unchanged) | pass |
| UnsupportedAttachedPartCommentTests.SpecExampleEachPartIsUnsupported (location 7,21) | FMT-UNSUP-014 | AssertUnsupportedFor | fail |
| …EachPartOfTheSpecExampleIsUnsupported (5 rows) | FMT-UNSUP-014 | AssertUnsupportedFor | fail |
| …SpecExampleDirectiveInFrontOfAnArgumentListIsUnsupportedInEveryBuild (location 6,1) | FMT-UNSUP-014 | AssertUnsupportedFor | fail |
| …SpecExampleCommentsOutsideTheGapsAreSupported | FMT-UNSUP-014 | AssertSupported | pass |
| …CommentInFrontOfTheArgumentListOfEveryOwnerIsUnsupported (11 owners, `nameof` included) | FMT-UNSUP-014 | AssertUnsupportedFor | fail |
| …CommentInFrontOfEveryElementAccessIsUnsupported (7 rows) | FMT-UNSUP-014 | AssertUnsupportedFor | fail |
| …CommentInAGapOfAConditionalElementAccessIsUnsupported (9 rows, both gaps, `x!?` and `x!`) | FMT-UNSUP-014 | AssertUnsupportedFor | fail |
| …CommentInFrontOfANullForgivingOperatorIsUnsupported (8 rows) | FMT-UNSUP-014 | AssertUnsupportedFor | fail |
| …EveryCommentPositionInTheGapIsUnsupported (13 rows) | FMT-UNSUP-014 / FC-021 | AssertUnsupportedFor | fail |
| …DocumentationCommentInAGapIsUnsupportedInEveryDocumentationMode (14 rows) | FMT-UNSUP-014 / FC-040 | AssertUnsupportedFor | fail |
| …EveryDirectiveInAGapIsUnsupported (7 rows) | FMT-UNSUP-014 / FC-020 | AssertUnsupportedFor | fail |
| …ConditionalGroupInAGapIsUnsupportedInEveryBuild (8 rows) | FMT-UNSUP-014 | AssertUnsupportedFor | fail |
| …ConditionalGroupAroundTheWholeStatementIsSupported | FMT-UNSUP-014 | AssertSupported | pass |
| …CommentInFrontOfATokenThatStartsNoSuchPartIsSupported (19 rows) | FMT-UNSUP-014 | AssertSupported | pass |
| …CommentInFrontOfAMemberTokenThatStartsNoSuchPartIsSupported (4 rows) | FMT-UNSUP-014 | AssertSupported | pass |
| …CommentOutsideTheGapsIsSupported (9 rows) | FMT-UNSUP-014 | AssertSupported | pass |
| …EveryPartIsCheckedWithItsOwnGap (9 positions) | FMT-UNSUP-014 / FC-039 | AssertUnsupportedFor | fail |
| …PartInDisabledTextIsNotChecked | FMT-UNSUP-014 | both | X not defined: pass; X defined: fail |
| …CommentInAnAttributeArgumentOfADeclarationKeepsTheReasonOfTheDeclaration | FMT-UNSUP-004, FMT-UNSUP-014 | AssertUnsupportedFor | fail (second assertion) |
| …DirectiveInFrontOfAPartInAFieldWithSeveralDeclaratorsKeepsTheReasonOfTheFieldDeclaration | FMT-UNSUP-010, FMT-UNSUP-014 | AssertUnsupportedFor | fail (second assertion) |
| …FirstShapeInTheTextIsReported | FMT-UNSUP-001, FMT-UNSUP-014 | AssertUnsupportedFor | fail |
| …FormatDocumentAsyncRefusesTheShape | FMT-UNSUP-014 | entry point, LF+CRLF | fail |
| …NodeLevelEntryPointsRefuseATargetThatHoldsTheShape | FMT-UNSUP-014 | FormatNode, FormatNodeInDocumentAsync, …WithContextAsync | fail |
| …ArgumentListAloneAndInvokedExpressionAloneDoNotHoldTheShape | FMT-UNSUP-014, FMT-UNSUP-003 / FC-041 | FindUnsupportedSyntax, node entry points | fail (the invocation must be reported) |
| …ElementBindingAloneDoesNotHoldTheGapBetweenQuestionMarkAndBracket | FMT-UNSUP-014, FMT-UNSUP-003 | FindUnsupportedSyntax | fail |
| …FindUnsupportedSyntaxAnswersForABuiltNode (4 rows) | FMT-UNSUP-003, FMT-UNSUP-014 | FindUnsupportedSyntax | fail |
| …FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck (5 rows) | FMT-UNSUP-014 | FindUnsupportedSyntax vs FormatSyntaxTree | pass (both null now) |
| …OutputOfFormattedAttachedPartsIsNotUnsupported (12 rows, 3 doc modes) | FMT-UNSUP-005, FMT-UNSUP-014 / FC-038 | two passes, LF+CRLF | pass |
| RH3202…AnalyzerTests.VerifyNoFixIsOfferedForACommentInFrontOfAnElementAccess (change 7) | SHARED-FIX-010, FMT-UNSUP-014 | VerifyNoCodeFixIsOffered, LF+CRLF | fail (fix offered) |
| RH3202…AnalyzerTests.VerifyNoFixIsOfferedForACommentInFrontOfAnAttachedPart (`Foo /* c */ (1)`, `n? /* c */ [0]`) | SHARED-FIX-010, FMT-UNSUP-014 | VerifyNoCodeFixIsOffered | fail |
| RH2001…AnalyzerTests.VerifyNoFixIsOfferedForACommentInFrontOfAnArgumentListOfTheInitializerValue (RH2001-007 example) | RH2001-007 | VerifyNoCodeFixIsOffered, LF+CRLF | fail |
| RH2001…AnalyzerTests.VerifyNoFixIsOfferedForALineCommentInFrontOfAnArgumentListOfTheInitializerValue (change 7) | RH2001-007 | VerifyNoCodeFixIsOffered | fail |
| RH2001…AnalyzerTests.VerifyNoFixIsOfferedForACommentInFrontOfAnAttachedPartOfTheInitializerValue (`[`, `!`, `?`–`[`) | RH2001-007 | VerifyNoCodeFixIsOffered | fail |
| RH7101…AnalyzerTests.CommentInFrontOfAnArgumentListInAnInitializerValueWithholdsTheFix (change 7) | RH7101-007 | VerifyNoCodeFixIsOffered, LF+CRLF | fail |
| RH7101…AnalyzerTests.CommentInFrontOfAnAttachedPartWithholdsTheFix (one-line value, method of the type, nested type `?`–`[`) | RH7101-007 | VerifyNoCodeFixIsOffered | fail |
| RH5112…AnalyzerTests.VerifyNoFixIsOfferedForACommentInFrontOfAnAttachedElementAccess (change 7) | RH5112-001, FMT-UNSUP-014 | VerifyNoCodeFixIsOffered, LF+CRLF | fail |
| RH5302…AnalyzerTests.VerifyFixJoinsOperandElementAccessBelowItsElement (change 5) | FMT-BREAK-040 | VerifyCodeFixWithEndOfLine (iterative fix) | fail (`arr⏎[0]` kept) |
| RH5302…AnalyzerTests.VerifyFixJoinsNullForgivingOperatorAndKeepsOperandListOnOneLine (change 5) | FMT-BREAK-040 | VerifyCodeFixWithEndOfLine | fail (one argument per line) |
| CliHiddenCodeEndToEndTests.CommentOrDirectiveInFrontOfAnAttachedPartIsReportedAsUnsupported (the 7 change-6 files) | FMT-UNSUP-014 | `Program.Main`, exit code, stderr line, file unchanged | fail (exit code 0) |

Existing tests changed. I searched every test project with a Roslyn scan of every string literal: first for the shape (FMT-UNSUP-014's gaps holding a comment, directive, or disabled text, in Parse and None mode), then for an attached part that starts a line with no trivia in the gap. I report each hit.

| Test | Change | Requirement tags now | Expected now |
|---|---|---|---|
| ArgumentListLineStartTests.RequirementExampleKeepsTheLineBreak… → `RequirementExampleRefusesTheInputWhenTheGapHoldsCommentDirectiveOrDisabledText` | FMT-BREAK-039's changed example: refused | FMT-BREAK-039, FMT-UNSUP-014 | fail |
| ArgumentListLineStartTests: `DelimitedCommentInFrontOfOpeningParenthesis…`, `CommentLineInTheGap…`, `PragmaDirectiveInTheGap…`, `RegionDirectiveInTheGap…`, `InactiveConditionalBlockInTheGap…`, `DelimitedDocumentationCommentBehindTheName…` (2), `DelimitedDocumentationCommentInFrontOfOpeningParenthesis…` (2), `SingleLineDocumentationCommentInTheGap…`, `CommentInTheGap…ForEveryOwner` → each renamed `…IsUnsupported…` | the kept line break becomes refused | FMT-UNSUP-014 (FMT-BREAK-039 tag removed) | fail |
| ChainLinkLayoutTests.ElementAccessWithoutMemberAccessKeepsItsBreak → `ElementAccessWithoutMemberAccessJoinsItsElement` | `list⏎[0]` → `list[0]` | FMT-BREAK-040 | fail |
| ChainLinkLayoutTests.CommentBeforeElementAccessKeepsElementAccessAtAnchorColumn, CommentInsideConditionalElementAccessPutsBracketUnderQuestionMark, CommentBeforeArgumentListKeepsArgumentListAtAnchorColumn → `CommentBeforeElementAccessInAChainIsUnsupported`, `CommentInsideConditionalElementAccessInAChainIsUnsupported`, `CommentBeforeArgumentListInAChainIsUnsupported` | the deleted column becomes refused | FMT-UNSUP-014 | fail |
| ConditionalAccessWrapBetweenOperatorTokensTests.CommentBetweenQuestionMarkAndBracketInShortChainAlignsBracketUnderQuestionMark → `…InShortChainIsUnsupported` | not in the handovers' lists; found by the scan | FMT-UNSUP-014 | fail |
| WithElementSpacingTests.SpecExampleRemovesTheSpaceBetweenWithAndItsArgumentList | the `d` line is dropped, as in the amended FMT-SPACE-028 example | FMT-SPACE-028 | pass |
| WithElementSpacingTests.CommentInTheGapStaysAndTheSpaceBehindItIsRemoved → `CommentInTheGapIsUnsupported` | refused | FMT-SPACE-028, FMT-UNSUP-014 | fail |
| ChainLinkLayoutTests: ConditionalElementAccessJoinsItsElement, ElementAccessJoinsItsElement (keep FMT-BREAK-020), TrailingNullForgivingJoinsItsElement, TrailingNullForgivingJoinsLastLink, TrailingNullForgivingInsideArgumentJoinsItsElement, OutermostElementAccessJoinsItsElement, OutermostConditionalElementAccessJoinsItsElement, OutermostElementAccessJoinsMemberName, ElementAccessAfterMemberNameJoinsInOnePass; ChainGapJoinTests.ConditionalElementAccessesWithoutLinksAreJoined | retagged only, assertions unchanged | + FMT-BREAK-040 | pass |

Existing tests the scans hit, left unchanged:

- **Analyzer-only tests.** Analyzers do not consult the precheck, so these stay green:
  - `RH5048FluentChainLinkAnalyzerTests.VerifyCommentBeforeWrappedConditionalElementAccessIsNotReported`
  - `RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzerTests.VerifyCommentBeforeAttachedElementAccessOfRootIsNotReported`
  - `RH5038…VerifyNoDiagnosticForCommentBeforeOpeningParenthesis`
  - `RH5038…VerifyNoDiagnosticForCommentBeforeConditionalElementAccessInArgument`
  - `RH5112…VerifyWrappedFirstLinkBehindAttachedElementAccessIsReported`
- **Fixes that are already withheld.** `RH5101…VerifyCommentBeforeTheOpeningParenthesisWithholdsTheCodeFix` and `RH5102…VerifyCommentBeforeTheOpeningParenthesisWithholdsTheCodeFix` stay unchanged.
- **RH3101.** `RH3101…AnalyzerTests` line 898 (`base /* a */ [0]` → `this /* a */ [0]`) stays unchanged. That fix keeps the shape and does not format through the formatter (05-skeptic). The test stays green on this head.
- **In-chain inputs whose expected output is already joined:**
  - `ChainLinkLayoutTests.SplitOperatorsInsidePrefixAndChainPartConvergeInOnePass`
  - `RH5112FluentChainLinkFormatterTests.VerifyWrappedConditionalElementAccessWithoutLinkIsNotReported`
  - `RH5201FluentChainLinkFormatterTests.VerifyWrappedElementAccessIsNotReported`
- **Other hits with no expected change.** Every other split-part hit is a `(` that FMT-BREAK-039 already joins, or an attribute list in a fragment with syntax errors.

## Not covered, and why

- **Throw sites of `SyntaxNotSupportedException`.** None. The change adds no phase refusal, and FMT-UNSUP-006 is unchanged (04-spec-author).
- **`new⏎[]`, `new int⏎[3]`, `stackalloc⏎[]` written with a line break.** FMT-BREAK-040 says it does not decide that gap, and these are follow-up candidates (02-skeptic). The requirement's third example locks the one-line forms as unchanged.
- **FC-013 analyzer/formatter parity.** No rule reports an attached part below its element (02-skeptic, P11), so a `VerifyFormatter` test has nothing to clear.
- **FC-008 Fix All across several RH5302 diagnostics.** The fix's code is unchanged and only its formatter output changes. The class's existing `GetFixAllScenario` and the iterative fix run in `VerifyCodeFixWithEndOfLine` cover convergence.
- **FC-006, FC-018, FC-023 to FC-026, FC-029, FC-036.** No rename, no member-kind rewrite, and no key/value layout is involved. `s!!` is checked to read back as two tokens (second pass).
- **FC-012 and FC-010.** These are covered by `AssertRuleResult`, which formats under LF and CRLF, runs a second pass, and runs `PhaseStabilityCheck` over the whole pipeline. No separate combined-pipeline test is needed.
- **The column of a part kept below by trivia.** The input is refused now, so no column is asserted.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~AttachedPartLineStartTests|FullyQualifiedName~UnsupportedAttachedPartCommentTests|FullyQualifiedName~ArgumentListLineStartTests|FullyQualifiedName~ChainLinkLayoutTests|FullyQualifiedName~ChainGapJoinTests|FullyQualifiedName~ConditionalAccessWrapBetweenOperatorTokensTests|FullyQualifiedName~WithElementSpacingTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~VerifyNoFixIsOfferedForACommentInFrontOfAnElementAccess|FullyQualifiedName~VerifyNoFixIsOfferedForACommentInFrontOfAnAttachedPart|FullyQualifiedName~ArgumentListOfTheInitializerValue|FullyQualifiedName~CommentInFrontOfAnArgumentListInAnInitializerValueWithholdsTheFix|FullyQualifiedName~CommentInFrontOfAnAttachedPartWithholdsTheFix|FullyQualifiedName~VerifyNoFixIsOfferedForACommentInFrontOfAnAttachedElementAccess|FullyQualifiedName~VerifyFixJoinsOperandElementAccessBelowItsElement|FullyQualifiedName~VerifyFixJoinsNullForgivingOperatorAndKeepsOperandListOnOneLine"
scripts/test.sh --project cli --filter "FullyQualifiedName~CommentOrDirectiveInFrontOfAnAttachedPartIsReportedAsUnsupported"
scripts/test.sh --project architecture
```

Results on this head:

- **Formatter.** The full project ran 6088 tests: 143 failed and 5945 passed. The 143 failures are exactly the tests this handover lists as failing. Each fails on the specified difference: a part still split, or the input not refused.
- **Analyzer.** The full project ran 10953 tests: 21 failed and 10932 passed. The 21 failures are exactly the new test cases, and each fails on a fix still being offered or on RH5302's split output.
- **CLI.** The full project ran 403 tests: 7 failed and 396 passed. The 7 failures are exactly the new cases, and each fails on exit code 0.
- **Architecture.** 198 passed. `EveryRequirementOfAnEnforcedSpecIsCovered` now passes.

## Findings for the lead

- **The change doc's CLI lines use the wrong prefix.** Changes 6 and 7 write the CLI's line as `Error: <file>(…): unsupported syntax: …`. The CLI prints `error:` in lower case: `src/Reihitsu.Cli/FormatCommandHandler.cs:536-539` writes the severity `error`, and `README.MD` and every existing CLI test agree. Decision 6 in `00-lead.md` says to use what the CLI prints today. The CLI test asserts `error:`. Only the doc text is affected; no output changes.

## Follow-up candidates

_None_ new.
