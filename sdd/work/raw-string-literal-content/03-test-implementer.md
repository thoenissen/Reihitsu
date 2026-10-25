Role: test implementer
Commit: 8d5a762
Verdict: #951: reproduced; #1133: reproduced; #1146: reproduced

Inputs: thoenissen/Reihitsu#951, thoenissen/Reihitsu#1133, thoenissen/Reihitsu#1146 and their comments (GitHub MCP), `00-lead.md`, `01-spec-author.md`, `02-skeptic.md` with its Re-check, and `git diff origin/main -- sdd/ documentation/`. The prompt carried no intended fix. The #951 body names a cause and a mechanism, and the #1146 body proposes Q3/Q5/Q6 rewordings; I used neither — expected values come from the spec examples and requirement text. Production code is untouched; every statement below was observed on production code at `8d5a762` with only the test files of this handover added.

No existing test asserted the reported observed output of any of the three issues (`rg` over `src/*Test*` for `$"""` with a multi-line hole, `RH5601` with interpolated raw strings, `RH5104` with `/* a */ /* b */` or literals), so every reproduction is a new test.

Test-helper change: `src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs` gains `AssertSecondPassIsStable(input, parseOptions)` — formats once and asserts a second pass leaves that output unchanged, under LF and CRLF. It is used only where the spec leaves the exact output open (FMT-RAW-007 coverage sentence) but FMT-PIPE-004 still applies.

`→` in the tables marks a tab, `·` a space, `⏎` a line break.

## thoenissen/Reihitsu#951

All formatter tests use `AssertRuleResult(input, expected)` (`Formatter.Test/Helpers/FormatterTestsBase`), which asserts that pass 1 equals the spec's expected output **and** that pass 2 equals it too, under LF and CRLF (the maintainer's second-pass requirement). `AssertRuleResult(input)` asserts that one pass leaves already-formatted input unchanged under LF and CRLF.

### Tests

