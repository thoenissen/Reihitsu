Role: test implementer
Commit: 0e707b3 (tests written on top of it; see the commit carrying this file)
Verdict: #1695: reproduced; #1696: tests written (behavior change, reproduced); #1707: reproduced (RH3003), not reproduced (RH3005, characterization tests); #1721: no scenario (Decision 6, nothing written)

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from FMT-PIPE-010 (as changed in ec7c0c6/0c3d3d9), RH8402-002/005/006, SHARED-FIX-006/008, RH3003-004 and RH3005-004, never from a patched copy of the code. Every new test was run on the unchanged code; every test listed as "fails now" fails on the issue's own expected-versus-actual difference (the foreign line break or the trailing whitespace at the end of the target's last line, the header's line ending, or the CRLF→LF drift in front of the replaced literal), nothing else.

## thoenissen/Reihitsu#1695

New file `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeTrailingLineEndTests.cs` (exact text, line endings included, via `AdhocWorkspace`; every case also asserts that a second pass with the same target changes nothing, except T7, whose second pass is not part of the requirement). Code-fix tests added to the RH3202, RH7101, and RH5111 test classes, one per write-back site: RH3202 → `FormatNodeInDocumentAsync` (`WriteBackTrailingTrivia`), RH5111 enum member → `FormatNodeInDocumentWithContextAsync` general path, RH7101 → the split-field path.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ReihitsuFormatterNodeTrailingLineEndTests.LineFeedBehindTheTargetInACarriageReturnLineFeedDocumentIsWrittenAsCarriageReturnLineFeed` (spec example, T1) | FMT-PIPE-010 | exact text, `FormatNodeInDocumentAsync` | fails (`}\n`) |
| `...LoneCarriageReturnBehindTheTargetIsWrittenAsLineFeed` (spec prose, T2) | FMT-PIPE-010 | same | fails (`}\r`) |
| `...LineFeedBehindACommentBehindTheTargetIsWrittenAsCarriageReturnLineFeed` (T3) | FMT-PIPE-010 | same | fails (`} // c\n`) |
| `...CarriageReturnLineFeedBehindTheTargetInALineFeedDocumentIsWrittenAsLineFeed` (T6) | FMT-PIPE-010 | same | fails (`}\r\n`) |
| `...LineBreakInsideACommentBehindTheTargetKeepsItsCharacters` (`/* a\n b */` kept, ending `\r\n`) | FMT-PIPE-010 | same | fails (ending `\n`) |
| `...TrailingTriviaWithoutALineBreakIsKeptAsItStands` (T7, other side) | FMT-PIPE-010 | same | passes |
| `...WhitespaceBehindADelimitedCommentBehindAConvertedMethodIsRemoved` (spec example, W4; LF+CRLF) | FMT-PIPE-010 | same | fails (`} /* c */···`) |
| `...WhitespaceBehindAConvertedMethodIsRemoved` (W1; LF+CRLF) | FMT-PIPE-010 | same | fails (`}···`) |
| `...WhitespaceBehindAKeptLastTokenIsRemoved` (W5; LF+CRLF) | FMT-PIPE-010 | same | fails (`}···`) |
| `...WhitespaceBehindALastLineFormattingLeavesUnchangedIsRemoved` (already laid-out `P`, tab + spaces; LF+CRLF) | FMT-PIPE-010 | same | fails (`}\t··`) |
| `...WhitespaceInFrontOfACommentBehindTheTargetStays` (W2, other side; LF+CRLF) | FMT-PIPE-010 | same | passes |
| `...WhitespaceInsideASingleLineCommentBehindTheTargetStays` (W3, other side; LF+CRLF) | FMT-PIPE-010 | same | passes |
| `...WhitespaceInFrontOfAForeignLineBreakIsRemovedAndTheLineBreakIsWrittenAsDetected` (both parts together) | FMT-PIPE-010 | same | fails (`}···\n`) |
| `...WithContextLineFeedBehindTheTargetInACarriageReturnLineFeedDocumentIsWrittenAsCarriageReturnLineFeed` (T4) | FMT-PIPE-010 | exact text, `FormatNodeInDocumentWithContextAsync` general path | fails |
| `...WithContextLoneCarriageReturnBehindTheTargetIsWrittenAsLineFeed` | FMT-PIPE-010 | same | fails |
| `...WithContextWhitespaceBehindTheTargetIsRemoved` (LF+CRLF) | FMT-PIPE-010 | same | fails |
| `...SplitFieldLineFeedBehindTheTargetInACarriageReturnLineFeedDocumentIsWrittenAsCarriageReturnLineFeed` (T5) | FMT-PIPE-010 | exact text, split-field path | fails (`_y;\n`) |
| `...SplitFieldLoneCarriageReturnBehindTheTargetIsWrittenAsLineFeed` | FMT-PIPE-010 | same | fails (`_y;\r`) |
| `...SplitFieldWhitespaceBehindTheTargetIsRemoved` (LF+CRLF) | FMT-PIPE-010 | same | fails |
| `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.FixRemovesWhitespaceBehindTheConvertedMethod` (LF, CRLF) | FMT-PIPE-010 | `VerifyCodeFixWithEndOfLine` | fails |
| `RH3202...FixRemovesWhitespaceBehindADelimitedCommentBehindTheConvertedMethod` (change doc example; LF, CRLF) | FMT-PIPE-010 | `VerifyCodeFixWithEndOfLine` | fails |
| `RH3202...FixWritesTheDetectedLineEndingBehindTheConvertedMethodInAMixedDocument` | FMT-PIPE-010, SHARED-FIX-008 | `ApplyCodeFixAsync`, exact text | fails |
| `RH3202...FixWritesTheDetectedLineEndingBehindTheConvertedMethodInALoneCarriageReturnDocument` | FMT-PIPE-010, SHARED-FIX-008 | `ApplyCodeFixAsync`, exact text | fails |
| `RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.FixWritesTheDetectedLineEndingBehindTheLastEnumMemberInAMixedDocument` | FMT-PIPE-010, SHARED-FIX-008 | `ApplyCodeFixAsync`, exact text | fails |
| `RH5111...FixRemovesWhitespaceBehindTheLastEnumMember` (LF, CRLF) | FMT-PIPE-010 | `VerifyCodeFixWithEndOfLine` | fails |
| `RH7101DoNotCombineFieldsAnalyzerTests.FixWritesTheDetectedLineEndingBehindTheLastFieldInAMixedDocument` | FMT-PIPE-010, SHARED-FIX-008 | `ApplyCodeFixAsync`, exact text | fails |
| `RH7101...FixRemovesWhitespaceBehindTheLastField` (LF, CRLF) | FMT-PIPE-010 | `VerifyCodeFixWithEndOfLine` | fails |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `}···` behind RH3202 target (issue's example) | LF, CRLF | code fix (RH3202), formatter entry point | reproduced | `}···` vs `}` |
| lone-`\r` document, line break behind the target (issue's example) | all lone `\r` | code fix (RH3202), both entry points, split path | reproduced | `}\r` vs `}\n` |
| mixed document, foreign line break behind the target | CRLF with one `\n`; LF with one `\r\n` | code fix (RH3202, RH5111, RH7101), both entry points, split path | reproduced | foreign break kept vs detected ending |

### Not covered, and why

- L1/L2 (leading boundary fusion with a lone `\r` in front of a blank line): dropped by Decision 4. The lone-`\r` tests avoid a blank line above the target for that reason.
- RH5111 on a non-last enum member (`B =\r\n2,\n`, skeptic's T4 shape): the `\n` there is the trailing trivia of the enum's separator comma, a token outside the target, so FMT-PIPE-010 ("keep the rest of the document") does not put it in the fix's trailing trivia; I tested the last member (no comma) instead. The lead may want the skeptic's T4 wording checked against this.
- FC-004/FC-019/FC-020/FC-021: comments behind the target are covered (`//`, `/* */`, multi-line `/* */`); a directive cannot stand in a trailing trivia (a directive starts a line, so it is the next token's leading trivia).
- FC-022: a literal's line breaks are not in a token's trailing trivia; FMT-PIPE-019/FMT-EOL-004 tests already cover literals.
- FC-008 (Fix All): the change sits in the entry point every fix calls once per node; existing Fix All scenarios of the three classes still pass. No new Fix All test.
- FC-028 (`FormatNode`): not involved; it writes no document trivia back.
- FC-038–FC-043: no shape is declared unsupported.

## thoenissen/Reihitsu#1696

Tests in `src/Reihitsu.Analyzer.Test/Documentation/RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests.cs`, through the class's own `VerifySpecificationFileFix` (the Roslyn verifier with the source text as written; it compares line endings exactly, as the `<CR><LF>` diffs show), so the analyzer's re-analysis of the fixed file also checks RH8402-002's comparison after the fix.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixWritesTheDetectedLineEndingWhenTheFirstLineEndsInTheMinorityLineEnding` (spec example) | RH8402-005 | `VerifySpecificationFileFix` | fails (header `\n`) |
| `FixWithHeaderEndingInLineBreakWritesTheDetectedLineEndingWhenTheFirstLineEndsInTheMinorityLineEnding` | RH8402-005 | same, header ending in a line break | fails |
| `FixWritesLineFeedWhenTheFirstLineEndsInCarriageReturnLineFeedAndTheOthersInLineFeed` | RH8402-005 | same | fails (header `\r\n`) |
| `FixWritesTheDetectedLineEndingWhenTheFirstLineEndsInTheMajorityLineEnding` (other side) | RH8402-005 | same | passes |
| `FixWritesLineFeedInAFileOfLoneCarriageReturns` (spec prose) | RH8402-005 | same | fails (header `\r`) |
| `FixWithHeaderEndingInLineBreakWritesLineFeedInAFileOfLoneCarriageReturns` | RH8402-005 | same | fails |
| `FixInsertsHeaderInFrontOfCode` — **changed**: expected line breaks now `Environment.NewLine` (spec prose, one-line file) | RH8402-005 | same | passes on Linux (cannot fail here, FC-017: `Environment.NewLine` is `\n`; would fail on Windows) |
| `FixWithHeaderEndingInLineBreakWritesTheMachineLineEndingInAOneLineFile` | RH8402-005 | same | passes on Linux (same reason) |
| `FixReplacingAnOldHeaderWritesTheDetectedLineEndingAndKeepsTheLineBreakBehindIt` | RH8402-005, RH8402-006 | same | fails (header's inner breaks `\n`) |
| `HeaderInTheFirstLineEndingOfAFileWithMixedLineBreaksIsNotReported` (analyzer keeps first-line comparison) | RH8402-002 | `VerifySpecificationFile` | passes |

The previous assertion of `FixInsertsHeaderInFrontOfCode` was `\n` for a one-line file — RH8402-005 now says `Environment.NewLine`; I rewrote it rather than leave a test that asserts the old behavior on Windows. Its LF-only expectations elsewhere (files that hold `\n`) are unchanged and still right.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| first line LF, rest CRLF | mixed | code fix | reproduced | header `\n` vs `\r\n` |
| all lone `\r` | lone `\r` | code fix | reproduced | header `\r` vs `\n` |
| one-line file | none | code fix | not observable on Linux | `\n` vs `Environment.NewLine` (equal on Linux) |

### Not covered, and why

- Files that start with a directive or a documentation comment in a mixed file: the line ending is chosen once for the whole header, independent of what starts the file (one call site, `ApplyCodeFixAsync` in the provider resolves the ending before `GetLeadingHeaderSpan`); the existing LF/CRLF tests cover the placement.
- Fix All (FC-008): the existing Fix All scenario runs; one header per file, so no overlapping spans.

## thoenissen/Reihitsu#1707

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3003UseStringEmptyForEmptyStringsAnalyzerTests.FixOfCommentedLiteralOnTheNextLineKeepsTheLineBreakInFrontOfIt` (issue's shape) | SHARED-FIX-006, RH3003-004 | `VerifyCodeFixWithEndOfLine` (LF, CRLF) | CRLF fails, LF passes |
| `RH3003...FixOfLiteralBehindACommentLineKeepsBothLineBreaksInFrontOfIt` | SHARED-FIX-006, RH3003-004 | same | CRLF fails (two breaks), LF passes |
| `RH3003...FixOfCommentedInitializerOnTheNextLineKeepsTheLineBreakInFrontOfIt` | SHARED-FIX-006, RH3003-004 | same | CRLF fails, LF passes |
| `RH3003...FixOfCommentedArgumentOnTheNextLineKeepsTheLineBreakInFrontOfIt` | SHARED-FIX-006, RH3003-004 | same | CRLF fails, LF passes |
| `RH3003...FixOfCommentedLiteralBehindALineEndingInACommentKeepsTheLineBreakInFrontOfIt` | SHARED-FIX-006, RH3003-004 | same | CRLF fails, LF passes |
| `RH3003...FixOfLiteralOnTheNextLineKeepsTheLineBreakInFrontOfIt` (no comment, other side) | SHARED-FIX-006, RH3003-004 | same | passes |
| `RH3003...FixOfLiteralBehindAnOperatorFollowedByACommentKeepsTheLineBreakInFrontOfIt` (comment in `+`'s trivia) | SHARED-FIX-006, RH3003-004 | same | passes |
| `RH3003...FixOfCommentedLiteralBehindAnOperatorThatStartsItsLineKeepsTheLineBreakInFrontOfTheOperator` (no break in the gap) | SHARED-FIX-006, RH3003-004 | same | passes |
| `RH3005UseReadableConditionsAnalyzerTests.FixOfCommentedComparisonOnTheNextLineKeepsTheLineBreakInFrontOfIt` | SHARED-FIX-006, RH3005-004 | same | passes (characterization) |
| `RH3005...FixOfComparisonBehindACommentLineKeepsBothLineBreaksInFrontOfIt` | SHARED-FIX-006, RH3005-004 | same | passes (characterization) |
| `RH3005...FixOfCommentedComparisonBehindALogicalOperatorKeepsTheLineBreakInFrontOfIt` | SHARED-FIX-006, RH3005-004 | same | passes (characterization) |
| `RH3005...FixOfComparisonFollowedByACommentAndALineBreakKeepsTheLineBreakBehindIt` | SHARED-FIX-006, RH3005-004 | same | passes (characterization) |

On this Linux host the CRLF arm fails and the LF arm passes (the code-action clean-up writes `\n`); on a host whose newline is `\r\n` the LF arm would be the failing one, so both arms stay.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH3003 `return a +⏎ /* x */ ""` | CRLF | code fix | reproduced | `+\n` vs `+\r\n` |
| RH3003 same | LF | code fix | holds | — |
| RH3005, four shapes with a comment and a line break next to the comparison | LF, CRLF | code fix | not reproduced | no drift |

### Not covered, and why

- RH3003 Fix All across several commented literals: the defect is per replacement (one replaced node's gap); the existing Fix All scenario (two literals with a comment between) passes. Not added.
- RH3003 with a directive in front of the literal (skeptic's holding row): not added; disabled text in front of `""` makes the active `""` hard to state in a short test, and the skeptic observed it holding; the no-comment and comment-in-`+` rows cover the "no comment in the literal's own leading trivia" side.
- RH3005 counterpart surface (formatter): not involved, the fix does not delegate to the formatter.

## thoenissen/Reihitsu#1721

No tests (Decision 6). The issue's shape is already pinned by `ReihitsuFormatterLineEndingIndependenceTests.KeptLineBreaksInsideLiteralsCommentsAndDisabledTextAreTheSameInTheCopy` and `ReihitsuFormatterTests.FormatSyntaxTreeWritesLineFeedWhenEveryLineBreakIsALoneCarriageReturn`. Verdict: no scenario for this run.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.ReihitsuFormatterNodeTrailingLineEndTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests|FullyQualifiedName~RH7101DoNotCombineFieldsAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3003UseStringEmptyForEmptyStringsAnalyzerTests|FullyQualifiedName~RH3005UseReadableConditionsAnalyzerTests"
```

Counts on this head: formatter class 16 failing / 3 passing; the six analyzer classes 23 failing / 467 passing, every failure one of the tests listed as failing above.
