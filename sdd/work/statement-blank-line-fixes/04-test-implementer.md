Role: test implementer
Commit: cfdf0b1
Verdict: #946: reproduced; #972: reproduced; #1096: reproduced; #1076: reproduced; #1626: tests written (none of its own); #1625: reproduced

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples and requirements on `8abfa05`, never from patched code (FC-035). No production file was touched. Every test file was formatted with `scripts/format.sh` (a second run changes nothing).

All test files are under `src/Reihitsu.Analyzer.Test/`; analyzer tests in `Formatting/`, formatter-parity tests in `Formatter/Formatting/`. "Verify, 1 it." means `Verify(source, fixedSource, …)` with `NumberOfIncrementalIterations = 1` and `NumberOfFixAllIterations = 1`, so one application and one Fix All iteration are asserted (FC-007, FC-017). "CRLF" tests run the same text through `NormalizeToCarriageReturnLineFeed` on both sides. "Analyzer" means `Verify(source)` (diagnostics only, default documentation mode Diagnose; "doc None" adds `ApplyDocumentationModeNoneToTestProject`).

Result on `cfdf0b1`: 94 new tests fail, each on the issue's own difference (checked one by one: wrong text with the reported shape, "Expected '1' iterations but found '2'", or one unexpected diagnostic; no compile error). Every other test in the 23 changed classes passes, including all new boundary tests. `scripts/test.sh --project architecture` passes (117), including `EveryRequirementOfAnEnforcedSpecIsCovered` — RH5011-008, RH5025-007, RH5026-005, RH5027-005, SHARED-AFTERBRACE-002 now have tagged tests.

## thoenissen/Reihitsu#946

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5005…AnalyzerTests.VerifyFixInsertsBlankLineAboveAwaitOfAwaitUsingStatement` | RH5005-002, SHARED-STMT-004 | Verify, 1 it. (spec example) | fails |
| `RH5005…AnalyzerTests.VerifyFixInsertsBlankLineAboveAwaitOfAwaitUsingStatementWithCarriageReturnLineFeed` | RH5005-002, SHARED-STMT-004 | Verify, 1 it., CRLF | fails |
| `RH5005…AnalyzerTests.VerifyFixMovesAwaitUsingStatementSharingLineWithPreviousStatement` | RH5005-002, SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5005…AnalyzerTests.VerifyFixInsertsBlankLineAboveCommentLeadingAwaitUsingStatement` | RH5005-002, SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5005…AnalyzerTests.VerifyFixAllInsertsBlankLinesAboveAwaitOfAdjacentAwaitUsingStatements` | RH5005-002, SHARED-STMT-004 (FC-008) | Verify, 2 incremental, Fix All 1 it. | fails |
| `RH5006…AnalyzerTests.VerifyFixInsertsBlankLineAboveAwaitOfAwaitForeachStatement` | RH5006-002, SHARED-STMT-004 | Verify, 1 it. (spec example) | fails |
| `RH5006…AnalyzerTests.VerifyFixInsertsBlankLineAboveAwaitOfAwaitForeachStatementWithCarriageReturnLineFeed` | RH5006-002, SHARED-STMT-004 | Verify, 1 it., CRLF | fails |
| `RH5006…AnalyzerTests.VerifyFixMovesAwaitForeachStatementSharingLineWithPreviousStatement` | RH5006-002, SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5006…AnalyzerTests.VerifyFixInsertsBlankLineAboveCommentLeadingAwaitForeachStatement` | RH5006-002, SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5006…AnalyzerTests.VerifyFixAllInsertsBlankLinesAboveAwaitOfAdjacentAwaitForeachStatements` | RH5006-002, SHARED-STMT-004 (FC-008) | Verify, 2 incremental, Fix All 1 it. | fails |
| `RH5005…FormatterTests.VerifyFormatterInsertsBlankLineAboveAwaitOfAwaitUsingStatement` | RH5005-002 | `VerifyFormatter(source, fixed, diagnostic at 6,15–6,20)` | passes |
| `RH5006…FormatterTests.VerifyFormatterInsertsBlankLineAboveAwaitOfAwaitForeachStatement` | RH5006-002 | `VerifyFormatter(source, fixed, diagnostic at 6,15–6,22)` | passes |

Every analyzer test marks the diagnostic on `using`/`foreach` (RH5005-002/RH5006-002), so a fix that moves the location fails them.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `var a = 1;⏎await using (resource) {…}` (issue) | LF, CRLF | code fix | reproduced | observed `await⏎⏎⏎using (resource)` (line breaks inserted between `await` and `using`); expected one blank line above `await` |
| same-line `var a = 1; await using (…)` | LF | code fix | reproduced | same mechanism |
| `// note` line above `await using` | LF | code fix | reproduced | same mechanism |
| two `await using` statements, Fix All | LF | code fix, Fix All | reproduced | same mechanism |
| the four rows with `await foreach` | LF, CRLF | code fix | reproduced | `await⏎⏎⏎foreach (…)` |
| `await using` / `await foreach` | LF, CRLF | formatter (`VerifyFormatter`) | passes | formatter already puts one blank line above `await` and clears the diagnostic |

