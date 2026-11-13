Role: test implementer
Commit: 6863ef8 (production as of ba83087; the commit carrying these tests follows it)
Verdict: tests written

Repair work for decisions 20–23 of `00-lead.md`. The prompt carried no intended fix, cause, or hoped-for verdict beyond those decisions. No production code was touched. I staged only test files and this handover; the spec author's files are not part of the commit. `scripts/format.sh` changed none of the 12 test files. Every expected value below comes from the requirement named in its row, not from the current output. Where the current output already matches, the row says so.

Run on this head (Release): the 12 affected test classes give 374 tests, 7 failed, 367 passed. The 7 failures are the brace-rule tests restored to `main` (decision 20). They fail only because the current production still has the narrowed brace fixes. They turn green once the implementer reverts that production. `scripts/test.sh --project architecture`: 117/117 passed.

## thoenissen/Reihitsu#1140

### Decision 20: brace fixes back to `main`

`RH5403StatementMustNotBeOnSingleLineAnalyzerTests.cs`, `RH5405BracesMustNotBeOmittedAnalyzerTests.cs`, `RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzerTests.cs`, and `RH5407UseBracesConsistentlyAnalyzerTests.cs` are restored to `origin/main` (`d6a01ee`, also the merge base) with `git checkout origin/main -- <file>`. That covers the #1144 `else if` tests in those files too.

Removed (added by this run):
- RH5403: `FixLaysOutOnlyTheReportedBlocksAndLeavesTheRestOfTheStatementAsWritten`, `FixOnTheElseBlockLeavesTheIfBlockAsWritten`, `FixInsertsNoBlankLineInFrontOfTheStatement`, `FixLaysOutTheBlockOfAnElseIfInTheColumnOfTheElse`
- RH5405: `FixWrapsOnlyTheReportedBodyAndLeavesTheRestOfTheStatementAsWritten`, `FixLeavesTheConditionAndTheFollowingStatementAsWritten`, `FixPlacesTheBracesOfAnElseIfBodyInTheColumnOfTheElse`, `FixPlacesTheBracesOfANestedElseIfBodyOnTheLineOfItsConditionInTheColumnOfTheElse`
- RH5406: `FixPlacesTheBracesOfAMultiLineElseIfBodyInTheColumnOfTheElse`
- RH5407: `FixPlacesTheBracesOfAnElseIfBodyInTheColumnOfTheElse`

Restored to `main`'s name and content: the three RH5405 blank-line tests (`VerifyBlankLineIsInsertedAboveFixedStatementAfterDeclaration`, `…AfterClosingBrace`, `VerifyBlankLinesAreInsertedAboveAdjacentFixedStatements`) and the `ElseIfWithTrailingCommentFixedCode` constant. The SHARED-FIX-010 tests of decision 15 (`RH5405…NoFixIsOfferedWhenTheIfStatementHoldsACommentTheFormatterRefuses`, `RH5403…NoFixIsOfferedWhenTheOwningStatementHoldsACommentTheFormatterRefuses`) are `main`'s and unchanged.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5405…Tests.VerifyBlankLineIsInsertedAboveFixedStatementAfterDeclaration` (main) | as on `main` | `Verify` | fail until the production revert (observed: no blank line above the braced `if`) |
| `RH5405…Tests.VerifyBlankLineIsInsertedAboveFixedStatementAfterClosingBrace` (main) | as on `main` | `Verify` | fail until the revert |
| `RH5405…Tests.VerifyBlankLinesAreInsertedAboveAdjacentFixedStatements` (main) | as on `main` | `Verify` | fail until the revert |
| `RH5405…Tests.VerifyFixKeepsTheTrailingCommentOfAnElseIfCondition` and `…WithCarriageReturnLineFeed` (main) | as on `main` | `Verify` | fail until the revert (`else if` brace column) |
| `RH5405…Tests.NoFixIsOfferedWhenTheIfStatementHoldsACommentTheFormatterRefuses` (main) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | fail until the revert (observed: one fix offered) |
| `RH5403…Tests.NoFixIsOfferedWhenTheOwningStatementHoldsACommentTheFormatterRefuses` (main) | SHARED-FIX-010 | `VerifyNoCodeFixIsOffered` | fail until the revert (observed: one fix offered) |
| every other test in the four files (main) | as on `main` | as on `main` | pass |
| `RH5204…Tests.VerifyEmbeddedIfBehindMultiLineCommentIsFixedWithOneApplicationPerDiagnostic` and `…WithCarriageReturnLineFeed` (renamed from `…IsFixedInOnePass`) | RH5204-006, SHARED-FIX-001 | `Verify` with `NumberOfIncrementalIterations = 2`, `NumberOfFixAllIterations = 1` | pass |
| `RH5204…Tests.GetFixAllScenario` (`FixAllCorrectsEveryDiagnostic`) | SHARED-FIX-002 (not tagged: base-class scenario) | `NumberOfFixAllIterations = 1` | pass |
| `RH5204…Tests.VerifyClosingBraceBehindStrayDocumentationCommentAndBlankLineIsDetectedAndFixed` (expected keeps the blank line; tag RH5204-006 added) | RH5204-001, RH5204-006 | `Verify` | pass |

Derivations:
- **Incremental count.** The input has two diagnostics, `{` and `}`. RH5204-006 lets each fix move only its own line, and SHARED-FIX-001 allows one application per diagnostic, so the count is 2. Fix All stays at 1 (SHARED-FIX-002).
- **Fix All scenario.** I set it to exactly 1, not to "at most two" as decision 17 allowed. SHARED-FIX-002 requires one application, and the scenario holds no guard that releases late. The test name changed because "in one pass" no longer describes the incremental count.
- **Blank line.** RH5204-006 says the fix MUST NOT change the blank lines around the line. Expected: `internal int A; /// x⏎⏎}`, with `}` in column 0.

