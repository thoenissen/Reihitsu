Role: test implementer
Commit: df4181b (tests written on this head; the commit carrying them follows it)
Verdict: #1140: reproduced; #1141: reproduced; #1144: reproduced (raw-string and #1238 Q2 instances not reproduced, kept as characterization); #1145: reproduced; #1094: reproduced (Q4 not reproduced, kept as characterization); #1393: not reproduced (characterization); #1511: reproduced; #1561: reproduced

The prompt carried no intended fix, cause, or hoped-for verdict beyond the lead's recorded decisions. No production code was touched. Every new or rewritten test was run on `df4181b` (Release build): the full analyzer suite fails in exactly the 76 tests listed as `fail` below and in no other test; `scripts/test.sh --project architecture` passes (117/117, `RequirementCoverageTests` included — RH5103-005, RH5205-004 and SHARED-ATTR-021 now have tagged tests). `scripts/format.sh` changed none of the 22 test files. "Both" in the line-ending column means the test asserts LF and CRLF in the same method. Each `fail` below was checked to fail on the issue's own expected-versus-actual difference (quoted under *Scenarios*), not on a compile error or unrelated layout.

## thoenissen/Reihitsu#1140

Narrowed per decision 3 to RH5204, RH6023/RH6024 (see #1141), RH5403/RH5405–RH5407 (here and #1144), RH5417/RH3202 (see #1144). No tests for the instances left on #1140.

### Tests

All in `src/Reihitsu.Analyzer.Test/Formatting/`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5204…Tests.FixMovesOnlyTheReportedLineAndLeavesTheRestOfTheBodyAsWritten` (spec example) | RH5204-006; FC-030 | `Verify`, both | fail |
| `RH5204…Tests.FixMovesOnlyTheMisindentedElseAndLeavesTheUnbracedBodiesAsWritten` | RH5204-006; FC-030, FC-026 | `Verify`, both | fail |
| `RH5204…Tests.FixKeepsTheColumnOfAWrappedArgumentBelowTheReportedLine` (spec example) | RH5204-006 | `Verify`, both | fail |
| `RH5204…Tests.FixMovesOnlyTheReportedMemberLineAndKeepsTheSpacingBehindItsFirstToken` | RH5204-006; FC-030 | `Verify`, both | fail |
| `RH5204…Tests.FixIsOfferedWhenAnotherStatementOfTheBlockHoldsACommentTheFormatterRefuses` (replaces the deleted `NoFixIsOfferedWhenTheFormattingScopeHoldsACommentTheFormatterRefuses`) | RH5204-006 | `Verify`, both | fail |
| `RH5204…Tests.VerifyStatementBehindStrayDocumentationCommentIsDetectedAndFixedInOnePass` (rewritten: comment stays behind `var x = 1;`, no blank line) | RH5204-001, RH5204-006 | `Verify` | fail |
| `RH5204…Tests.VerifyIfStatementBehindStrayDocumentationCommentIsDetectedAndFixed` (rewritten likewise) | RH5204-001, RH5204-006 | `Verify` | fail |
| `RH5204…Tests.VerifyEmbeddedIfBehindMultiLineCommentIsFixedInOnePass` and `…WithCarriageReturnLineFeed` (rewritten: unreported `X();` keeps column 8) | RH5204-006, SHARED-FIX-001 | `Verify` | fail |
| `RH5405…Tests.FixWrapsOnlyTheReportedBodyAndLeavesTheRestOfTheStatementAsWritten` (spec example) | SHARED-BRACE-002; FC-030 | `Verify`, both | fail |
| `RH5405…Tests.FixLeavesTheConditionAndTheFollowingStatementAsWritten` | SHARED-BRACE-002 | `Verify`, both | fail |
| `RH5405…Tests.VerifyNoBlankLineIsInsertedAboveFixedStatementAfterDeclaration` (was `VerifyBlankLineIsInserted…`, rewritten) | SHARED-BRACE-002; FC-017 | `Verify` | fail |
| `RH5405…Tests.VerifyNoBlankLineIsInsertedAboveFixedStatementAfterClosingBrace` (rewritten) | SHARED-BRACE-002; FC-017 | `Verify` | fail |
| `RH5405…Tests.VerifyNoBlankLinesAreInsertedAboveAdjacentFixedStatements` (rewritten) | SHARED-BRACE-002; FC-017 | `Verify` | fail |
| `RH5403…Tests.FixLaysOutOnlyTheReportedBlocksAndLeavesTheRestOfTheStatementAsWritten` (spec example) | RH5403-003; FC-026 | `Verify`, both | fail |
| `RH5403…Tests.FixOnTheElseBlockLeavesTheIfBlockAsWritten` | RH5403-003 | `Verify`, both | fail |
| `RH5403…Tests.FixInsertsNoBlankLineInFrontOfTheStatement` (#1220 Q2, `while (a) { }` / `do { M(a); } while (a);`) | RH5403-003 | `Verify`, both | fail |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH5204 misindented `return` behind `if (a) M(a);` and `int  y` | LF, CRLF | code fix | reproduced | observed: `if` body braced, `int y`, blank lines above `int` and `return`; expected: only `return` moves to column 8 |
| RH5204 misindented `else` with unbraced bodies | LF, CRLF | code fix | reproduced | observed: both bodies braced; expected: only `else` moves |
| RH5204 wrapped argument below reported line | LF, CRLF | code fix | reproduced | observed: `2` moved from column 15 to 17; expected: stays at 15 |
| RH5204 member with `int  _a;` neighbour | LF, CRLF | code fix | reproduced | observed: `int _a;`, `private int _b;`; expected: spacing kept |
| RH5204 refused comment `int /* c */ y` elsewhere in the block | LF, CRLF | code fix | reproduced | observed: `return` stays at column 9; expected: column 8 |
| RH5204 `return` behind stray `/// stray` | LF | code fix | reproduced | observed: comment moved to its own line with a blank line above; expected: only `return` re-indented |
| RH5405 spec example | LF, CRLF | code fix | reproduced | observed: blank line above `if`, `x = 2;` in the else block; expected: both as written |
| RH5405 `if (a==1)` | LF, CRLF | code fix | reproduced | observed: `if (a == 1)`; expected: condition as written |
| RH5405 blank-line tests (three) | LF | code fix | reproduced | observed: blank line inserted above the braced `if`; expected: none |
| RH5403 spec example / else block / `do` after `while (a) { }` | LF, CRLF | code fix | reproduced | observed: blank line inserted above the statement and `x = 2;` / `int x = 1;` in unreported blocks; expected: as written |

## thoenissen/Reihitsu#1141

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH6024…Tests.FixLeavesAWrappedGapAndTheOperatorOnItsLine` (spec example, `a+⏎b`) | SHARED-OPSPACE-004; FC-019 | `Verify`, both | fail |
| `RH6024…Tests.FixLeavesALambdaInTheOperandAsWritten` | SHARED-OPSPACE-004; FC-030 | `Verify`, both | fail |
| `RH6024…Tests.FixLeavesAGapWithALineBreakAndACommentAsWritten` (the dropped `/* c */` instance, decision 5) | SHARED-OPSPACE-004; FC-021 | `Verify`, both | pass (characterization) |
| `RH6024…Tests.FixAllPutsOneSpaceAroundEveryOperatorOfAChain` (`a+b+c`) | SHARED-OPSPACE-004; FC-008 | `Verify` (Fix All), both | fail |
| `RH6024…Tests.FixAllPutsOneSpaceAroundNestedComparisonAndLogicalOperators` (`a==b&&b>0`) | SHARED-OPSPACE-004; FC-008 | `Verify` (Fix All), both | fail |
| `RH6023…Tests.FixLeavesALambdaAndAnArrayInitializerAsWritten` (spec example) | SHARED-OPSPACE-004; FC-030 | `Verify`, both | fail |
| `RH6023…Tests.FixLeavesAWrappedGapBehindTheOperatorAsWritten` (`a=⏎1;`) | SHARED-OPSPACE-004; FC-019 | `Verify`, both | fail |
| `RH6023…Tests.FixLeavesAWrappedGapInFrontOfTheOperatorAsWritten` (`a⏎=1;`) | SHARED-OPSPACE-004; FC-019 | `Verify`, both | fail |
| `RH6023…Tests.FixAllPutsOneSpaceAroundNestedAssignments` (`d[k=1]=2`) | SHARED-OPSPACE-004; FC-008 | `Verify` (Fix All), both | fail |
| `RH6023…Tests.VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberWhoseValueIsWrapped` (rewritten: `.C =⏎ 5;`, the value stays on its line; tag added) | RH6023-002, FMT-PIPE-024, SHARED-OPSPACE-004 | `Verify`, both | fail |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `_ = a+⏎b;` | LF, CRLF | code fix | reproduced | observed `_ = a⏎+ b;` (operator moved); expected `_ = a +⏎b;` |
| `_ = a+N(a, () => { return 1; });` | LF, CRLF | code fix | reproduced | observed: lambda broken over five lines; expected: as written |
| `Action f=() => { M(a, b); };`, `var x=new[] {1,2};` | LF, CRLF | code fix | reproduced | observed: lambda broken, `new[] {1, 2 }`; expected: as written |
| `a=⏎1;`, `a⏎=1;` | LF, CRLF | code fix | reproduced | observed `a = 1;` (joined); expected: line break kept |
| `_ = a⏎/* c */ +b;` | LF, CRLF | code fix | not reproduced | `/* c */ + b;`, no blank line, as expected |
| Fix All `a+b+c`, `a==b&&b>0`, `d[k=1]=2` | LF, CRLF | Fix All | reproduced | Fix All needs 2 iterations; expected 1 (the framework's default for an unconfigured test) |

## thoenissen/Reihitsu#1144

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5205…Tests.NoFixIsOfferedForALambdaWithAnAttributeList` (spec example) | RH5205-004 | `VerifyNoCodeFixIsOffered`, both | fail |
| `RH5203…Tests.FixMovesACommentLineAboveTheArgumentIntoTheAlignmentColumn` (spec example) | RH5203-003 | `Verify`, both, `AssertFormattingChangesNothing` | fail |
| `RH5203…Tests.FixMovesEveryCommentLineAboveTheArgumentAndKeepsTheBlankLineBetweenThem` | RH5203-003 | `Verify`, both | fail |
| `RH5203…Tests.FixLeavesACommentBehindTheCommaOnItsLine` | RH5203-003 | `Verify`, both | pass |
| `RH5405…Tests.FixPlacesTheBracesOfAnElseIfBodyInTheColumnOfTheElse` | SHARED-BRACE-002, SHARED-FIX-007; FC-026 | `Verify`, both, `AssertFormattingChangesNothing` | fail |
| `RH5405…Tests.FixPlacesTheBracesOfANestedElseIfBodyOnTheLineOfItsConditionInTheColumnOfTheElse` | SHARED-BRACE-002, SHARED-FIX-007; FC-026 | `Verify`, both, `AssertFormattingChangesNothing` | fail |
| `RH5405…Tests.VerifyFixKeepsTheTrailingCommentOfAnElseIfCondition` and `…WithCarriageReturnLineFeed` (constant `ElseIfWithTrailingCommentFixedCode` rewritten: braces in the column of `else`) | SHARED-FIX-003 | `Verify` | fail |
| `RH5406…Tests.FixPlacesTheBracesOfAMultiLineElseIfBodyInTheColumnOfTheElse` | SHARED-BRACE-002, SHARED-FIX-007; FC-016 | `Verify`, both | fail |
| `RH5407…Tests.FixPlacesTheBracesOfAnElseIfBodyInTheColumnOfTheElse` | SHARED-BRACE-002, SHARED-FIX-007; FC-016 | `Verify`, both | fail |
| `RH5403…Tests.FixLaysOutTheBlockOfAnElseIfInTheColumnOfTheElse` | RH5403-003, SHARED-FIX-007; FC-026 | `Verify`, both, `AssertFormattingChangesNothing` | fail |
| `RH5417…Tests.FixGivesTheFixedMemberAndTheMemberOnItsLineLinesOfTheirOwn` (SHARED-FIX-007 example) | SHARED-FIX-007 | `Verify`, both | fail |
| `RH5417…Tests.FixPutsTwoMethodsOnOneLineOnLinesOfTheirOwn` (#1170 Q1) | SHARED-FIX-007; FC-007 | `Verify`, both | fail |
| `RH3202…AnalyzerTests.FixPutsAMemberBehindTheExpressionBodyOnTheNextLine` | SHARED-FIX-007; FC-025 | `Verify`, both | fail |
| `RH5409…Tests.FixLeavesNoTrailingWhitespaceWhereTheCommaStoodBehindASpace` (#1168, `B ,`) | SHARED-FIX-007 | `Verify`, both, `AssertFormattingChangesNothing` | fail |
| `RH5409…Tests.FixPutsTheClosingBraceBehindARemovedCommaInItsColumn` (#1168, `, }`) | SHARED-FIX-007 | `Verify`, both, `AssertFormattingChangesNothing` | fail |
| `Clarity/RH3102…Tests.FixKeepsTheNextStatementOnItsLine` (#1230) | SHARED-FIX-007 | `Verify`, both, `AssertFormattingChangesNothing` | fail |
| `Clarity/RH3201…Tests.FixLeavesNoEmptyLineBehindTheOpeningBraceWhereAMultiLineCommentStood` (#1235 Q3) | SHARED-FIX-007 | `Verify`, both, `AssertFormattingChangesNothing` | fail |
| `RH5101…Tests.FixLaysOutAMovedRawStringLiteralAsTheFormatterDoes` | none (characterization, decision 6) | `VerifyCodeFixMatchesFormatter`, both | pass |
| `RH5102…Tests.FixLaysOutARawStringLiteralArgumentAsTheFormatterDoes` | none (characterization, decision 6) | `VerifyCodeFixMatchesFormatter`, both | pass |
| `Design/RH3106…Tests.FixLeavesOneSpaceBetweenTheKeywordAndTheBlock` (#1238 Q2) | none (characterization, decision 6) | `Verify`, both | pass |
| `Design/RH3107…Tests.FixLeavesOneSpaceBetweenTheCommentsAroundTheRemovedParentheses` (#1238 Q2) | none (characterization, decision 6) | `Verify`, both | pass |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH5205 `[Obsolete] (x) =>` | LF, CRLF | code fix | reproduced | observed: fix offered; expected: withheld (RH5205-004) |
| RH5203 `M(1,⏎ // two⏎ 2);` | LF, CRLF | code fix | reproduced | observed: `// two` in column 0; expected: column 10 |
| RH5405/RH5406/RH5407/RH5403 `else if` | LF, CRLF | code fix | reproduced | observed: braces in column 13 (17 nested); expected: column of `else` |
| RH5417 SHARED-FIX-007 example, `void M() { } void N() { }` | LF, CRLF | code fix | reproduced | observed: `} int _x;`, `int _y; void P()` with braces at column 12, `} void N()` with braces at column 6; expected: own lines, no blank lines |
| RH3202 `=> _value; private int _x;` | LF, CRLF | code fix | reproduced | observed `} private int _x;`; expected: next line |
| RH5409 `B ,` / `B⏎, }` | LF, CRLF | code fix | reproduced | observed `B ` (trailing space) / `     }`; expected `B` / `}` in column 0 |
| RH3102 `a++; ;⏎a++;` | LF, CRLF | code fix | reproduced | observed `a++;         a++;`; expected two lines |
| RH3201 three-line whitespace `/* */` after `{` | LF, CRLF | code fix | reproduced | observed: empty line left behind `{`; expected none |
| RH5101 raw string `M(⏎"""⏎a⏎""",⏎2);` | LF, CRLF | code fix vs document formatting | not reproduced | fix output equals the formatter's |
| RH5102 raw string `M(1, """⏎a⏎""");` | LF, CRLF | code fix vs document formatting | not reproduced | fix output equals the formatter's |
| RH3106 `delegate () { }`, RH3107 `[Obsolete /* a */ ( ) /* b */]` | LF, CRLF | code fix | not reproduced | one space each, as expected |

## thoenissen/Reihitsu#1145

### Tests

All in `RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzerTests`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixMovesAStatementBehindAnIfStatementWithAnEmbeddedStatement`, `…AnIfElseStatement`, `…AnIfStatementWithABlock`, `…AWhileLoop`, `…AForLoop`, `…AForEachLoop`, `…AUsingStatement`, `…ALockStatement`, `…ATryStatement`, `…ACheckedStatement`, `…ALabeledStatement`, `…ALocalFunction` (12 tests, the skeptic's table) | RH5103-003; FC-001, FC-016 | `Verify`, both | fail |
| `NoFixIsOfferedInABlockWhoseOpeningBraceIsOnTheLineOfThePrecedingStatement` (spec example) | RH5103-005; FC-027 | `VerifyNoCodeFixIsOffered`, both | fail |
| `FixIsOfferedInABlockWhoseOpeningBraceIsOnAnEarlierLine` (`void N() {` then `M(); M();`) | RH5103-005 | `Verify`, both | pass |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `<stmt> M();` for each of the 12 preceding-statement kinds | LF, CRLF | code fix | reproduced | observed: no fix (line unchanged); expected: `M();` on the next line in the preceding statement's column |
| `() => { M(); M(); }`, `void O() { M(); M(); }` | LF, CRLF | code fix | reproduced | observed: fix offered; expected: withheld |

## thoenissen/Reihitsu#1094

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5513…Tests.FixPutsTheAttributeListBehindAFieldOnALineOfItsOwn` (Q2) | SHARED-FIX-007 | `Verify`, both | fail |
| `RH5513…Tests.NoFixIsOfferedWhenTheEnclosingBraceIsOffItsDepthColumn` (Q3, spec example with `[Obsolete]` for `[A]`) | SHARED-ATTR-021, RH5513-005; FC-027 | `VerifyNoCodeFixIsOffered`, both | fail |
| `RH5517…Tests.NoFixIsOfferedWhenTheTypesOpeningBraceIsOffItsDepthColumn` (type brace at column 2) | SHARED-ATTR-021, RH5517-004; FC-016 | `VerifyNoCodeFixIsOffered`, both | fail |
| `RH5513…Tests.FixIsOfferedWhenTheEnclosingLambdaBraceIsAtItsDepthColumn` (other side of the condition) | SHARED-ATTR-021, SHARED-ATTR-008 | `Verify`, both | pass |
| `RH5513…Tests.FixInTabIndentedCodeIndentsTheDeclarationWithSpacesToTheAttributeListsColumn` (Q4, decision 9) | none (characterization) | `ApplyCodeFixAsync`, both | pass |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `int _a; [Obsolete] void M()` | LF, CRLF | code fix | reproduced | observed `int _a; [Obsolete]⏎    void M()`; expected `int _a;⏎    [Obsolete]⏎    void M()` |
| local function in a lambda block aligned under `(` | LF, CRLF | code fix | reproduced | observed: fix offered; expected: withheld |
| field in a type whose `{` is at column 2 | LF, CRLF | code fix | reproduced | observed: fix offered; expected: withheld |
| `\t\t\t[Obsolete] void L()` | LF, CRLF | code fix | not reproduced | `\t\t\t[Obsolete]⏎` + twelve spaces + `void L()`, the #1030 Q4 output |

## thoenissen/Reihitsu#1393

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5104…Tests.FixMovesACommentBehindTheSecondOfTwoStatementsAboveItWithoutABlankLine` (answer 3A) | RH5104-005 | `Verify`, both | pass (characterization) |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `M();⏎M(); // second` | LF, CRLF | code fix | not reproduced | `M();⏎// second⏎M();`, no blank line, as expected |

## thoenissen/Reihitsu#1511

### Tests

All in `RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FixMovesTheFirstTypeOfAFileBehindADocumentationComment` (spec example) | RH5204-006; FC-007 | `Verify`, both | fail |
| `FixMovesTheFirstTypeOfAFileBehindADirective` | RH5204-006; FC-007 | `Verify`, both | fail |
| `FixMovesTheFirstTypeOfAFileWithoutLeadingTrivia` | RH5204-006; FC-007 | `Verify`, both | fail |
| `FixMovesTheFirstTypeOfAFileBehindALineCommentAndKeepsItsBody` | RH5204-006; FC-007 | `Verify`, both | fail |
| `FixMovesANamespaceThatStartsTheFileAndKeepsItsBody` | RH5204-006; FC-007 | `Verify`, both | fail |
| `FixMovesAMisindentedCommentThatStartsTheFile` | RH5204-006; FC-007 | `Verify`, both | fail |
| `FixMovesALaterTopLevelTypeAndLeavesTheTypeInFrontOfItAsWritten` | RH5204-006; FC-030 | `Verify`, both | fail |
| `FixMovesATypeInAFileScopedNamespaceAndKeepsItsEmptyBody` | RH5204-006; FC-030 | `Verify`, both | fail |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| first type behind `///`, `#nullable enable`, nothing | LF, CRLF | code fix | reproduced | observed: line keeps its four spaces and the class becomes `class C;` (no progress); expected: column 0, body as written |
| `// header⏎    class C⏎{ int _a; }` | LF, CRLF | code fix | reproduced | observed: class stays at column 4, body re-indented; expected: column 0, `{ int _a; }` as written |
| `    namespace N⏎{ class C { } }` | LF, CRLF | code fix | reproduced | observed: stays at column 4, body reformatted; expected: column 0 |
| `    // header⏎class C⏎{⏎}` | LF, CRLF | code fix | reproduced | observed: comment stays, `class C;`; expected: comment at column 0, class as written |
| later top-level type, file-scoped namespace | LF, CRLF | code fix | reproduced | observed: whole file formatted (`int _a`, `class B;`); expected: only the reported line |

## thoenissen/Reihitsu#1561

### Tests

All in `RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests`; each asserts LF and CRLF.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `CaseLabelBehindTheClosingLineOfAMultiLineDocumentationCommentIsNotReported` (issue shape, `/** */`) | RH5204-001, SHARED-BLANK-008; FC-013, FC-040 | `Verify` (analyzer) | fail |
| `CaseLabelBehindTheClosingLineOfAMultiLineBlockCommentIsNotReported` | RH5204-001, SHARED-BLANK-008; FC-040 | `Verify` (analyzer) | fail |
| `ClosingBraceBehindTheClosingLineOfAMultiLineCommentOnItsOwnLineIsNotReported` | RH5204-001, SHARED-BLANK-008 | `Verify` (analyzer) | fail |
| `OpeningBraceBehindTheClosingLineOfAMultiLineCommentIsNotReported` | RH5204-001, SHARED-BLANK-008 | `Verify` (analyzer) | fail |
| `ElseBehindTheClosingLineOfAMultiLineCommentIsNotReported` | RH5204-001, SHARED-BLANK-008 | `Verify` (analyzer) | fail |
| `CatchBehindTheClosingLineOfAMultiLineCommentIsNotReported` | RH5204-001, SHARED-BLANK-008 | `Verify` (analyzer) | fail |
| `ReturnBehindTheClosingLineOfAMultiLineCommentIsNotReported` | RH5204-001, SHARED-BLANK-008 | `Verify` (analyzer) | fail |
| `MemberBehindTheClosingLineOfAMultiLineCommentIsNotReported` | RH5204-001, SHARED-BLANK-008 | `Verify` (analyzer) | fail |
| `MisindentedMultiLineCommentInFrontOfAStatementBehindItsClosingLineIsReportedAndFixed` (comment out of place: reported on the comment only, fix moves only its first line) | RH5204-002, RH5204-006; FC-021 | `Verify`, both | fail |

### Scenarios

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `case`, `}`, `{`, `else`, `catch`, `return`, `private int _b;` behind `b */` at column 0, comment in place | LF, CRLF | analyzer | reproduced | observed: one RH5204 on the token; expected: none |
| comment at column 4 in a method body, `b */ return;` | LF, CRLF | analyzer | reproduced | observed: two diagnostics (comment and `return`); expected: one, on the comment |

## Not covered, and why

- **Existing SHARED-FIX-010 tests that assume the old scope, left unchanged for the lead to decide**: `RH5405…Tests.NoFixIsOfferedWhenTheIfStatementHoldsACommentTheFormatterRefuses`, `RH5403…Tests.NoFixIsOfferedWhenTheOwningStatementHoldsACommentTheFormatterRefuses`, `RH6024…Tests.NoFixIsOfferedWhenTheBinaryExpressionHoldsACommentTheFormatterRefuses`. Each asserts that a refused comment in another block or operand withholds the fix, and its comment explains this by the old scope ("the fix formats the whole if statement" / "the binary expression"). SHARED-BRACE-002, RH5403-003 and SHARED-OPSPACE-004 no longer require that scope, and SHARED-FIX-010 withholds only for the node the formatter actually receives, so whether these stay green depends on the implementation, not on a requirement. If the implementer formats only the reported body/block or edits only the gaps, they turn red and should be rewritten to "fix offered" (as I did for RH5204, where RH5204-006 formats nothing and the contradiction is direct).
- **`RH5204…Tests.VerifyMultiLineSingleLineDocumentationCommentIsDetectedAndFixed`**, left unchanged: it asserts that every line of a three-line `///` comment moves with the reported first line. RH5204-006's sentence "the second line of a comment that spans several lines keeps its column" could be read as covering a `///` run (one trivia in Roslyn), which would contradict this test; the spec author did not name it. The lead decides the reading.
- **`RH5204…Tests.GetFixAllScenario`** sets `NumberOfFixAllIterations = 2` (an exact count in the testing framework). With every line fixed by a whitespace replacement, Fix All may converge in one iteration and trip that count. That is a test-mechanics count, not a requirement; the implementer may need to relax it to `-2` (at most two) through the lead.
- **RH5205-004, other side** (a lambda without an attribute list is still fixed): covered by the existing RH5205-003 tests in the same class (for example `VerifyIssueIsDetectedAndFixed`); no new test.
- **RH5103-003 third statement of `M(); M(); M();`**: covered by the existing `ThreeStatementChainFixAllKeepsBlockIndentation`; no new test.
- **SHARED-ATTR-021 in the other twelve placement rules**: the guard is specified in the shared base `TargetAttributePlacementCodeFixProviderBase`; I covered RH5513 (lambda block) and RH5517 (type brace) as two different shapes and two rules. The other rules' targets (assembly, module, class, struct, enum, constructor, property, event, interface, delegate, return value, parameter-like) cannot sit in a lambda block, and the type-brace shape is identical for them; not repeated per rule.
- **Formatter second-pass idempotency / combined-pipeline tests**: no formatter phase changes in this run (only code fixes and the RH5204 analyzer). Where the expected output is the formatter's layout for the whole file, the test also calls `AssertFormattingChangesNothing`.
- **Naming fixes, Renamer**: no naming fix involved.
- **Unsupported shapes (FC-038 to FC-043)**: no shape is declared in this run (answer 4A).
- **Directive in an operator gap (RH6023/RH6024)**: already covered by `RH6024…Tests.VerifyGapWithDirectiveIsNotReported` (and CRLF); the fix is not offered where no diagnostic exists.
- **#1140 instances left on the issue by decision 3** (RH5101/5102 content, RH5110, RH5111, RH5112/RH5201, RH5113, RH5206, RH5303–RH5308, RH5401, RH5404, RH5408, RH5412–RH5416, RH3201 Q2): no tests, as decided; `RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzerTests.VerifyBlankLineAboveCommentOfFixedClassInFileScopedNamespaceIsInserted` stays as it is.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5405BracesMustNotBeOmittedAnalyzerTests|FullyQualifiedName~RH5406BracesMustNotBeOmittedFromMultiLine|FullyQualifiedName~RH5407UseBracesConsistentlyAnalyzerTests|FullyQualifiedName~RH5403StatementMustNotBeOnSingleLineAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzerTests|FullyQualifiedName~RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5205Statement|FullyQualifiedName~RH5203Multi|FullyQualifiedName~RH5417Function|FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH5409Final|FullyQualifiedName~RH3102Code|FullyQualifiedName~RH3201Comments|FullyQualifiedName~RH5101First|FullyQualifiedName~RH5102Arguments|FullyQualifiedName~RH3106Unn|FullyQualifiedName~RH3107Unn"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5103Code|FullyQualifiedName~RH5513Method|FullyQualifiedName~RH5517Field|FullyQualifiedName~RH5104Comments"
scripts/test.sh --project architecture
```
