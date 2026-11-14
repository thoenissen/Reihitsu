Role: test implementer
Commit: 68baffa
Verdict: #1345: tests written; #1346: tests written; #1347: tests written; #1348: tests written; #1270: tests written; #1092: tests written (characterization only, decision 8); #1153: tests written; #1617: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples and requirement text as of `68baffa` (`git diff cf6d26b -- sdd/specs documentation`), the change doc `sdd/changes/2026-11/documentation-rules-reported-elements.md`, and the maintainer answers in `00-lead.md`; no expected value was taken from running a patched copy. "RED" below means the test fails on `68baffa`; each RED test was read and fails on the difference the spec states (missing report, extra report, wrong location), not on a compile error or unrelated layout. "GREEN" tests pin a side of a boundary that already holds and must stay.

Run on `68baffa` (Debug): analyzer filter below — 637 tests, 69 failed, every failure one of the RED tests listed here; formatter `BlankLineRequirementTests` — 20 passed; architecture project — 117 passed (requirement coverage now finds tests for RH8303-006, RH8401-006, RH8402-007; no test tags retired RH8307-006).

## thoenissen/Reihitsu#1345

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8301…AnalyzerTests.VerifySpecificationExampleCommentWrittenBehindCodeIsDetectedAndFixed | RH8301-001, RH8301-004 | `Verify(source, fixed)` | RED (not reported) |
| RH8301….VerifyElementCommentWrittenBehindCodeIsDetectedAndFixed | RH8301-001, RH8301-004 | `Verify(source, fixed)` | RED |
| RH8301….VerifyEveryLineOfMultiLineCommentWrittenBehindCodeIsDetectedAndFixed | RH8301-001, RH8301-004 | `Verify(source, fixed)` | RED (1 of 2 reported) |
| RH8301….VerifyTabSeparatorOfCommentWrittenBehindCodeIsDetectedAndReplaced | RH8301-001, RH8301-004 | `Verify(source, fixed)` | RED |
| RH8301….VerifyCommentWrittenBehindCodeWithSingleSpaceDoesNotProduceDiagnostics | RH8301-001 | `Verify(source)` | GREEN |
| RH8301….VerifyFixOfCommentWrittenBehindCodeWithCarriageReturnLineFeed | RH8301-004 | `VerifyCodeFixWithEndOfLine` CRLF | RED |
| RH8301….VerifyCommentWrittenBehindCodeIsNotReportedWhenDocumentationModeIsNone | RH8301-001 | `Verify` + mode None | GREEN |
| RH8302…AnalyzerTests.VerifySpecificationExampleBlankLineAfterCommentWrittenBehindCodeIsDetectedAndFixed | RH8302-001, RH8302-004 | `Verify(source, fixed)` | RED |
| RH8302….VerifyRunAfterMultiLineCommentWrittenBehindCodeIsDetectedAndFixed | RH8302-001, RH8302-003, RH8302-004 | `Verify(source, fixed)` | GREEN (last line starts its own line) |
| RH8302….VerifyFixAfterCommentWrittenBehindCodeWithCarriageReturnLineFeed | RH8302-004 | `VerifyCodeFixWithEndOfLine` CRLF | RED |
| RH8303…AnalyzerTests.VerifySpecificationExampleCommentWrittenBehindCodeIsMovedOntoItsOwnLine | RH8303-001, RH8303-004, RH8303-006 | `Verify` with one incremental and one Fix All iteration (FC-007) | RED (reported on line 2) |
| RH8303….VerifySingleLineCommentWrittenBehindCodeIsMovedOntoItsOwnLine | RH8303-001, RH8303-004, RH8303-006 | `Verify`, one iteration | RED (not reported) |
| RH8303….VerifySpecificationExampleCommentWrittenBehindOpeningBraceDoesNotProduceDiagnostics | RH8303-001, RH8303-002 | `Verify(source)` | GREEN |
| RH8303….VerifyCommentWrittenBehindClosingBraceIsMovedOntoItsOwnLine | RH8303-001, RH8303-006 | `Verify(source, fixed)` | RED |
| RH8303….VerifyFixRemovesWhiteSpaceBetweenCodeAndMovedComment | RH8303-006 | `Verify(source, fixed)`, spaces and a tab behind the code | RED |
| RH8303….VerifyMovedCommentTakesTheColumnOfTheAttributeList | RH8303-006 | `Verify(source, fixed)` | RED |
| RH8303….VerifyMovedCommentTakesTheIndentationOfTheDeclaration | RH8303-006 | `Verify(source, fixed)` | RED |
| RH8303….VerifyCommentWrittenBehindCodeInNamespaceIsMovedOntoItsOwnLine | RH8303-006 | `Verify(source, fixed)` | RED |
| RH8303….VerifyCommentWrittenBehindCodeIsMovedWithCarriageReturnLineFeed | RH8303-006 | `VerifyCodeFixWithEndOfLine` CRLF | RED |
| RH8303….VerifyFixAllMovesCommentsWrittenBehindCodeNextToCommentOnItsOwnLine | RH8303-005, RH8303-006 | `Verify(source, fixed)`, three diagnostics in one document (FC-008) | RED (2 of 3 reported) |