File `src/Reihitsu.Formatter.Test/Unit/RawStringAlignment/RawStringInterpolationHoleTests.cs` (new, 27 tests) unless noted.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `MisalignedLiteralMovesHoleLinesAtTheBlockColumnOnlyWithTheLiteral` (issue case 1, spec example 1) | FMT-RAW-007, FMT-INDENT-011 | `AssertRuleResult(input, expected)` | red |
| `AlignedLiteralKeepsTheColumnsOfItsHoleLines` (spec example 2) | FMT-RAW-006, FMT-RAW-007, FMT-INDENT-011 | `AssertRuleResult(input)` | red |
| `WrappedArgumentInHoleKeepsItsAlignmentAfterTheShift` (spec example 3) | FMT-RAW-007 | `AssertRuleResult(input, expected)` | green (characterization, 1B) |
| `LiteralInsideLambdaBodyMovesHoleLinesOnlyWithTheLiteral` (spec example 4, skeptic R1) | FMT-RAW-007, FMT-INDENT-011 | `AssertRuleResult(input, expected)` | red |
| `CommentStartingAHoleLineTakesTheColumnOfTheTokenBelowItAfterTheShift` (spec example 5) | FMT-RAW-007 | `AssertRuleResult(input, expected)` | red |
| `CommentInAlignedLiteralMovesToTheColumnOfTheTokenBelowIt` (skeptic R2 input) | FMT-RAW-006, FMT-RAW-007 | `AssertRuleResult(input, expected)` | red |
| `CommentInFrontOfTheClosingBraceOfAHoleIsIndentedOneLevelDeeperThanTheBrace` | FMT-RAW-007 | `AssertRuleResult(input, expected)` | red |
| `DoubleDollarLiteralMovesHoleLinesOnlyWithTheLiteral` (`{"a": {{⏎name⏎}}}`) | FMT-RAW-007; FC-016 | `AssertRuleResult(input, expected)` | red |
| `HoleClosedOnTheLineOfItsExpressionMovesOnlyWithTheLiteral` (`Hello {⏎name}`) | FMT-RAW-007 | `AssertRuleResult(input, expected)` | red |
| `AlignmentAndFormatClauseLinesMoveOnlyWithTheLiteral` (`{x⏎,10⏎:N2}`) | FMT-RAW-007; FC-001 | `AssertRuleResult(input, expected)` | red |
| `LeftwardShiftMovesHoleLinesOnlyWithTheLiteral` | FMT-RAW-007; FC-003 | `AssertRuleResult(input, expected)` | red |
| `LiteralAsArgumentMovesHoleLinesOnlyWithTheLiteral` | FMT-RAW-007, FMT-INDENT-013; FC-027 | `AssertRuleResult(input, expected)` | red |
| `LiteralInsideArrayInitializerMovesHoleLinesOnlyWithTheLiteral` | FMT-RAW-007; FC-027, FC-016 | `AssertRuleResult(input, expected)` | red |
| `WrappedArgumentInHoleOfAlignedLiteralIsRealignedToTheParenthesis` | FMT-RAW-007; FC-003 (other side of FMT-RAW-006) | `AssertRuleResult(input, expected)` | green |
| `WrappedBinaryOperatorInHoleAlignsToItsLeftOperandAfterTheShift` (FMT-INDENT-018 in a hole) | FMT-RAW-007; FC-001 | `AssertRuleResult(input, expected)` | green |
| `WrappedConditionalInHoleIsIndentedFromItsConditionAfterTheShift` (FMT-INDENT-020 in a hole) | FMT-RAW-007; FC-001 | `AssertRuleResult(input, expected)` | green |
| `TabInFrontOfHoleLineOfAlignedLiteralBecomesFourSpaces` | FMT-RAW-007 (FMT-CLEAN-002 sentence) | `AssertRuleResult(source.Replace("→", "\t"), expected)` | red |
| `AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass` (`{M(delegate { return 1; })}`, misaligned) | FMT-PIPE-004; FC-010 | `AssertSecondPassIsStable(input)` (column unspecified by FMT-RAW-007) | red |
| `BlockLambdaBrokenInsideHoleOfAlignedLiteralKeepsItsAlignment` (skeptic Question 1 (b)) | none — characterization of today's stable output the 1B answer keeps | `AssertRuleResult(input, expected)` | green |
| `NestedLiteralOpeningAHoleLineMovesWithTheOuterLiteralAndIsAligned` (`Outer {⏎"""⏎inner⏎"""}`) | FMT-RAW-007, FMT-RAW-008 | `AssertRuleResult(input, expected)` | red |
| `NestedRawStringIsAlignedInTheSamePassAsTheOuterLiteral` (issue case 2, spec example 1) | FMT-RAW-008 | `AssertRuleResult(input, expected)` | red |
| `NestedRawStringOfAlignedLiteralIsAligned` (spec example 2) | FMT-RAW-008 | `AssertRuleResult(input, expected)` | green (characterization) |
| `RawStringsNestedThreeDeepAreAlignedInOnePass` (middle literal interpolated) | FMT-RAW-008; FC-027 | `AssertRuleResult(input, expected)` | red |
| `NestedUtf8RawStringIsAlignedInTheSamePassAsTheOuterLiteral` | FMT-RAW-008; FC-016 | `AssertRuleResult(input, expected)` | red |
| `ContinuationLineOfVerbatimStringInHoleStaysAsWritten` (spec example 1) | FMT-RAW-009; FC-022 | `AssertRuleResult(input, expected)` | red |
| `FollowingLineOfCommentInHoleStaysAsWritten` (spec example 2) | FMT-RAW-009; FC-004 | `AssertRuleResult(input, expected)` | red |
| `ContinuationLineOfInterpolatedVerbatimStringInHoleStaysAsWritten` (`$@"a⏎b{name}"` in a hole) | FMT-RAW-009; FC-022 | `AssertRuleResult(input, expected)` | red |
| `RH5202RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests.VerifyFormatterAlignsMultiLineHoleAndClearsTheDiagnostic` (case 1) | FMT-RAW-007; FC-013 | `FormatterTestsBase<RH5202>.VerifyFormatter(source, fixed, diag)` | red |
| `RH5202RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests.VerifyFormatterAlignsNestedRawStringInOnePassAndClearsTheDiagnostics` (case 2, two diagnostics) | FMT-RAW-008; FC-013 | `FormatterTestsBase<RH5202>.VerifyFormatter(source, fixed, diags)` | red |

Each red test was watched failing on the spec's expected-versus-actual difference, never on a compile error or unrelated layout:

