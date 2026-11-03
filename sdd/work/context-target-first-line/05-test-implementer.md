Role: test implementer
Commit: d99bdb2
Verdict: tests written

The prompt carried no intended fix or hoped-for verdict. Expected values come from the FMT-PIPE-017 and RH5111-004 examples and the change doc's "after" outputs 1 to 18, which the maintainer chose (A on all 18, `00-lead.md`). Every input is one a code fix reaches: the RH5111 fix (declarator with its declaration, parameter with its parameter list, enum member with its enum) or the RH3208 fix (local function with the labeled statement that starts its line). The formatter-level tests call `FormatNodeInDocumentWithContextAsync` with exactly those target and context nodes.

Every new test that asserts a changed output was run on `d99bdb2` (code equal to `main`) and fails only on the line the change doc names: the observed column is the change doc's "before" column in every case, and the diagnostics match (each analyzer failure is in "Iterative code fix application"). Every test for unchanged output passes.

## Tests

Files:
- `src/Reihitsu.Analyzer.Test/Formatting/RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5203MultiLineArgumentsShouldBeAlignedAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests.cs`
- `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeColumnTests.cs`

All analyzer tests use `Verify(source, fixedSource, …)` (diagnostic, fix result, no RH5111/RH3208 diagnostic on the result). "LF+CRLF" means the test also runs the CRLF-normalized pair, via the class's existing `VerifyUnderBothLineEndings` helper in RH5111. The formatter tests use `AssertFormatsTargetWithContext`, which runs LF and CRLF and asserts that a second pass changes nothing.

### RH5111 code fix — changed outputs (change doc numbers)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyDeclaratorOnALaterLineKeepsItsColumnAndKeepsTheWrappedArgumentUnderTheFirstArgument` (change 1; replaces the characterization test `VerifyDeclaratorOnALaterLineKeepsItsFirstLineColumnAndLeavesTheWrappedArgumentInTheFormattersLayout`) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007 | Verify, LF+CRLF | fail: `2` col 18, want 24 |
| `VerifyDeclaratorOnALaterLineKeepsTheBracesOfAnObjectCreationUnderNew` (2) | RH5111-004, FMT-PIPE-017 | Verify | fail: braces col 12, want 18 |
| `VerifyDeclaratorOnALaterLineKeepsTheBlockOfALambdaUnderItsParameterList` (2) | RH5111-004, FMT-PIPE-017 | Verify | fail: braces col 12, want 18 |
| `VerifyEnumMemberOnALaterLineKeepsItsColumnAndKeepsTheWrappedOperatorUnderTheValue` (3) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007 | Verify | fail: `+` col 8, want 10 |
| `VerifyParameterOnALaterLineKeepsItsColumnAndKeepsTheWrappedOperatorUnderTheValue` (3) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007 | Verify | fail: `+` col 22, want 27 |
| `VerifyParameterBehindAnotherParameterOnItsLineStaysOnThatLineAndKeepsTheWrappedOperatorUnderTheValue` (4; RH5111-004 example) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007, FMT-PIPE-024 | Verify, LF+CRLF | fail: `+` col 22, want 29 |
| `VerifyDeclaratorBehindTheClosingLineOfARawStringKeepsTheWrappedOperatorUnderTheValue` (5) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007 | Verify, LF+CRLF | fail: `+` col 28, want 21 |
| `VerifyFirstDeclaratorOnTheLineAfterItsTypeKeepsTheWrappedArgumentUnderTheFirstArgument` (7) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007 | Verify | fail: col 18, want 22 |
| `VerifyFieldDeclaratorOnALaterLineKeepsTheWrappedArgumentUnderTheFirstArgument` (8) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007 | Verify | fail: col 15, want 34 |
| `VerifyForInitializerDeclaratorOnALaterLineKeepsTheWrappedArgumentUnderTheFirstArgument` (9) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007 | Verify | fail: col 18, want 27 |
| `VerifyDeclaratorOnALaterLineOfATabIndentedDeclarationKeepsTheWrappedArgumentUnderTheFirstArgument` (10) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007, FMT-PIPE-018 | Verify | fail: 12 spaces, want 16 |
| `VerifyFirstParameterOnTheLineAfterTheOpeningParenthesisStaysOnItsLineAndKeepsTheWrappedOperatorUnderTheValue` (11) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007, FMT-PIPE-024 | Verify, LF+CRLF | fail: col 19, want 16 |
| `VerifyEnumMemberBehindTheOpeningBraceStaysOnItsLineAndKeepsTheWrappedOperatorUnderTheValue` (12) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007, FMT-PIPE-024 | Verify | fail: col 8, want 13 |
| `VerifyFirstDeclaratorBehindItsTypeAndSeveralSpacesKeepsTheWrappedArgumentUnderTheFirstArgument` (13) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007 | Verify | fail: col 22, want 25 |
| `VerifyFirstParameterBehindTheOpeningParenthesisAndSeveralSpacesKeepsTheWrappedOperatorUnderTheValue` (14) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007 | Verify | fail: col 19, want 22 |
| `VerifyParameterBehindAnotherParameterAndSeveralSpacesKeepsTheWrappedOperatorUnderTheValue` (15) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007 | Verify | fail: col 22, want 32 |
| `VerifyDeclaratorBehindAnotherDeclaratorAndSeveralSpacesKeepsTheWrappedArgumentUnderTheFirstArgument` (16) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007 | Verify | fail: col 29, want 32 |
| `VerifyEnumMemberBehindAnotherEnumMemberAndSeveralSpacesKeepsTheWrappedOperatorUnderTheValue` (17) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007 | Verify | fail: col 15, want 18 |
| `VerifyParameterBehindTheClosingLineOfARawStringKeepsTheWrappedOperatorUnderTheValue` (18) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007 | Verify | fail: col 22, want 24 |

