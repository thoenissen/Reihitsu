Role: test implementer
Commit: 083eb2a
Verdict: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict. All tests are in `src/Reihitsu.Analyzer.Test/Formatting/RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests.cs` (class `RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests`, `BatchCodeFixTestsBase`). Expected values are the spec examples of RH5202-004/005 and the skeptic's fixtures with the shift computed from RH5202-003; none was taken from code output. The four formatter-parity tests compare against `Format(...)`, and the formatter's output there equals the spec examples exactly (visible in their failure messages).

Characterization search: `rg RH5202 src` found only the existing analyzer/code-fix tests and the formatter parity class; none asserted the reported observed output, so every reproduction is new.

## Tests

`Verify(source, fixedSource, …)` runs the single fix iteratively and Fix All, and re-analyzes the result. Under that helper every red test below fails on the reported difference only: the line that starts inside nested text (or the empty line) is shifted by the delta.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| VerifyFixLeavesContinuationLineOfVerbatimStringInHoleWhenShiftingRight | RH5202-004 (issue input 1, A1) | Verify | red: `b"}` written at col 13, expected col 0 |
| …WhenShiftingRightWithCrLf | RH5202-004; FC-033 | Verify + NormalizeToCarriageReturnLineFeed | red, same difference |
| VerifyFixLeavesContinuationLineOfVerbatimStringInHoleWhenShiftingLeft | RH5202-004 (A3); FC-003 | Verify | red: `b"}` moved col 18 → 10 |
| …WhenShiftingLeftWithCrLf | RH5202-004; FC-033 | Verify | red, same |
| VerifyFixLeavesFollowingLineOfTrailingCommentInHoleAndShiftsClosingBraceLine | RH5202-004 (issue input 2, A2; `}` line is the "ended" side) | Verify | red: `  b */` moved col 2 → 15 |
| …ShiftsClosingBraceLineWithCrLf | RH5202-004; FC-033 | Verify | red, same |
| VerifyFixLeavesFollowingLineOfCommentInHoleWhenShiftingLeft | RH5202-004 (A4) | Verify | red: `b */` col 18 → 10 |
| VerifyFixLeavesEmptyLineInsideVerbatimStringInHoleEmpty | RH5202-004 (B2) | Verify | red: 13 spaces written into the verbatim string |
| …EmptyWithCrLf | RH5202-004; FC-033 | Verify | red, same |
| VerifyFixLeavesContinuationLineOfInterpolatedVerbatimStringInHole | RH5202-004 (`$@`, A5, line holds a hole) | Verify | red |
| VerifyFixLeavesContinuationLineOfInterpolatedVerbatimStringWrittenAtDollarInHole | RH5202-004 (`@$`, B4) | Verify | red |
| VerifyFixLeavesContinuationLineOfUtf8VerbatimStringInHole | RH5202-004 (`@"…"u8`, B3; written `.Length` so the hole compiles) | Verify | red |
| VerifyFixLeavesLinesOfVerbatimStringInHoleOfNestedInterpolatedVerbatimString | RH5202-004 (A8, two levels); FC-027 | Verify | red: both `b"}` and `y"}` shifted |
| VerifyFixLeavesContinuationLineOfVerbatimStringInMultiLineHoleOfNestedRegularInterpolatedString | RH5202-004 (B9; also the "begins" and "ended" sides inside the nested hole) | Verify | red: only `b"` is wrong; `@"a` and `}"}` lines shift as expected |
| VerifyFixLeavesContinuationLineOfVerbatimStringArgumentInHole | RH5202-004 (B7) | Verify | red |
| VerifyFixLeavesFollowingLineOfLeadingCommentInHole | RH5202-004 (B1; the `/* a` line is the "begins" side) | Verify | red |
| VerifyFixLeavesFollowingLineOfDocumentationStyleCommentInHole | RH5202-004 (`/** */`, B8) | Verify | red |
| VerifyFixLeavesLineHoldingEndOfCommentAndClosingBraceOfHole | RH5202-004 (B6, `b */}`) | Verify | red |
| VerifyFixLeavesContinuationLineOfVerbatimStringInHoleOfDoubleDollarLiteral | RH5202-004 (A9, `$$`) | Verify with `MarkupMode.None` (the markup parser reads `$$` as a caret, as in the existing `$$` test) | red |
| VerifyFixShiftsContentLineAfterVerbatimStringInHoleHasEnded | RH5202-004 ("starts inside the text", other side) | Verify | red only on `b"} tail` (the verbatim line); the `next` line already shifts |
| VerifyFixShiftsLineAfterSingleLineCommentInHole | RH5202-004 (other side: `//` comment ended at the line break) | Verify | green |
| VerifyFixShiftsLineAfterSingleLineVerbatimStringInHole | RH5202-004 (other side: verbatim ended on the previous line) | Verify | green |
| VerifyFixShiftsLinesOfRawStringNestedInHoleAndNestedLiteralStaysReported | RH5202-004 (nested raw string example; "other than a raw string" side) | Verify (2 diagnostics) + ApplyCodeFixAsync (one single fix, exact text) + Verify (1 diagnostic remains) | green |
| VerifyFixLeavesNestedTextAsWrittenForSeveralLiteralsInOneDocument | RH5202-004; FC-008 (Fix All over two literals, right and left shift) | Verify, 2 diagnostics | red |
| VerifyFixMatchesFormatterForVerbatimStringInHole | RH5202-004 (result is the formatter's layout); FC-013 | VerifyCodeFixMatchesFormatter (right and left) | red |
| VerifyFixMatchesFormatterForCommentInHole | RH5202-004; FC-013 | VerifyCodeFixMatchesFormatter | red |
| VerifyFixMatchesFormatterForEmptyLineInsideVerbatimStringInHole | RH5202-004; FC-013 | VerifyCodeFixMatchesFormatter | red |
| VerifyFixLeavesEmptyContentLineEmpty | RH5202-005 (example 1, B5) | Verify | red: 12 spaces on the empty line |
| VerifyFixLeavesEmptyContentLineEmptyWithCrLf | RH5202-005; FC-033 | Verify | red, same |
| VerifyFixLeavesEmptyContentLineOfInterpolatedLiteralEmpty | RH5202-005 (example 2) | Verify | red: 13 spaces |
| VerifyFixLeavesEmptyContentLineOfInterpolatedLiteralEmptyWithCrLf | RH5202-005; FC-033 | Verify | red, same |
| VerifyFixLeavesEmptyContentLineEmptyWhenShiftingLeft | RH5202-005; FC-003 (shift direction) | Verify | green |
| VerifyFixShiftsWhitespaceOnlyContentLine | RH5202-005 ("only whitespace is not empty", 6 → 18 spaces) | Verify + ExpandWhitespaceMarkers | green |
| VerifyFixMatchesFormatterForEmptyContentLine | RH5202-005; FC-013 (plain and interpolated) | VerifyCodeFixMatchesFormatter | red |

Existing tests keep their RH5202-001/002/003 tags and pass. Run on 083eb2a plus these tests: 29 red, 20 green (15 existing + the 5 green above). `RequirementCoverageTests` and the whole architecture project pass (115/115).

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input 1, verbatim `@"a⏎b"` in a hole | LF | code fix (single + Fix All) | reproduced | `b"}` at col 13 vs col 0 |
| Issue input 1 | CRLF | code fix | reproduced | same |
| Issue input 2, `/* a⏎  b */` in a hole | LF | code fix | reproduced | `  b */` at col 15 vs col 2 |
| Issue input 2 | CRLF | code fix | reproduced | same |
| Issue inputs 1 and 2 | LF | formatter (`Format` inside VerifyCodeFixMatchesFormatter) | holds | formatter output equals the spec examples; only the fix differs |
| Leftward shift (A3, A4) | LF, CRLF (A3) | code fix | reproduced | continuation lines moved left by 8 |
| Deeper nesting (A8, B9, B7), `$$`, `$@`, `@$`, `u8`, `/** */`, leading comment, `*/}` line | LF | code fix | reproduced | each nested-text line shifted by the delta |
| Empty content line (B5), plain and interpolated | LF, CRLF | code fix | reproduced | 12 / 13 spaces written onto the empty line |
| Nested raw string in a hole | LF | code fix | holds | outer fix shifts inner lines; inner literal stays reported |

## Not covered, and why

- FC-019/FC-020/FC-005 (comment and directive in a gap): the comment arms are covered (trailing, leading, `/** */`, `//`). A directive or disabled text inside a hole is a parse error (skeptic A7), so the analyzer reports nothing and no fix is reached.
- FC-007 (one-pass convergence): `Verify` re-analyzes the fixed state; `VerifyCodeFixMatchesFormatter` applies exactly one fix and then asserts no diagnostic. The nested-raw case keeps its second diagnostic on purpose (RH5202-004).
- FC-009 (action that cannot change the document): no diagnostic is reported on an aligned literal (RH5202-001 tests), so no no-op action is offered.
- FC-010/FC-012 (formatter idempotency, phase interaction): the formatter does not change in this run; its FMT-RAW-009 tests stay.
- FC-018, FC-023, FC-024, FC-025, FC-029, FC-036: no renaming, identifiers, member kinds, or key/value layout involved.
- FC-030 (fix formats more than its node): the B7 test pins the fix's exact output, and that output keeps the `string.Concat("x", @"a` argument list unwrapped where whole-document formatting wraps it (skeptic, B7), so a route that runs the full pipeline fails that test.
- Empty line inside a hole that is not inside nested text (`name +⏎⏎ 1`): unspecified (spec author's internal choice; RH5202-005 covers content lines only), so no test.
- Comment-looking text inside a regular string in a hole (FC-022): a regular string cannot span lines, so it cannot hold a line start.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5202RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests"
```
