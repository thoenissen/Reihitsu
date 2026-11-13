Role: test implementer
Commit: 50a22b1
Verdict: tests written

Tests for decisions 26 and 27 (RH3004-006 and SHARED-FIX-007 as changed in bffd78a). The prompt carried no intended fix. Run in Debug (decision 28: Release fails on RH7305 in the RH7109 test file).

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH3004 `FixWritesAWhitespaceOnlyLineAmongTheMovedStatementsEmpty` (spec example) | RH3004-006 | `Verify` | fails: the 12 spaces stay on the line |
| RH3004 `FixWritesAWhitespaceOnlyLineAmongTheMovedStatementsEmptyWithCarriageReturnLineFeed` | RH3004-006 | `ApplyCodeFixAsync` | fails: same |
| RH3004 `FixWritesABlankLineHoldingATabAmongTheMovedStatementsEmpty` | RH3004-006 | `Verify` | fails: `····→····` stays |
| RH3004 `FixWithheldForATabInFrontOfAMovingStatementLine` (spec example) | RH3004-006 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH3004 `FixWithheldForATabInFrontOfAMovingStatementLineWithCarriageReturnLineFeed` | RH3004-006 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH3004 `FixWithheldForATabInFrontOfALaterMovingStatementLine` | RH3004-006 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH3004 `FixWithheldForATabInFrontOfAMovingCommentLine` | RH3004-006 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH3004 `FixWithheldForATabInFrontOfAMovingStatementLineWhoseShiftIsZero` ("whatever the shift": tab + 27 spaces = column 28, the statements' column) | RH3004-006 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| RH3004 `FixAllLeavesAnAnonymousMethodWithATabInFrontOfAMovingLineReported` | RH3004-006 | `Verify` (Fix All, markup allowed) | fails: the tabbed method is rewritten |
| RH3004 `FixIsOfferedForATabBetweenTwoTokensOfAMovingStatementLine` | RH3004-006 | `Verify` | passes (other side of the condition) |
| RH3004 `FixIsOfferedForATabInsideACommentAndInsideAStringOfMovingStatementLines` | RH3004-006 | `Verify` | passes |
| RH3004 `FixIsOfferedForATabInFrontOfTheFirstLineOfTheAnonymousMethod` (tab kept, braces from the tab counted as one column) | RH3004-006 | `Verify` | passes |
| RH3004 `FixIsOfferedForATabInFrontOfASingleReturnStatementOnTheExpressionBodyPath` | RH3004-006 | `Verify` | passes (refusal is block-body only) |
| RH7004 `WhitespaceOnlyLineAmongTheMovedStatementsIsWrittenEmpty` (LF, CRLF) | SHARED-FIX-007 | `VerifyCodeFixWithEndOfLine` | fails on both: the 8 spaces stay on the line |

Every failing test fails on the specified difference (whitespace left on the blank line, or a fix offered), not on unrelated layout. Note for the lead: 11-spec-author says main writes these blank lines empty; on this head (50a22b1) the RH3004 and RH7004 fixes keep the whitespace, so decision 27 is a regression to repair here, not a no-op.

## Not covered, and why
- Tab in front of a statement that shares the opening brace's line: the whitespace in front of that line belongs to the brace line, not a moving line's start; the spec names no outcome for it.
- Column-0 comment lines need no tab case: they have no leading whitespace (11-spec-author, internal choices).
- RH7109 / member moves with a whitespace-only line: the lead asked for one RH7004 test for SHARED-FIX-007; the member-move path is a separate code path not covered here.

## Commands
```shell
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests.FixWritesA|FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests.FixWithheldForATab|FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests.FixIsOfferedForATab|FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests.FixAllLeavesAnAnonymousMethodWithATab|FullyQualifiedName~RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests.WhitespaceOnlyLine"
```