### RH5111 code fix — trivia above a target, and Fix All

These outputs follow from changes 3 and 1 and FMT-PIPE-018 together. The trivia stays as it is today, and only the aligned line moves.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyParameterOnALaterLineBehindACommentLineKeepsTheCommentAndKeepsTheWrappedOperatorUnderTheValue` | RH5111-004, FMT-PIPE-017, SHARED-FIX-007, FMT-PIPE-018; FC-004 | Verify | fail: `+` col 22, want 27 |
| `VerifyEnumMemberWithDocumentationCommentBehindThePreviousMemberKeepsThatLineAndKeepsTheWrappedOperatorUnderTheValue` | same; FC-019/FC-021 | Verify, LF+CRLF | fail: `+` col 8, want 10 |
| `VerifyEnumMemberBehindConditionalDirectiveKeepsTheDirectiveAndKeepsTheWrappedOperatorUnderTheValue` | same; FC-020 | Verify | fail: `+` col 8, want 10 |
| `VerifyTwoEnumMembersOnLaterLinesAreFixedTogetherAndKeepTheWrappedOperatorsUnderTheirValues` (2 diagnostics; the verifier also runs Fix All) | RH5111-004, FMT-PIPE-017, SHARED-FIX-007; FC-007 | Verify | fail: both `+` col 8, want 10 |

### RH5111 code fix — unchanged outputs (the change doc's *Unchanged* line, skeptic's p1, p4, p5, j4, s4)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFirstParameterOnTheLineAfterTheOpeningParenthesisInTheJoinedColumnKeepsTheWrappedOperatorUnderTheValue` (p1) | RH5111-004, FMT-PIPE-017; FC-003 | Verify | pass |
| `VerifyDeclaratorOneSpaceBehindAnotherDeclaratorKeepsTheWrappedArgumentUnderTheFirstArgument` (p4) | RH5111-004, FMT-PIPE-017; FC-003 | Verify | pass |
| `VerifyEnumMemberOneSpaceBehindAnotherEnumMemberKeepsTheWrappedOperatorUnderTheValue` (p5) | RH5111-004, FMT-PIPE-017; FC-003 | Verify | pass |
| `VerifyParameterOnALaterLineInTheColumnOfTheFormatterKeepsTheWrappedOperatorUnderTheValue` (j4) | RH5111-004, FMT-PIPE-017; FC-003 | Verify | pass |
| `VerifyDeclaratorBehindTheLastLineOfAMultiLineCommentKeepsTheWrappedArgumentUnderTheFirstArgument` (s4) | RH5111-004, FMT-PIPE-017 | Verify | pass |

