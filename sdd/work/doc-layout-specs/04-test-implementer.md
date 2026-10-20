Role: test implementer
Commit: 0cff1b9dc6838c1d56d231444110e32c53b4ed3b
Verdict: tests written

Run: thoenissen/Reihitsu#985, slug `doc-layout-specs`, lane "Specs and tests for existing behavior". Inputs: `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, `03-spec-author-repair.md`, and the specs RH8301–RH8309, RH8401, RH8402, RH8501. My prompt carried no intended fix or hoped-for verdict. No production code was changed. No spec example contradicted the code: every test derived from a spec example passed on the unchanged code at the first run.

## Tests

The existing tests of the twelve rules are tagged with every requirement they verify. 64 new tests were added, one per spec example plus focused tests for gaps the handovers named. Every test is in `src/Reihitsu.Analyzer.Test`. All of them pass on the unchanged code ("Expected now: pass").

### New tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH8301 `VerifyMissingAndTabSeparatorsAreReportedOnTheSlashes` (spec example) | RH8301-001, -003 | `Verify(source, expected)` | pass |
| RH8301 `VerifyTabOnlySuffixIsDetectedAndRemoved` | RH8301-001, -004 | `Verify(source, fixed)` | pass |
| RH8301 `VerifySingleSpaceOnlySuffixDoesNotProduceDiagnostics` | RH8301-001 | `Verify` | pass |
| RH8301 `VerifyNoBreakSpaceSeparatorIsDetectedAndReplaced` | RH8301-001, -004 | `Verify(source, fixed)` | pass |
| RH8301 `VerifyLinesOutsideDocumentationCommentsDoNotProduceDiagnostics` (spec example; FC-022 verbatim literal) | RH8301-002 | `Verify` | pass |
| RH8301 `VerifyFixWritesTheSeparatorAndKeepsTheContent` (spec example) | RH8301-001, -003, -004 | `Verify(source, fixed)` | pass |
| RH8302 `VerifyBlankLinesAfterDocumentationCommentsAreReportedWhateverFollows` (spec example, `#if !NEVER`) | RH8302-001, -003 | `Verify` | pass |
| RH8302 `VerifyOtherCommentsAndPositionsDoNotProduceDiagnostics` (spec example) | RH8302-002 | `Verify` | pass |
| RH8302 `VerifyRunOfBlankLinesIsReportedAsOneDiagnostic` (spec example) | RH8302-003 | `Verify` | pass |
| RH8302 `VerifyWhitespaceOnBlankLineIsPartOfTheReportedRun` | RH8302-003, -004 | `Verify(source, fixed)` | pass |
| RH8302 `VerifyFixRemovesTheBlankLines` (spec example) | RH8302-004 | `Verify(source, fixed)` | pass |
| RH8302 `VerifyFixRemovesTheBlankLinesWithCarriageReturnLineFeed` | RH8302-004; FC-017/FC-010 | `ApplyCodeFixAsync` | pass |
| RH8303 `VerifyDocumentationCommentsBelowNonBlankLinesAreReported` (spec example) | RH8303-001, -004 | `Verify` | pass |
| RH8303 `VerifyFirstDocumentationCommentOfBlockOrFileDoesNotProduceDiagnostics` (spec example) | RH8303-002 | `Verify` | pass |
| RH8303 `VerifyDocumentationCommentAfterCommentOrDirectiveDoesNotProduceDiagnostics` (spec example; FC-020 `#region`) | RH8303-003 | `Verify` | pass |
| RH8303 `VerifyFixInsertsOneEmptyLineAboveTheComment` (spec example) | RH8303-001, -004, -005 | `Verify(source, fixed)` | pass |
| RH8303 `VerifyFixInsertsEmptyLineAboveDelimitedCommentWithCarriageReturnLineFeed` | RH8303-005; FC-017 | `ApplyCodeFixAsync` | pass |
| RH8304 `VerifyElementsOnTheLineOfThePreviousElementAreReported` (spec example, incl. `&#32;`) | RH8304-001, -003 | `Verify` | pass |
| RH8304 `VerifySeparatedNestedAndDelimitedElementsDoNotProduceDiagnostics` (spec example) | RH8304-002 | `Verify` | pass |
| RH8304 `VerifyElementsSeparatedByNonWhitespaceCharacterReferenceDoNotProduceDiagnostics` (`&amp;`) | RH8304-002 | `Verify` | pass |
| RH8304 `VerifyFixMovesEachElementOntoANewDocumentationLine` (spec example; two diagnostics on one line, FC-008) | RH8304-004 | `Verify(source, fixed)` | pass |
| RH8304 `VerifyFixInsertsTheLineBreakOfTheLineWithCarriageReturnLineFeed` | RH8304-004; FC-017 | `ApplyCodeFixAsync` | pass |
| RH8305 `VerifySummariesOnFewerThanThreeLinesAreReported` (spec example, later comment) | RH8305-001, -003 | `Verify` | pass |
| RH8305 `VerifyOtherSummariesDoNotProduceDiagnostics` (spec example) | RH8305-002 | `Verify` | pass |
| RH8305 `VerifyFixLaysTheSummaryOutOnThreeLines` (spec example) | RH8305-004 | `Verify(source, fixed)` | pass |
| RH8305 `VerifyFixLaysTheSummaryOutWithCarriageReturnLineFeed` | RH8305-004; FC-017 | `ApplyCodeFixAsync` | pass |
| RH8305 `VerifyOneLineSummaryDoesNotProduceDiagnosticsWhenDocumentationModeIsNone` | RH8305-001, SHARED-DOC-008 | `Verify` + `ApplyDocumentationModeNoneToTestProject` | pass |
| RH8306 `VerifySummaryOfEveryPropertyIsChecked` (spec example; FC-024 explicit interface implementation) | RH8306-001, -004 | `Verify` | pass |
| RH8306 `VerifySummariesEndingLikeSentencesAreReported` (spec example) | RH8306-002 | `Verify` | pass |
| RH8306 `VerifySummariesStartingWithNarratingVerbsAreReported` (spec example: lower case, `Getset`) | RH8306-003 | `Verify` | pass |
| RH8306 `VerifyEveryNarratingVerbIsReported` (DataRow, all nine verbs) | RH8306-003 | `Verify` | pass |
| RH8307 `VerifyContentStartingBesideTheStartTagIsReported` (spec example, nested `<para>`) | RH8307-001, -003 | `Verify` | pass |
| RH8307 `VerifyCompleteLayoutsDoNotProduceDiagnostics` (spec example) | RH8307-002 | `Verify` | pass |
| RH8307 `VerifyFixLaysTheElementOutAsTheFormatterDoes` (spec example) | RH8307-004 | `Verify(source, fixed)` | pass |
| RH8308 `VerifyTextAfterTopLevelElementsIsReported` (spec example) | RH8308-001, -003 | `Verify` | pass |
| RH8308 `VerifyTextOnLaterLineInsideElementOrInDelimitedCommentDoesNotProduceDiagnostics` (spec example) | RH8308-002 | `Verify` | pass |
| RH8308 `VerifyFixRemovesTheText` (spec example, `a <c>x</c> b`; FC-008) | RH8308-001, -003, -004 | `Verify(source, fixed)` | pass |
| RH8308 `VerifyFixRemovesTabInFrontOfTheText` | RH8308-004 | `Verify(source, fixed)` | pass |
| RH8309 `VerifyElementsOutOfThePrescribedOrderAreReported` (spec example) | RH8309-001, -003 | `Verify` | pass |
| RH8309 `VerifySameKindCustomAndNestedElementsDoNotProduceDiagnostics` (spec example) | RH8309-002 | `Verify` | pass |
| RH8309 `VerifyOnlyTheFirstElementOutOfOrderIsReported` (spec example) | RH8309-003 | `Verify` | pass |
| RH8309 `VerifyFixSortsTheElementsAroundACustomElement` (spec example) | RH8309-004 | `Verify(source, fixed)` | pass |
| RH8401 `VerifyCommentsWithoutXmlAreReportedWhereverTheyStand` (spec example) | RH8401-001, -003 | `Verify` | pass |
| RH8401 `VerifyCommentsWithXmlAndOtherCommentFormsDoNotProduceDiagnostics` (spec example) | RH8401-002 | `Verify` | pass |
| RH8401 `VerifyFixTurnsTheWholeCommentIntoAnOrdinaryComment` (spec example) | RH8401-003, -004 | `Verify(source, fixed)` | pass |
| RH8401 `VerifyFixAppliedToTheSecondLineConvertsEveryLine` | RH8401-004 ("whichever line's diagnostic"); FC-026 | `GetCodeFixActionsAsync` with the second line's `///` as location | pass |
| RH8402 `NoCopyrightSection`, `WhitespaceCopyrightText`, `ConfigurationWithProblem` (`{owner}` without a setting) | RH8402-001 | `VerifySpecificationFile` (new private helper) | pass |
| RH8402 `SpecificationHeaderWithPlaceholders` (spec example) | RH8402-002 | `VerifySpecificationFile` | pass |
| RH8402 `SpecificationHeaderWithCarriageReturnLineFeed` | RH8402-002 | `VerifySpecificationFile` | pass |
| RH8402 `FileNotStartingWithHeader` (DataRow: the three spec examples) | RH8402-003, -004 | `VerifySpecificationFile` | pass |
| RH8402 `HeaderWithDifferentCasing` | RH8402-003 | `VerifySpecificationFile` | pass |
| RH8402 `EmptyFile` | RH8402-003, -004 | `VerifySpecificationFile` | pass |
| RH8402 `FixInsertsHeaderInFrontOfCode`, `FixReplacesLeadingBlankLine`, `FixKeepsLeadingCommentBelowHeader` (the three spec examples) | RH8402-005 | `VerifySpecificationFileFix` (new private helper) | pass |
| RH8402 `FixInsertsHeaderInFrontOfDirective`, `FixInsertsHeaderInFrontOfDocumentationComment` | RH8402-005 | `VerifySpecificationFileFix` | pass |
| RH8402 `FixWritesHeaderWithCarriageReturnLineFeed` | RH8402-002, -005; FC-017 | `VerifySpecificationFileFix` | pass |
| RH8501 `VerifySuppressionsWithoutJustificationAreReportedHoweverWritten` (spec example: assembly target, qualified, alias, `Other.SuppressMessage`) | RH8501-001, -003 | `Verify` | pass |
| RH8501 `VerifyModuleSuppressionWithoutJustificationIsReported` | RH8501-001, -003 | `Verify` | pass |
| RH8501 `VerifyJustificationsWithoutTextAreReported` (spec example: `""`, white space, `null`, empty constant, constant, concatenation) | RH8501-002 | `Verify` | pass |
| RH8501 `VerifyNameofJustificationDoesNotProduceDiagnostics` | RH8501-002 | `Verify` | pass |