## thoenissen/Reihitsu#1141

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH6024…Tests.FixIsOfferedWhenAnOperandHoldsACommentTheFormatterRefuses` (rewritten from `NoFixIsOfferedWhenTheBinaryExpressionHoldsACommentTheFormatterRefuses`; the SHARED-FIX-010 tag is replaced) | SHARED-OPSPACE-004 | `VerifyUnderBothLineEndings` | pass |
| `RH6023…Tests.VerifyFixKeepsTheWrappedLinkOfACompoundAssignedMemberBehindAMemberBinding` (`.C` keeps column 9) | RH6023-002, SHARED-OPSPACE-004 | `VerifyUnderBothLineEndings` | pass |
| `RH6023…Tests.VerifyFixKeepsTheWrappedLinkOfAnAssignedMemberInANestedConditionalAccess` (`.C` keeps column 12) | RH6023-002, SHARED-OPSPACE-004 | `VerifyUnderBothLineEndings` | pass |

Derivations:
- **RH6024.** SHARED-OPSPACE-004 lets the fix change only the gaps next to the operator, so `}){|RH6024:+|}1;` becomes `}) + 1;` and the lambda, with its `int /* c */ y`, stays as written. SHARED-FIX-010 applies only to a fix that formats through the formatter, and this fix no longer does, so nothing withholds it.
- **RH6023.** SHARED-OPSPACE-004 says the operands keep their layout, so the wrapped link keeps the column it was written in. Previously the expectation was column 10 and column 13.
- **Tags.** The FMT-PIPE-024 tag is dropped on these two tests: the fix no longer calls a node-level entry point, so the requirement is not exercised. The other six RH6023 tests tagged FMT-PIPE-024 now touch it only incidentally. They stay green and I did not change them; that is for the spec checker to note.

## thoenissen/Reihitsu#1144

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5203…Tests.FixMovesACommentLineAboveTheArgumentIntoTheAlignmentColumn` (`AssertFormattingChangesNothing` dropped, decision 22; both `Verify` calls unchanged) | RH5203-003 | `VerifyUnderBothLineEndings` | pass |
| `RH3102…Tests.FixOfEmptyStatementBehindAStatementKeepsTheNextLineOnItsOwn` (was the characterization `…JoinsTheNextLine`) | SHARED-FIX-007 | `ApplyCodeFixAsync` | pass |
| `RH3201…Tests.EmptyMultiLineCommentWithNewlinesIsReportedAndFixed` (no empty line behind `{`; tag SHARED-FIX-007 added) | RH3201-001, RH3201-004, SHARED-FIX-007 | `Verify` | pass |
| `RH3201…Tests.FixOfBlockCommentSpanningSeveralLinesLeavesNoEmptyLine` (was the characterization `…LeavesAnEmptyLine`) | SHARED-FIX-007 | `ApplyCodeFixAsync` | pass |
| `RH5409…Tests.FixKeepsTheGapsAroundTheRemovedCommaInFrontOfATrailingLineComment` (new, decision 23: `B , // c` gives `B  // c`) | SHARED-FIX-007 | `VerifyUnderBothLineEndings`, `AssertFormattingChangesNothing` | pass |
| `RH5409…Tests.FixKeepsTheGapInFrontOfATrailingLineCommentDirectlyBehindTheRemovedComma` (new: `B ,// c` gives `B // c`) | SHARED-FIX-007 | same | pass |
| `RH5409…Tests.FixKeepsTheGapsAroundTheRemovedCommaInFrontOfATrailingBlockComment` (new: `B , /* c */` gives `B  /* c */`) | SHARED-FIX-007 | same | pass |
| `RH5409…Tests.FixLeavesNoTrailingWhitespaceWhereTheCommaStoodBehindABlockComment` (new: `B /* c */ ,` gives `B /* c */`) | SHARED-FIX-007 | same | pass |