`VerifyMovedCommentTakesTheIndentationOfTheDeclaration` asserts the literal reading of RH8303-006: the comment's first line takes the white space in front of the declaration (12 spaces) while its other lines stay as written (8 spaces). That output is what the requirement's two sentences give together; if the lead reads it as unintended, it is a spec question, not a test fix.

### Scenarios (bug fix)
_N/A_ — derived from spec examples; the reproduction of the behind-code shapes is the RED rows above (single-line not reported, multi-line reported on line 2), observed on LF and CRLF.

## thoenissen/Reihitsu#1346

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8401….VerifyDiagnosticAndCodeFixWhenDocumentationModeIsNone (rewritten from `VerifyNoDiagnosticsWhenDocumentationModeIsNone`, which asserted the opposite of SHARED-DOC-008) | RH8401-006 | `Verify(source, fixed)` + mode None | RED |
| RH8401….VerifySpecificationExampleWhenDocumentationModeIsNone | RH8401-006 | `Verify(source, fixed)` + mode None | RED |
| RH8401….VerifyCommentsWithoutXmlAreReportedWhereverTheyStandWhenDocumentationModeIsNone | RH8401-006 | `Verify` + mode None (mirror of the mode-Parse test) | RED |
| RH8401….VerifyCommentsWithXmlAreNotReportedWhenDocumentationModeIsNone | RH8401-006 | `Verify` + mode None | GREEN |
| RH8401….VerifyCommentContinuedBelowCodeWithElementIsNotReportedWhenDocumentationModeIsNone | RH8401-006 (FC-040) | `Verify` + mode None | GREEN |
| RH8401….VerifyBlankLineEndsTheCommentWhenDocumentationModeIsNone | RH8401-006 | `Verify(source, fixed)` + mode None | RED |
| RH8401….VerifyCodeFixWithCarriageReturnLineFeedWhenDocumentationModeIsNone | RH8401-006 | `Verify` CRLF + mode None | RED |
| RH8401….VerifyNoCodeFixForByteOrderMarkInFrontOfSecondLineWhenDocumentationModeIsNone | RH8401-006 | `VerifyNoCodeFixIsOffered(source, DocumentationMode.None)` | RED |
| RH8402….SpecificationExampleMissingHeaderWhenDocumentationModeIsNone | RH8402-007 | new private `VerifySpecificationFileFixWhenDocumentationModeIsNone` | RED |
| RH8402….SpecificationExampleDocumentationCommentBehindOldHeaderIsKeptWhenDocumentationModeIsNone | RH8402-007 | same | RED |
| RH8402….DelimitedDocumentationCommentBehindOldHeaderIsKeptWhenDocumentationModeIsNone | RH8402-007 | same | RED |
| RH8402….FixInsertsHeaderInFrontOfDocumentationCommentWhenDocumentationModeIsNone | RH8402-007 | same | RED |
| RH8402….FixReplacesOldHeaderAndKeepsBlankLineInFrontOfDocumentationCommentWhenDocumentationModeIsNone | RH8402-007 | same | RED |
| RH8402….DocumentationCommentBehindOldHeaderIsKeptWithCarriageReturnLineFeedWhenDocumentationModeIsNone | RH8402-007 | same, CRLF | RED |
| RH8402….FileNotStartingWithHeaderWhenDocumentationModeIsNone (3 rows) | RH8402-007 | `VerifySpecificationFile` + mode None | RED |
| RH8402….FileStartingWithHeaderIsNotReportedWhenDocumentationModeIsNone (2 rows) | RH8402-007 | same | GREEN |

