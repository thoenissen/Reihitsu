Role: test implementer
Commit: 9eb0efb
Verdict: #1074: reproduced; #1079: reproduced; #1276: reproduced; #1613: reproduced; #1708: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples and requirements; formatter-parity expectations were checked against the CLI output of `scripts/format.sh --force` on head, which the analyzer change does not touch.

Paths are under `src/Reihitsu.Analyzer.Test/`. "Fails" = fails now on head for the stated reason; "passes" = holds now and must keep holding. The full analyzer project on head with these tests: 78 failed, 9041 passed; every failure is one of the new or rewritten tests below, and each fails on the issue's own expected-versus-observed difference (missing or misplaced diagnostic, extra diagnostic, or a fix that does not converge), never on a compile error or unrelated layout.

## thoenissen/Reihitsu#1074

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Formatting/RH5601UseTabsCorrectlyAnalyzerTests.VerifyEveryTabOfTheRequirementExampleIsReported` | RH5601-001, SHARED-DIAG-003 | `Verify(source)` | fails: 11 of 13 reported (line-start tab before `"abc"`, tab behind `/** c */` missing) |
| `…RH5601….VerifyTabStartingALineInFrontOfAStringLiteralIsDetectedAndFixed` (+ `…WithCarriageReturnLineFeed`) | RH5601-001, RH5601-004 | `Verify(source, fixed, …)` | fails: 0 diagnostics |
| `…RH5601….VerifyTabStartingALineInFrontOfARawStringIsDetectedAndFixed` | RH5601-001, RH5601-004 | `Verify(source, fixed, …)` | fails: 0 diagnostics |
| `…RH5601….VerifyTabBehindADelimitedDocumentationCommentInFrontOfAStringLiteralIsDetectedAndFixed` (verifier default, `DocumentationMode.Diagnose`) | RH5601-001, RH5601-004; FC-040 | `Verify(source, fixed, …)` | fails: 0 diagnostics |
| `…InDocumentationModeParse` | RH5601-001, RH5601-004; FC-040 | `Verify` + `ApplyDocumentationModeParseToTestProject` | fails: 0 diagnostics |
| `…InDocumentationModeNone` | RH5601-001, RH5601-004; FC-040 | `Verify` + `ApplyDocumentationModeNoneToTestProject` | passes |
| `…RH5601….VerifyTabBehindAStringLiteralIsDetectedAndFixed` (+ `…WithCarriageReturnLineFeed`) | RH5601-001, RH5601-004 | `Verify(source, fixed, …)` | fails: 0 diagnostics |
| `…RH5601….VerifyTabBetweenAStringLiteralAndItsSemicolonIsDetectedAndFixed` | RH5601-001, RH5601-004 | `Verify(source, fixed, …)` | fails |
| `…RH5601….VerifyTabBehindACharacterLiteralIsDetectedAndFixed` | RH5601-001, RH5601-004 | `Verify(source, fixed, …)` | fails |
| `…RH5601….VerifyTabInFrontOfACharacterLiteralIsDetectedAndFixed` | RH5601-001, RH5601-004; FC-003 | `Verify(source, fixed, …)` | passes (tab in the previous token's trailing trivia) |
| `…RH5601….VerifyTabBehindAVerbatimStringLiteralIsDetectedAndFixed` | RH5601-001, RH5601-004 | `Verify(source, fixed, …)` | fails |
| `…RH5601….VerifyTabBehindAUtf8StringLiteralIsDetectedAndFixed` | RH5601-001, RH5601-004 | `Verify(source, fixed, …)` | fails |
| `…RH5601….VerifyTabBehindASingleLineRawStringLiteralIsDetectedAndFixed` | RH5601-001, RH5601-004 | `Verify(source, fixed, …)` | fails |
| `…RH5601….VerifyTabBehindTheClosingQuotesOfARawStringIsDetectedAndFixed` | RH5601-001, RH5601-004 | `Verify(source, fixed, …)` | fails |
| `…RH5601….VerifyTabsAroundAStringLiteralInAHoleAreDetectedAndFixed` | RH5601-001, RH5601-004, SHARED-DIAG-003; FC-008 (2 diagnostics, Fix All) | `Verify(source, fixed, …)` | fails: 1 of 2 (tab behind `"x"` missing) |
| `…RH5601….VerifyCodeFixReplacesTheTabBehindAStringLiteralAndKeepsTheTabInsideIt` | RH5601-001, RH5601-002; FC-003 | `Verify(source, fixed, …)` | fails: 0 diagnostics |
| `Formatter/Formatting/RH5601UseTabsCorrectlyFormatterTests.VerifyFormatterFixesTabBehindAStringLiteral` | RH5601-001; FC-013 | `VerifyFormatter(source, fixed, …)` (LF + CRLF, 2 passes) | fails at the analyzer step: 0 diagnostics |
| `Formatting/RH5602CodeMustNotContainTrailingWhitespaceAnalyzerTests.VerifyEveryRunOfTheRequirementExampleIsReported` | RH5602-001, RH5602-002, SHARED-DIAG-003 | `Verify(source, onConfigure)` | fails: 5 of 11 reported (every run behind a literal's end and in the hole missing) |
| `…RH5602….VerifyWhitespaceBehindAStringLiteralIsDetectedAndFixed` (+ `…WithCarriageReturnLineFeed`) | RH5602-001, RH5602-004 | `Verify(source, fixed, …)` | fails: 0 diagnostics |
| `…RH5602….VerifyWhitespaceBehindAnInterpolatedStringIsDetectedAndFixed` | RH5602-001, RH5602-004 | `Verify(source, fixed, …)` | fails |
| `…RH5602….VerifyWhitespaceBehindAVerbatimStringLiteralIsDetectedAndFixed` | RH5602-001, RH5602-004 | `Verify(source, fixed, …)` | fails |
| `…RH5602….VerifyWhitespaceBehindAUtf8StringLiteralIsDetectedAndFixed` | RH5602-001, RH5602-004 | `Verify(source, fixed, …)` | fails |
| `…RH5602….VerifyWhitespaceBehindTheClosingQuotesOfARawStringIsDetectedAndFixed` | RH5602-001, RH5602-004 | `Verify(source, fixed, …)` | fails |
| `…RH5602….VerifyWhitespaceBehindAMultiLineVerbatimStringIsDetectedAndFixedAndItsTextIsKept` | RH5602-001, RH5602-002, RH5602-004; FC-003 | `Verify(source, fixed, …)` | fails: 0 diagnostics (the `a··` in the text must stay) |
| `…RH5602….VerifyWhitespaceAtTheEndOfAHoleLineOfAnInterpolatedVerbatimStringIsDetectedAndFixed` | RH5602-001, RH5602-004, SHARED-DIAG-003 | `Verify(source, fixed, …)` | fails |
| `…RH5602….VerifyWhitespaceAtTheEndOfAHoleLineOfARegularInterpolatedStringIsDetectedAndFixed` | RH5602-001, RH5602-004, SHARED-DIAG-003; FC-027 | `Verify(source, fixed, …)` | fails |
| `…RH5602….VerifyWhitespaceAtTheEndOfAHoleLineOfAnInterpolatedRawStringIsDetectedAndFixed` | RH5602-001, RH5602-004, SHARED-DIAG-003 | `Verify(source, fixed, …)` | fails |
| `…RH5602….VerifyWhitespaceOnlyLineInAHoleOfAnInterpolatedRawStringIsDetectedAndFixed` | RH5602-001, RH5602-004, SHARED-DIAG-003 | `Verify(source, fixed, …)` | fails |
| `…RH5602….VerifyWhitespaceBehindSeveralLiteralsIsDetectedAndFixedTogether` | RH5602-001, RH5602-004; FC-008 (4 diagnostics, Fix All) | `Verify(source, fixed, …)` | fails |
| `…RH5602….VerifyNoDiagnosticsForWhitespaceBehindTheOpeningQuotesOfARawString` | RH5602-002; FC-003 | `Verify(source)` | passes |
| `…RH5602….VerifyNoDiagnosticsForWhitespaceOnlyContentLinesOfMultiLineLiterals` | RH5602-002; FC-003 | `Verify(source)` | passes |
| `…RH5602….VerifyWhitespaceAtTheEndOfTheLineInFrontOfAStringLiteralIsDetectedAndFixed` | RH5602-001, RH5602-004; FC-003 | `Verify(source, fixed, …)` | passes |
| `Formatter/Formatting/RH5602CodeMustNotContainTrailingWhitespaceFormatterTests.VerifyFormatterRemovesWhitespaceBehindAStringLiteral` | RH5602-001; FC-013 | `VerifyFormatter(source, fixed, …)` | fails at the analyzer step |
| `Formatting/RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzerTests.VerifyEveryBraceOfTheRequirementExampleIsReported` | RH6009-001, SHARED-DIAG-003 | `Verify(source)` | fails: 6 of 7 |
| `…RH6009….VerifyTheRequirementFixExampleIsFixed` | RH6009-004; FC-008 | `Verify(source, fixed)` | fails: 4 of 5 |
| `…RH6009….VerifyArrayInitializerBraceInAHoleIsDetectedAndFixed` (+ `…WithCarriageReturnLineFeed`) | RH6009-001, RH6009-003, RH6009-004 (CRLF: -001, -004) | `Verify(source, fixed, …)` | fails |
| `…RH6009….VerifyLambdaAnonymousObjectAndSwitchBracesInHolesAreDetectedAndFixed` | RH6009-001, RH6009-004; FC-027 | `Verify(source, fixed, …)` | fails: 0 of 3 |
| `…RH6009….VerifyArrayInitializerBraceInAHoleOfAnInterpolatedVerbatimStringIsDetectedAndFixed` | RH6009-001, RH6009-004 | `Verify(source, fixed, …)` | fails |
| `…RH6009….VerifyArrayInitializerBraceInAMultiLineHoleOfADoubleDollarRawStringIsDetectedAndFixed` | RH6009-001, RH6009-002, RH6009-004 | `Verify` with `MarkupMode.None` and an explicit span (`$$` is the markup caret) | fails |
| `…RH6009….VerifyNoDiagnosticsForBracesOfNestedInterpolations` | RH6009-002; FC-003 | `Verify(source)` | passes |
| `…RH6009….VerifyNoDiagnosticsForInterpolationBracesOfADoubleDollarRawString` | RH6009-002; FC-003 | `Verify` with `MarkupMode.None` | passes |
| `Formatter/Formatting/RH6009OpeningBracesMustBeSpacedCorrectlyFormatterTests.VerifyFormatterFixesArrayInitializerBraceInAHole` | RH6009-001; FC-013 | `VerifyFormatter(source, fixed, …)` | fails at the analyzer step |
| `Formatting/RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzerTests.VerifyEveryBraceOfTheRequirementExampleIsReported` | RH6010-001, SHARED-DIAG-003 | `Verify(source)` | fails: 4 of 5 |
| `…RH6010….VerifyTheRequirementFixExampleIsFixed` | RH6010-004; FC-008 | `Verify(source, fixed)` | fails: 3 of 4 |
| `…RH6010….VerifyArrayInitializerBraceInAHoleIsDetectedAndFixed` (+ `…WithCarriageReturnLineFeed`) | RH6010-001, RH6010-003, RH6010-004 (CRLF: -001, -004) | `Verify(source, fixed, …)` | fails |
| `…RH6010….VerifyEmptyInitializerBraceInAHoleIsDetectedAndFixed` | RH6010-001, RH6010-004 | `Verify(source, fixed, …)` | fails |
| `…RH6010….VerifyLambdaAnonymousObjectAndSwitchBracesInHolesAreDetectedAndFixed` | RH6010-001, RH6010-004; FC-027 | `Verify(source, fixed, …)` | fails: 0 of 3 |
| `…RH6010….VerifyArrayInitializerBraceInAHoleOfAnInterpolatedVerbatimStringIsDetectedAndFixed` | RH6010-001, RH6010-004 | `Verify(source, fixed, …)` | fails |
| `…RH6010….VerifyArrayInitializerBraceInAHoleOfADoubleDollarRawStringIsDetectedAndFixed` (`{ 1}}}` — initializer brace directly before the closing `}}`) | RH6010-001, RH6010-002, RH6010-004 | `Verify` with `MarkupMode.None` and an explicit span | fails |
| `…RH6010….VerifyNoDiagnosticsForBracesOfNestedInterpolations` | RH6010-002; FC-003 | `Verify(source)` | passes |
| `…RH6010….VerifyNoDiagnosticsForInterpolationBracesOfADoubleDollarRawString` (`{ 1 }}}`) | RH6010-002; FC-003 | `Verify` with `MarkupMode.None` | passes |
| `Formatter/Formatting/RH6010ClosingBracesMustBeSpacedCorrectlyFormatterTests.VerifyFormatterFixesArrayInitializerBraceInAHole` | RH6010-001; FC-013 | `VerifyFormatter(source, fixed, …)` | fails at the analyzer step |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH5601 tab at line start before `"abc"`; tab behind `"a"`, `'a'`, `@"a"`, `"a"u8`, `"""a"""`, multi-line raw `"""`; tab behind `"x"` in a hole | LF; CRLF for two | analyzer + fix | reproduced | not reported vs reported and replaced by four spaces |
| RH5601 tab behind `/** c */` before `"abc"` | LF | analyzer, `Diagnose` / `Parse` / `None` | reproduced in `Diagnose` and `Parse`; `None` reports it already | not reported vs reported |
| RH5602 whitespace behind a literal's end (regular, `$"…"`, `@"…"`, `u8`, raw closing, multi-line verbatim last line), in a hole line (`$@`, `$"…"` multi-line hole, `$"""`), whitespace-only hole line | LF; CRLF for one | analyzer + fix | reproduced | not reported vs reported and deleted |
| RH6009/RH6010 braces of an initializer, lambda block, anonymous object, switch expression in holes of `$"…"`, `$@"…"`, `$$"""…"""` (also multi-line hole) | LF; CRLF for one each | analyzer + fix | reproduced | not reported vs reported and spaced |
| Counterpart surface: each of the four rules on its formatter-parity input | LF + CRLF (base) | formatter | formatter already clears the input; the test fails on the analyzer step | analyzer reports nothing on the input the formatter changes |

### Not covered, and why

- RH5601 tab behind a `$$` interpolated literal: same token kinds as `$` (`InterpolatedStringEndToken`, `InterpolatedRawStringEndToken`), already reported today per the skeptic's dispatch; not a new arm.
- RH5601 tab behind a UTF-8 raw literal (`"""a"""u8`): same predicate branch as the single-line raw and UTF-8 cells, both tested.
- RH5602 whitespace behind a hole's `}` on a raw content line, and in interpolated verbatim text: already covered on the content side by `VerifyNoDiagnosticsForTrailingWhitespaceBehindAHoleInAnInterpolatedRawString` and `VerifyInterpolatedVerbatimStringsDoNotProduceDiagnostics`; tab content side by the existing `VerifyNoDiagnosticsForTab…` tests.
- RH6009/RH6010 `PropertyPatternClause` and `with` initializers in a hole: same brace tokens and the same exemption predicate (`FormattingTextAnalysisUtilities.IsInsideInterpolatedString`, which checks only for an `InterpolatedStringExpressionSyntax` ancestor), so the four owners tested reach the same call site; one owner per string kind would add no arm.
- FC-019 (comment / directive in the gap): RH5601 has the `/** */` cell and the `#pragma→` and `#define→` cells (spec-example test); RH6009 keeps its `/* note */` cell. A directive cannot stand inside an interpolation hole, so no directive cell exists for the hole cases.
- Formatter parity on a tab behind `/** c */` before an argument was dropped: `scripts/format.sh --force` on `M(/** c */→"abc");` writes `M(⏎⏎          /** c */    "abc");` (comment moved to its own line behind a blank line, the gap kept at four spaces). That is the documentation-comment phase's output, not RH5601's question; recorded here as an observation for the lead (reachable through the CLI), not tested in this run.
- Formatter parity on a hole line with trailing whitespace was dropped: the formatter also re-breaks the hole (`{1 +` / `2}` becomes `{1` / `+ 2}`), so the parity test would assert line-break-phase layout outside this issue.