## thoenissen/Reihitsu#972

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5002…AnalyzerTests.VerifyFixInsertsOneBlankLineForSpecExampleOfCommentsEndingOnStatementLine` | SHARED-STMT-004 | Verify, 3 incremental, Fix All 1 it. (spec example, three comment shapes) | fails |
| `RH5002…AnalyzerTests.VerifyFixInsertsOneBlankLineAboveBlockCommentStartingStatementLine` | SHARED-STMT-004 | Verify, 1 it. (issue) | fails |
| `RH5002…AnalyzerTests.VerifyFixInsertsOneBlankLineAboveBlockCommentStartingStatementLineWithCarriageReturnLineFeed` | SHARED-STMT-004 | Verify, 1 it., CRLF | fails |
| `RH5002…AnalyzerTests.VerifyFixInsertsOneBlankLineAboveMultiLineCommentEndingOnStatementLine` | SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5002…AnalyzerTests.VerifyFixInsertsOneBlankLineAboveCommentLineAboveBlockCommentStartingStatementLine` | SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5002…AnalyzerTests.VerifyFixInsertsOneBlankLineAboveTwoBlockCommentsStartingStatementLine` | SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5002…AnalyzerTests.VerifyFixInsertsOneBlankLineAboveBlockCommentWhenLineCommentEndsPreviousLine` | SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5002…AnalyzerTests.VerifyFixInsertsOneBlankLineAboveDelimitedDocumentationCommentStartingStatementLine` | SHARED-STMT-004 (FC-040, `/** */`) | Verify, 1 it. | fails |
| `RH5008…AnalyzerTests.VerifyFixInsertsOneBlankLineAboveBlockCommentStartingReturnLine` | SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5008…AnalyzerTests.VerifyFixInsertsOneBlankLineAboveBlockCommentStartingReturnLineWithCarriageReturnLineFeed` | SHARED-STMT-004 | Verify, 1 it., CRLF | fails |
| `RH5029…AnalyzerTests.VerifyFixInsertsOneBlankLineAboveBlockCommentStartingDeclarationLine` | SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5021…AnalyzerTests.VerifyFixInsertsOneBlankLineAboveBlockCommentStartingStatementLineAfterDeclaration` | SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5002…AnalyzerTests.VerifyFixMovesStatementBehindCommentStartingBehindPreviousStatement` | SHARED-STMT-004 (third bullet; boundary, FC-003) | Verify, 1 it. | passes |
| `RH5002…AnalyzerTests.VerifyFixMovesStatementBehindCommentStartingBehindPreviousStatementWithCarriageReturnLineFeed` | SHARED-STMT-004 | Verify, 1 it., CRLF | passes |
| `RH5002…AnalyzerTests.VerifyFixInsertsOneBlankLineAboveBlockCommentLineEndedByLineComment` | SHARED-STMT-004 (boundary: comment line ends before the statement) | Verify, 1 it. | passes |
| `RH5002…AnalyzerTests.VerifyFixMovesStatementBehindSingleLineCommentSharingPreviousStatementLine` | SHARED-STMT-004 (boundary: `a++; /* c */ if`) | Verify, 1 it. | passes |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `a++;⏎/* block */ if (a > 7) { }` (issue), RH5002 | LF, CRLF | code fix | reproduced | two blank lines above `/* block */`; expected one |
| same on RH5008 (`return`), RH5029 (`var`), RH5021 (expression statement after a declaration) | LF (RH5008 also CRLF) | code fix | reproduced | two blank lines |
| rows of 02-skeptic's matrix: comment line above + block comment, `/* x */ /* y */`, `// x` ending the previous line, `/** d */`, multi-line comment starting its own line | LF | code fix | reproduced | two blank lines |
| `a++; /* a⏎b */ if`, `a++; /* c */ if`, `⏎/* b */ // c⏎if` | LF (first also CRLF) | code fix | passes | one blank line, as specified |

## thoenissen/Reihitsu#1096

### Tests