### RH3208 code fix

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyLocalFunctionBehindALabelAndSeveralSpacesKeepsItsWrappedParameterUnderTheFirstParameter` (change 6; FMT-PIPE-017 second-way example) | RH3208-003, FMT-PIPE-017, SHARED-FIX-007 | Verify, LF+CRLF | fail: `int b` col 17, want 20 |
| `VerifyLocalFunctionOneSpaceBehindALabelKeepsItsWrappedParameterUnderTheFirstParameter` (FMT-PIPE-017 other-side example) | RH3208-003, FMT-PIPE-017; FC-003 | Verify | pass |
| `VerifyLocalFunctionDirectlyBehindALabelKeepsItsWrappedParameterUnderTheFirstParameter` (p7) | RH3208-003, FMT-PIPE-017; FC-003 | Verify | pass |

### Other rules on the fix's output (FC-007)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5203…Tests.VerifyNoDiagnosticsForWrappedArgumentOfADeclaratorOnALaterLine` (change 1 "after") | FC-007, untagged | Verify(source) | pass |
| `RH5301…Tests.VerifyNoDiagnosticsForObjectCreationOfADeclaratorOnALaterLine` (change 2 "after") | FC-007, untagged | Verify(source) | pass |

### Formatter, `FormatNodeInDocumentWithContextAsync` with the code fixes' target and context (LF, CRLF, second pass)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `DeclaratorTargetOnALaterLineOfItsContextKeepsItsColumnAndAlignsTheWrappedArgumentUnderTheFirstArgument` | FMT-PIPE-017; FC-010 | AssertFormatsTargetWithContext | fail: col 18, want 24 |
| `DeclaratorTargetOnALaterLineOfATabIndentedContextAlignsTheWrappedArgumentWithEachTabInFrontOfItAsOneColumn` | FMT-PIPE-017, FMT-PIPE-018 | same | fail: 12 spaces, want 16 |
| `FirstParameterTargetOnTheLineAfterTheOpeningParenthesisIsNotJoinedAndAlignsTheWrappedOperatorUnderTheValue` | FMT-PIPE-017, FMT-PIPE-024 | same | fail: col 19, want 16 |
| `ParameterTargetBehindAnotherParameterIsNotBrokenOffItsLineAndAlignsTheWrappedOperatorUnderTheValue` | FMT-PIPE-017, FMT-PIPE-024 | same | fail: col 22, want 29 |
| `EnumMemberTargetBehindTheOpeningBraceIsNotBrokenOffItsLineAndAlignsTheWrappedOperatorUnderTheValue` | FMT-PIPE-017, FMT-PIPE-024 | same | fail: col 8, want 13 |
| `DeclaratorTargetBehindItsTypeAndSeveralSpacesKeepsThemAndAlignsTheWrappedArgumentUnderTheFirstArgument` | FMT-PIPE-017 | same | fail: col 22, want 25 |
| `LocalFunctionTargetBehindALabelAndSeveralSpacesKeepsThemAndAlignsTheWrappedParameterUnderTheFirstParameter` (input: the block-bodied function the RH3208 fix builds) | FMT-PIPE-017 | same | fail: col 17, want 20 |
| `DeclaratorTargetBehindTheClosingLineOfAMovingRawStringKeepsItsColumnAndAlignsTheWrappedOperatorUnderTheValue` | FMT-PIPE-017; FC-022 | same | fail: col 28, want 21 |

### Removed and edited existing tests

