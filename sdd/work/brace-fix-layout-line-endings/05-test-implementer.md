Role: test implementer
Commit: 4e0b567 (every "Result" below measured on this production code, Debug build; tests added on top)
Verdict: #1416: reproduced; #1388: reproduced (RH5011, RH8304, RH7309, brace fixes in lone-`\r`/mixed documents), not reproduced for RH7004 (characterization); #1627: reproduced; #1142: reproduced

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples and requirements of the spec diff `c2866ea..HEAD -- sdd/specs` and the maintainer answers 1–6 in `00-lead.md`.

Shared test infrastructure added to `src/Reihitsu.Analyzer.Test/Base/AnalyzerTestsBase{TAnalyzer,TCodeFix}.cs` (test project only): `TryApplyCodeFixAtAsync(source, id, locationProvider)` (apply the fix of one chosen diagnostic, raw text, `null` when withheld), `ConvertEndOfLine(text, eol)` (LF/CRLF/lone-`\r` documents from one text), `RemoveMarkup(source)`.

## thoenissen/Reihitsu#1416

### Tests

File: `src/Reihitsu.Analyzer.Test/Formatting/RH5405BracesMustNotBeOmittedAnalyzerTests.cs` unless noted; RH5406 = `RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzerTests.cs`, RH5407 = `RH5407UseBracesConsistentlyAnalyzerTests.cs`. `[LF/CRLF]` = `DataRow` per line ending through `VerifyCodeFixWithEndOfLine` (verifier plus raw-text comparison).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixKeepsTheHeaderTheBodyAndTheBlankLinesAsWritten` [LF/CRLF] | SHARED-BRACE-002 | `VerifyCodeFixWithEndOfLine` | red |
| `FixKeepsTheColumnOfAMisindentedBodyBelowItsIf` [LF/CRLF] | SHARED-BRACE-002 | `VerifyCodeFixWithEndOfLine` | red |
| `FixKeepsTheColumnOfABodyIndentedFurtherThanOneLevel` | SHARED-BRACE-002 | `Verify` | red |
| `FixLeavesTheOtherBodyAsWritten` [LF/CRLF] | SHARED-BRACE-002 | `VerifyCodeFixWithEndOfLine` | red |
| `FixPlacesTheBracesOfAnElseIfBodyInTheColumnOfTheElse` [LF/CRLF] | SHARED-BRACE-002 | `VerifyCodeFixWithEndOfLine` | red |
| `FixPlacesTheBracesOfAnElseIfBodyOnItsLineInTheColumnOfTheElse` | SHARED-BRACE-002 | `Verify` | red |
| `FixKeepsAnElseBehindTheBodyOnTheLineOfTheClosingBrace` [LF/CRLF] | SHARED-BRACE-002 | `TryApplyCodeFixAtAsync` (if body) | red |
| `FixKeepsAnElseBehindABodyBelowItsIfOnTheLineOfTheClosingBrace` | SHARED-BRACE-002 | `TryApplyCodeFixAtAsync` | red |
| `NoFixIsOfferedForAnElseBodyBehindAnUnbracedBodyOnItsLine` | SHARED-BRACE-003 | `TryApplyCodeFixAtAsync` (else body) | red |
| `FixAllWrapsBothBodiesOfAnIfAndAnElseOnOneLineInTwoIterations` (incremental and Fix All = 2, exact) | SHARED-BRACE-002, SHARED-BRACE-003; FC-008 | `Verify` | red |
| `FixAllWrapsTheOuterAndTheInnerBodyOfNestedIfsOnOneLineInTwoIterations` (2, exact) | SHARED-BRACE-002, SHARED-BRACE-003; FC-008 | `Verify` | red |
| `NoFixIsOfferedForTheBodyOfAnIfBehindTheHeaderOfAnotherIf` (inner: none; outer: wraps `if (a) M(a, b, i);` as written) | SHARED-BRACE-003 | `TryApplyCodeFixAtAsync` | red |
| `FixMovesAStatementBehindABodyOnTheLineOfItsIfBelowTheClosingBrace` [LF/CRLF] | SHARED-BRACE-002 (answer 2) | `VerifyCodeFixWithEndOfLine` | red |
| `FixMovesAStatementBehindABodyBelowItsIfBelowTheClosingBrace` [LF/CRLF] | SHARED-BRACE-002 (answer 2) | `VerifyCodeFixWithEndOfLine` | red |
| `FixRemovesTheBlankLineBelowTheIfAndKeepsTheCommentAboveTheBody` [LF/CRLF] | SHARED-BRACE-002 (answer 3) | `VerifyCodeFixWithEndOfLine` | green |
| `FixRemovesTheBlankLinesBetweenTheIfAndItsBody` [LF/CRLF] | SHARED-BRACE-002 (answer 3) | `VerifyCodeFixWithEndOfLine` | green |
| `FixKeepsADelimitedCommentBetweenTheHeaderAndTheBodyBehindTheHeader` | SHARED-BRACE-002; FC-021 | `Verify` | green |
| `NoFixIsOfferedForABodyOnTheLineOfItsIfWhoseCommentContinuesOntoALaterLine` [LF/CRLF] | SHARED-BRACE-003 (answer 1C) | `VerifyNoCodeFixIsOffered` | red |
| `FixIsOfferedForABodyOnTheLineOfItsIfWithAClosedDelimitedCommentBehindIt` | SHARED-BRACE-002, SHARED-BRACE-003 | `Verify` | green |
| `NoFixIsOfferedForAnIfBehindAStatementOnItsLine` / `…ACaseLabelOnItsLine` / `…AnOpeningBraceOnItsLine` / `…ALoopHeaderOnItsLine` / `…ACommentOnItsLine` | SHARED-BRACE-003 | `VerifyNoCodeFixIsOffered` | red (each) |
| `NoFixIsOfferedForAPragmaDirectiveBetweenTheIfAndItsBody` [LF/CRLF] | SHARED-BRACE-003 (answer 5); FC-020 | `VerifyNoCodeFixIsOffered` | red |
| `NoFixIsOfferedForARegionAroundTheBody`, `NoFixIsOfferedForANullableDirectiveBetweenTheIfAndItsBody`, `NoFixIsOfferedForDisabledTextBetweenTheIfAndItsBody` | SHARED-BRACE-003; FC-005, FC-020 | `VerifyNoCodeFixIsOffered` | red (each) |
| `FixIsOfferedForAPragmaDirectiveBehindTheBody` | SHARED-BRACE-002, SHARED-BRACE-003 | `Verify` | green |
| `NoFixIsOfferedForATabInFrontOfTheIf` | SHARED-BRACE-003 | `VerifyNoCodeFixIsOffered` | red |
| `FixIsOfferedForATabInFrontOfABodyBelowItsIf` | SHARED-BRACE-002, SHARED-BRACE-003 | `Verify` | red (body moved to column 12, tabs replaced) |
| `FixIsOfferedForATabInsideAStringLiteralOfTheBody` | SHARED-BRACE-002, SHARED-BRACE-003; FC-022 | `Verify` | green |
| `FixIsOfferedWhenTheElseBlockHoldsACommentTheFormatterRefuses` (rewritten from `NoFixIsOfferedWhenTheIfStatementHoldsACommentTheFormatterRefuses`, was SHARED-FIX-010) | SHARED-BRACE-002 | `Verify` | red (withheld today) |
| `VerifyNoBlankLineIsInsertedAboveFixedStatementAfterDeclaration`, `…AfterClosingBrace`, `VerifyNoBlankLinesAreInsertedAboveAdjacentFixedStatements` (rewritten from the `…IsInserted…` tests) | SHARED-BRACE-002 | `Verify` | red |
| `VerifyFixKeepsTheTrailingCommentOfAnElseIfCondition` (+ `WithCarriageReturnLineFeed`), expected braces moved from column 13 to the `else` column 8 | SHARED-BRACE-002, SHARED-FIX-003 | `Verify` | red |
| RH5406 `NoFixIsOfferedWhenAMultiLineChildStartsOnTheLineOfItsIf` [LF/CRLF] (rewritten from `VerifyIssueIsFixedWhenChildStartsOnParentLine`) | SHARED-BRACE-003, RH5406-001, RH5406-002 | `VerifyNoCodeFixIsOffered` | red |
| RH5406 `FixKeepsTheContinuationColumnOfABodyBelowItsIf` [LF/CRLF] | SHARED-BRACE-002 | `VerifyCodeFixWithEndOfLine` | red |
| RH5406 `FixKeepsEveryColumnOfAMisindentedMultiLineBodyBelowItsIf` [LF/CRLF] | SHARED-BRACE-002 (answer 6) | `VerifyCodeFixWithEndOfLine` | red |
| RH5406 `NoFixIsOfferedForAMultiLineBodyOnTheLineOfItsElse` | SHARED-BRACE-003 | `VerifyNoCodeFixIsOffered` | red |
| RH5406 `NoFixIsOfferedForABodyOnTheLineOfItsIfWithAVerbatimStringSpanningLines` | SHARED-BRACE-003; FC-022 | `VerifyNoCodeFixIsOffered` | red |
| RH5406 `FixKeepsAVerbatimStringSpanningLinesInABodyBelowItsIf` [LF/CRLF] | SHARED-BRACE-002; FC-022 | `VerifyCodeFixWithEndOfLine` | red |
| RH5406 `FixIsOfferedForADirectiveBetweenTwoLinesOfABodyBelowItsIf` | SHARED-BRACE-002, SHARED-BRACE-003; FC-020 | `Verify` | green |
| RH5406 `NoFixIsOfferedForAPragmaDirectiveBetweenTheIfAndItsBody` | SHARED-BRACE-003 | `VerifyNoCodeFixIsOffered` | red |
| RH5406 `FixKeepsTheLinesOfABodyWithBracesOfItsOwn` [LF/CRLF], `FixKeepsTheLinesOfABodyWithAStatementLambda` | SHARED-BRACE-002; FC-027 | `VerifyCodeFixWithEndOfLine`, `Verify` | red |
| RH5407 `FixKeepsAnElseBehindAClosingBraceAndPlacesTheBracesInTheColumnOfThatBrace` [LF/CRLF] | SHARED-BRACE-002 | `VerifyCodeFixWithEndOfLine` | red |
| RH5407 `FixPlacesTheBracesOfAnElseIfBodyBehindAClosingBraceInTheColumnOfThatBrace` [LF/CRLF] | SHARED-BRACE-002; FC-026 | `VerifyCodeFixWithEndOfLine` | red |
| RH5407 `NoFixIsOfferedForAnElseBehindACommentBehindAClosingBrace` | SHARED-BRACE-003 | `VerifyNoCodeFixIsOffered` | red |
| RH5407 `NoFixIsOfferedForAPragmaDirectiveBetweenTheElseAndItsBody` | SHARED-BRACE-003 | `VerifyNoCodeFixIsOffered` | red |

Existing tests left unchanged because they already state the specified output: the RH5405/RH5406/RH5407 tests with the body below a starting `if`/`else` (continuation columns there already equal the input), `VerifyFixWritesMultiLineTrailingCommentOfBodyOnce`, the nested-`if`/loop-body comment tests, the Fix All scenarios over independent `if`s.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `b++;⏎if ( a )⏎    M(a,b);` | LF, CRLF | RH5405 fix | reproduced | observed: blank line inserted above `if`, `(a)`, `M(a, b)`; expected: all as written |
| body below `if`, column 10 / multi-line `M(a,` 10, `b);` 14 | LF, CRLF | RH5405, RH5406 fix | reproduced | observed: body to column 12, continuation re-aligned; expected: columns kept |
| `else if (b)⏎    M(a, b);` | LF, CRLF | RH5405 fix | reproduced | observed: braces column 13; expected: column 8 |
| `} else M(a, b);`, `} else if (b) M(a, b);⏎else⏎{…}` | LF, CRLF | RH5407 fix | reproduced | observed: `else` moved / braces column 15 and the braced `else` re-indented; expected: `} else` kept, braces in the `}` column |
| `if (a) M(a, b); else M(a, b);` | LF, CRLF | RH5405 fix on the if body | reproduced | observed: both bodies braced, `else` on its own line; expected: `} else M(a, b);` |
| `if (a) M(a, b); /* c */ b++;` and body-below variant | LF, CRLF | RH5405 fix | reproduced | observed: `} b++;`; expected: `b++;` on its own line in the `}` column |
| other `if` body reformatted (`M(a,⏎  b)` under `else`) | LF, CRLF | RH5405 fix | reproduced | observed: the `else` body's continuation re-aligned; expected: kept |
| withhold classes of SHARED-BRACE-003 (multi-line body on the header line, code in front, directive/disabled text, tab in front, foreign line ending) | LF, CRLF, mixed, lone `\r` | RH5405–RH5407 fix | reproduced | observed: a fix is offered (for `#pragma`/`#nullable` an offered no-op, as the skeptic measured); expected: none |