Q1, the moved statement's column after a multi-line `case … when` label:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5029…AnalyzerTests.VerifyMovedStatementTakesSectionColumnWhenLabelSpansMultipleLinesAndPrecedingStatementSharesItsContinuationLine` | SHARED-STMT-004 | Verify (diagnostic list) | fails — the existing characterization `VerifyDiagnosticOverIndentsWhenLabelSpansMultipleLinesAndPrecedingStatementSharesItsContinuationLine`, run green on `8abfa05`, renamed, tagged, assertion rewritten from column 24 to the column of `break;` (20) |
| `RH5029…AnalyzerTests.VerifyMovedStatementTakesSectionColumnForSpecExampleInsideObjectInitializer` | SHARED-STMT-004 | Verify, 1 it. (spec example, `Holder`) | fails |
| `RH5029…AnalyzerTests.VerifyMovedStatementTakesSectionColumnWhenLabelSpansMultipleLinesWithCarriageReturnLineFeed` | SHARED-STMT-004 | Verify, 1 it., CRLF | fails |
| `RH5029…AnalyzerTests.VerifyMovedStatementTakesSectionColumnWhenWhenClauseStartsContinuationLine` | SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5029…AnalyzerTests.VerifyMovedStatementTakesSectionColumnWhenSecondOfTwoLabelsSpansMultipleLines` | SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5029…AnalyzerTests.VerifyMovedStatementTakesSectionColumnWhenGuardSpansThreeLines` | SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5029…AnalyzerTests.VerifyMovedStatementTakesSectionColumnWhenLabelSpansMultipleLinesInsideAnonymousObject` | SHARED-STMT-004 (FC-027) | Verify, 1 it. | fails |
| `RH5002…AnalyzerTests.VerifyMovedIfStatementTakesSectionColumnWhenLabelSpansMultipleLinesInsideObjectInitializer` | SHARED-STMT-004 | Verify, 1 it. | fails |
| `RH5029…AnalyzerTests.VerifyMovedStatementKeepsSectionColumnWhenPrecedingStatementStartsItsOwnLineBelowMultiLineLabel` | SHARED-STMT-004 (boundary) | Verify, 1 it. | passes |
| `RH5029…AnalyzerTests.VerifyMovedStatementTakesNestingColumnWhenLabelSpansMultipleLinesInMethodBody` | SHARED-STMT-004 (boundary: no initializer) | Verify, 1 it. | passes |

The single-line-label boundary inside an initializer is already covered by `VerifyFixAllCompensatesBothDiagnosticsOnSharedLabelLineWithoutDoubleCompensation` and `VerifyDiagnosticCompensatesTabIndentedLabelLineByAppendingFourSpaces` (unchanged, passing). The summary of `VerifyDiagnosticDoesNotCompensateWhenSwitchLabelItselfIsNotFirstOnItsLine` called its shape "a documented limitation"; the rule-page paragraph is gone (176b419) and SHARED-STMT-004 no longer mentions the shape, so the summary now says no requirement decides it and the test characterizes today's output (FC-031). Its assertion is unchanged.

Q2, the whitespace-only line below a trivia that carries its own line break:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5002…AnalyzerTests.VerifyNoDiagnosticForSpecExampleEmptyLineBelowDocumentationComment` | SHARED-BLANK-001 | Analyzer (spec example) | fails |
| `RH5002…AnalyzerTests.VerifyNoDiagnosticWhenLineBelowDocumentationCommentHoldsSpaces` | SHARED-BLANK-001 | Analyzer, `ExpandWhitespaceMarkers` | fails |
| `RH5002…AnalyzerTests.VerifyNoDiagnosticWhenLineBelowDocumentationCommentHoldsSpacesWithCarriageReturnLineFeed` | SHARED-BLANK-001 | Analyzer, CRLF | fails |
| `RH5002…AnalyzerTests.VerifyNoDiagnosticWhenLineBelowDocumentationCommentHoldsSpacesWithDocumentationModeNone` | SHARED-BLANK-001 (FC-040) | Analyzer, doc None | passes |
| `RH5002…AnalyzerTests.VerifyNoDiagnosticWhenBlankLineBelowDocumentationCommentPrecedesCommentLine` | SHARED-BLANK-001 | Analyzer | fails |
| `RH5002…AnalyzerTests.VerifyNoDiagnosticForSpecExampleEmptyLineBelowEndIfDirective` | SHARED-BLANK-001 (FC-020) | Analyzer (spec example) | fails |
| `RH5002…AnalyzerTests.VerifyNoDiagnosticWhenLineBelowEndIfDirectiveHoldsSpaces` | SHARED-BLANK-001 | Analyzer | fails |
| `RH5002…AnalyzerTests.VerifyNoDiagnosticWhenLineBelowEndIfDirectiveHoldsSpacesWithCarriageReturnLineFeed` | SHARED-BLANK-001 | Analyzer, CRLF | fails |
| `RH5002…AnalyzerTests.VerifyNoDiagnosticWhenEmptyLineFollowsPragmaDirective` | SHARED-BLANK-001 (FC-020) | Analyzer | fails |
| `RH5002…AnalyzerTests.VerifyDiagnosticWhenDocumentationCommentDirectlyPrecedesIfStatement` | SHARED-BLANK-001, SHARED-STMT-004 (boundary: no blank line) | Verify, 1 it. | passes |
| `RH5029…AnalyzerTests.VerifyNoDiagnosticWhenEmptyLineFollowsEndIfDirective` | SHARED-BLANK-001 | Analyzer | fails |
| `RH5011…AnalyzerTests.VerifyNoDiagnosticForSpecExampleEmptyLineBelowDocumentationComment` | SHARED-BLANK-001 | Analyzer (spec example) | fails |
| `RH5011…AnalyzerTests.VerifyNoDiagnosticWhenLineBelowDocumentationCommentHoldsSpaces` | SHARED-BLANK-001 | Analyzer (issue: `break;⏎/// doc⏎    ⏎a++;`) | fails |
| `RH5011…AnalyzerTests.VerifyNoDiagnosticWhenLineBelowDocumentationCommentHoldsSpacesWithCarriageReturnLineFeed` | SHARED-BLANK-001 | Analyzer, CRLF | fails |
| `RH5011…AnalyzerTests.VerifyNoDiagnosticWhenLineBelowDocumentationCommentHoldsSpacesWithDocumentationModeNone` | SHARED-BLANK-001 (FC-040) | Analyzer, doc None | passes |
| `RH5011…AnalyzerTests.VerifyNoDiagnosticWhenEmptyLineFollowsPragmaDirectiveBelowComment` | SHARED-BLANK-001 | Analyzer (`// c⏎#pragma …⏎⏎`) | fails |
| `RH5011…AnalyzerTests.VerifyDiagnosticAndFixWhenDocumentationCommentDirectlyFollowsBreak` | RH5011-001 (the new `/// doc` line of its example, answer 1A) | Verify, 1 it. | passes |
| `RH5002…FormatterTests.VerifyFormatterKeepsBlankLineBelowEndIfDirectiveAndAnalyzerReportsNothing` | SHARED-BLANK-001 (FC-013) | `VerifyFormatter(source)` | fails (analyzer reports the input) |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Q1 issue input inside an object initializer's lambda, RH5029 | LF, CRLF | code fix | reproduced | `var y = 1;` at column 24 (4 deeper than `break;` at 20); expected 20 |
| Q1 `case 1⏎    when …`, two labels, three-line guard, anonymous object, RH5002 `if` | LF | code fix | reproduced | 4 columns deeper than `break;` in every row |
| Q1 issue input in a plain method; preceding statement starting its own line | LF | code fix | passes | already the column of `break;` |
| Q2 `break;⏎/// doc⏎⏎` and `⏎    ⏎`, RH5011 and RH5002 | LF, CRLF | analyzer (Diagnose) | reproduced | reported; expected not reported |
| Q2 same under `DocumentationMode.None` | LF | analyzer | passes | `///` is an ordinary comment there; the line below already counts |
| Q2 `#endif⏎⏎// note⏎if`, `#pragma …⏎⏎// note⏎if` (RH5002, RH5029), `// c⏎#pragma …⏎⏎` (RH5011) | LF, CRLF (`#endif`) | analyzer | reproduced | reported; expected not reported |
| Q2 `#endif` input | LF, CRLF | formatter (`VerifyFormatter(source)`) | reproduced as FC-013 | formatter leaves the input unchanged but RH5002 reports it |