Every mode-None test applies `ApplyDocumentationModeNoneToTestProject` (FC-033: a test that only expects no diagnostics would pass without it, so the GREEN ones sit next to RED ones in the same mode).

## thoenissen/Reihitsu#1347

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8307….VerifyContentStartingBesideTheStartTagIsReported (changed: nested `<para>` block removed, as in the spec example; 3 diagnostics) | RH8307-001, RH8307-003 | `Verify(source)` | GREEN |
| RH8307….VerifyElementOfDelimitedDocumentationCommentDoesNotProduceDiagnostics (was `VerifySpecificationExampleNoCodeFixForElementOfDelimitedDocumentationComment`, RH8307-006) | RH8307-002 | `Verify(source)` | RED |
| RH8307….VerifyNestedElementOfDelimitedDocumentationCommentDoesNotProduceDiagnostics (was `VerifyNoCodeFixForNested…`, RH8307-006) | RH8307-002 | `Verify(source)` | RED |
| RH8307….VerifyNestedElementOfSingleLineDocumentationCommentDoesNotProduceDiagnostics (was `VerifyFixForNested…`, RH8307-004/-006) | RH8307-002 | `Verify(source)` | RED |
| RH8307….VerifySpecificationExampleElementsOfDelimitedDocumentationCommentsDoNotProduceDiagnostics | RH8307-002 | `Verify(source)` | RED |
| RH8307….VerifySpecificationExampleNestedElementsDoNotProduceDiagnostics | RH8307-002 | `Verify(source)` | RED |
| RH8307….VerifySpecificationExampleOnlyTheTopLevelElementIsReported | RH8307-001, RH8307-002 (FC-002/FC-003) | `Verify(source)` | RED |
| RH8307….VerifyTopLevelElementStartingWithHalfWrappedNestedElementIsReportedAlone | RH8307-001, RH8307-002 | `Verify(source)` | RED |
| RH8307….VerifyElementsNestedTwoLevelsDeepDoNotProduceDiagnostics | RH8307-002 (FC-027) | `Verify(source)` | RED |
| RH8307….VerifyInlineElementsNestedInSummaryDoNotProduceDiagnostics | RH8307-002 | `Verify(source)` | RED |

No test tags retired RH8307-006 any more.

