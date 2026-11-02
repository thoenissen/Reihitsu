Role: test implementer
Commit: fb5fdb8
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec diff (aec8dc6..754c3a5) and the change doc. Where the change doc says an output stays as on `main`, the expected value is `main`'s output, run through `scripts/format.sh` on 754c3a5 and checked stable on a second run. A throwaway worktree, deleted afterwards, had FMT-BREAK-005's move disabled, and later the phase order swapped. I used it only to find existing tests that assert retired behavior and to check that the new tests catch the skeptic's failing class. No expected value comes from it (FC-035).

## Tests

All 33 formatter failures and 5 analyzer failures below fail on 754c3a5 because of the issue's own difference: the comment moved above the `if`, the blank-line phase splitting lines, the old order, or the wrap dropping a blank line. No compile or helper failure is among them. The full formatter suite on this commit gives 33 failed and 4310 passed, all failures listed here. The architecture tests give 1 failure, `OwnershipMapMatchesRuntimePhaseOrder`, which was already failing; `RequirementCoverageTests` passes.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FormattingPipelineTests.ExecuteObserverReportsAllPhasesInOrderWithoutChangingOutput` (order list updated) | FMT-PIPE-001 | observer | fail |
| `BlockStatementHeadCommentTests.SpecExampleKeepsEveryHeadCommentBehindItsHead` | FMT-BREAK-036 | `AssertRuleResult` | fail |
| `BlockStatementHeadCommentTests.LineCommentBehindHeadWithBraceOnNextLineStays` × 8 heads (`if`, `else if`, `else`, `while`, `for`, `foreach`, `using`, `lock`) | FMT-BREAK-036 | `AssertRuleResult` | `if` row fails, 7 pass |
| `BlockStatementHeadCommentTests.DelimitedCommentBehindHeadWithBraceOnItsLineStays` × 8 heads | FMT-BREAK-036 | `AssertRuleResult` | `if` row fails, 7 pass |
| `BlockStatementHeadCommentTests.LineCommentBehindIfConditionBehindStatementStays` (change doc 1) | FMT-BREAK-036 | `AssertRuleResult` | fail |
| `BlockStatementHeadCommentTests.DelimitedCommentBehindIfConditionWithBodyOnItsLineStays` (change doc 2) | FMT-BREAK-036 | `AssertRuleResult` | fail |
| `BlockStatementHeadCommentTests.LineCommentBehindIfConditionInLoopBodyStays` (issue input, change doc 3) | FMT-BREAK-036 | `AssertRuleResult` | fail |
| `BlockStatementHeadCommentTests.LineCommentBehindIfConditionWithUnbracedBodyStays` (change doc 4) | FMT-BREAK-036 | `AssertRuleResult` | fail |
| `BlockStatementHeadCommentTests.LineCommentBehindFirstIfConditionStays` (`if // c` … `else // d`) | FMT-BREAK-036 | `AssertRuleResult` | fail |
| `ElseIfConditionCommentTests.TrailingCommentOnElseIfConditionDoesNotSplitElseIf` (retagged from FMT-BREAK-005) | FMT-BREAK-036 | `AssertRuleResult` | pass |
| `ElseIfConditionCommentTests.TrailingCommentOnPlainIfConditionStaysBehindCondition` (was `…MovesToOwnLine`, asserted the retired move) | FMT-BREAK-036 | `AssertRuleResult` | fail |
| `ReihitsuFormatterDetachedRootTests.ElseIfTargetKeepsTheLineCommentBehindItsCondition`, `…BlockComment…` (FMT-PIPE-024 tag replaced; the `else if` fact is gone) | FMT-PIPE-023, FMT-BREAK-036 | `FormatNodeInDocumentAsync` | pass |
| `ReihitsuFormatterDetachedRootTests.FormatNodeOnElseIfWithTrailingCommentGetsTheResultOfTheNodeWithoutParent` (expected `if (y) // note`, was `// note` above) | FMT-PIPE-012, FMT-BREAK-036 | `FormatNode` | fail |
| `PhaseLineBreakOwnershipTests.SwitchSectionsOnOneLineGetLinesAndBlankLineInOneRun` (issue comment's input) | FMT-PIPE-025 | `AssertRuleResult` | pass (holds on `main` too, as the skeptic traced) |
| `PhaseLineBreakOwnershipTests.CommentBehindIfConditionInLoopBodyIsKeptInOneRun` (issue input) | FMT-PIPE-026 | `AssertRuleResult` | fail |
| `PhaseLineBreakOwnershipTests.BlankLinePhaseKeepsWhichTokensShareALine` × 5 fixtures | FMT-PIPE-025 | pipeline observer | 3 fail (switch on one line, empty-statement pairs, same-line statements/sections), 2 pass |
| `PhaseLineBreakOwnershipTests.PhasesAfterBlankLinePhaseKeepLineBreaksAndBlankLines` × 5 fixtures | FMT-PIPE-025 | pipeline observer | 4 fail (the old order puts the line-break and switch phases after it), chain fixture passes |
| `PhaseLineBreakOwnershipTests.PhasesAfterLineBreakPhaseKeepWhetherCommentsStartTheirLine` × 4 fixtures | FMT-PIPE-026 | pipeline observer | pass |
| `EmptyStatementLineBreakTests.SpecExampleSplitsEmptyStatementPairsThatABlankLineSeparates` | FMT-BREAK-006 | `AssertRuleResult` | pass (`main` output; fails on the reorder alone) |
| `EmptyStatementLineBreakTests.SectionWithEmptyStatementAndReturnOnOneLineIsBracedInFirstRun` | FMT-BREAK-006, FC-017 | `AssertRuleResult` | pass (fails on the reorder alone) |
| `EmptyStatementLineBreakTests.SectionWithEmptyStatementAndThrowOnOneLineIsBracedInFirstRun` | FMT-BREAK-006, FC-017 | `AssertRuleResult` | pass (fails on the reorder alone) |
| `EmptyStatementLineBreakTests.SectionWithEmptyStatementAndThrowOnLabelLineIsBracedInFirstRun` | FMT-BREAK-006, FC-017 | `AssertRuleResult` | pass (fails on the reorder alone) |
| `EmptyStatementLineBreakTests.SectionWithEmptyStatementAndReturnNextToMultiLineSectionIsSplit` (skeptic row c = yes) | FMT-BREAK-006 | `AssertRuleResult` | pass |
| `EmptyStatementLineBreakTests.EmptyStatementInFrontOfTerminalBreakIsLeftAsWritten` (other side) | FMT-BREAK-006 | `AssertRuleResult` | pass |
| `EmptyStatementLineBreakTests.SectionWithEmptyStatementAndContinueOnOneLineIsBracedInFirstRun` | FMT-BREAK-006 | `AssertRuleResult` | pass |
| `EmptyStatementLineBreakTests.EmptyStatementIsSplitOnlyFromStatementsABlankLineSeparates` (`; Foo();` kept; `; var`, `; yield`, `; break` split) | FMT-BREAK-006 | `AssertRuleResult` | pass |
| `LineBreakPhaseEmptyStatementTests.StatementSeparatedByBlankLineRequirementStartsItsLine` × 6 (return, throw, if, local declaration, break, `;` behind `}`) | FMT-BREAK-006 | `LineBreakPhase` alone | fail |
| `LineBreakPhaseEmptyStatementTests.YieldBehindEmptyStatementStartsItsLine` | FMT-BREAK-006 | `LineBreakPhase` alone | fail |
| `LineBreakPhaseEmptyStatementTests.ReturnBehindEmptyStatementInSwitchSectionStartsItsLine` | FMT-BREAK-006 | `LineBreakPhase` alone | fail |
| `LineBreakPhaseEmptyStatementTests.ThrowBehindEmptyStatementOnLabelLineStartsItsLine` | FMT-BREAK-006 | `LineBreakPhase` alone | fail |
| `LineBreakPhaseEmptyStatementTests.EmptyStatementAndExpressionStatementAreLeftAsWritten` (other side) | FMT-BREAK-006 | `LineBreakPhase` alone | pass |
| `LineBreakPhaseEmptyStatementTests.EmptyStatementInFrontOfTerminalBreakIsLeftAsWritten` (other side) | FMT-BREAK-006 | `LineBreakPhase` alone | pass |
| `SwitchBraceLayoutOwnerTests.InsertedBracesStartTheLinesOfASectionBlock` | FMT-SWITCH-015 | `AssertRuleResult` | pass |
| `SwitchBraceLayoutOwnerTests.InsertedBracesStartTheirLinesBehindTrailingComments` | FMT-SWITCH-015 | `AssertRuleResult` | pass |
| `SwitchBraceLayoutOwnerTests.BlankLinesAroundInsertedBlocksAreThoseOfAnyBlock` | FMT-SWITCH-016 | `AssertRuleResult` | pass |
| `SwitchBraceLayoutOwnerTests.BlankLineInFrontOfFirstStatementIsRemovedBehindInsertedBrace` (`/// d`, `/** e */`, `/* c */ A();`) | FMT-SWITCH-016 | `AssertRuleResult` | pass |
| `SwitchCaseBracePhaseLayoutTests.InsertedBracesStartTheLinesOfASectionBlock` | FMT-SWITCH-015 | `SwitchCaseBracePhase` alone, trimmed lines | pass |
| `SwitchCaseBracePhaseLayoutTests.InsertedBracesStartTheirLinesBehindTrailingComments` | FMT-SWITCH-015 | `SwitchCaseBracePhase` alone | pass |
| `SwitchCaseBracePhaseLayoutTests.BraceInsertionWritesAndRemovesNoBlankLine` (spec example, 4 blank lines kept) | FMT-SWITCH-016 | `SwitchCaseBracePhase` alone | fail |
| `SwitchCaseBracePhaseLayoutTests.BraceInsertionKeepsBlankLineInFrontOfFirstStatementComment` × 3 | FMT-SWITCH-016 | `SwitchCaseBracePhase` alone | fail |
| `BlankLinePhaseChainCommentTests.CommentGluedToChainLinkOnItsClosingLineGetsNoBlankLine` (no prediction, decision 8) | FMT-BLANK-018 | `BlankLinePhase` alone | fail |
| `BlankLinePhaseChainCommentTests.CommentInFrontOfChainLinkOnItsOwnLineGetsBlankLine` (other side) | FMT-BLANK-018 | `BlankLinePhase` alone | pass |
| `RH5405…Tests.VerifyFixKeepsTheTrailingCommentOfAnIfCondition` (change doc 5), `…WithCarriageReturnLineFeed`, `…InALoopBody` | FMT-BREAK-036 | `Verify` (code fix, Fix All base) | fail |
| `RH5406…Tests.VerifyFixKeepsTheTrailingCommentOfAnIfCondition` | FMT-BREAK-036 | `Verify` | fail |
| `RH5407…Tests.VerifyFixKeepsTheTrailingCommentOfAnIfCondition` | FMT-BREAK-036 | `Verify` | fail |

Probe results, for the implementer and not as expected values. With only the move disabled, every FMT-BREAK-036 test above passes, formatter and analyzer, so no other layout difference is hidden in them. With only the order swapped, `EmptyStatementLineBreakTests` fails its spec example and the three return/throw rows (the skeptic's class), and `BlankLinePhaseKeepsWhichTokensShareALine("EmptyStatementPairs")` fails. The FMT-BREAK-006 change is what turns these green.

## Scenarios (bug fix)

Not a bug-fix lane. The issue's reproduction, the `while`/`if (a) // why` input, is kept unchanged in one run under LF and CRLF (`LineCommentBehindIfConditionInLoopBodyStays`, `CommentBehindIfConditionInLoopBodyIsKeptInOneRun`); the output follows maintainer answer 1. The issue comment's input `switch (x) { case 1: break; case 2: break; }` is stable in one run (`SwitchSectionsOnOneLineGetLinesAndBlankLineInOneRun`). It already passes on 754c3a5.

## For the lead — found while writing the tests

1. **An RH5104 parity test asserts the retired move, and SHARED-DIAG-010 now conflicts with RH5104 (FC-013).** `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5104CommentsMustBeOnTheirOwnLineFormatterTests.cs`, `VerifyFormatterFixesViolation`, calls `VerifyFormatter`. It expects the formatter to clear RH5104 on `if (left == right) // Compare the values.` by moving the comment above the `if`. After FMT-BREAK-005 is retired, this test fails: the probe with the move disabled showed it failing as the only analyzer test. Neither earlier handover listed it.
   - RH5104 (Comments must be on their own line) is a layout rule with a code fix. Its rule page shows `if (a == b) // comment` as the violation.
   - SHARED-DIAG-010 says such a rule MUST NOT report a shape the formatter deliberately keeps. FMT-BREAK-036 now makes `if (a) // c` (and the other seven heads) such a shape.
   - Formatter output for `if (a) // c` will carry an RH5104 warning, as `while (a) // c` already does on `main`.
   - I did not change the test or the rule. Deciding between dropping or rewriting the parity test, narrowing RH5104 (a visible analyzer change), or recording an exception to SHARED-DIAG-010 is a lead or maintainer decision. The implementer cannot make that test pass without it.
2. **The `AGENTS.md` `BlankLinePhase` row has a stale sentence.** It still says "it writes a line break that ends a line only for a statement the line-break phase leaves on its predecessor's line next to an empty statement". That is the FMT-BLANK-021 wording that decisions 6, 7 and 12 removed. Spec-side, not my file.
3. **FMT-SWITCH-015 and FMT-SWITCH-016 say different things about the line below the opening brace.** FMT-SWITCH-015 says the first statement, or its first own-line comment, "starts the line below the opening brace". FMT-SWITCH-016 says a blank line carried in front of it stays in the trivia, so it can stand two lines below. My phase-alone tests avoid the conflict: the FMT-SWITCH-015 tests use input without blank lines, and the FMT-SWITCH-016 tests count blank lines and compare lines with text. The wording should say "the next line with text", or something similar.

## Not covered, and why

- **The unchanged-output seeds from 01-spec-author** (FMT-BLANK-004 same-line section, FMT-BLANK-005 same-line pair, FMT-SWITCH-007's examples, FMT-BREAK-033 `/** d */`, FMT-BREAK-030). Existing tests already cover them: `SwitchSectionLineStartTests`, `LineBreakPhaseSwitchSectionTests`, `BlankLineRequirementTests` (`if (a > 0) { a++; } a++;`), the FMT-SWITCH-007 tests in 4 files, the FMT-BREAK-030 tests in 4 files, and `ReihitsuFormatterNodeBlankLineTests`. I did not duplicate them.
- **The FMT-BLANK-018 chain example in the full pipeline.** Existing FMT-BLANK-018 tests in 3 files guard it. I added only the phase-alone no-prediction test and the `ChainLinkBehindMultiLineComment` observer fixture.
- **`PhaseOwnershipDocumentationTests.OwnershipMapMatchesRuntimePhaseOrder`.** It already fails against `AGENTS.md`, so no change was needed.
- **FC-019, FC-020 (a comment or directive in the gap).** Comments: FMT-BREAK-036 is about a comment in the gap, and the observer fixtures carry comments around a section and its terminal `break`. Directives: FMT-SWITCH-004-style guards are unchanged, the reorder does not touch directive handling in a gap, and the existing `#if` switch tests cover them. No new directive case is specified.
- **FC-028 (node level).** `FormatNode` and `FormatNodeInDocumentAsync` on an `else if` target are covered (detached-root tests). The code fixes cover node-level `if` targets.
- **FC-007, FC-008 (code-fix convergence and Fix All).** `Verify` on the batch bases runs iterative application and the existing `GetFixAllScenario`. The change only alters the layout the formatter returns.
- **FC-010, FC-012, FC-017.** Every `AssertRuleResult` test checks a second pass and the phase-level stability check under LF and CRLF. The observer tests check the neighbor phases directly.
- **FC-004, FC-006, FC-018, FC-022 to FC-025, FC-029, FC-036.** No join, rename, literal, or member-kind rewrite changes in this run.
- **FC-031 (stale comments).** These are code comments for the implementer, which the spec author and skeptic already listed. They are not testable.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~BlockStatementHeadCommentTests|FullyQualifiedName~ElseIfConditionCommentTests|FullyQualifiedName~EmptyStatementLineBreakTests|FullyQualifiedName~SwitchBraceLayoutOwnerTests|FullyQualifiedName~BlankLinePhaseChainCommentTests|FullyQualifiedName~LineBreakPhaseEmptyStatementTests|FullyQualifiedName~PhaseLineBreakOwnershipTests|FullyQualifiedName~SwitchCaseBracePhaseLayoutTests|FullyQualifiedName~FormattingPipelineTests|FullyQualifiedName~ReihitsuFormatterDetachedRootTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5405BracesMustNotBeOmittedAnalyzerTests|FullyQualifiedName~RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzerTests|FullyQualifiedName~RH5407UseBracesConsistentlyAnalyzerTests|FullyQualifiedName~RH5104CommentsMustBeOnTheirOwnLineFormatterTests"
scripts/test.sh --project architecture
```