## thoenissen/Reihitsu#1076

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5022…AnalyzerTests.VerifyFixDeletesRunOfBlankLinesBelowOpeningBraceInOneApplication` | RH5022-004, SHARED-FIX-001 | Verify, 1 it. (spec example) | fails |
| `RH5022…AnalyzerTests.VerifyFixDeletesRunOfBlankLinesBelowOpeningBraceInOneApplicationWithCarriageReturnLineFeed` | RH5022-004, SHARED-FIX-001 | Verify, 1 it., CRLF | fails |
| `RH5022…AnalyzerTests.VerifyFixDeletesRunOfWhitespaceOnlyLinesBelowOpeningBraceInOneApplication` | RH5022-004, SHARED-FIX-001 | Verify, 1 it., spaces | fails |
| `RH5022…AnalyzerTests.VerifyFixAllDeletesRunsOfBlankLinesBelowTwoOpeningBracesInOneIteration` | RH5022-004, SHARED-FIX-001 (FC-008) | Verify, 2 incremental, Fix All 1 it. | fails |
| `RH5024…AnalyzerTests.VerifyFixDeletesRunOfBlankLinesAboveClosingBraceInOneApplication` (+ `…WithCarriageReturnLineFeed`) | RH5024-005, SHARED-FIX-001 | Verify, 1 it. (spec example), LF and CRLF | fails |
| `RH5024…AnalyzerTests.VerifyFixAllDeletesRunsOfBlankLinesAboveTwoClosingBracesInOneIteration` | RH5024-005, SHARED-FIX-001 (FC-008) | Verify, 2 incremental, Fix All 1 it. | fails |
| `RH5025…AnalyzerTests.VerifyFixDeletesRunOfBlankLinesAboveOpeningBraceInOneApplication` (+ CRLF) | RH5025-004, SHARED-FIX-001 | Verify, 1 it. (spec example), LF and CRLF | fails |
| `RH5026…AnalyzerTests.VerifyFixDeletesRunOfBlankLinesAboveChainedKeywordInOneApplication` (+ CRLF) | RH5026-004, SHARED-FIX-001 | Verify, 1 it. (spec example), LF and CRLF | fails |
| `RH5027…AnalyzerTests.VerifyFixDeletesRunOfBlankLinesAboveDoStatementFooterInOneApplication` (+ CRLF) | RH5027-004, SHARED-FIX-001 | Verify, 1 it. (spec example), LF and CRLF | fails |
| `RH5023…AnalyzerTests.VerifyEndOfFileAfterOneBlankLineIsNotReported` (+ `…WithCarriageReturnLineFeed`) | RH5023-001 | Analyzer, `"class C\n{\n}\n\n"` (spec example) | fails |
| `RH5023…AnalyzerTests.VerifyOnlySecondBlankLineAtEndOfFileIsReportedAndFixedInOneApplication` (+ CRLF) | RH5023-001 | Verify, 1 it., `"…}\n\n\n"` (spec example) | fails |
| `RH5023…AnalyzerTests.VerifyLastLineHoldingOnlySpacesWithoutLineBreakIsNotReported` | RH5023-001 | Analyzer, `"…}\n\n    "` (spec example) | fails |
| `RH5023…AnalyzerTests.VerifyEndOfFileAfterCommentAndOneBlankLineIsNotReported` | RH5023-001 | Analyzer, `"…\n// c\n\n"` | fails |
| `RH5023…AnalyzerTests.VerifyEndOfFileAfterDirectiveAndOneBlankLineIsNotReported` | RH5023-001 (FC-020) | Analyzer, `"…\n#if X\n#endif\n\n"` | fails |
| `RH5011…AnalyzerTests.VerifyBreakBeforeContinuingKeywordIsNotReportedForSpecExample` | RH5011-008 | Analyzer (spec example; only the last `break` marked) | fails (5 reported, 1 expected) |
| `RH5011…AnalyzerTests.VerifyBreakBeforeElseOnSameLineIsNotReported` | RH5011-008 | Analyzer | fails |
| `RH5011…AnalyzerTests.VerifyBreakBeforeElseOnNextLineIsNotReported` | RH5011-008 | Analyzer | fails |
| `RH5011…AnalyzerTests.VerifyBreakBeforeElseIsNotReportedWithCarriageReturnLineFeed` | RH5011-008 | Analyzer, CRLF | fails |
| `RH5011…AnalyzerTests.VerifyBreakBeforeDoStatementFooterOnSameLineIsNotReported` | RH5011-008 | Analyzer | fails |
| `RH5011…AnalyzerTests.VerifyBreakBeforeDoStatementFooterOnNextLineIsNotReported` | RH5011-008 | Analyzer | fails |
| `RH5011…AnalyzerTests.VerifyBreakBeforeWhileStatementIsReported` | RH5011-008 (boundary: `while` statement) | Verify, 1 it. | passes |
| `RH5024…AnalyzerTests.VerifyRunOfBlankLinesBelowRegionDirectiveAboveClosingBraceIsNotReported` | RH5024-001 | Analyzer (spec example) | fails |
| `RH5024…AnalyzerTests.VerifyBlankLineBelowRegionDirectiveAboveClosingBraceOfTypeIsNotReported` | RH5024-001 | Analyzer (type level) | fails |
| `RH5024…AnalyzerTests.VerifyBlankLineBelowRegionDirectiveAboveClosingBraceIsNotReportedWithCarriageReturnLineFeed` | RH5024-001 | Analyzer, CRLF | fails |
| `RH5025…AnalyzerTests.VerifyBlankLineBelowRegionDirectivesAboveInitializerElementIsNotReported` | RH5025-007 | Analyzer (spec example, `#region` and `#endregion`) | fails |
| `RH5025…AnalyzerTests.VerifyBlankLineBelowRegionDirectiveAboveMethodBodyIsNotReported` | RH5025-007 | Analyzer (`void M()⏎#region R⏎⏎{`) | fails |
| `RH5025…AnalyzerTests.VerifyRunOfBlankLinesBelowRegionDirectiveAboveOpeningBraceIsNotReported` | RH5025-007 | Analyzer (run of two) | fails |
| `RH5025…AnalyzerTests.VerifyBlankLineBelowRegionDirectiveAboveOpeningBraceIsNotReportedWithCarriageReturnLineFeed` | RH5025-007 | Analyzer, CRLF | fails |
| `RH5025…AnalyzerTests.VerifyBlankLineBelowCommentAboveInitializerElementIsReported` | RH5025-007 (boundary: comment above) | Verify, 1 it. | passes |
| `RH5026…AnalyzerTests.VerifyBlankLineBelowRegionDirectiveAboveElseIsNotReported` | RH5026-005 | Analyzer (spec example) | fails |
| `RH5026…AnalyzerTests.VerifyBlankLineBelowEndRegionDirectiveAboveCatchIsNotReported` | RH5026-005 | Analyzer (`#endregion`, `catch`) | fails |
| `RH5026…AnalyzerTests.VerifyRunOfBlankLinesBelowRegionDirectiveAboveElseIsNotReported` | RH5026-005 | Analyzer (run of two) | fails |
| `RH5026…AnalyzerTests.VerifyBlankLineBelowRegionDirectiveAboveElseIsNotReportedWithCarriageReturnLineFeed` | RH5026-005 | Analyzer, CRLF | fails |
| `RH5026…AnalyzerTests.VerifyBlankLineBelowCommentAboveElseIsReported` | RH5026-005 (boundary: comment above) | Verify, 1 it. | passes |
| `RH5027…AnalyzerTests.VerifyBlankLineBelowRegionDirectiveAboveDoStatementFooterIsNotReported` | RH5027-005 | Analyzer (spec example, both footers) | fails |
| `RH5027…AnalyzerTests.VerifyBlankLineBelowEndRegionDirectiveAboveDoStatementFooterIsNotReported` | RH5027-005 | Analyzer (`#endregion`, `while` footer line) | fails |
| `RH5027…AnalyzerTests.VerifyRunOfBlankLinesBelowRegionDirectiveAboveDoStatementFooterIsNotReported` | RH5027-005 | Analyzer (run of two) | fails |
| `RH5027…AnalyzerTests.VerifyBlankLineBelowRegionDirectiveAboveDoStatementFooterIsNotReportedWithCarriageReturnLineFeed` | RH5027-005 | Analyzer, CRLF | fails |
| `RH5027…AnalyzerTests.VerifyBlankLineBelowEndRegionDirectiveAboveFooterStartingWithClosingBraceIsReported` | RH5027-005 (boundary: `#endregion⏎⏎} while`) | Verify, 1 it. | passes |
| `RH5032…AnalyzerTests.VerifyNoDiagnosticWhenBlankLineFollowsRegionDirectivesInFrontOfChainedKeywordAndFooter` | — (FC-012/SHARED-FIX-004 cycle check, characterization) | Analyzer | passes |
| `RH5024…FormatterTests.VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing` | RH5024-001 (FC-013) | `VerifyFormatter(source)` | fails |
| `RH5025…FormatterTests.VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing` | RH5025-007 (FC-013) | `VerifyFormatter(source)` | fails |
| `RH5026…FormatterTests.VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing` | RH5026-005 (FC-013) | `VerifyFormatter(source)` | fails |
| `RH5027…FormatterTests.VerifyFormatterKeepsBlankLineBelowRegionDirectiveAndAnalyzerReportsNothing` | RH5027-005 (FC-013) | `VerifyFormatter(source)` | fails |
| `RH5011…FormatterTests.VerifyFormatterKeepsBreakDirectlyAboveElseAndAnalyzerReportsNothing` | RH5011-008 (FC-013) | `VerifyFormatter(source)` | fails |

