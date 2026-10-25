Role: test implementer
Commit: 482ab95
Verdict: tests written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. Expected values come from the spec examples (`tab-replacement.md`, `pipeline.md` FMT-PIPE-015/016, `indentation.md` FMT-INDENT-030, `raw-string-alignment.md` FMT-RAW-005) and from the change doc's table of changed outputs. Where the spec gives one column only, the other lines are today's output, which the change doc says stays. Three kinds of test take their whole expected text from today's unpatched code on the four-space variant of the input: t06, t07 (the change doc says these now give the output of the space variant), and the cells that hold today (t01–t03 and the left-token comment gap). Each such test also asserts the four-space variant against the same expected text, so the tab input and the space input are pinned to one output. Every result below was observed on 482ab95 plus these test files. In names, t06/t07/g01–g08/g05b/i30 refer to the inputs in `02-skeptic.md`, `03-spec-author.md`, and the change doc.

## Tests

New helpers: `src/Reihitsu.Formatter.Test/Helpers/ObservedPipeline.cs` and `ObservedPhase.cs`. They run `FormattingPipeline.Execute` with the phase observer and record each phase's input and output. Three more helpers live in `ObservedPipeline`:
- `RunTabReplacementPhase`: asserts that the first phase is named `TabReplacementPhase` and returns its output.
- `FindTabsInCodeAndDirectiveWhitespace`: lists tabs in `WhitespaceTrivia` of tokens from `DescendantTokens()`, which does not descend into documentation comments, plus tabs in the `WhitespaceTrivia` of the tokens inside each `DirectiveTriviaSyntax`. This is the S1 scope.
- `GetLinesWithoutTrailingWhitespace`: returns the lines without their end-of-line characters and trailing whitespace, and drops the empty lines at the end of the file.

The tests reference no production type that the change adds or removes, so the test projects compile on today's code.