Derivations:
- **RH3102 and RH3201.** The expected values are those of the run's SHARED-FIX-007 tests `FixKeepsTheNextStatementOnItsLine` and `FixLeavesNoEmptyLineBehindTheOpeningBraceWhereAMultiLineCommentStood`. The same reasoning applies to an empty line behind a type's `{`. The RH3102 test now has the same input as `FixKeepsTheNextStatementOnItsLine` and asserts the same output through another helper. It is redundant, and the lead may delete it.
- **Decision 23.** `scripts/format.sh` on the original inputs gives `B  // c`, `B // c`, `B  /* c */`, and `B /* c */`. On these inputs the formatter removes the comma and keeps the whitespace on both sides of it. Formatting the expected outputs again changes nothing. The suspected `B  // c` is therefore the formatter's own layout, which SHARED-FIX-007 asks for. All four tests are green on the current production, so no defect was found. `B ,  // c` also formats to `B   // c`, but I did not add a test for it: it is the same branch as `B , // c`.

## thoenissen/Reihitsu#1145

No test changed in this round.

## thoenissen/Reihitsu#1094

### Tests

All in `RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzerTests` (decision 21). Each test now expects the attribute list and the constructor on lines of their own, at column 4.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyCodeFixIndentsAtNestingDepthWhenPrecededByOtherSourceOnSameLine` (`private int _pad;⏎    [First]⏎    internal Example() { }`) | SHARED-FIX-007 | `Verify` | pass |
| `VerifyCodeFixUsesSpacesRegardlessOfSurroundingTabIndentation` (`\tprivate int _pad;\n    [First]\n    internal Example() { }`) | SHARED-FIX-007 | `Verify` | pass |
| `VerifyCodeFixIndentsCorrectlyWhenLineBeginsInsideMultiLineVerbatimString` (`b";⏎    [First]⏎    internal …`) | SHARED-FIX-007 | `Verify` | pass |
| `VerifyCodeFixIndentsCorrectlyWhenAttributeListFollowsMultiLineParameterList` (`int b) { }⏎    [First]⏎    internal …`) | SHARED-FIX-007 | `Verify` | pass |
| `VerifyCodeFixIndentsCorrectlyWhenAttributeListFollowsMultiLineInitializer` (`1 + 2;⏎    [First]⏎    internal …`) | SHARED-FIX-007 | `Verify` | pass |

Derivations:
- **Shared line.** Under SHARED-FIX-007, the changed member must not share its line with the member in front of it. The edited lines are laid out as the formatter lays them out: the list starts on the next line in the declaration's column, and no whitespace ends the line before it. The new RH5513 test `FixPutsTheAttributeListBehindAFieldOnALineOfItsOwn` expects the same.
- **Tab.** Under the whitespace bullet of SHARED-FIX-007, the tab in front of `private` is not part of the changed code, so it stays. The new lines get spaces.
- **Line endings.** These five tests stay LF-only, as they were on `main`. The CRLF variant of the shared base's path is covered by the RH5513 test.

## thoenissen/Reihitsu#1393

No test changed in this round.

## thoenissen/Reihitsu#1511

No test changed in this round. The RH5204 changes are listed under #1140.

## thoenissen/Reihitsu#1561

No test changed in this round.

## Whole run

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH5409 `B , // c`, `B ,// c`, `B , /* c */`, `B /* c */ ,` | LF, CRLF | code fix and formatter | no defect | The fix output equals `scripts/format.sh` on the original input, and formatting it again changes nothing |

### Red tests on this head and why

1. **Seven RH5405/RH5403 tests from `main`.** They fail only because `StatementBracesCodeFixProviderBase` and `RH5403…CodeFixProvider` at `ba83087` still carry the narrowed scope: no blank line above the braced `if`, the `else if` braces at the `else` column, and the fix offered despite the refused comment. These are the tests the implementer's revert under decision 20 must turn green. They are listed under #1140.
2. **Nothing else.** All 367 other tests in the 12 classes pass, including every test rewritten or added here.

### Not covered, and why

- **`RH5204…Tests.VerifyMultiLineSingleLineDocumentationCommentIsDetectedAndFixed`** stays unchanged. Decision 16 and RH5204-006 now say that a `///` run moves as one comment.
- **RH5410/RH5411.** These use the same trailing-comma helper, so I did not repeat the decision 23 shapes for them. `rg "TrailingCommaCodeFixHelper" src` finds one caller, `TrailingCommaRemovalCodeFixProviderBase`, and every trailing-comma rule derives from it with no override of the removal.
- **Formatter idempotency and CRLF beyond the above.** No formatter phase changed.

### Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5403StatementMustNotBeOnSingleLine|FullyQualifiedName~RH5405BracesMustNotBeOmitted|FullyQualifiedName~RH5406BracesMustNotBeOmitted|FullyQualifiedName~RH5407UseBracesConsistently"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5204Indentation|FullyQualifiedName~RH6023Assignment|FullyQualifiedName~RH6024Binary"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5203Multi|FullyQualifiedName~RH3102Code|FullyQualifiedName~RH3201Comments|FullyQualifiedName~RH5409Final|FullyQualifiedName~RH5511Constructor"
scripts/test.sh --project architecture
```