The RH8402 helpers check the file `/0/C.cs` with the spec's configuration (`copyrightText` with `{fileName}` and `{company}`, `company` = `Example`), as the spec's examples state.

### Existing tests tagged

| Class | Tags |
|---|---|
| RH8301 | -001 on `VerifyNoDiagnosticsWhenCodeIsClean`, `VerifyNestedListIndentationDoesNotProduceDiagnostics`, `VerifyTabContentIndentationDoesNotProduceDiagnostics`; -001/-003/-004 on `VerifyIssueIsDetectedAndFixed`, `VerifyInvalidSeparatorFixPreservesContentIndentation`, `VerifyWhitespaceOnlySuffixIsDetectedAndFixed`; -002 on the raw-string, multi-line-comment, and disabled-text tests; -001 + SHARED-DOC-008 on `VerifyNoDiagnosticsWhenDocumentationModeIsNone` |
| RH8302 | -002 on the clean, raw-string, four-slash, multi-line-comment, and disabled-text tests; -001/-003/-004 on the two detect-and-fix tests; -004 on `VerifyMultipleBlankLinesAreRemovedByOneCodeFix`; -001 + SHARED-DOC-008 on the `None` test |
| RH8303 | -001 on `VerifyNoDiagnosticsWhenCodeIsClean`; -001/-004/-005 on every detect-and-fix test except the switch label; -003 on the after-comment/directive/block-comment tests; -001/-003/-004/-005 on `VerifyMixedDocumentationFormsProduceOneDiagnostic`; -002 on every opening-brace test and `VerifyDocumentationAtStartOfScopeDoesNotProduceDiagnostics`; -005 on the CRLF test; -001 + SHARED-DOC-008 on the `None` test |
| RH8304 | -001, -002, -001/-003/-004 as fits each test; -001 + SHARED-DOC-008 on the `None` test |
| RH8305 | -002 on the two clean tests; -001/-003/-004 on the three detect-and-fix tests; -001/-003 only on `VerifyEmptySingleLineSummaryIsDetectedAndFixed` |
| RH8306 | -002/-003 by condition, -004 on every reporting test, -001 on the inheritdoc, empty-summary, and nested-summary tests; -001 + SHARED-DOC-008 on the `None` test |
| RH8307 | -002 on the two clean tests; -001/-003/-004 on the four detect-and-fix tests; -001 + SHARED-DOC-008 on the `None` test |
| RH8308 | -002 on the next-line and nested-inline tests; -001/-003/-004 on the two detect-and-fix tests; -001 + SHARED-DOC-008 on the `None` test |
| RH8309 | -002 on the six in-order tests; -001/-003/-004 on the twelve reorder tests; -001 + SHARED-DOC-008 on the `None` test |
| RH8401 | -001/-003/-004 on the two `Verify` fix tests; -004 on the five line-terminator tests and `VerifyCodeFixPreservesContentAfterTheExterior` |
| RH8402 | -002 on the seven `ValidConfiguredHeader*` tests; -003/-004/-005 on `MissingConfiguredHeader` and `NonCopyrightLeadingCommentIsPreserved`; -001 on `NoConfiguration` |
| RH8501 | -001/-002/-003 on `VerifyDiagnostics` |