### Not covered, and why

- `case 1: if (a) M();` where the formatter braces the section: covered only as a withheld fix (SHARED-BRACE-003); no layout to assert.
- FC-018, FC-036 (rename), FC-023, FC-024, FC-025 (member kinds), FC-029, FC-038–FC-043 (no unsupported shape declared, answer 5): not reachable by a brace fix.
- FC-010/FC-011/FC-012 (formatter stability): the run changes no formatter phase.
- FC-030: covered by every test whose output keeps the header, the other body, and the neighbour lines as written.
- FC-007 (one-pass convergence): every `Verify` call re-runs the analyzer on the result and checks the iteration count.

## thoenissen/Reihitsu#1388

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5011…AnalyzerTests.FixInsertsLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` | RH5011-004, SHARED-FIX-008 | `ApplyCodeFixAsync` raw | red |
| `RH5011…AnalyzerTests.FixInsertsTheDetectedLineEndingWhenTheBreakLineEndsInAnotherOne` | RH5011-004, SHARED-FIX-008 | `ApplyCodeFixAsync` raw | red |
| `RH8304…AnalyzerTests.FixInsertsLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` (spec example, both elements, two applications) | RH8304-004, SHARED-FIX-008 | `ApplyCodeFixAsync` raw | red |
| `RH8304…AnalyzerTests.FixInsertsTheDetectedLineEndingWhenTheDocumentationLineEndsInAnotherOne` | RH8304-004, SHARED-FIX-008 | `ApplyCodeFixAsync` raw | red |
| `RH7309…AnalyzerTests.NoFixIsOfferedForALineFeedInAMovedBlockOfACarriageReturnLineFeedDocument` | RH7309-007 | `VerifyNoCodeFixIsOffered` | red |
| `RH7309…AnalyzerTests.NoFixIsOfferedForALineFeedInsideACommentOfAMovedBlock` | RH7309-007 | `VerifyNoCodeFixIsOffered` | red |
| `RH7309…AnalyzerTests.NoFixIsOfferedInADocumentWhoseLineBreaksAreLoneCarriageReturns` | RH7309-007 | `VerifyNoCodeFixIsOffered` | red |
| `RH7309…AnalyzerTests.FixIsOfferedForALineFeedBetweenTheBlocksOfACarriageReturnLineFeedDocument` | RH7309-004, RH7309-007 | `ApplyCodeFixAsync` raw | green |
| `RH7309…AnalyzerTests.FixIsOfferedInADocumentWhoseLineBreaksAreAllCarriageReturnLineFeed` | RH7309-004, RH7309-007 | `VerifyCodeFixWithEndOfLine` | green |
| `RH7004…AnalyzerTests.FixWritesTheDocumentsLineEndingAroundADirectiveAndACommentInFrontOfAFollowingStatement` [LF/CRLF] | RH7004-007 | `VerifyCodeFixWithEndOfLine` | green (characterization) |
| Brace fixes in lone-`\r` and mixed documents: RH5405 `NoFixIsOfferedInADocumentWhoseLineBreaksAreLoneCarriageReturns` (rewritten from `FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns`), `NoFixIsOfferedForABodyBelowItsIfInADocumentWhoseLineBreaksAreLoneCarriageReturns`, `NoFixIsOfferedForALineFeedBehindTheIfInACarriageReturnLineFeedDocument`, `NoFixIsOfferedForALineFeedInsideACommentBehindTheBodyInACarriageReturnLineFeedDocument`; RH5406 `NoFixIsOfferedInADocumentWhoseLineBreaksAreLoneCarriageReturns` and `NoFixIsOfferedForABodyOnTheLineOfItsIfInADocumentWhoseLineBreaksAreLoneCarriageReturns` (both rewritten from the two lone-`\r` fix tests), `NoFixIsOfferedForALineFeedInsideAVerbatimStringOfTheBodyInACarriageReturnLineFeedDocument`; RH5407 `NoFixIsOfferedInADocumentWhoseLineBreaksAreLoneCarriageReturns` | SHARED-BRACE-003 (+ SHARED-FIX-008 where tagged) | `VerifyNoCodeFixIsOffered` | red (each) |
| RH5405 `FixWritesCarriageReturnLineFeedWhenALineFeedStandsAboveTheIf` | SHARED-BRACE-002, SHARED-BRACE-003, SHARED-FIX-008 | `ApplyCodeFixAsync` raw | red (today a blank line is inserted above `if`; the line endings themselves hold) |
| RH5405 `FixIsOfferedWhenALineFeedStandsBehindTheOtherBody` | SHARED-BRACE-003, SHARED-FIX-008 | `ApplyCodeFixAsync` raw | green |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH5011-004 example, `switch` section | lone `\r` | RH5011 fix | reproduced | observed: `break;\r\r` (the break line's own `\r` copied); expected: one `\n` inserted |
| `break;` line ending in LF | CRLF document | RH5011 fix | reproduced | observed: `break;\n\n`; expected: `break;\n\r\n` |
| RH8304-004 example | lone `\r` | RH8304 fix | reproduced | observed: `\r` inserted; expected: `\n` |
| doc line ending in LF | CRLF document | RH8304 fix | reproduced | observed: LF inserted; expected: CRLF |
| RH7309-004 example, LF inside the moved `Fields` block / inside a comment of a moved block / lone `\r` | mixed, lone `\r` | RH7309 fix | reproduced | observed: fix offered; expected: none |
| RH7004, own-line comment and `#nullable enable` among the following statements | LF, CRLF | RH7004 fix | not reproduced | output and line endings as specified (characterization, decision 4) |
| RH5405 CRLF `if (a) M(a, b); /* one⏎ two */` | CRLF | RH5405 fix | now a withhold case | `NoFixIsOfferedForABodyOnTheLineOfItsIfWhoseCommentContinuesOntoALaterLine` (red: offered today) |