The other side of RH5024-001 (`#endregion⏎⏎}` stays reported) is the existing test around line 255 of `RH5024…AnalyzerTests.cs` (unchanged, passing). The existing RH5023 tests `OneApplicationPerDiagnosticCorrectsTheRun*` (two incremental iterations, one line per diagnostic) stay as they are: RH5023 shares `BlankLineSpanRemovalCodeFixProviderBase` with RH5022–RH5027 and must keep deleting only its own line, so they guard the run change from leaking into RH5023.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| run of two blank lines, RH5022/RH5024/RH5025/RH5026/RH5027 | LF, CRLF | code fix | reproduced | "Expected '1' iterations but found '2'" |
| run of two lines holding spaces, RH5022 | LF | code fix | reproduced | same |
| two runs in one document, Fix All, RH5022 and RH5024 | LF | Fix All | reproduced | one blank line of each run left after the allowed iterations |
| `"…}\n\n"`, `"…}\n\n    "`, `"…// c\n\n"`, `"…#endif\n\n"` | LF (first also CRLF) | analyzer | reproduced | one diagnostic at the end-of-file position; expected none |
| `"…}\n\n\n"` | LF, CRLF | analyzer + code fix | reproduced | two diagnostics (line 5 and the empty position at line 6); expected only line 5 |
| `break` before `else` / do footer, one line and multi-line | LF, CRLF (one) | analyzer | reproduced | reported; expected not reported |
| `break;⏎while (…)` | LF | analyzer + code fix | passes | reported and fixed, as RH5011-008 keeps it |
| blank line below `#region`/`#endregion` before `}`, `{`, `else`/`catch`, do footer | LF, CRLF | analyzer | reproduced | reported (RH5032 re-inserts it, per 02-skeptic); expected not reported |
| same inputs | LF, CRLF | formatter (`VerifyFormatter(source)`) | reproduced as FC-013 | formatter keeps the line, the analyzer reports it |