## thoenissen/Reihitsu#1348

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8302….VerifySpecificationExampleBlankLineBeforeRegionDirectiveDoesNotProduceDiagnostics | RH8302-002 | `Verify(source)` | RED |
| RH8302….VerifyBlankLineBeforeRegionDirectiveDoesNotProduceDiagnosticsWithCarriageReturnLineFeed | RH8302-002 | `Verify` CRLF | RED |
| RH8302….VerifyRunOfBlankLinesBeforeRegionDirectiveDoesNotProduceDiagnostics | RH8302-002 | `Verify(source)` | RED |
| RH8302….VerifyBlankLineBetweenDelimitedCommentAndRegionDirectiveDoesNotProduceDiagnostics | RH8302-002 | `Verify(source)` | RED |
| RH8302….VerifyBlankLineBeforeOtherDirectiveIsReportedNextToRegionDirective | RH8302-001, RH8302-002 (FC-003: `#if` still reported) | `Verify(source)` | RED (region run also reported) |
| RH8302…FormatterTests.VerifyBlankLineBeforeRegionDirectiveIsNotReportedBeforeOrAfterFormatting | RH8302-002 (FC-012, FC-013) | `FormatterTestsBase.VerifyFormatter(source, fixed)`, LF and CRLF, second pass | RED (RH8302 on the input's `#endregion` run) |
| BlankLineRequirementTests.RegionDirectiveBelowDocumentationCommentKeepsOneBlankLine | FMT-BLANK-010 | `AssertRuleResult` (LF, CRLF, idempotency, phase stability) | GREEN |
| BlankLineRequirementTests.OneBlankLineBetweenDocumentationCommentAndRegionDirectiveStays | FMT-BLANK-010 | `AssertRuleResult` | GREEN |
| BlankLineRequirementTests.SeveralBlankLinesBetweenDocumentationCommentAndRegionDirectiveCollapseToOne | FMT-BLANK-010 | `AssertRuleResult` | GREEN |
| BlankLineRequirementTests.EndRegionDirectiveDirectlyBelowDocumentationCommentGetsOneBlankLine | FMT-BLANK-010 | `AssertRuleResult` | GREEN |
| BlankLineRequirementTests.BlankLineBeforeDeclarationIsRemovedNextToRegionDirective | FMT-BLANK-010 | `AssertRuleResult` | GREEN |

The formatter output already meets FMT-BLANK-010 (decision 9); these pin it.

## thoenissen/Reihitsu#1270

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8106….VerifySpecificationExampleEmptyReturnsElementIsReportedInEitherOrder | RH8106-001 | `Verify(source)` | RED |
| RH8106….VerifySpecificationExampleEveryEmptyReturnsElementIsReported | RH8106-002 | `Verify(source)` | RED |
| RH8106….VerifyNoDiagnosticForSeveralReturnsElementsWithText | RH8106-001 | `Verify(source)` | GREEN |
| RH8106….VerifyOnlyTheTopLevelEmptyReturnsElementIsReportedNextToNestedOne | RH8106-001 | `Verify(source)` | GREEN |
| RH8203….VerifySpecificationExampleFieldLikeEventWithEventInheritingNothingIsReported | RH8203-001 | `Verify(source)` | RED |
| RH8203….VerifyFieldLikeEventWhoseEventsAllImplementIsNotReported | RH8203-001 | `Verify(source)` | GREEN |
| RH8305….VerifySpecificationExampleSecondSummaryOfCommentIsDetectedAndFixed | RH8305-001, RH8305-004 | `Verify(source, fixed)` | RED |
| RH8305….VerifyTwoOneLineSummariesOfCommentAreDetectedAndFixed | RH8305-001, RH8305-003, RH8305-004 (FC-008) | `Verify(source, fixed)` | RED |
| RH8305….VerifyFirstSummaryOfCommentIsReportedAlone | RH8305-001, RH8305-002 | `Verify(source)` | GREEN |
| RH8306….VerifySpecificationExampleSentenceSummaryIsReportedInEitherOrder | RH8306-001 | `Verify(source)` | RED |
| RH8306….VerifySpecificationExampleEverySentenceSummaryIsReported | RH8306-004 | `Verify(source)` | RED |
| RH8306….VerifyNoDiagnosticForSeveralNounPhraseSummaries | RH8306-001 | `Verify(source)` | GREEN |

The partial-property "reported once" side is already covered by the existing `VerifyPartialPropertyWith…` tests (RH8306-001, SHARED-DIAG-012), which pass and must keep passing.

## thoenissen/Reihitsu#1092

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8002….VerifyUndocumentedPartWithoutAccessibilityOfPublicPartialClassIsReported | none (characterization, decision 8) | `Verify(source)` | GREEN — RH8002 reports the modifier-less nested part `Visible` |

## thoenissen/Reihitsu#1153

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8032….VerifySpecificationExampleParameterWithoutNameIsNotReported | RH8032-001 | `Verify(source)` | RED (extra RH8032 without a location) |
| RH8032….VerifyParameterWithoutNameOfGenericExtensionDeclarationIsNotReported | RH8032-001 | `Verify(source)` | RED |
| RH8032….VerifyParameterWithoutNameOfExtensionDeclarationWithStaticMemberIsNotReported | RH8032-001 | `Verify(source)` | RED |

The named-parameter side (`extension(string value)` reported) is inside the spec-example test.

## thoenissen/Reihitsu#1617

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8201….VerifyDiagnosticForDocumentedDefiningPartialInterfaceImplementation (tag added only) | RH8201-001 | `Verify(source, fixed)` | GREEN |
| RH8201….VerifyDiagnosticForDocumentedImplementingPartialInterfaceImplementation (was `VerifyNoDiagnosticFor…`, which asserted the opposite of amended RH8201-001) | RH8201-001, RH8201-005 | `Verify(source, fixed)` | RED |
| RH8201….VerifySpecificationExamplePartialMemberIsJudgedPartByPart | RH8201-001, RH8201-004 | `Verify(source)` | RED |
| RH8201….VerifySpecificationExampleFixOfPartialMemberReplacesEachPart | RH8201-005 | `Verify(source, fixed)` | RED |
| RH8201….VerifyFixOfImplementingPartLeavesDefiningPartReported | RH8201-001, RH8201-005 (FC-007) | `TryApplyCodeFixAtAsync` + `Verify` of the result | RED |
| RH8201….VerifyOnlyThePartWithoutInheritdocOfPartialMethodIsReported | RH8201-001 | `Verify(source)` | RED |
| RH8201….VerifyBothPartsOfPartialPropertyAndIndexerAreDetectedAndFixed | RH8201-001, RH8201-005 | `Verify(source, fixed)` | RED |
| RH8201….VerifyBothPartsOfPartialEventAreDetectedAndFixed | RH8201-001, RH8201-005 (defining part is `EventFieldDeclarationSyntax`) | `Verify(source, fixed)` | RED |
| RH8201….VerifyBothPartsOfOverridingPartialMethodAreDetectedAndFixed | RH8201-001, RH8201-005 | `Verify(source, fixed)` | GREEN |
| RH8201….VerifyPartsOfPartialMethodInTwoDocumentsAreDetectedAndFixed | RH8201-001, RH8201-005 (FC-008) | `Verify` with second document in Test and Fixed state | RED |
| RH8201….VerifyDocumentedPartsOfPartialMethodInheritingNothingAreNotReported | RH8201-001 | `Verify(source)` | GREEN |
| RH8203….VerifySpecificationExamplePartialMethodIsJudgedPartByPart (`M`, `N`, `O`) | RH8203-001 | `Verify(source)` | RED |
| RH8203….VerifyPartialPropertyIsJudgedPartByPart | RH8203-001 | `Verify(source)` | RED |
| RH8203….VerifyPartialIndexerIsJudgedPartByPart | RH8203-001 | `Verify(source)` | RED |
| RH8203….VerifyPartialEventIsJudgedPartByPart | RH8203-001 | `Verify(source)` | RED |
| RH8203….VerifyPartialMethodWithInheritdocOnlyOnDefiningPartIsReportedThere | RH8203-001 | `Verify(source)` | GREEN |
| RH8203….VerifyOverridingPartialMethodWithInheritdocOnBothPartsIsNotReported | RH8203-001 | `Verify(source)` | GREEN |
| RH8203….VerifyPartialMethodImplementingInterfaceMemberAcrossDocumentsIsNotReported | RH8203-001 | `Verify` with a second document | RED |
| RH8203….VerifyUndocumentedPartialMethodImplementingInterfaceMemberIsNotReported (the issue's own input) | none (characterization) | `Verify(source)` | GREEN |

### Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input (no documentation on either part) | LF | RH8203 analyzer | not reproduced | nothing reported, as expected; with documentation added the premise reproduces (RED rows) |

## Not covered, and why

- RH8303-006 parity with the formatter (`VerifyCodeFixMatchesFormatter`): the formatter also expands the summary (RH8305's concern), so whole-document output differs from the fix by design; the column the comment takes is asserted directly.
- A `/** */` comment behind code (RH8301–RH8303): not covered by the specs.
- A `///` behind code that no declaration follows: not covered; RH8301-002's existing `VerifyCommentKeptBehindCodeDoesNotProduceDiagnostics` already pins it.
- A region directive in an `#if`-excluded branch below a documentation comment: unreachable (the comment would be disabled text too; spec author 01, skeptic 02).
- RH8307 top-level inline formatting element (`<b>` outside every element): not covered by RH8307-001 (decision 15); no test changes its output.
- RH8203 `<include>` in one part of a partial member: rare (skeptic 04); RH8203-001 covers per-part content, but no spec example; left out.
- RH8402 `/**/` empty block comment after an old header: not covered by RH8402-006 (run holds only copyright comments).
- FC-018, FC-036 (renames), FC-022/FC-023, FC-025, FC-029: no rename, literal, or expression-body rewrite in this run. FC-038–FC-043: no shape is declared unsupported (Q3 B). FC-004/FC-019/FC-020/FC-021: covered where an edit touches a gap — RH8303-006 move (white space incl. tab, attribute list, closing brace), RH8302 with `#region`/`#endregion`/`#if`; a comment between the code and a behind-code `///` is not a documentation comment shape the specs cover.
- FC-028: no node-level formatter change.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH8002|FullyQualifiedName~RH8032|FullyQualifiedName~RH8106|FullyQualifiedName~RH8201|FullyQualifiedName~RH8203|FullyQualifiedName~RH830|FullyQualifiedName~RH840"
scripts/test.sh --project formatter --filter "FullyQualifiedName~BlankLineRequirementTests"
scripts/test.sh --project architecture
```

## Whole run

- Files: the 14 analyzer test files and `src/Reihitsu.Formatter.Test/Regression/BlankLines/BlankLineRequirementTests.cs`; formatted with `scripts/format.sh`. No production code touched.
- Follow-up candidate: none new.
