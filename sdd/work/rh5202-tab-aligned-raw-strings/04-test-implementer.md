Role: test implementer
Commit: 5c9281f
Verdict: tests written

Inputs: thoenissen/Reihitsu#1925, `00-lead.md` (answers 1A, 2B; decisions 1–3), `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md`, the change doc `sdd/changes/2026-11/rh5202-tab-aligned-raw-strings.md`, `sdd/specs/analyzer/RH5202.md` (RH5202-001, -007, -008, -009). The prompt carried no intended fix, cause, or hoped-for verdict.

Files: `tests/Reihitsu.Analyzer.Test/Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyTabIndentationTests.cs`, `tests/Reihitsu.Analyzer.Test/Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyHoleTests.cs`, `tests/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests.cs`. `TabIndentation` = the first, `Hole` = the second, `FormatterTests` = the third.

## Tests

`VerifyFix…UnderBothLineEndings` / `VerifyCodeFixWithEndOfLine` = `Verify(source, fixedSource)` (diagnostic, one-pass result, re-analysis clean, Fix All) plus `ApplyCodeFixAsync` exact text, LF and CRLF. "Red: not reported" means the run on the unchanged production code fails with "expected N actual N-1 (or 0)" diagnostics — the issue's own difference.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| TabIndentation `VerifyLiteralAlignedInCharactersBehindTabsIsReportedAndItsTabsAreWrittenAsSpaces` — **rewritten** from `VerifyLiteralAlignedInCharactersBehindTabsIsNotReported` (RH5202-009 example 1, the issue input) | RH5202-001, RH5202-009 | `VerifyFixUnderBothLineEndings` | red: not reported |
| TabIndentation `VerifyInterpolatedLiteralAlignedInCharactersBehindTabsIsReportedAndKeepsTheWhitespaceBehindTheWidth` (example 2) | RH5202-009 | same | red: not reported |
| TabIndentation `VerifyLiteralIndentedWithTabsAndAlignedWithSpacesIsReportedAndItsTabsAreWrittenAsSpaces` (example 3, smart tabs, answer 1A) | RH5202-009 | same | red: not reported |
| TabIndentation `VerifyLiteralAlignedBehindATabBetweenSpacesIsReportedAndItsTabIsWrittenAsASpace` ("wherever it stands") | RH5202-009 | same | red: not reported |
| TabIndentation `VerifyAlignedLiteralWithoutATabInFrontOfTheClosingQuotesAndATabOnAContentLineIsNotReported` (example 4) | RH5202-009 | `Verify(source)` LF + CRLF | green |
| TabIndentation `VerifyAlignedLiteralWithoutATabInFrontOfTheClosingQuotesAndATabOnAWhitespaceOnlyLineIsNotReported` ("within the width" arm; expects CS9003, see *Not covered*) | RH5202-009 | `Verify(source, CS9003)` LF + CRLF | green |
| TabIndentation `VerifyUtf8LiteralAlignedInCharactersBehindTabsIsReportedAndItsTabsAreWrittenAsSpaces` | RH5202-009 | `VerifyFixUnderBothLineEndings` | red: not reported |
| TabIndentation `VerifyLiteralWithTwoDollarSignsAlignedInCharactersBehindTabsIsReportedAndItsTabsAreWrittenAsSpaces` | RH5202-009 | `Verify(…, MarkupMode.None, span)` LF + CRLF (the markup parser reads `$$` as a caret) | red: not reported |
| TabIndentation `VerifyLiteralAlignedBehindTabsAndANoBreakSpaceIsReportedAndBothAreWrittenAsSpaces` | RH5202-009 | `VerifyFixUnderBothLineEndings` | red: not reported |
| TabIndentation `VerifyFixOfLiteralAlignedBehindTabsWritesAWhitespaceOnlyLineShorterThanTheWidthAsOneSpacePerCharacter` (14 spaces) | RH5202-007, RH5202-009 | same | red: not reported |
| TabIndentation `VerifyFixOfInterpolatedLiteralAlignedBehindTabsLeavesTheLineThatStartsInsideAHole` | RH5202-009 | same | red: not reported |
| TabIndentation `VerifyLiteralAlignedInCharactersBehindTabsIsFixedInOneApplicationWithLoneCarriageReturns` | RH5202-009 | `VerifyCodeFixWithEndOfLine(…, "\r")` | red: not reported |
| TabIndentation `VerifyFixAllCorrectsEveryLiteralAlignedInCharactersBehindTabs` (FC-008) | RH5202-009 | `Verify(source, fixed, 2 diagnostics)` LF + CRLF | red: not reported |
| TabIndentation `VerifySingleLineRawStringLiteralsOnLinesIndentedWithTabsAreNotReported` ("multi-line" boundary) | RH5202-009 | `Verify(source)` LF + CRLF | green |
| TabIndentation `VerifyFixMatchesFormatterForALiteralAlignedInCharactersBehindTabs` (SHARED-FIX-007 via RH5202-009's Why) | RH5202-009 | `VerifyCodeFixMatchesFormatter` | red: not reported |
| Hole `VerifyFixIsWithheldWhenRawStringNestedInHoleIsAlignedBehindTabsAndOfferedAfterTheNestedFix` — **rewritten** from `VerifyFixIsWithheldWhenUnreportedRawStringNestedInHoleIsIndentedWithTabs` (RH5202-008 example 1: nested reported, outer withheld, nested fix → intermediate, outer fix → result) | RH5202-008, RH5202-009 | `VerifyOuterLiteralFixIsWithheldAndNestedLiteralFixIsOffered`, `TryApplyCodeFixAtAsync`, `Verify(intermediate, result)`, LF + CRLF | red: 1 of 2 diagnostics |
| Hole `VerifyEveryFixApplicationConvergesWhenRawStringNestedInHoleIsAlignedBehindTabs` (FC-007, FC-008) | RH5202-008, RH5202-009 | `Verify(…, NumberOfFixAllIterations = 2)` | red: 1 of 2 diagnostics |
| Hole `VerifyFixIsWithheldWhenOuterAndNestedRawStringsAreAlignedBehindTabsAndOfferedAfterTheNestedFix` (zero-shift outer withheld) | RH5202-008, RH5202-009 | as the rewritten example-1 test | red: 0 of 2 diagnostics |
| Hole `VerifyFixOfOuterLiteralAlignedBehindTabsLeavesTheLinesOfRawStringNestedInHoleWithoutATab` | RH5202-007, RH5202-009 | `VerifyCodeFixWithEndOfLine` LF + CRLF | red: not reported |
| Hole `VerifyRawStringAlignedBehindTabsInHoleOfVerbatimInterpolatedStringIsReportedAndFixed` | RH5202-009 | same | red: not reported |
| Hole `VerifyRawStringAlignedBehindTabsInHoleOfRegularInterpolatedStringIsReportedAndFixed` | RH5202-009 | same | red: not reported |
| Hole `VerifyFixIsWithheldWhenRawStringNestedTwoLevelsDeepIsAlignedBehindTabsAndOfferedAfterTheNestedFix` — **rewritten** from `…TwoLevelsDeepIsIndentedWithTabs` | RH5202-008, RH5202-009 | new private helper `VerifyNestedLiteralFixRunsBeforeTheOuterLiteralFix` (outer withheld, nested offered, nested fix's exact text, only outer reported after it, outer fix then offered; LF + CRLF) | red: 1 of 2 diagnostics |
| Hole `VerifyFixIsWithheldWhenInterpolatedRawStringNestedInHoleIsAlignedBehindTabsAndOfferedAfterTheNestedFix` — **rewritten** from `…InterpolatedRawStringNestedInHoleIsIndentedWithTabs` | RH5202-008, RH5202-009 | same helper | red: 1 of 2 diagnostics |
| Hole `VerifyFixIsWithheldWhenUtf8RawStringNestedInHoleIsAlignedBehindTabsAndOfferedAfterTheNestedFix` — **rewritten** from `…Utf8RawStringNestedInHoleIsIndentedWithTabs` | RH5202-008, RH5202-009 | same helper | red: 1 of 2 diagnostics |
| Hole `VerifyFixIsWithheldWhenShiftingLeftAndRawStringNestedInHoleIsAlignedBehindTabsAndOfferedAfterTheNestedFix` — **rewritten** from `…ShiftingLeftAndRawStringNestedInHoleIsIndentedWithTabs` | RH5202-008, RH5202-009 | same helper | red: 1 of 2 diagnostics |
| FormatterTests `VerifyFormatterWritesTabsOfLiteralAlignedByTheAnalyzersMeasureAsSpacesAndClearsTheDiagnostic` — **changed** from `VerifyFormatterWritesTabsOfLiteralAlignedByTheAnalyzersMeasureAsSpaces`: the input now expects the diagnostic (FC-013 parity) | FMT-RAW-006, FMT-RAW-010, RH5202-009 | `VerifyFormatter(source, fixed, diagnostic)` | red: not reported |

On the unchanged code: 23 red (each on the missing diagnostic), 129 green, of 152 RH5202 tests; `scripts/test.sh --project architecture`: 198 passed (`RequirementCoverageTests` now finds RH5202-009 covered).

Expected values come from the spec examples and RH5202-007's rule (one space per stripped whitespace character, zero shift). As a cross-check only, `scripts/format.sh` on the inputs of the 16 tests whose code around the literal is formatter layout writes exactly the expected texts, except `VerifyFixOfInterpolatedLiteralAlignedBehindTabsLeavesTheLineThatStartsInsideAHole`, where the formatter also indents the hole's continuation line `+ 1}`, which the fix leaves as written (RH5202-007 "the lines RH5202-004 and RH5202-005 leave as written stay so").

## Scenarios (bug fix)

_N/A_ (behavior change).

## Not covered, and why

- **Existing tests whose output the change alters** — searched the RH5202 test classes and `RawStringLiteralUtilitiesTests` for every tab fixture (`rg "→"`): only the six the skeptic listed (TabIndentation 1, Hole 5 — rows 10, 11 — and FormatterTests 1) asserted a tab-aligned literal unreported or a fix-free nested literal; all six are rewritten above. `RH5202…AnalyzerTests.cs` holds no tab fixture. `VerifyEveryFixApplicationConvergesWhenRawStringNestedInHoleIsIndentedWithTabs`, `VerifyFixIsWithheldWhenReportedRawStringNestedInHoleIsIndentedWithTabsAndOfferedAfterTheNestedFix`, and `VerifyFixIsOfferedAndKeepsTheTabOfRawStringNestedInHoleWithoutATabInFrontOfItsClosingQuotes` assert misaligned nested literals and stay valid unchanged. `RawStringLiteralUtilitiesTests` pins `GetClosingLineShift` in characters, which the spec keeps.
- **Closing whitespace of no-break spaces only, aligned → not reported** — pinned by `FormatterTests.VerifyFormatterKeepsLiteralAlignedByTheAnalyzersMeasureBehindNoBreakSpaces` (`VerifyFormatter(source)` asserts analyzer-clean, LF + CRLF).
- **"In the column" boundary, misaligned literal with a tab** — pinned by the existing RH5202-007 tests (`VerifyFixShiftsLeftOverTabsAndWritesTheRestAsSpaces`, `VerifyFixWritesTheStrippedTabsAsSpacesAndKeepsTheTabBehindTheWidth`, …).
- **"Within the width" arm of RH5202-009's no-tab bullet** — a non-whitespace content line cannot hold a tab where the closing whitespace holds a space: the compiler rejects it (CS9003), and so it does for a whitespace-only line. The only test of the arm is therefore on code that does not compile; it asserts CS9003 and no RH5202. This is an observation for the spec checker: the phrase "within or behind the width" in RH5202-009 has no compiling input on the "within" side.
- **FC-009** (action that changes nothing) — every reported case in the matrix holds at least one tab, and every fix test asserts an exact differing text; no separate test.
- **FC-019/FC-020/FC-021, FC-004/FC-005** — the whitespace in front of the closing quotes and at the start of content lines is literal text; no comment or directive can stand there. Not applicable.
- **FC-010/FC-012/FC-028** — formatter not changed (skeptic: not involved); parity covered by the FormatterTests change and `VerifyFixMatchesFormatter…`.
- **FC-038–FC-043** — no shape declared unsupported (lead decision 3).
- **FC-018, FC-023–FC-026, FC-029, FC-036** — no rename, identifiers, member kinds, or paired layouts involved.
- **Skeptic row 18** — covered by `VerifyFixOfInterpolatedLiteralAlignedBehindTabsLeavesTheLineThatStartsInsideAHole`; all other skeptic rows 1–17 map to the table above.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5202"
scripts/test.sh --project architecture
```

## Follow-up candidates

_None._