### Finding against RH5011-004 (for the lead)

RH5011-004's lone-`\r` sentence says the inserted empty line "ends in `\n`, and the line breaks around it stay a lone `\r`". Written literally, the text is `break;\r` + `\n` = `break;\r\n`: the `\r` and the `\n` read as one CRLF line break, and there is no blank line. `FixInsertsLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` therefore asserts the one text that inserts exactly one `\n`, keeps every `\r` of the input as a lone `\r`, and leaves a blank line: `break;\n\r            default:`. The `\n` comes in front of the break line's own `\r`. The spec sentence may need rewording to that text. The lone-`\r` and mixed-document rows are rare documents.

### Not covered, and why

- `FormatNode` instance: dropped (decision 3, no caller).
- RH8402: follow-up candidate, outside the run.
- FC-008 Fix All in lone-`\r` documents for RH5011/RH8304: the existing Fix All scenarios run the same fix code path per diagnostic; the line ending comes from one source in each provider (`rg -n "EndIncludingLineBreak|GetLineBreak" src/Reihitsu.Analyzer.CodeFixes` for RH5011 and RH8304). The tests apply the fix once per diagnostic.

## thoenissen/Reihitsu#1627

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5405 `FixKeepsTrailingWhitespaceBehindACommentOnTheBodyLineAndWritesNoneBehindTheClosingBrace` [LF/CRLF] | SHARED-BRACE-002 (answer 4) | `VerifyCodeFixWithEndOfLine` | red |
| RH5405 `FixKeepsTrailingWhitespaceOnTheBodyLineAndWritesNoneBehindTheClosingBrace` [LF/CRLF] | SHARED-BRACE-002 | `VerifyCodeFixWithEndOfLine` | red |
| RH5405 `FixKeepsTrailingWhitespaceOfABodyOnTheLineOfItsIfOnTheBodyLine` [LF/CRLF × no comment / `/* x */`] | SHARED-BRACE-002 | `VerifyCodeFixWithEndOfLine` | red |
| RH5405 `FixKeepsTheSpacesAtTheEndOfASingleLineCommentBehindTheBody` [LF/CRLF] | SHARED-BRACE-002 | `VerifyCodeFixWithEndOfLine` | green |
| RH5406 `FixKeepsTrailingWhitespaceOnTheLastBodyLineAndWritesNoneBehindTheClosingBrace` [LF/CRLF] | SHARED-BRACE-002 | `VerifyCodeFixWithEndOfLine` | red |
| RH5407 `FixKeepsTrailingWhitespaceOnTheElseBodyLineAndWritesNoneBehindTheClosingBrace` [LF/CRLF] | SHARED-BRACE-002 | `VerifyCodeFixWithEndOfLine` | red |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `M(a, b); /* x */···` below `if (a)` | LF, CRLF | RH5405, RH5406, RH5407 fix | reproduced | observed: `}···`, body line stripped; expected: `···` stays on the body line, nothing behind `}` |
| `M(a, b);···` (no comment), and on the `if` line | LF, CRLF | RH5405 fix | reproduced | same |
| `// x···` | LF, CRLF | RH5405 fix | not reproduced | spaces are comment text and stay (characterization) |