Code-fix and Fix All requirements: every class with a code fix already derives from `BatchCodeFixTestsBase`. Its `FixAllCorrectsEveryDiagnostic` carries SHARED-FIX-001 and SHARED-FIX-002, and `CodeFixProvidesFixAllProvider` confirms the base matches the provider. None of the twelve specs has a Fix All requirement that differs from the shared one, so no class overrides the test. RH8306 and RH8501 have no code fix and stay on `AnalyzerTestsBase<TAnalyzer>`.

### Deliberately left untagged

These tests still run and pin today's behavior as characterization tests.

- Excluded at the lead's instruction or by a coverage sentence:
  - RH8401 `VerifyNoDiagnosticsWhenDocumentationModeIsNone` (RH8401 Q2);
  - RH8303 `VerifyDocumentationAfterSwitchLabelProducesDiagnostic` (RH8303 Q1);
  - RH8402 `MismatchedConfiguredHeader` and `MismatchedBlockCommentHeader` (RH8402 Q1). These two are not tagged even for -003, as instructed.
- Shapes or subjects that no requirement specifies:
  - RH8302 `VerifyDanglingMultiLineDocumentation*` (a comment that documents nothing);
  - RH8303 `VerifyFourSlashCommentDoesNotProduceDiagnostics` (ordinary comment, a non-goal), `VerifyMultiLineCommentsDoNotProduceDiagnostics`, and `VerifyDisabledTextDoesNotProduceDiagnostics`. RH8303 has no requirement for literals, block comments, or excluded code;
  - RH8305 `VerifyNoDiagnosticsWhenDocumentationModeIsNone`: its source has no documentation comment, so it verifies nothing about SHARED-DOC-008. The new `None` test covers that;
  - RH8306 `VerifyNoDiagnosticForMethodSummaryEndingWithPeriod` and `VerifyNoDiagnosticForParamEndingWithPeriod` (non-goal);
  - RH8308 `VerifyNoDiagnosticsWhenAnotherElementFollowsClosingTag` (RH8304's subject, a non-goal).
- RH8305 `VerifyEmptySingleLineSummaryIsDetectedAndFixed` has no -004 tag: RH8305-004's coverage excludes content that is only white space (RH8305 Q7).

No test was written for an excluded shape: F3, F1, and the coverage exclusions of 03 (behind code, in a method body, `/** */` for RH8307, unclosed elements, inline formatting elements, partial properties, indexers, `<Pending>`, `UnconditionalSuppressMessage`, a leading copyright comment).

## Scenarios (bug fix)

_N/A_ (existing-behavior lane).

## Not covered, and why

- **Weak-test check.** Every new test passed on the unchanged code, as the lane requires. To show that the hand-built tests are not vacuous, I changed the expected text of two of them for one run, and both failed:
  - `FixWritesHeaderWithCarriageReturnLineFeed`, with one LF line break put in the expected text: the verifier compares line endings exactly.
  - `VerifyFixAppliedToTheSecondLineConvertsEveryLine`, with the first line left as `///`.

  I then restored both files.
- **FC-022:** RH8301–RH8303 already have raw-string tests, and RH8301-002 now has a verbatim-literal test. Interpolated raw and UTF-8 literals were not added, because no spec requirement names them. The skeptic established that one shared helper skips every literal kind (`FormattingTextAnalysisUtilities.GetNonFormattableLineIndices`). A literal kind no test covers could still break that helper unnoticed, so I list it as a follow-up candidate rather than claim it is covered.
- **FC-020 and FC-021:** these are covered by the `#region`/`#endregion` and `#if !NEVER` tests, and by the existing `#if DEBUG`/`#endif` and `#if false` tests. No new test puts a documentation comment in both the `#if` and `#else` branches, because no requirement specifies it.
- **FC-007:** RH8308 Q1, RH8309 Q4, and RH8401 Q3 are excluded or open. I added no characterization test for them, because those inputs are filed as questions and a test would only pin a shape the maintainer has not decided yet.
- **FC-009 and FC-030:** RH8305, RH8307, and RH8309 on a file with a syntax error, on `/** */`, and on an unformatted declaration. Coverage sentences exclude these, and they are filed (RH8305 Q3/Q4, RH8307 Q1/Q2, RH8309 Q4/Q5). Not tested.
- **FC-024:** a partial property (RH8306 Q5) is excluded and not tested. The explicit interface implementation is covered.
- **FC-004:** RH8309 with text or XML comments between elements is excluded from RH8309-004 (Q6). RH8402 with a leading comment block that holds "copyright" is excluded from RH8402-005 (Q1/Q2). Not tested.
- **FC-006:** the F4 and F5 inputs are excluded and were not tested.
- **Not applicable:** naming, Renamer, token gap, and line join. None of these rules renames a symbol or joins lines.
- **Formatter parity:** `FormatterTestsBase` parity tests already exist for RH8301, RH8302, RH8303, RH8305, and RH8307. This lane changes no formatter behavior, so none was added.

## Commands

```shell
export PATH="/root/.dotnet:$PATH"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH830|FullyQualifiedName~RH8401|FullyQualifiedName~RH8402|FullyQualifiedName~RH8501"   # 255 passed
scripts/test.sh --project architecture                                                                                                                  # 72 passed, RequirementCoverageTests included
scripts/test.sh --project analyzer --filter "FullyQualifiedName~SelfHosting"                                                                                   # 17 passed
scripts/test.sh --project analyzer --filter "TestCategory=RH8402-005"                                                                                   # per-requirement run
```