## thoenissen/Reihitsu#1079

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Formatting/RH5603FileMustNotEndWithANewlineAnalyzerTests.VerifyWhitespaceOnlyLineInTheTrailingLineBreaksIsPartOfTheDiagnosticAndIsDeleted` (the issue's `}⏎···⏎`) | RH5603-001, RH5603-003, RH5603-004; FC-007 | `Verify(source, fixed, …)` | fails: span starts on line 4 instead of line 3 |
| `…RH5603….VerifyWhitespaceOnlyLineInTheTrailingCarriageReturnLineFeedsIsPartOfTheDiagnosticAndIsDeleted` | RH5603-001, RH5603-003, RH5603-004 | `Verify(source, fixed, …)` | fails: same |
| `…RH5603….VerifyOneApplicationOfTheFixDeletesTheWholeTail` | RH5603-004; FC-007, FC-017 | `ApplyCodeFixAsync` (raw text, LF and CRLF) | fails: `class C\n{\n}\n   ` vs `class C\n{\n}` |
| `…RH5603….VerifyWhitespaceOnlyLineInTheTrailingCarriageReturnsIsPartOfTheDiagnosticAndIsDeleted` | — (lone `\r` is not named by RH5603-001; untagged characterization of the analyzer's own `\r` handling, FC-033) | `Verify(source, fixed, …)` | fails: span starts on line 4 |
| `…RH5603….VerifySeveralWhitespaceOnlyLinesInTheTrailingLineBreaksArePartOfTheDiagnosticAndAreDeleted` (`}⏎··⏎→⏎⏎`) | RH5603-001, RH5603-003, RH5603-004 | `Verify(source, fixed, …)` | fails: span starts on line 5 |
| `…RH5603….VerifyWhitespaceBehindTheLastCharacterIsNotPartOfTheDiagnosticAndStays` (`}··⏎` → `}··`) | RH5603-001, RH5603-003, RH5603-004; FC-003 | `Verify(source, fixed, …)` | passes |
| `…RH5603….VerifyWhitespaceBehindTheLastCharacterStaysAndTheWhitespaceOnlyLineIsDeleted` (`}··⏎···⏎` → `}··`) | RH5603-001, RH5603-003, RH5603-004; FC-003 | `Verify(source, fixed, …)` | fails: span starts on line 4 |
| `…RH5603….VerifyWhitespaceOnlyLineBehindAFinalCommentIsPartOfTheDiagnosticAndIsDeleted` | RH5603-001, RH5603-004 | `Verify(source, fixed, …)` | fails: span starts on line 6 |
| `…RH5603….VerifyNoDiagnosticsWhenTheLastLineHoldsWhitespaceOnlyWithoutALineBreak` (`}⏎···`) | — (spec non-goal; untagged characterization) | `Verify(source)` | passes |
| `Formatter/Formatting/RH5603FileMustNotEndWithANewlineFormatterTests.VerifyFormatterRemovesATailWithAWhitespaceOnlyLine` | RH5603-001, RH5603-003; FC-013 | `VerifyFormatter(source, fixed, ExpectedDiagnostic(4,2,6,1))` | fails at the analyzer step: reported on line 5 |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `class C⏎{⏎}⏎···⏎`, fix once | LF, CRLF, lone `\r` | analyzer + fix | reproduced | diagnostic on the last line break only, fix leaves `}⏎···` vs one diagnostic from the first line break after `}` and `}` after one fix |
| Same input | LF + CRLF | formatter | formatter output `}`; analyzer span wrong | — |
| `}··⏎` | LF | analyzer + fix | holds | — |

The issue's "RH5603 reports `}⏎···` again" was not reproduced: `}⏎···` without a final break is not reported by RH5603 (`VerifyNoDiagnosticsWhenTheLastLineHoldsWhitespaceOnlyWithoutALineBreak` passes). The reproduced defect is the fix result.

### Not covered, and why

- Tail behind a final directive line or `///` comment: follow-up candidate 1 in `00-lead.md`, outside this run; no test asserts either outcome.
- Files ending in U+2028/U+2029/U+0085: dropped by decision 8.
- Fix All: one diagnostic per file; the existing two-document `GetFixAllScenario` covers it.