- Pass-1 mismatches at the hole lines: case 1 (`}` at 21 instead of 17 — exactly the issue's "Actual" first pass), lambda body and argument and initializer (`}` shifted twice), comment tests (`}` at 21; aligned variant: `// c`, `name`, `}` pulled to 8), `$$` (`}}}` at 22), leftward shift (`name`/`}` clamped to column 1), tab test (`name`/`}` re-indented to 8).
- Pass-2 flips ("Formatter is not idempotent"): `Hello {⏎name}`, the `,10`/`:N2}` lines, the inner `"""` opening a hole line, the anonymous method (body back at 8/12/8).
- Nesting: case 2 leaves `inner` at 21 (the issue's observation); depth 3 leaves the innermost literal at 25 instead of 24; the `u8` nest like case 2.
- FMT-RAW-009: `b"}` and `b{name}"}` moved from column 0 to 13; `  b */` moved from 2 to 15 (literal value / comment text changed).
- RH5202 parity: the analyzer step (diagnostic spans) passes, the formatter-output step fails with the same hole-line and nesting differences.

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Case 1, issue input verbatim | LF | formatter, 2 passes | reproduced | pass 1: `name`/`}` at 21/21, expected 21/17; pass 2 (trace): back to 8/8 |
| Case 1 | CRLF | formatter, `scripts/trace.sh --passes 3` | reproduced | identical to LF: IndentationPhase moves `}` 4→8, RawStringAlignmentPhase +13, pass 2 puts `name`/`}` at 8 |
| Case 2, issue input verbatim | LF | formatter | reproduced | pass 1: `inner`/`"""}` at 21, expected 24 |
| Case 2 | CRLF | formatter, `trace.sh` | reproduced | pass 1 at 21, pass 2 moves to 24 |
| Case 1 and case 2 | LF (the CRLF half of the helper is not reached) | RH5202 analyzer + formatter (counterpart) | reproduced | RH5202 reports the expected spans; formatter output differs as above |
| Sibling shapes (nesting one level deeper, `$$`, clause starters, argument, initializer, lambda body, comment lines, nested verbatim / comment) | LF | formatter | reproduced | see the red rows above |

Code-fix path: #951 names no code fix; the RH5202 fix is out of scope (`02-skeptic.md`, follow-up candidate), and the RH5202 parity tests exercise only the analyzer and the formatter.

## thoenissen/Reihitsu#1133

### Tests

Files `src/Reihitsu.Analyzer.Test/Formatting/RH5601UseTabsCorrectlyAnalyzerTests.cs` (13 new), `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5601UseTabsCorrectlyFormatterTests.cs` (1 new), `src/Reihitsu.Analyzer.Test/Formatting/RH5602CodeMustNotContainTrailingWhitespaceAnalyzerTests.cs` (1 new). Fixtures use `ExpandWhitespaceMarkers` (`→` tab, `·` space).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoDiagnosticsForTabsInTheClosingLineOfAnInterpolatedRawString` (issue input) | RH5601-002, SHARED-DIAG-003 | `Verify(source)` | red |
| `VerifyNoDiagnosticsForTabsInTheClosingLineOfAnInterpolatedRawStringWithCarriageReturnLineFeed` | RH5601-002, SHARED-DIAG-003; FC-010/FC-033 | `Verify(CRLF source)` | red |
| `VerifyNoDiagnosticsForTabsInTheClosingLineOfARawString` (`string w`) | RH5601-002, SHARED-DIAG-003 | `Verify(source)` | green (characterization) |
| `VerifyNoDiagnosticsForTabAfterTheOpeningQuotesOfAnInterpolatedRawString` (`string z`, 3A) | RH5601-002, SHARED-DIAG-003 | `Verify(source)` | red |
| `VerifyNoDiagnosticsForTabAfterTheOpeningQuotesOfARawString` (`string y`) | RH5601-002, SHARED-DIAG-003 | `Verify(source)` | green (characterization) |
| `VerifyNoDiagnosticsForTabsInTheClosingLineAfterAHoleLine` (`→→{x}` / `→→""";`) | RH5601-002, SHARED-DIAG-003; FC-016 | `Verify(source)` | red |
| `VerifyNoDiagnosticsForTabsInTheClosingLineOfADoubleDollarRawString` | RH5601-002, SHARED-DIAG-003; FC-016 | `Verify(source, MarkupMode.None)` — the markup parser reads `$$` as a position marker | red |
| `VerifyNoDiagnosticsForMixedIndentationOfAnInterpolatedRawString` (`→··a` / `→··"""`) | RH5601-002, SHARED-DIAG-003 | `Verify(source)` | red |
| `VerifyTabInAHoleLineOfAnInterpolatedRawStringIsDetectedAndFixed` (`→+ 2}`) | RH5601-001; FC-001 | `Verify(source, fixed, diag)` | green |
| `VerifyTabBehindTheClosingQuotesOfAnInterpolatedRawStringIsDetectedAndFixed` (`"""→;`) | RH5601-001; FC-001/FC-003 | `Verify(source, fixed, diag)` | green |
| `VerifyTabBehindAnInterpolatedStringIsDetectedAndFixed` (`$"a"→;`) | RH5601-001 | `Verify(source, fixed, diag)` | green |
| `VerifyTabInFrontOfTheOpeningQuotesOfAnInterpolatedRawStringIsDetectedAndFixed` (`→$"""`) | RH5601-001; FC-001/FC-003 | `Verify(source, fixed, diag)` | green |
| `VerifyCodeFixLeavesTheTabsOfAnInterpolatedRawStringUntouched` (one code tab beside the issue's literal; incremental and Fix All) | RH5601-002, SHARED-FIX-003; FC-006 | `Verify(source, fixed, diag)` | red |
| `RH5601UseTabsCorrectlyFormatterTests.VerifyFormatterKeepsTheClosingLineTabOfAnInterpolatedRawStringAndItIsNotReported` (aligned literal, closing line `16 spaces + →"""`) | RH5601-002; FC-013 | `FormatterTestsBase<RH5601>.VerifyFormatter(source)` | red |
| `RH5602...VerifyNoDiagnosticsForTrailingWhitespaceBehindAHoleInAnInterpolatedRawString` (`{1}··`) | RH5602-002 | `Verify(source)` | green (characterization) |

Red reasons, all observed: the analyzer reports the tabs in the closing line (`(5,1)`, `(5,2)`; `(7,1)`, `(7,2)` after a hole line; one for `→··"""`), the tab after `$"""` (`(3,20)`), and in the code-fix test two extra diagnostics in the closing line next to the expected one. The formatter-parity test fails at its first analyzer step (`(5,17)` reported).

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input `$"""⏎→→a⏎→→""";` | LF | analyzer | reproduced | 2 diagnostics on the closing line, expected 0 |
| Issue input | CRLF | analyzer | reproduced | same |
| Code tab beside the issue's literal | LF | code fix + Fix All | reproduced | 3 diagnostics instead of 1, so the fix would rewrite the closing line |
| Aligned literal with a tab in the closing line | LF (the CRLF half of the helper is not reached) | formatter (counterpart) | reproduced | formatter keeps it; analyzer reports it |
| Plain `"""` twin, `"""→` | LF | analyzer | not reported (characterization) | as expected |

## thoenissen/Reihitsu#1146

### Tests

File `src/Reihitsu.Analyzer.Test/Formatting/RH5104CommentsMustBeOnTheirOwnLineAnalyzerTests.cs` (23 new, 1 updated) and `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5104CommentsMustBeOnTheirOwnLineFormatterTests.cs` (1 new).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoDiagnosticsForTwoBlockCommentsOnALineWithoutCode` (Q4) | RH5104-002 | `Verify(source)` | red |
| `VerifyNoDiagnosticsForBlockCommentAndLineCommentOnALineWithoutCode` | RH5104-002 | `Verify(source)` | red |
| `VerifyNoDiagnosticsForBlockCommentBehindTheLastLineOfAMultiLineComment` | RH5104-002; FC-003 | `Verify(source)` | red |
| `VerifyNoDiagnosticsForBlockCommentBehindDelimitedDocumentationComment` | RH5104-002; FC-016 | `Verify(source)` | red |
| `VerifyNoDiagnosticsForLineCommentBehindDelimitedDocumentationComment` | RH5104-002; FC-016 | `Verify(source)` | red |
| `VerifyNoDiagnosticsForCommentsOnTheLastLineWithoutLineBreak` (`/* a */ // b`, no final line break) | RH5104-002; FC-002 | `Verify(source)` | red |
| `VerifyNoDiagnosticsForCommentOnTheLastLineWithoutLineBreak` (`// end`) | RH5104-002; FC-002 | `Verify(source)` | green (characterization) |
| `VerifyCommentsAroundCodeAreReportedWhenAnotherCommentSharesTheLine` (`/* a */ N(x); // b`) | RH5104-001; FC-003 | `Verify(markup)` analyzer only | green |
| `VerifyTwoBlockCommentsInFrontOfCodeAreReported` (`/* a */ /* b */ N(x);`) | RH5104-001; FC-003 | `Verify(markup)` analyzer only | green |
| `VerifyFixInsertsNoBlankLineAboveTheMovedComment` (spec example, 5 diagnostics) | RH5104-005; FC-008 | `Verify(source, fixed)` (incremental + Fix All) | red |
| `VerifyRelocatedCommentGetsNoBlankLineWhenPreviousLineIsCode` — **updated**, was `...GetsPrecedingBlankLine...`; expected output now has no blank line above the moved comment (J1) | RH5104-005 | `Verify(source, fixed, diag)` | red |
| `VerifyNoCodeFixIsOfferedWhenTheLineStartsInsideADelimitedDocumentationComment` | RH5104-006; FC-009 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyNoCodeFixIsOfferedWhenTheLineStartsInsideAVerbatimString` | RH5104-006; FC-022 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyNoCodeFixIsOfferedWhenTheLineStartsInsideARawString` | RH5104-006; FC-022 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyNoCodeFixIsOfferedWhenTheLineStartsInsideAnInterpolatedRawString` | RH5104-006; FC-022 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyNoCodeFixIsOfferedWhenTheLineStartsInsideAUtf8RawString` | RH5104-006; FC-022 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyNoCodeFixIsOfferedWhenTheLineStartsAtAHoleOfAnInterpolatedVerbatimString` (`$@"a⏎{x}b"`, R4) | RH5104-006; FC-003 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyNoCodeFixIsOfferedWhenTheLineStartsAtAHoleOfAVerbatimInterpolatedString` (`@$"a⏎{x}b"`, R4) | RH5104-006; FC-003 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyNoCodeFixIsOfferedWhenTheLineStartsInTheTextOfAnInterpolatedVerbatimString` (`$@"a⏎b{x}"`) | RH5104-006; FC-003 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyNoCodeFixIsOfferedWhenTheLineStartsInsideAHoleThatSpansLines` (`$"{⏎x}"`, 4B) | RH5104-006 | `VerifyNoCodeFixIsOffered` | red |
| `VerifyNoCodeFixIsOfferedWhenTheLineStartsInsideABlockComment` (`/* a⏎   b */ N(x); // c`: both reported; no action for `// c`, an action for `/* a…b */`) | RH5104-006; FC-003 | `Verify(markup)` + `GetCodeFixActionsAsync` per location | red |
| `VerifyCommentOnTheLineWhereAVerbatimStringStartsIsMoved` (spec example) | RH5104-006; FC-003 | `Verify(source, fixed)` | green |
| `VerifyCommentOnTheLineWhereAHoleThatSpansLinesStartsIsMoved` | RH5104-006; FC-003 | `Verify(source, fixed)` | green |
| `VerifyFixAllMovesFixableCommentAndLeavesWithheldCommentReported` (withheld `/* c */` in a verbatim string + fixable `// d`) | RH5104-006; FC-008 | `Verify(source, fixed)` with the withheld diagnostic in the fixed markup | red |
| `RH5104CommentsMustBeOnTheirOwnLineFormatterTests.VerifyFormatterKeepsTwoCommentsOnALineWithoutCodeAndTheyAreNotReported` | RH5104-002; FC-013 | `FormatterTestsBase<RH5104>.VerifyFormatter(source)` | red |

Red reasons, all observed: Q4 shapes report 1 or 2 diagnostics where 0 are expected; the RH5104-005 tests show the inserted blank line (`+` empty line above `// second`, above `// Compare the values.`); every withheld form registers exactly 1 action ("No code fix may be offered … actual count: 1"); the Fix All test shows `/* c */` written into the verbatim string (`@"a⏎⏎/* c */⏎b" + x;`), which changes its value.

`VerifyFixAllMovesFixableCommentAndLeavesWithheldCommentReported` relies on the Roslyn verifier skipping a diagnostic that has no action and on its default, inexact iteration count; its passing state cannot be watched before the implementation.

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Q1: `@"a⏎b" + /* c */ x;` and siblings (raw, `$"""`, `u8`, `$@"`, `@$"`, `$"{⏎x}"`, `/** */`, `/* */`) | LF | code fix | reproduced | 1 action offered, expected none |
| Q1 with Fix All beside a fixable comment | LF | Fix All | reproduced | comment written into the literal |
| Q4: `/* a */ /* b */`, `/* c */ // d` and siblings | LF | analyzer | reproduced | reported, expected not |
| Q4 | LF (the CRLF half of the helper is not reached) | formatter (counterpart) | reproduced | formatter keeps the line (also traced: no phase moves those comments); analyzer reports it |
| J1: blank line above the moved comment | LF | code fix + Fix All | reproduced | blank line inserted, expected none |

The analyzer tests run LF only, as the existing RH5104 suite does; the fix's CRLF handling is covered by the existing `VerifyInsertedLineBreakUsesDetectedCarriageReturnLineFeedEndOfLine`, and the withheld guard decides on positions, not on line-break characters. Not run on CRLF: the RH5104 analyzer and code-fix reproductions.

## Whole run

### Not covered, and why

- **FC-019, FC-020 (directives in the gap) for #951:** a directive inside an interpolation hole does not parse, and the formatter leaves files with syntax errors alone (`02-skeptic.md`, "Not candidates"). For #1146, the directive exemption is unchanged by this run and covered by `VerifyCommentBehindConditionalDirectiveDoesNotProduceDiagnostics` and `VerifyRelocatedCommentBelowDirectiveGetsNoBlankLine`.
- **FMT-RAW-007, hole lines indented less than a leftward shift:** unspecified by its coverage sentence; no test asserts a column for them. The leftward-shift test keeps every hole line right of column 0.
- **FMT-RAW-007, the column of a line an earlier phase breaks inside a hole:** unspecified; only stability is asserted (`AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass`, FMT-PIPE-004). `BlockLambdaBrokenInsideHoleOfAlignedLiteralKeepsItsAlignment` pins today's output only because the 1B answer keeps it.
- **FMT-INDENT-003 and FMT-INDENT-004 scope-depth lines inside a hole (skeptic R3):** not tested separately; their only route into a hole is the body of an anonymous method or lambda, which `LineBreakPhase` lays out first, so their column is the unspecified one above. The anonymous-method stability test exercises that body.
- **Formatter tab-indented raw strings:** out of scope (thoenissen/Reihitsu#1432). The RH5601 formatter-parity fixture uses spaces before the tab so the raw-string phase sees an aligned literal.
- **FC-007:** the RH5104 fix after J1 raises RH5020 by design (RH5104-005 Why, `02-skeptic.md`); RH5601's fix is single-pass and unchanged.
- **FC-018, FC-023, FC-024, FC-025, FC-026, FC-029, FC-036:** no rename, identifier, partial/explicit member, expression body, repeated-token edit, or key/value layout is touched by these changes.
- **FC-028:** no code fix of this run formats through `FormatNode`/`FormatNodeInDocumentAsync`.
- **FC-030, FC-031, FC-032, FC-035, FC-037:** concern the implementation, its comments, and later evidence; expected values here come from the spec examples or, for characterization tests, from the requirement's wording, never from patched code.
- **Follow-up candidates already listed by the spec author and skeptic** (RH5202 fix shifts nested verbatim text; RH5104 reordering for `M(); /* a */ // b` and `/* a */ /* b⏎ c */ M();`): not codified, as the handovers require.

### Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.RawStringAlignment.RawStringInterpolationHoleTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5202RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5601UseTabsCorrectly|FullyQualifiedName~RH5602CodeMustNotContainTrailingWhitespaceAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5104CommentsMustBeOnTheirOwnLine"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests"
```

Results on `8d5a762` plus these tests:

- Formatter project, full run: 2710 tests, 20 failed at the time of that run — exactly the red rows of `RawStringInterpolationHoleTests`, before the initializer test was added (now 21 red of 27); `SelfHostingTests` and every other formatter test pass.
- Analyzer filter over RH5104, RH5601, RH5602, and the RH5202 formatter tests: 110 tests, 30 failed — exactly the red rows above.
- Architecture project: 84 passed, including `EveryRequirementOfAnEnforcedSpecIsCovered` (FMT-RAW-007, FMT-RAW-008, FMT-RAW-009, RH5104-006 now covered).

`scripts/format.sh` over all changed test files reports no further change.

# Repair round 1

Role: test implementer
Commit: 4c985a2
Verdict: #1146: test repaired (harness configuration); #951, #1133: nothing to repair

Input: the "Disputed tests or scope" entry of `04-implementer.md` for thoenissen/Reihitsu#1146. I reproduced it myself instead of relying on the probe it describes.

## thoenissen/Reihitsu#1146

- **Reproduced the dispute.** On `4c985a2`, `VerifyFixAllMovesFixableCommentAndLeavesWithheldCommentReported` fails with `Context: Diagnostics of fixed state — Mismatch between number of diagnostics returned, expected "0" actual "1"`, `WithSpan(6, 6, 6, 13)`. That span is the withheld `/* c */`, and the expected fixed text (`{|RH5104:/* c */|}`) requires it to stay reported. The verifier's default fixed-state markup handling ignores markup for an ID the provider lists as fixable, so the fixed markup never became an expected diagnostic. This is a fault in the harness configuration, not in the assertion.
- **Repair.** The call is now `Verify(testData, fixedData, static test => test.FixedState.MarkupHandling = MarkupMode.Allow)`, with `using Microsoft.CodeAnalysis.Testing;`. That is the pattern of `RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzerTests` and `RH4107ProtectedFieldCasingAnalyzerTests`. The input, the expected fixed text, and the withheld diagnostic are unchanged: the fixed state must still hold exactly that one RH5104 diagnostic, so a fix that is offered, or a comment that stops being reported, still fails the test. `BatchFixedState` is not set; with no `BatchFixedCode`, the Fix All check uses the fixed state.
- **Still falsifiable.** I ran the repaired test against the production code of `493f956` (before the implementation, in a temporary worktree outside the repository, since removed). It is red for the original reason: `Iterative code fix application … did not match`, with `/* c */` written into the verbatim string (`@"a⏎⏎/* c */⏎b" + x;`).
- **Passes on the implementation.** On `4c985a2` plus this repair: `RH5104CommentsMustBeOnTheirOwnLineAnalyzerTests` 45/45.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixAllMovesFixableCommentAndLeavesWithheldCommentReported` | RH5104-006; FC-008 | `Verify(source, fixed, FixedState.MarkupHandling = Allow)` | green on `4c985a2`; red on `493f956` |

Release configuration does not build on `4c985a2`, because of `RH5104CommentsMustBeOnTheirOwnLineAnalyzer.cs(92,35): error S3267`. That is the cleaner's concern, so I ran these checks with `--configuration Debug`.

## Commands

```shell
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~RH5104CommentsMustBeOnTheirOwnLineAnalyzerTests"
```

# Repair round 1 — auditor findings

Role: test implementer
Commit: 193b3a5
Verdict: #951: findings 1–5 reproduced; #1133: nothing to do (no finding); #1146: finding 6 / RH5104-007 reproduced

Inputs: `07-auditor.md` (findings 1–7), the "Repair round 1" section of `00-lead.md`, and RH5104-007 with the RH5104-004 change in `sdd/specs/analyzer/RH5104.md` (193b3a5). I took the inputs from the auditor's counterexamples. I took every expected value from the spec, not from the auditor's "suggested change". Finding 7 is a naming and side-effect concern of `LayoutModel`, so no output test can show it. All runs used `--configuration Debug`, on production code at 193b3a5, which is the implementation of f50a0ba plus 924eaa0.

## thoenissen/Reihitsu#951

| Test | Finding | Requirement / failure class | Helper | Expected now | Observed vs expected (LF, pass 1) |
|---|---|---|---|---|---|
| `RawStringInterpolationHoleTests.ArgumentAnchoredOnTheClosingLineOfANestedLiteralKeepsItsAlignment` | 1(a) | FMT-RAW-007, FMT-RAW-008; FC-012, FC-027 | `AssertRuleResult(input, expected)` | red | `"b"` at 41, expected 44 under `"a"` (outer +13, inner +3) |
| `RawStringInterpolationHoleTests.ArgumentAnchoredOnTheContinuationLineOfAVerbatimStringKeepsItsAlignment` | 1(b) | FMT-RAW-007, FMT-RAW-009; FC-012, FC-022 | `AssertRuleResult(input, expected)` | red | `"b"` at 32, expected 19 under `"a"` on the kept `b"` line |
| `RawStringInterpolationHoleTests.CommentAboveTabIndentedHoleLineTakesItsColumnAfterTheTabReplacement` | 2, comment | FMT-RAW-007; FC-003, FC-012 | `AssertRuleResult(source with tabs, expected)` | red | `// c` at 18, expected 21, the column of `name` after FMT-CLEAN-002 |
| `RawStringInterpolationHoleTests.ArgumentAnchoredOnATabIndentedHoleLineAlignsAfterTheTabReplacement` | 2, anchor | FMT-RAW-007; FC-003, FC-012 | `AssertRuleResult(source with tabs, expected)` | red | `"b"` at 32, expected 35 under `"a"` after the tab became four spaces |
| `RawStringInterpolationHoleTests.DocumentationCommentLinesInHoleMoveWithTheTokenBelowThem` | 3 | FMT-RAW-007; FC-001 | `AssertRuleResult(input, expected)` | red | `/// second` left at 8, expected 21 with `/// first` and `name` |
| `RawStringInterpolationHoleTests.ConstraintClauseInHoleOfAlignedLiteralIsIndentedFromItsDeclaration` | 4, aligned | FMT-RAW-007, FMT-INDENT-017; FC-015, FC-016 | `AssertRuleResult(input, expected)` | red | `where` at 16 (scope depth), expected 25, one level deeper than `void` at 21 |
| `RawStringInterpolationHoleTests.ConstraintClauseInHoleOfMisalignedLiteralIsIndentedFromItsDeclarationAfterTheShift` | 4, misaligned by 13 | FMT-RAW-007, FMT-INDENT-017; FC-015, FC-016 | `AssertRuleResult(input, expected)` | red | `where` at 29, expected 25 |
| `RawStringAlignmentPipelineTests.ChainLinkBehindSecondRawStringOnASharedLineAlignsAfterBothLiteralsMoved` | 5 | FMT-INDENT-029, FMT-INDENT-030; FC-015 | `AssertRuleResult(input, expected)` | red | `.ToString()` at 13, expected 24 under `.Trim` (SHARED-CHAIN-007 anchor), after the first literal moved by 11 and the second by 17 |

Every test asserts pass 1 equal to the expected output and pass 2 equal to it as well, under both LF and CRLF, through `AssertRuleResult`. Each one fails at the LF pass-1 assertion, so its CRLF half has not been watched failing. The auditor reports these mechanisms as independent of the line ending.

Finding 5 is not inside a hole. The auditor reports that `origin/main` also needs three passes for that input, for a different reason. I did not re-run `origin/main`.

## thoenissen/Reihitsu#1133

No finding, so nothing changed.

## thoenissen/Reihitsu#1146

All in `RH5104CommentsMustBeOnTheirOwnLineAnalyzerTests`:

| Test | Requirement / failure class | Helper | Expected now | Observed vs expected |
|---|---|---|---|---|
| `VerifyCodeFixIsOfferedOnlyForTheFirstCommentOfALine` (both shapes of the RH5104-007 example: `M(); /* a */ // b` and `/* c */ /* d⏎   e */ M();`) | RH5104-007; FC-004, FC-009 | `Verify(markup)` + `GetCodeFixActionsAsync` per diagnostic | red | All three are reported, as expected. `// b` gets 1 action, expected 0. The test also asserts that `/* a */` keeps an action and that `/* d…e */` gets 0 |
| `VerifyNoCodeFixIsOfferedForACommentBehindAnUnreportedCommentOnItsLine` (`/* c */ /* d⏎   e */ M();` alone) | RH5104-007; FC-009 | `VerifyNoCodeFixIsOffered` | red | 1 action offered, expected none |
| `VerifyFixAllKeepsTheOrderOfCommentsThatShareALine` (full example, incremental and Fix All, 2 iterations each) | RH5104-007; FC-004, FC-008 | `Verify(source, fixed, FixedState.MarkupHandling = Allow, NumberOfIncrementalIterations = 2, NumberOfFixAllIterations = 2)` | red | `/* d⏎   e */` is moved above `/* c */`, which reorders the comments. Expected: it stays reported in place, and the result is `/* a */`⏎`// b`⏎`M();` |
| `VerifyCommentIsMovedOnceTheCommentBeforeItHasLeftItsLine` (the example's intermediate state) | RH5104-007; FC-003 | `Verify(source, fixed, FixedState.MarkupHandling = Allow)` | red | `// b` lands between `/* a */` and `M();` as expected. The test is red only because `/* d⏎   e */` is still moved above `/* c */` |

The iteration counts are exact: the fix for `// b` exists only after `/* a */` has left the line. That makes one pass for `/* a */` and one for `// b`, both one by one and under Fix All. Whether the counts hold can only be seen once the fix exists.

## Whole run

```shell
scripts/test.sh --project formatter --configuration Debug --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.RawStringAlignment"
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~RH5104CommentsMustBeOnTheirOwnLineAnalyzerTests"
scripts/test.sh --project architecture --configuration Debug
```

- Formatter `Unit.RawStringAlignment`: 51 tests, 8 red. The red ones are exactly the 8 above; every earlier test passes.
- RH5104 analyzer tests: 49 tests, 4 red. The red ones are exactly the 4 above.
- Architecture: 84/84, including coverage of RH5104-007.
- `scripts/format.sh` was run over the three changed test files.

## Follow-on: auditor hint 1 (Commit: 78766ed)

The input is the auditor's hint 1, which `04-implementer.md` reports as fixed with no test pinning it. The literal is not in a hole. A wrapped argument is anchored behind its closing quotes:

`return M("""⏎    a⏎    """ + string.Concat("x",⏎                        "y"));`

The expected output follows from the specs:

- FMT-RAW-001 and FMT-RAW-005 move the literal 13 columns right, to the opening quotes at column 17. The closing line moves with it.
- So `string.Concat(`'s `"x"` ends at column 37.
- FMT-INDENT-012 aligns `"y"` to the first argument `"x"`. FMT-INDENT-030 measures that anchor at its final column, which gives column 37. FMT-INDENT-013 leaves that line to the nested argument list.
- FMT-PIPE-004 requires that a second pass keeps this output.

| Test | Requirement / failure class | Helper | Expected now | Observed |
|---|---|---|---|---|
| `RawStringAlignmentPipelineTests.WrappedArgumentAnchoredBehindTheClosingQuotesOfARawStringAlignsAfterTheLiteralMoved` | FMT-INDENT-012, FMT-INDENT-030; FC-012 | `AssertRuleResult(input, expected)`: two passes, LF and CRLF | green on 78766ed | **78766ed:** passes, both passes, LF and CRLF. **193b3a5:** red, with `"y"` at 24 instead of 37 in LF pass 1. I ran that check with this test file in a temporary worktree outside the repository, then removed it. |

```shell
scripts/test.sh --project formatter --configuration Debug --filter "FullyQualifiedName~RawStringAlignmentPipelineTests.WrappedArgumentAnchoredBehindTheClosingQuotesOfARawStringAlignsAfterTheLiteralMoved"
```

# Repair round 2

Role: test implementer
Commit: d50b936
Verdict: #951: findings 2 (narrowed) and 8 reproduced; #1133: nothing to do; #1146: nothing to do

Inputs: the "Repair round 1" section of `07-auditor.md` (findings 2 and 8) and the "Repair round 2" worklist in `00-lead.md`. The inputs are the auditor's counterexamples. The expected values come from FMT-RAW-007, FMT-RAW-008, and FMT-CLEAN-002. A hole line that no construct inside the hole places is not re-indented. Its tabs become four spaces each, so the line's tokens end at that column, with the tab counted as four. A wrapped argument aligns to its anchor at that final column (FMT-INDENT-012, FMT-INDENT-030). A nested literal aligns under its opening quotes at their final column. FMT-PIPE-004 requires a second pass to keep the result.

I ran the tests on the production code at d50b936, which is unchanged since f7ca92a, with `--configuration Debug`.

## thoenissen/Reihitsu#951

All three are in `RawStringInterpolationHoleTests`. Each uses `AssertRuleResult(source.Replace("→", "\t"), expected)`, which checks pass 1 and pass 2 under LF and CRLF.

| Test | Finding | Requirement / failure class | Expected now | Observed vs expected (LF, pass 1) |
|---|---|---|---|---|
| `ArgumentAnchoredBehindATabIndentedCommentOnAHoleLineAlignsAfterTheTabReplacement`. Aligned literal at 17; hole line `→/* c */ string.Concat("a",` after 17 spaces; `"b")}` after 17 spaces | 2 (narrowed) | FMT-RAW-007; FC-001, FC-003, FC-012 | red | `"b"` at 40. Expected 43: the tab becomes four spaces, so `/* c */` is at 21 and `"a"` at 43 |
| `NestedLiteralOpeningATabIndentedHoleLineOfAlignedLiteralIsAlignedAfterTheTabReplacement`. Aligned outer literal at 17; hole line `→"""` after 17 spaces | 8, aligned | FMT-RAW-007, FMT-RAW-008; FC-012, FC-016 | red | `inner` and `"""}` at 18. Expected 21, under the opening quotes after the tab became four spaces |
| `NestedLiteralOpeningATabIndentedHoleLineOfMisalignedLiteralIsAlignedAfterTheTabReplacement`. Outer literal misaligned by 13; hole line `→"""` after 4 spaces | 8, misaligned | FMT-RAW-007, FMT-RAW-008; FC-012, FC-016 | red | `inner` and `"""}` at 18. Expected 21: the quotes move by 13 from column 8, with the tab counted as four |

Each test fails at the LF pass-1 assertion, so I have not watched its CRLF half fail. The mechanism does not depend on the line ending (`07-auditor.md`). Findings 9 (stale XML summaries) and hint A (a shared tab helper) have no output behavior that a test could pin.

## Whole run

```shell
scripts/test.sh --project formatter --configuration Debug --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.RawStringAlignment"
```

- 55 tests, 3 red, exactly the three above. Every earlier test passes.
- `scripts/format.sh` reports no change to the test file.