### Not covered, and why

- RH3202 and the other formatter-delegating fixes (`}···` behind an expression body converted to a block): follow-up candidate in `00-lead.md` (decision 10), outside the run.

## thoenissen/Reihitsu#1142

### Tests

File `RH5514MethodAttributeListsMustFollowShapeRulesAnalyzerTests.cs` unless noted; `VerifySplitFix` (local helper) runs the verifier and applies the fix once per diagnostic on the raw text, line endings included.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SplitFixStartsEachFurtherListOnANewLine` [LF/CRLF] | RH5514-004, SHARED-ATTR-017 | `VerifySplitFix` | green |
| `SplitFixKeepsTheCodeAroundTheListAsWritten` [LF/CRLF] | RH5514-004, SHARED-ATTR-017; FC-020 | `VerifySplitFix` | red |
| `SplitFixKeepsTheWhitespaceAtTheEndOfTheLinesAroundTheList` [LF/CRLF] | RH5514-004, SHARED-ATTR-017 | `VerifySplitFix` | red |
| `SplitFixKeepsAListInFrontOfTheReportedListOnItsLine` / `…AListBehind…` / `…ACommentInFront…` / `…AReturnValueListInFront…` [LF/CRLF each] | RH5514-004, SHARED-ATTR-017; FC-021 | `VerifySplitFix` | red |
| `SplitFixStartsTheFurtherListOfALambdaBehindTheWhitespaceOfItsLine` [LF/CRLF] | RH5514-004, SHARED-ATTR-017; FC-027 | `VerifySplitFix` | LF green, CRLF red (drift) |
| `SplitFixCopiesTheTabInFrontOfTheReportedList` [LF/CRLF], `SplitFixCopiesATabAndFourSpacesInFrontOfTheReportedList` | RH5514-004, SHARED-ATTR-017 | `VerifySplitFix` | red |
| `SplitFixInsertsLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns` | SHARED-ATTR-017, SHARED-FIX-008 | `ApplyCodeFixAsync` raw | green |
| `SplitFixInsertsTheDetectedLineEndingWhenTheListLineEndsInAnotherOne` | SHARED-ATTR-017, SHARED-FIX-008 | `ApplyCodeFixAsync` raw | green |
| `FixAllCorrectsEveryDiagnostic` (scenario rewritten: `[First]` / `[Second] [Third]` / `[Fourth]`; `NumberOfFixAllIterations` 2 → -2, at most two) | SHARED-FIX-001, SHARED-FIX-002 (inherited tags); FC-008 | `Verify` | red |
| `VerifyCodeFixGivesEachMethodAttributeAListOfItsOwnWithCarriageReturnLineFeed` (raw CRLF assertion added) | RH5514-004, SHARED-ATTR-017, SHARED-FIX-008 | `Verify` + `ApplyCodeFixAsync` | green |
| RH5506 `SplitFixKeepsTheClassDeclarationBehindTheLastNewList` [LF/CRLF], `SplitFixKeepsANestedClassDeclarationBehindTheLastNewList` [LF/CRLF] | RH5506-003, SHARED-ATTR-017 | `VerifyCodeFixWithEndOfLine` | red |
| RH5502, RH5504, RH5506, RH5508, RH5510, RH5512, RH5516, RH5518, RH5520, RH5522, RH5526, RH5528: the existing `VerifyCodeFixGivesEach…AListOfItsOwnWithCarriageReturnLineFeed` gains a raw CRLF assertion | each rule's -003 requirement, SHARED-ATTR-017, SHARED-FIX-008 (added) | `Verify` + `ApplyCodeFixAsync` | green |
| RH5531 `VerifySplitFixWritesCarriageReturnLineFeedInACarriageReturnLineFeedDocument` | RH5531-005, SHARED-ATTR-017, SHARED-FIX-008 | `ApplyCodeFixAsync` raw | green |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| column-0 braces, directive and comments around `    [A, B]` | LF, CRLF | RH5514 fix | reproduced | observed: lists, declaration, and comments re-indented; expected: only `[B]` added in column 4 |
| trailing whitespace around the list | LF, CRLF | RH5514 fix | reproduced | observed: removed; expected: kept, `···` behind `[B]` |
| `[A] [B, D2] void M()`, `[A, B] [D2] void N()`, `/* c */ [A, B]`, `[return: A] [A, B] int R()` | LF, CRLF | RH5514 fix | reproduced | observed: neighbours broken off; expected: only the reported list split |
| `→[A, B]`, `→····[A, B]` | LF, CRLF | RH5514 fix | reproduced | observed: tabs rewritten as spaces; expected: whitespace copied |
| `[A, B] class C`, nested `[A, B] class C { }` | LF, CRLF | RH5506 fix | reproduced | observed: declaration moved to its own line (with LF drift under CRLF); expected: `[B] class C` |
| lambda `System.Action a = [A, B] () => { };` | CRLF | RH5514 fix | reproduced (drift only) | observed: LF in a CRLF document; LF holds |
| simple list on its own line, every split rule | CRLF | 13 split fixes | not reproduced | the provider's own inserted break is already the detected ending; drift occurs only where Roslyn's post-processing adds a break (the shared-line shapes above) |
| lone `\r` / LF on the list line in a CRLF document | lone `\r`, mixed | RH5514 fix | not reproduced | inserted break already `\n` / CRLF |

### Not covered, and why

- One CRLF raw test per split rule rather than the full matrix per rule: every split provider registers through the sealed `TargetAttributeListShapeCodeFixProviderBase.RegisterCodeFixesAsync` into the private `ApplySplitCodeFixAsync`. `rg -n "ApplySplitCodeFixAsync" src/Reihitsu.Analyzer.CodeFixes` shows the single call site. The one parameter that differs is `placementMode`, from `ResolvePlacementMode`. Each rule therefore keeps one per-rule test, and the shape matrix runs on RH5514 and RH5506.
- Merge path (RH5524, RH5529) and placement fixes: the skeptic measured them holding; outside the issue.
- FC-018/FC-036, FC-038–FC-043: not reachable.

## Whole run

### Commands

```shell
scripts/build.sh --configuration Debug
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~RH5405BracesMustNotBeOmittedAnalyzerTests|FullyQualifiedName~RH5406BracesMustNotBeOmittedFromMultiLine|FullyQualifiedName~RH5407UseBracesConsistentlyAnalyzerTests"
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~RH5011Break|FullyQualifiedName~RH8304Xml|FullyQualifiedName~RH7309Regions|FullyQualifiedName~RH7004Using|FullyQualifiedName~AttributeListsMustFollowShapeRules"
scripts/test.sh --project architecture --configuration Debug
```

### Results on 4e0b567 plus these tests

- Brace classes: 152 tests, 82 red, 70 green. Every red test fails on the issue's own expected-versus-actual difference (diffs inspected: reformatted header/body/continuation, inserted blank line, brace column, `else` moved, `} b++;`, `}···`, or "a code fix was offered"). None fails on a compile error or unrelated layout.
- #1388/#1142 classes: 547 tests, 28 red, all listed above as red.
- Architecture tests: 117/117 green. That includes the requirement-coverage test, which needed tags for SHARED-BRACE-003 and RH7309-007.
- Full analyzer project (`scripts/test.sh --project analyzer --configuration Debug`): 8642 tests, 110 red. These are exactly the 82 + 28 above; no other test fails.

### Follow-up candidates

None beyond `00-lead.md`.