## thoenissen/Reihitsu#1276

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Formatting/RH5404ElementMustNotBeOnSingleLineAnalyzerTests.VerifySingleLineExtensionBlockIsFlagged` — rewritten: it was the characterization that asserted a diagnostic without location, run green on 9eb0efb; now asserts the location on `extension` and no fix | RH5404-001, RH5404-002, RH5404-003 | `Verify` + `VerifyNoCodeFixIsOffered` | fails: diagnostic is in file "" (no location) |
| `…RH5404….VerifyEmptySingleLineExtensionBlockIsReportedOnItsKeywordWithoutCodeFix` | RH5404-001, RH5404-002, RH5404-003 | `VerifyNoCodeFixIsOffered` | fails: no location |
| `…RH5404….VerifyGenericSingleLineExtensionBlockIsReportedOnItsKeywordWithoutCodeFix` | RH5404-002, RH5404-003 | `VerifyNoCodeFixIsOffered` | fails: no location |
| `…RH5404….VerifySingleLineExtensionBlockBehindAnotherTypeIsReportedOnItsKeywordWithoutCodeFix` (the fix today edits `class A`) | RH5404-002, RH5404-003; FC-030 | `VerifyNoCodeFixIsOffered` | fails: no location |
| `…RH5404….VerifySingleLineExtensionBlockBehindAUsingDirectiveIsReportedOnItsKeywordWithoutCodeFix` | RH5404-002, RH5404-003 | `VerifyNoCodeFixIsOffered` | fails: no location |
| `…RH5404….VerifySingleLineTypeHoldingASingleLineExtensionBlockKeepsItsFixWhileTheBlockGetsNone` (the RH5404-002 example) | RH5404-002, RH5404-003; FC-009 | `Verify` + `GetCodeFixActionsAsync` at `X` (non-empty) and at the `extension` keyword (empty) | fails: no location |
| `…RH5404….VerifyTheRequirementExampleIsReported` (the RH5404-001 example) | RH5404-001 | `Verify(source)` | fails: no location |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| One-line extension block (empty, one member, generic; first type / after a type / after a using; in a one-line containing type) | LF | analyzer + fix | reproduced | diagnostic without a source location vs on the `extension` keyword, no fix |

### Not covered, and why

- CRLF: the location is a token span, no layout is involved, and no fix is offered.
- Formatter parity: the formatter's extension-block braces are out of this run (#1446).
- The fix result of the containing type `X` is not asserted; RH5404-003 only says it is offered as usual, and its layout of the inner block is the formatter's (#1446).

## thoenissen/Reihitsu#1613

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests.VerifyAlignedLiteralIsNotReportedAndMisalignedLiteralIsReportedWithLoneCarriageReturns` (RH5202-001 example) | RH5202-001; FC-033 | `Verify(ConvertEndOfLine(…, "\r"), …)` | fails: 2 reported, the aligned `a` too |
| `…RH5202….VerifyAlignedLiteralsAreNotReportedWithLoneCarriageReturns` (plain, `u8`, `$`) | RH5202-001 | `Verify(ConvertEndOfLine(…, "\r"))` | fails: 3 reported |
| `…RH5202….VerifyMisalignedInterpolatedLiteralIsFixedInOneApplicationWithLoneCarriageReturns` (RH5202-003 example) | RH5202-003; FC-007 | `VerifyCodeFixWithEndOfLine(…, "\r")` | fails: 2 iterations instead of 1 |
| `…RH5202….VerifyMisalignedPlainLiteralIsFixedInOneApplicationWithLoneCarriageReturns` | RH5202-003; FC-007 | `VerifyCodeFixWithEndOfLine(…, "\r")` | fails: 2 iterations |
| `…RH5202….VerifyFormatterOutputForLoneCarriageReturnDocumentIsNotReported` (formatter's own output: LF outside, lone `\r` kept inside the literals) | RH5202-001; FC-013 | `Verify(Format(…))` | fails: 2 reported |
| `…RH5202….VerifyAlignedLiteralsAreNotReportedWith{NextLineBreaks,LineSeparators,ParagraphSeparators}InTheLiteral` (U+0085, U+2028, U+2029 inside plain, `u8`, `$` literals in an LF document) | RH5202-001 | `Verify` via private `CreateAlignedLiteralsWithLiteralLineBreak` | fail: 3 reported each |
| `…RH5202….VerifyMisalignedLiteralsAreFixedWith{NextLineBreaks,LineSeparators,ParagraphSeparators}InTheLiteral` (plain + `$`, the terminators kept in the fixed text) | RH5202-001, RH5202-003; FC-007, FC-008 | `Verify(source, fixed, …)` via private `VerifyMisalignedLiteralsWithLiteralLineBreak` | fail: code-fix iteration limit exceeded |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Aligned literal, last line break a lone `\r` / U+0085 / U+2028 / U+2029 | lone `\r` doc; LF doc with the terminator in the literal | analyzer | reproduced | reported vs not reported |
| Misaligned literal, same terminators | as above | fix | reproduced | fix aligns, analyzer keeps reporting (no convergence) vs aligned after one application |
| Lone-`\r` document | lone `\r` | formatter (`Format`) then analyzer | reproduced (skeptic had this by code only; now run) | formatter output reported vs not reported |

The issue's own "aligned" example is misaligned (columns 12 and 8, as the spec author noted); the tests use the spec's aligned literal.

### Not covered, and why

- `$$` literals: same token kind (`InterpolatedMultiLineRawStringStartToken`) and the same `OnInterpolatedStringExpression` path as `$`; `$$` is also the markup caret, so a test would need `MarkupMode.None` for no new arm.
- `u8` misaligned: same `OnStringLiteralExpression` branch as plain (`Utf8MultiLineRawStringLiteralToken` beside `MultiLineRawStringLiteralToken`); the aligned `u8` cell is tested.
- CRLF documents with a special terminator inside the literal: the terminator inside the literal decides the closing line; the LF arm with each terminator covers it, and CRLF/LF without terminators are covered by existing tests.

## thoenissen/Reihitsu#1708

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Naming/RH4105MethodParameterCasingAnalyzerTests.VerifyUnnamedReceiverParametersOfExtensionBlocksAreNotReported` (RH4105-004 example) | RH4105-004; FC-023 | `Verify(source)` | fails: 3 reported |
| `…RH4105….VerifyUnnamedReferenceReceiverParameterIsNotReported` (`extension(ref int)`) | RH4105-004; FC-027 | `Verify(source)` | fails: 2 reported (count as observed) |
| `…RH4105….VerifyUnnamedReceiverParameterOfAnExtensionBlockWithAStaticMemberIsNotReported` | RH4105-004 | `Verify(source)` | fails: 1 reported |
| `…RH4105….VerifyNamedReceiverParameterIsReportedNextToAnUnnamedOne` | RH4105-001, RH4105-004; FC-003 | `Verify(source, fixed, …)` | fails: 2 reported instead of 1 |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `extension(long)`, `extension<T>(T[])`, `extension(int)`, `extension(ref int)` | LF | analyzer | reproduced | reported without location vs not reported |
| `extension(int Value)` next to `extension(long)` | LF | analyzer + fix | reproduced for the unnamed neighbor; the named one is reported and renamed | — |

The issue's "twice per block" count: `extension(ref int)` alone yields 2 diagnostics and `extension(long)` with a static member yields 1, so the count is not constant per block; recorded as observed, no assertion depends on it.

### Not covered, and why

- `ref readonly`, `scoped`, attributes, nullable and tuple receiver types: the same `SyntaxKind.Parameter` registration and the same identifier check (`CasingAnalyzerBase` → `RH4105….GetLocations`); `ref` is tested as the modifier arm.
- Formatter parity and line endings: no layout involved.

## Findings for the lead (not test defects)

1. **RH5602-001 example does not compile.** It declares `int x;` and `string x = """…"""`, and `int w;` and `string w = @"…"`, in one class (CS0102). `VerifyEveryRunOfTheRequirementExampleIsReported` keeps the example verbatim and sets `CompilerDiagnostics.None`, with a comment saying why; the spec author may want to rename the fields, after which that setting can go.
2. **`UsesListsExactlyTheReferencedRequirements` (architecture tests) fails on 9eb0efb, before any test of mine:** `analyzer/RH5404.md: SHARED-FIX-001 is referenced but missing from Uses` — introduced by the RH5404-003 `Why:` amendment (6ab9935).
3. Formatter observation (no test written): `M(/** c */→"abc");` is formatted to `M(⏎⏎          /** c */    "abc");` by the CLI on 9eb0efb.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5601UseTabsCorrectly|FullyQualifiedName~RH5602CodeMustNotContainTrailingWhitespace|FullyQualifiedName~RH6009OpeningBraces|FullyQualifiedName~RH6010ClosingBraces"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5603FileMustNotEndWithANewline"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5404ElementMustNotBeOnSingleLine"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5202RawStringLiterals"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH4105MethodParameterCasing"
scripts/test.sh --project architecture --filter "FullyQualifiedName~UsesListsExactlyTheReferencedRequirements"
```