"Fails" below means fails on 482ab95, for the reason given; "passes" means passes on 482ab95 and must keep passing.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Unit.TabReplacement.TabReplacementPhaseTests.TabsBeforeTrailingCommentsBecomeFourSpacesEach` | FMT-TAB-001 | `ObservedPipeline.RunTabReplacementPhase`, LF+CRLF | fails: first phase is `StructuralTransformPhase` |
| `…TabReplacementPhaseTests.TabsInIndentationBetweenTokensAndAtLineEndBecomeFourSpacesEach` | FMT-TAB-001 | same | fails: same |
| `…TabReplacementPhaseTests.TabInFrontOfHoleLineOfMultiLineInterpolatedRawStringBecomesFourSpaces` | FMT-TAB-001 | same | fails: same |
| `…TabReplacementPhaseTests.TabInHoleCodeIsReplacedAndTabInInterpolatedTextIsKept` | FMT-TAB-001, FMT-TAB-003 | same | fails: same |
| `…TabReplacementPhaseTests.TabsBetweenDirectiveTokensBecomeFourSpacesEach` | FMT-TAB-002 | same | fails: same |
| `…TabReplacementPhaseTests.TabBetweenRegionKeywordAndDescriptionIsReplacedAndTabsInsideFreeTextAreKept` | FMT-TAB-002 | same | fails: same |
| `…TabReplacementPhaseTests.TabsInsideCommentsLiteralsAndDisabledTextAreKept` | FMT-TAB-003 | same | fails: same |
| `…TabReplacementPhaseTests.TabsInsideCharacterInterpolatedAndRawStringLiteralsAreKept` | FMT-TAB-003 | same | fails: same |
| `…TabReplacementPhaseTests.TabInFrontOfEndifAfterDisabledTextIsReplacedAndTabInsideDisabledTextIsKept` | FMT-TAB-001, FMT-TAB-003 | same | fails: same |
| `…TabReplacementPhaseTests.TabInsideDocumentationCommentIsKeptAndTabInFrontOfMemberIsReplaced` | FMT-TAB-001, FMT-TAB-003 | same | fails: same |
| `…TabReplacementPhaseTests.TabInFrontOfDocumentationCommentContinuationLineIsKept` | FMT-TAB-003 | same | fails: same |
| `Regression.TabReplacement.TabReplacementTests.TabsBeforeTrailingCommentsBecomeFourSpacesEach` (moved from `WhitespaceCleanupTests`) | FMT-TAB-001 | `AssertRuleResult` | passes |
| `…TabReplacementTests.ChainLinkBehindTabAndDelimitedDocumentationCommentAlignsInOnePass` (t06) | FMT-TAB-001, FMT-PIPE-004 | `AssertRuleResult`, tab and space variants | fails: tab variant puts `.C()` at 26, expected 29 |
| `…TabReplacementTests.ChainLinkBehindRawStringBehindTabAndDelimitedDocumentationCommentAlignsInOnePass` (t07) | FMT-TAB-001, FMT-PIPE-004 | same | fails: `.ToUpper` at 34, expected 37 |
| `…TabReplacementTests.ArgumentBehindTabAndDelimitedDocumentationCommentFormatsLikeFourSpaces` (t01) | FMT-TAB-001 | same | passes |
| `…TabReplacementTests.BinaryOperandBehindTabAndDelimitedDocumentationCommentFormatsLikeFourSpaces` (t02) | FMT-TAB-001 | same | passes |
| `…TabReplacementTests.InitializerElementBehindTabAndDelimitedDocumentationCommentFormatsLikeFourSpaces` (t03) | FMT-TAB-001 | same | passes |
| `…TabReplacementTests.TabInFrontOfCommentCarriedByTheLeftTokenCollapsesLikeFourSpaces` | FMT-TAB-001 | same | passes |
| `…TabReplacementTests.FormatNodeAlignsChainLinkBehindTabAndDelimitedDocumentationCommentInOnePass` (t06 via `FormatNode`) | FMT-TAB-001; FC-028 | `ReihitsuFormatter.FormatNode`, LF+CRLF | fails: `.C()` at 26 |
| `…TabReplacementTests.FormatNodeInDocumentAlignsChainLinkBehindTabAndDelimitedDocumentationCommentInOnePass` (t06 via `FormatNodeInDocumentAsync`, target the method) | FMT-TAB-001; FC-028 | `AdhocWorkspace`, LF+CRLF | fails: `.C()` at 26 |
| `…TabReplacementTests.TabsBetweenDirectiveTokensBecomeFourSpacesEach` (moved) | FMT-TAB-002 | `AssertRuleResult` | passes |
| `…TabReplacementTests.TabBetweenRegionKeywordAndDescriptionKeepsFourSpacesUnlessTheRegionLineIsRewritten` | FMT-TAB-002 | `AssertRuleResult` | passes |
| `…TabReplacementTests.TabInsideRegionDescriptionIsKeptInBothDirectives` | FMT-TAB-002 | `AssertRuleResult` | passes |
| `…TabReplacementTests.TabsInsideRegionDescriptionAndWarningMessageAreKept` (moved) | FMT-TAB-002 | `AssertRuleResult` | passes |
| `…TabReplacementTests.TabsInsideOtherLiteralsAndDocumentationCommentsAreKept` (moved) | FMT-TAB-003 | `AssertRuleResult` | passes |
| `…TabReplacementTests.TabInsideDocumentationCommentElementIsKeptAndTabInFrontOfMemberIsReplaced` | FMT-TAB-003 | `AssertRuleResult` | passes |
| `…TabReplacementTests.TabIndentedHoleLineStartsInTheColumnOfItsNeighboursAndMovesWithTheLiteral` | FMT-TAB-003 | `AssertRuleResult` | passes |
| `Regression.Cleanup.WhitespaceCleanupTests.TabsAndSpacesInsideCommentsLiteralsAndDisabledTextAreKept` (retagged) | FMT-CLEAN-004, FMT-TAB-003 | `AssertRuleResult` | passes |
| `Unit.Pipeline.FormattingPipelineTests.ExecuteObserverReportsAllPhasesInOrderWithoutChangingOutput` (expected list changed: `TabReplacementPhase` first, no `RawStringAlignmentPhase`) | FMT-PIPE-001 | observer | fails: phase list differs |
| `Unit.Pipeline.PhaseColumnOwnershipTests.LinesOfTheSpecExampleAreFinalAfterTheIndentationPhase` | FMT-PIPE-015 | `ObservedPipeline.Run`, LF+CRLF | fails: `RawStringAlignmentPhase` changes lines after `IndentationPhase` |
| `…PhaseColumnOwnershipTests.TrailingCommentBehindATabKeepsItsColumnAfterTheIndentationPhase` | FMT-PIPE-015 | same | fails: `CleanupPhase` turns `Call(1,\t// c` into four spaces |
| `…PhaseColumnOwnershipTests.AnchorBehindTheClosingMarkerOfAMovedRawStringIsFinalAfterTheIndentationPhase` (i30) | FMT-PIPE-015 | same | fails: `RawStringAlignmentPhase` moves lines |
| `…PhaseColumnOwnershipTests.TabIndentedHoleLineOfAMovedRawStringIsFinalAfterTheIndentationPhase` | FMT-PIPE-015 | same | fails: same |
| `…PhaseColumnOwnershipTests.FirstPhaseReplacesEveryIndentationTabAndNoLaterPhaseSeesOne` | FMT-PIPE-016 | same | fails: first phase is `StructuralTransformPhase` |
| `…PhaseColumnOwnershipTests.NoPhaseAfterTheFirstSeesATabInTheWhitespaceOfTheCodeOrBetweenDirectiveTokens` | FMT-PIPE-016 | same | fails: same |
| `…PhaseColumnOwnershipTests.TabInFrontOfDocumentationContinuationLineIsReplacedByTheIndentationPhase` | FMT-PIPE-016 | same | fails: same |
| `Unit.RawStringAlignment.IndentationPhaseRawStringMoveTests` — `MisalignedNonInterpolatedRawStringIsCorrected`, `FourQuoteRawStringIsAlignedCorrectly`, `MultipleContentLinesAreAdjustedUniformly`, `LeftwardShiftIsHandledCorrectly`, `MisalignedUtf8RawStringIsCorrected`, `MisalignedInterpolatedRawStringIsCorrected`, `InterpolatedRawStringWithMultipleHolesIsAligned`, `DoubleDollarInterpolatedRawStringIsAlignedCorrectly`, `MultipleRawStringsAreAlignedIndependently` (moved from `RawStringAlignmentPhaseTests`; inputs, expected values, and tags unchanged) | FMT-RAW-001, FMT-RAW-002, FMT-RAW-003, FMT-RAW-004 (as before) | `FormatterPhaseTestsBase`, `new IndentationPhase().Execute` | fail: `IndentationPhase` leaves content and closing lines where they are |
| `…IndentationPhaseRawStringMoveTests` — `AlreadyAlignedNonInterpolatedRawStringIsNotModified`, `SingleLineRawStringIsNotModified`, `AlreadyAlignedUtf8RawStringIsNotModified`, `AlreadyAlignedInterpolatedRawStringIsNotModified` (moved) | FMT-RAW-004, FMT-RAW-006 (as before) | same | pass |
| `Regression.Indentation.CommentLedLineAlignmentTests.ArgumentAlignsToFirstArgumentBehindCommentThatLeadsTheLine` (g02) | FMT-INDENT-030 | `AssertRuleResult` | fails: `2` at 13, expected 21 |
| `…CommentLedLineAlignmentTests.ArgumentAlignsToFirstArgumentBelowLineCommentOnTheLineAbove` (g02, `// c` side) | FMT-INDENT-030 | same | passes |
| `…CommentLedLineAlignmentTests.ArgumentAlignsToFirstArgumentBehindDelimitedDocumentationCommentThatLeadsTheLine` (g04) | FMT-INDENT-030 | same | fails: `2` at 13, expected 22 |
| `…CommentLedLineAlignmentTests.ArgumentAlignsToFirstArgumentBehindTwoCommentsThatLeadTheLine` | FMT-INDENT-030 | same | fails: `2` at 13, expected 29 |
| `…CommentLedLineAlignmentTests.ArgumentAlignsToFirstArgumentBehindTheLastLineOfAMultiLineComment` (g05b) | FMT-INDENT-030 | same | fails: `2` at 13, expected 18 |
| `…CommentLedLineAlignmentTests.ChainLinkAlignsToFirstLinkBehindCommentThatLeadsTheLine` (g07) | FMT-INDENT-030 | same | fails: `.C()` at 9, expected 17 |
| `…CommentLedLineAlignmentTests.ChainLinkAlignsToFirstLinkBelowDelimitedCommentThatEndsItsLine` (qualifier's other side) | FMT-INDENT-030 | same | passes |
| `Unit.RawStringAlignment.RawStringAlignmentPipelineTests.ChainLinkAlignsToAnchorBehindClosingMarkerOfAMovedRawString` (i30, spec example) | FMT-INDENT-030 | `AssertRuleResult` | passes |
| `…RawStringAlignmentPipelineTests.ChainLinkBehindClosingMarkerOfAnUnmovedRawStringIsLeftUnchanged` (its other side) | FMT-INDENT-030 | same | passes |
| `…RawStringAlignmentPipelineTests.RawStringOnCommentLedLineIsAlignedToOpeningQuotesBehindTheComment` (g03) | FMT-RAW-005 | same | passes |
| `…RawStringAlignmentPipelineTests.ChainLinkAlignsToAnchorBehindClosingMarkerOfRawStringOnCommentLedLine` (g01) | FMT-INDENT-030 | same | fails: `.ToUpper` at 19, expected 27 |
| `…RawStringAlignmentPipelineTests.ChainLinkAlignsToAnchorBehindClosingMarkerOfInterpolatedRawStringOnCommentLedLine` (g06) | FMT-INDENT-030 | same | fails: 20, expected 28 |
| `…RawStringAlignmentPipelineTests.ChainLinkAlignsToAnchorBehindClosingMarkerOfRawStringArgumentOnCommentLedLine` (g08, blank line kept, literal at 29) | FMT-INDENT-030 | same | fails: 24, expected 32 |
| `…RawStringAlignmentPipelineTests.FormatNodeAlignsChainLinkBehindClosingMarkerOfRawStringOnCommentLedLine` (g01 via `FormatNode`) | FMT-INDENT-030; FC-028 | `ReihitsuFormatter.FormatNode`, LF+CRLF | fails: 19, expected 27 |
| `Reihitsu.Analyzer.Test.Formatter.Formatting.RH5202RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests.VerifyFormatterAlignsLiteralOnCommentLedLineAndClearsTheDiagnostic` (g01) | FMT-RAW-005; FC-013 | `FormatterTestsBase<TAnalyzer>.VerifyFormatter(source, fixed, diagnostic)` | fails: `.ToUpper` at 19 |
| `…RH5601UseTabsCorrectlyFormatterTests.VerifyFormatterAlignsChainLinkBehindTabAndDelimitedDocumentationCommentInOnePass` (t06 with `.Trim()`/`.ToUpper()` so it compiles) | FMT-TAB-001; FC-013 | same | fails: `.ToUpper` at 26, expected 29 |

Removed, because their behavior moved:
- `CleanupPhaseTests.ReplacesTabsInWhitespaceTriviaWithSpaces`, `.ReplacesTabsInsideDirectiveInteriorWithSpaces`, `.PreservesTabsInsideComments`. They called `CleanupPhase` directly. Their shapes are now asserted on the first phase's output in `TabReplacementPhaseTests`: the tab before a trailing comment, a tab between directive tokens, and a tab in a comment.
- `RawStringAlignmentPhaseTests.cs` is deleted (`git rm`). All 13 tests now live in `IndentationPhaseRawStringMoveTests`, because `RawStringAlignmentPhase` leaves the pipeline.

`WhitespaceCleanupTests` lost four tab tests, which moved unchanged into `TabReplacementTests` with FMT-TAB tags. Its summary no longer mentions tabs.

Coverage, observed with `scripts/test.sh --project architecture`: 82 passed, 2 failed. Both failures wait on production code:
- `EveryCodeOwnerNamesAnExistingType`: `TabReplacementPhase` does not exist yet.
- `OwnershipMapMatchesRuntimePhaseOrder`: the runtime still has `RawStringAlignmentPhase`.

`EveryRequirementReferenceNamesAnActiveRequirement` and `EveryRequirementOfAnEnforcedSpecIsCovered` now pass: no test is tagged FMT-CLEAN-002/003 any more, and FMT-PIPE-015/016 and FMT-TAB-001..003 are tagged.

Full formatter project on 482ab95 with these files: 2950 tests, 40 failed (before the `FormatNode` raw-string test was added; it brings the count to 41). Every failure is a test listed above as "fails". No test that passed before fails.

## Not covered, and why

- **t04** (`var x = c→/** d */ ? 1` / `: 2;`): not tested. It is not idempotent with four spaces either, because of a blank line that appears in pass 2 (follow-up candidate 3 in `02-skeptic.md`; thoenissen/Reihitsu#962). `AssertRuleResult` would fail on that unrelated defect.
- **Node-level column of a target behind a tab, and `AdjustNodeIndentation` on raw-string lines** (follow-ups 1 and 2 in `02-skeptic.md`): the change doc's inventory marks both "Not changed here". The FC-028 tests above put the tab inside the target, not in front of it.
- **RH5202 code fix** (thoenissen/Reihitsu#1458) and **`RootListPosition`**: non-goals of the change doc.
- **Moved phase-level raw-string tests under both line endings**: they keep their original single line ending (two of them are CRLF). The CRLF dimension of the move is covered by every `AssertRuleResult` and observer test above, which run LF and CRLF.
- **FC-011 (loop without a decreasing measure)**: the raw-string move joins the alignment sweep loop. A hang would show as a timeout in the existing shared-line test `RawStringAlignmentPipelineTests.ChainLinkBehindSecondRawStringOnASharedLineAlignsAfterBothLiteralsMoved`, in the nested-literal tests of `RawStringInterpolationHoleTests`, and in `SelfHostingTests`. No new dedicated test.
- **Failure classes that cannot reach this change:**
  - FC-004, FC-005, FC-020: the change joins no line and moves or deletes no token, member, comment, or directive.
  - FC-006, FC-023, FC-024, FC-025, FC-029, FC-036: no syntax or semantic rewrite and no rename.
  - FC-007, FC-008, FC-009, FC-018, FC-030: no code fix changes.
  - FC-014: no new rule.
  - FC-031, FC-032, FC-037: about comments and process, not testable here; for the auditor.
- **Failure classes covered above without a separate test:**
  - FC-019 and FC-021: comments in the gaps (trailing comment, `/** */`, `/* */` ending in the middle of a line, the tail of a multi-line comment) and directives (`#define`, `#pragma`, `#region`, `#warning`, `#endif` after disabled text) are in the tests above.
  - FC-022 and FC-027: plain, UTF-8, four-quote, `$`, and `$$` raw strings; a literal as an argument. Nested literals in holes stay covered by the existing `RawStringInterpolationHoleTests`, which pass.
  - FC-012, FC-015, FC-016, and the catalog matrix "Tab width across formatter phases": its cells stay covered by the existing `RawStringInterpolationHoleTests` tab tests, which pass. The FMT-SPACE-014 gap that the matrix lacks is covered by t06 and t07.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~TabReplacement|FullyQualifiedName~PhaseColumnOwnershipTests|FullyQualifiedName~FormattingPipelineTests|FullyQualifiedName~IndentationPhaseRawStringMoveTests|FullyQualifiedName~RawStringAlignmentPipelineTests|FullyQualifiedName~CommentLedLineAlignmentTests|FullyQualifiedName~WhitespaceCleanupTests|FullyQualifiedName~CleanupPhaseTests|FullyQualifiedName~RawStringInterpolationHoleTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5202RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests|FullyQualifiedName~RH5601UseTabsCorrectlyFormatterTests"
scripts/test.sh --project architecture
```

The formatter command observed on 482ab95 plus these files: 138 tests, 41 failed, 97 passed. The analyzer command: 2 failed (the two new tests), the other tests of both classes passed.