## thoenissen/Reihitsu#1626

No tests of its own: the refactor has no visible output except RH3208's under SHARED-AFTERBRACE-002 (#1625 below). The existing tests of both owners stay the guard: `RH5030…AnalyzerTests` (RH5030-003 withholding, including `VerifyNoFixIsOfferedWhenCommentHoldsEveryLineBreak` under CRLF, and the SHARED-AFTERBRACE-001 rows) and every RH5001–RH5019, RH5021, RH5029 analyzer test class. If the base's gap rebuild moves into `BlankLineCodeFixUtilities`, the #972 and #1096 Q1 tests above apply to it through the base rules; RH5030 stays withheld for the comment shape (RH5030-003), which those tests pin.

## thoenissen/Reihitsu#1625

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3208…AnalyzerTests.VerifyStatementBehindCommentHoldingEveryLineBreakMovesBelowComment` | SHARED-AFTERBRACE-002, RH3208-002 | Verify, 1 it. (spec example) | fails |
| `RH3208…AnalyzerTests.VerifyStatementBehindCommentHoldingEveryLineBreakMovesBelowCommentWithCarriageReturnLineFeed` | SHARED-AFTERBRACE-002, RH3208-002 | Verify, 1 it., CRLF | fails |
| `RH3208…AnalyzerTests.VerifyFixAllMovesStatementsBehindCommentsHoldingEveryLineBreakInOneIteration` | SHARED-AFTERBRACE-002, RH3208-002 (FC-008) | Verify, 2 incremental, Fix All 1 it. | fails |
| `RH3208…AnalyzerTests.VerifyStatementBehindCommentHoldingEveryLineBreakMovesBelowCommentInSwitchSection` | SHARED-AFTERBRACE-002, RH3208-002 (FC-027) | Verify, 1 it. | fails |
| `RH3208…AnalyzerTests.VerifyStatementBelowCommentEndingItsLineGetsOneBlankLine` | SHARED-AFTERBRACE-001, RH3208-002 (boundary: line break outside the comment) | Verify, 1 it. | passes |
| `RH3208…AnalyzerTests.VerifyStatementBehindSingleLineCommentStaysOnClosingBraceLine` | — (characterization, see *Not covered*) | Verify, 1 it. | passes |
| `RH3208…AnalyzerTests.VerifyStatementOnLocalFunctionLineStaysOnClosingBraceLine` | — (characterization) | Verify, 1 it. | passes |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `int L() => 1; /* a⏎    b */ L();` (issue) | LF, CRLF | code fix | reproduced | `b */ ` with a trailing space and `L();` at column 0; expected `b */`, blank line, `L();` at the block's column |
| same, two methods, Fix All | LF | Fix All | reproduced | same in both |
| same inside a switch section | LF | code fix | reproduced | same |
| ` /* a⏎b */⏎L();`, ` /* a */ L();`, ` L();` | LF | code fix | passes | as stated by 03-spec-author's boundaries |

The formatter's output for the issue input equals the spec example (02-skeptic); I added no formatter test, because the RH3208 tests assert the exact text the spec gives.

## Whole run

### Not covered, and why

- **SHARED-AFTERBRACE-002 wording, ` /* a */ L();`.** The requirement says "when every line break between the closing brace and the next statement lies inside a comment"; with no line break at all this is vacuously true, and "the statement starts behind the comment on the comment's last line" also holds. 03-spec-author lists ` /* a */ L();` and ` L();` as boundaries that keep `L();` on the brace's line, so I wrote both as untagged characterizations of that reading, not as tests of SHARED-AFTERBRACE-002. For the lead: the spec author may want "at least one line break" in the condition. SHARED-STMT-004's third bullet has the same wording; its one-line boundary (`a++; /* c */ if`) is decided by the "shares its line" sentence there, so no ambiguity in that spec.
- **One test per base for the analyzer measure (SHARED-BLANK-001).** `rg -l "StatementShouldBePrecededByABlankLineAnalyzerBase<" src/Reihitsu.Analyzer` finds the 20 rules plus the base; both analyzer bases call `SyntaxTriviaUtilities.ContainsBlankLine` once (`StatementShouldBePrecededByABlankLineAnalyzerBase{TStatement}.cs:102`, `StatementShouldBeFollowedByABlankLineAnalyzerBase{TStatement}.cs:76`), with no override of that check in the rules. So I tested RH5002 and RH5029 for the preceding base and RH5011 for the following base, not all 21 rules. Decision #8 lets the implementer change the structure; if a rule gains its own check, this proof no longer holds.
- **One code-fix call site for #972 and #1096 Q1.** `rg -l "StatementShouldBePrecededByABlankLineCodeFixProviderBase" src/Reihitsu.Analyzer.CodeFixes` finds the 20 providers plus the base; the gap rebuild and `GetIndentation` live in the base. Covered with RH5002, RH5008, RH5021, RH5029, not every rule.
- **Not one call site: the five run-deleting fixes.** All derive from `BlankLineSpanRemovalCodeFixProviderBase`, which RH5023 shares and whose RH5023 behavior must stay per line, so each rule has its own run test (no shared-call-site proof claimed).
- **RH5005/RH5006 Fix All of mixed `using`/`await using` statements.** Not added; the token choice is per statement and the adjacent-`await` Fix All test exercises the stale-position class (FC-008).
- **FC-040 on the code-fix path for `///`.** Nothing is reported for the Q2 inputs, so no fix runs; the analyzer is tested under Diagnose (default) and None. `/** */` is covered for #972.
- **RH5103 and the single-line `switch` inside an initializer.** Follow-up candidates in `00-lead.md`; no test changed beyond the stale summary noted above.
- **FC-031 in production code.** The source comment on `StatementShouldBePrecededByABlankLineCodeFixProviderBase.GetIndentation` still calls the multi-line label a documented limitation (01-spec-author); it is production code, so it is the implementer's or cleaner's.
- **Not applicable:** FC-006, FC-022–FC-025 (no rewrite of tokens or literals changes; RH3208's body conversion is unchanged and covered by its existing tests), FC-018/FC-036 (no rename), FC-010/FC-011/FC-012 for formatter phases and FC-014 (no formatter change, no new rule), FC-028 (no formatter entry-point change), FC-038–FC-043 (no shape declared unsupported), FC-037 (no comment-only change claimed). The self-hosting scan is CI's (not duplicated).

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~AwaitUsing|FullyQualifiedName~AwaitForeach"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~BlockComment|FullyQualifiedName~CommentsEndingOnStatementLine|FullyQualifiedName~MultiLineCommentEndingOnStatementLine|FullyQualifiedName~CommentStartingBehindPreviousStatement|FullyQualifiedName~DelimitedDocumentationCommentStartingStatementLine|FullyQualifiedName~TwoBlockComments|FullyQualifiedName~SingleLineCommentSharingPreviousStatementLine"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~SectionColumn|FullyQualifiedName~NestingColumnWhenLabel"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~BelowDocumentationComment|FullyQualifiedName~EndIfDirective|FullyQualifiedName~PragmaDirective|FullyQualifiedName~DocumentationCommentDirectly"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RunOfBlankLines|FullyQualifiedName~RunsOfBlankLines|FullyQualifiedName~RunOfWhitespaceOnlyLines|FullyQualifiedName~BelowRegionDirective|FullyQualifiedName~BelowEndRegionDirective|FullyQualifiedName~BelowCommentAbove|FullyQualifiedName~BreakBefore|FullyQualifiedName~RegionDirectivesInFrontOfChainedKeywordAndFooter|FullyQualifiedName~AnalyzerReportsNothing"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"
```

Every changed class at once (23 classes; 94 fail on `cfdf0b1`, all new; the rest pass): run `scripts/test.sh --project analyzer` with a filter of the class names from `git diff --name-only 8abfa05 cfdf0b1 -- src`.