- Removed from `ReihitsuFormatterNodeColumnTests`: `ArgumentTargetOnALaterLineOfItsContextKeepsItsFirstLineColumnAndLeavesTheAlignedLineInThePhaseLayout` and `InvocationTargetInsideAMovingRawStringLiteralTakesThePhasesColumnOnItsSecondLine`. No code fix passes these targets, and the change contradicts their expectations (`00-lead.md`, decision 4). The code-fix-shaped tests above cover the three ways instead.
- Replaced in RH5111: the untagged characterization test of change 1's "before". It is now the first row above, with the specified output.
- Comment-only edits (FC-031) to three remaining formatter tests whose summaries cited "the gap FMT-PIPE-017 records": `TargetStartingTheFirstLineOfItsContextIsLaidOutFromTheWhitespaceInFrontOfIt`, `RawStringLiteralTargetWhoseLinesMoveIsFormattedUnderItsOpeningQuotes`, and `InvocationTargetInsideAnAlignedRawStringLiteralPlacesItsSecondLineUnderTheFirstArgument`. Their assertions are unchanged.
- In RH5111, the existing private helper `VerifyUnderBothLineEndings` moved to the end of the `Tests` region, because RH7110 requires the public tests to come before it. Its body is unchanged.

## Not covered, and why

- FC-032, RH5112/RH5201 fluent-chain fixes: no new test. The existing tests in `RH5112*`/`RH5201*` are the check, and on `d99bdb2` they pass (141 passed). They must still pass after the implementation.
- FMT-PIPE-018's two existing examples and the existing trivia tests (blank lines, region, documentation comment, several comments, tab-indented one-line value) are unchanged and passing. They are the "must stay" side for the trivia, so I added no duplicates without a wrapped value.
- The skeptic's s2 and s5 (parameter or declarator behind a sibling with extra spaces) are changes 15 and 16, both covered. s3 is change 14. j1 and j2 are change 11. j3 is change 9. f1 is change 8. t1 is change 10. p2 is change 7. p3 is change 12. s1 is change 13. s7 is change 18. f2 and j5 have the same shape as p4 and j4. p6 (one-line value) is covered by the existing `VerifyParameterOnALaterLineIsJoinedInItsColumn`, `VerifyEnumMemberOnALaterLineIsJoinedInItsColumn`, and the tab one-line test. t2 is covered by the existing `VerifyParameterBehindTwoBlankLinesAndCommentKeepsThemAsInTheDocument`.
- Declarators in `using`/`fixed` statements and event fields: the RH5111 fix passes them through the same `Parent.Parent` arm of `GetFormattingContextNode` as the local declaration and the `for` initializer, which are tested. The skeptic named that arm in `02-skeptic.md`. I did not re-prove the shared call site with `rg`, so the case rests on the skeptic's trace and is not a static proof.
- Fix All for RH3208 with two labeled functions in one block: not added. One Fix All test covers RH5111, and the existing RH3208 Fix All scenario covers the batch path. The labeled case only adds another target of the same entry point.
- Comment or directive inside the gap in front of a mid-line target (for example `int b, /* x */ string c`): not added. RH5111-005 governs comments inside the construct, and the gap in front of the target is outside the target and written back as in the document (FMT-PIPE-010). I did not check whether the fix is offered there, so this is a candidate the auditor may want to probe.
- Naming and Renamer: not applicable.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests|FullyQualifiedName~RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH5203MultiLineArgumentsShouldBeAlignedAnalyzerTests|FullyQualifiedName~RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5112|FullyQualifiedName~RH5201"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.ReihitsuFormatterNodeColumnTests"
scripts/test.sh --project architecture
```

Results on `d99bdb2`:
- First command: 24 failed (the 23 RH5111 and 1 RH3208 "fail" rows), 154 passed.
- RH5112/RH5201: 141 passed.
- Formatter: 8 failed (the "fail" rows), 36 passed.
- Architecture: 92 passed, requirement tags included.
