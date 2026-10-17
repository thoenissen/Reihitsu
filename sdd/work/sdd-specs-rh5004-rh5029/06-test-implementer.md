Role: test implementer
Commit: e5d51f6 (production code unchanged since 33a29f7)
Verdict: tests written

Lane: specs and tests for existing behavior. Every test below passes against the unchanged production code. No production file was touched. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

All tests are in `src/Reihitsu.Analyzer.Test/Formatting/RH50xx…AnalyzerTests.cs`, the 16 existing classes of the rules in scope. Each class already derives from `BatchCodeFixTestsBase`. `Verify` with markup `{|#n:…|}` pins the exact diagnostic span (FC-017). Where the input has markers, the expected fixed source is the input with exactly one blank line inserted above each marked line and nowhere else. That expectation comes from SHARED-STMT-004, not from running the code (FC-035). The Roslyn verifier then also checks one-pass convergence, incremental iterations, and Fix All for each such test.

## Tests

### New (26)

Tests named after a spec example (`…ButNot…`, `…AfterStatementAndFor…`, `…EvenWhenAnEarlierYieldStatementExists`, `…FirstDeclarationOfEachRun…`, `…GotoLabelGotoCaseAndGotoDefault…`, `…AfterLocalOrUsingDeclaration…`, `…AssignmentsAfterLocalDeclarationButDiagnosticForIncrement`) carry that spec example verbatim, in the example's own shape (FC-034): `class C`, only the `{|RH####:…|}` markers renumbered to `{|#n:…|}`. The others are focused single-qualifier tests on one side of a qualifier (FC-002, FC-003).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5005…Tests.VerifyDiagnosticForUsingStatementButNotForUsingDeclaration` (new) | RH5005-001, RH5005-002 | `Verify` | diagnostic and fix, passes |
| `RH5005…Tests.VerifyNoDiagnosticForUsingDeclarationAfterStatement` (new) | RH5005-001 | `Verify` | no diagnostic, passes |
| `RH5008…Tests.VerifyDiagnosticForReturnAfterStatementAndForReturnEndingSwitchSection` (new) | RH5008-001, RH5008-002 | `Verify` | diagnostic and fix, passes |
| `RH5008…Tests.VerifyDiagnosticForReturnStatementEndingSwitchSection` (new) | RH5008-001, RH5008-002 | `Verify` | diagnostic and fix, passes |
| `RH5009…Tests.VerifyDiagnosticForGotoLabelGotoCaseAndGotoDefaultEndingSwitchSections` (new) | RH5009-001, RH5009-002 | `Verify` | diagnostic and fix, passes |
| `RH5013…Tests.VerifyDiagnosticForThrowStatementButNotForThrowExpression` (new) | RH5013-001, RH5013-002 | `Verify` | diagnostic and fix, passes |
| `RH5013…Tests.VerifyNoDiagnosticForThrowExpressionAfterStatement` (new) | RH5013-001 | `Verify` | no diagnostic, passes |
| `RH5014…Tests.VerifyDiagnosticForSwitchStatementButNotForSwitchExpression` (new) | RH5014-001, RH5014-002 | `Verify` | diagnostic and fix, passes |
| `RH5014…Tests.VerifyNoDiagnosticForSwitchExpressionAfterStatement` (new) | RH5014-001 | `Verify` | no diagnostic, passes |
| `RH5015…Tests.VerifyDiagnosticForCheckedStatementButNotForUncheckedStatementOrCheckedExpression` (new) | RH5015-001, RH5015-002 | `Verify` | diagnostic and fix, passes |
| `RH5015…Tests.VerifyNoDiagnosticForUncheckedStatementAfterStatement` (new) | RH5015-001 | `Verify` | no diagnostic, passes |
| `RH5015…Tests.VerifyNoDiagnosticForCheckedExpressionAfterStatement` (new) | RH5015-001 | `Verify` | no diagnostic, passes |
| `RH5016…Tests.VerifyDiagnosticForUncheckedStatementButNotForCheckedStatementOrUncheckedExpression` (new) | RH5016-001, RH5016-002 | `Verify` | diagnostic and fix, passes |
| `RH5016…Tests.VerifyNoDiagnosticForCheckedStatementAfterStatement` (new) | RH5016-001 | `Verify` | no diagnostic, passes |
| `RH5016…Tests.VerifyNoDiagnosticForUncheckedExpressionAfterStatement` (new) | RH5016-001 | `Verify` | no diagnostic, passes |
| `RH5019…Tests.VerifyDiagnosticForYieldStatementAfterStatementEvenWhenAnEarlierYieldStatementExists` (new) | RH5019-001, RH5019-002, RH5019-003 | `Verify` | diagnostic and fix, passes |
| `RH5019…Tests.VerifyNoDiagnosticForYieldBreakStatementAfterYieldReturnStatement` (new) | RH5019-002 | `Verify` | no diagnostic, passes |
| `RH5021…Tests.VerifyDiagnosticForExpressionStatementAfterLocalOrUsingDeclarationButNotAfterExpressionStatement` (new) | RH5021-001, RH5021-003 | `Verify` | diagnostic and fix, passes |
| `RH5021…Tests.VerifyDiagnosticForExpressionStatementAfterUsingDeclaration` (new) | RH5021-001, RH5021-003 | `Verify` | diagnostic and fix, passes |
| `RH5021…Tests.VerifyNoDiagnosticForAssignmentsAfterLocalDeclarationButDiagnosticForIncrement` (new) | RH5021-002, RH5021-001, RH5021-003 | `Verify` | diagnostic and fix, passes |
| `RH5021…Tests.VerifyNoDiagnosticForCompoundAssignmentAfterLocalDeclaration` (new) | RH5021-002 | `Verify` | no diagnostic, passes |
| `RH5021…Tests.VerifyNoDiagnosticForNullCoalescingAssignmentAfterLocalDeclaration` (new) | RH5021-002 | `Verify` | no diagnostic, passes |
| `RH5029…Tests.VerifyDiagnosticForFirstDeclarationOfEachRunIncludingConstAndUsingDeclarations` (new) | RH5029-001, RH5029-002 | `Verify` | diagnostic and fix, passes |
| `RH5029…Tests.VerifyDiagnosticForConstDeclarationAfterStatement` (new) | RH5029-001, RH5029-002 | `Verify` | diagnostic and fix, passes |
| `RH5029…Tests.VerifyDiagnosticForUsingDeclarationAfterStatement` (new) | RH5029-001, RH5029-002 | `Verify` | diagnostic and fix, passes |
| `RH5029…Tests.VerifyNoDiagnosticForConstDeclarationAfterLocalDeclaration` (new) | RH5029-001 | `Verify` | no diagnostic, passes |

### Existing, tagged (88)

Every existing tag is on a test whose input decides the requirement, not one that touches it only in passing. All pass with the `Verify` helper.

- RH5004: `VerifyDiagnosticForDoStatementWithoutPrecedingBlankLine` → RH5004-001, RH5004-002; `VerifyNoDiagnosticForDoStatementWithPrecedingBlankLine` → RH5004-001; `VerifyNoDiagnosticForDoStatementAtStartOfBlock` → RH5004-001; `VerifyDiagnosticForDoStatementWhenCommentLineDirectlyPrecedesIt` → RH5004-001
- RH5005: `VerifyDiagnosticForUsingStatementWithoutPrecedingBlankLine` → RH5005-001, RH5005-002; `VerifyNoDiagnosticForUsingStatementWithPrecedingBlankLine` → RH5005-001; `VerifyNoDiagnosticForUsingStatementAtStartOfBlock` → RH5005-001; `VerifyDiagnosticForUsingStatementWhenCommentLineDirectlyPrecedesIt` → RH5005-001
- RH5006: `VerifyDiagnosticForForeachStatementWithoutPrecedingBlankLine` → RH5006-001, RH5006-002; `VerifyNoDiagnosticForForeachStatementWithPrecedingBlankLine` → RH5006-001; `VerifyNoDiagnosticForForeachStatementAtStartOfBlock` → RH5006-001; `VerifyDiagnosticForForeachStatementWhenCommentLineDirectlyPrecedesIt` → RH5006-001; `VerifyDiagnosticForDeconstructionForeachStatementWithoutPrecedingBlankLine` → RH5006-001, RH5006-002
- RH5007: `VerifyDiagnosticForForStatementWithoutPrecedingBlankLine` → RH5007-001, RH5007-002; `VerifyNoDiagnosticForForStatementWithPrecedingBlankLine` → RH5007-001; `VerifyNoDiagnosticForForStatementAtStartOfBlock` → RH5007-001; `VerifyDiagnosticForForStatementWhenCommentLineDirectlyPrecedesIt` → RH5007-001
- RH5008: `VerifyDiagnosticForReturnStatementWithoutPrecedingBlankLine` → RH5008-001, RH5008-002; `VerifyNoDiagnosticForReturnStatementWithPrecedingBlankLine` → RH5008-001; `VerifyNoDiagnosticForReturnStatementAfterCaseLabel` → RH5008-001; `VerifyDiagnosticForReturnStatementWhenCommentLineDirectlyPrecedesIt` → RH5008-001; `VerifyNoDiagnosticForReturnStatementWhenBlankLineThenCommentPrecedesIt` → RH5008-001; `VerifyNoDiagnosticForReturnStatementDirectlyAfterPreprocessorDirective` → SHARED-STMT-003; `VerifyNoDiagnosticForReturnStatementDirectlyAfterEndIfDirective` → SHARED-STMT-003; `VerifyNoDiagnosticForReturnStatementAsEmbeddedWhileBody` → SHARED-STMT-002; `VerifyNoDiagnosticForReturnStatementAsEmbeddedForBody` → SHARED-STMT-002; `VerifyNoDiagnosticForReturnStatementAsEmbeddedForeachBody` → SHARED-STMT-002; `VerifyNoDiagnosticForReturnStatementAsEmbeddedUsingBody` → SHARED-STMT-002; `VerifyNoDiagnosticForReturnStatementAsEmbeddedLockBody` → SHARED-STMT-002; `VerifyNoDiagnosticForReturnStatementAsEmbeddedFixedBody` → SHARED-STMT-002; `VerifyNoDiagnosticForReturnStatementAsEmbeddedElseBody` → SHARED-STMT-002; `VerifyNoDiagnosticForReturnStatementAsEmbeddedDoBody` → SHARED-STMT-002
- RH5009: `VerifyDiagnosticForGotoStatementWithoutPrecedingBlankLine` → RH5009-001, RH5009-002; `VerifyDiagnosticForGotoCaseStatementWithoutPrecedingBlankLine` → RH5009-001, RH5009-002; `VerifyDiagnosticForGotoDefaultStatementWithoutPrecedingBlankLine` → RH5009-001, RH5009-002; `VerifyNoDiagnosticForGotoStatementWithPrecedingBlankLine` → RH5009-001; `VerifyNoDiagnosticForGotoStatementAfterLabel` → SHARED-STMT-002; `VerifyDiagnosticForGotoStatementWhenCommentLineDirectlyPrecedesIt` → RH5009-001
- RH5012: `VerifyDiagnosticForContinueStatementWithoutPrecedingBlankLine` → RH5012-001, RH5012-002; `VerifyNoDiagnosticForContinueStatementWithPrecedingBlankLine` → RH5012-001; `VerifyNoDiagnosticForContinueStatementAtStartOfBlock` → RH5012-001; `VerifyDiagnosticForContinueStatementWhenCommentLineDirectlyPrecedesIt` → RH5012-001
- RH5013: `VerifyDiagnosticForThrowStatementWithoutPrecedingBlankLine` → RH5013-001, RH5013-002; `VerifyNoDiagnosticForThrowStatementWithPrecedingBlankLine` → RH5013-001; `VerifyNoDiagnosticForThrowStatementAfterCaseLabel` → RH5013-001; `VerifyDiagnosticForThrowStatementWhenCommentLineDirectlyPrecedesIt` → RH5013-001
- RH5014: `VerifyDiagnosticForSwitchStatementWithoutPrecedingBlankLine` → RH5014-001, RH5014-002; `VerifyNoDiagnosticForSwitchStatementWithPrecedingBlankLine` → RH5014-001; `VerifyNoDiagnosticForSwitchStatementAtStartOfBlock` → RH5014-001; `VerifyDiagnosticForSwitchStatementWhenCommentLineDirectlyPrecedesIt` → RH5014-001
- RH5015: `VerifyDiagnosticForCheckedStatementWithoutPrecedingBlankLine` → RH5015-001, RH5015-002; `VerifyNoDiagnosticForCheckedStatementWithPrecedingBlankLine` → RH5015-001; `VerifyNoDiagnosticForCheckedStatementAtStartOfBlock` → RH5015-001; `VerifyDiagnosticForCheckedStatementWhenCommentLineDirectlyPrecedesIt` → RH5015-001
- RH5016: `VerifyDiagnosticForUncheckedStatementWithoutPrecedingBlankLine` → RH5016-001, RH5016-002; `VerifyNoDiagnosticForUncheckedStatementWithPrecedingBlankLine` → RH5016-001; `VerifyNoDiagnosticForUncheckedStatementAtStartOfBlock` → RH5016-001; `VerifyDiagnosticForUncheckedStatementWhenCommentLineDirectlyPrecedesIt` → RH5016-001
- RH5017: `VerifyDiagnosticForFixedStatementWithoutPrecedingBlankLine` → RH5017-001, RH5017-002; `VerifyNoDiagnosticForFixedStatementWithPrecedingBlankLine` → RH5017-001; `VerifyNoDiagnosticForFixedStatementAtStartOfBlock` → RH5017-001; `VerifyDiagnosticForFixedStatementWhenCommentLineDirectlyPrecedesIt` → RH5017-001
- RH5018: `VerifyDiagnosticForLockStatementWithoutPrecedingBlankLine` → RH5018-001, RH5018-002; `VerifyNoDiagnosticForLockStatementWithPrecedingBlankLine` → RH5018-001; `VerifyNoDiagnosticForLockStatementAtStartOfBlock` → RH5018-001; `VerifyDiagnosticForLockStatementWhenCommentLineDirectlyPrecedesIt` → RH5018-001
- RH5019: `VerifyDiagnosticForYieldStatementWithoutPrecedingBlankLine` → RH5019-001, RH5019-003; `VerifyDiagnosticForYieldBreakStatementWithoutPrecedingBlankLine` → RH5019-001, RH5019-003; `VerifyNoDiagnosticForConsecutiveYieldStatementsInSwitchSection` → RH5019-002; `VerifyNoDiagnosticForYieldStatementWithPrecedingBlankLine` → RH5019-001; `VerifyNoDiagnosticForConsecutiveYieldStatements` → RH5019-002; `VerifyDiagnosticForYieldStatementWhenCommentLineDirectlyPrecedesIt` → RH5019-001; `VerifyNoDiagnosticForYieldStatementAsEmbeddedForeachBody` → SHARED-STMT-002
- RH5021: `VerifyDiagnosticWhenLocalDeclarationIsFollowedByExpressionStatementWithoutBlankLine` → RH5021-001, RH5021-003; `VerifyDiagnosticWhenLocalDeclarationWithoutInitializerIsFollowedByExpressionStatementWithoutBlankLine` → RH5021-001, RH5021-003; `VerifyNoDiagnosticForConsecutiveExpressionStatements` → RH5021-001; `VerifyNoDiagnosticWhenLocalDeclarationAndExpressionStatementAreAlreadySeparatedByBlankLine` → RH5021-001; `VerifyNoDiagnosticForLocalDeclarationAdjacentToControlFlowStatement` → RH5021-001; `VerifyNoDiagnosticForAssignmentStatementAdjacentToExpressionStatement` → RH5021-001, RH5021-002; `VerifyDiagnosticWhenLocalDeclarationIsFollowedByCommentLineThenExpressionStatement` → RH5021-001
- RH5029: `VerifyDiagnosticWhenLocalDeclarationIsNotPrecededByBlankLine` → RH5029-001, RH5029-002; `VerifyDiagnosticWhenLocalDeclarationInSwitchSectionIsNotPrecededByBlankLine` → RH5029-001, RH5029-002; `VerifyNoDiagnosticWhenLocalDeclarationIsFirstStatementInBlock` → RH5029-001; `VerifyNoDiagnosticWhenLocalDeclarationIsFirstStatementInSwitchSection` → RH5029-001; `VerifyNoDiagnosticForConsecutiveLocalDeclarations` → RH5029-001; `VerifyNoDiagnosticWhenLocalDeclarationAlreadyHasPrecedingBlankLine` → RH5029-001; `VerifyNoDiagnosticWhenLocalDeclarationDirectlyFollowsPreprocessorDirective` → SHARED-STMT-003; `VerifyNoDiagnosticWhenLocalDeclarationDirectlyFollowsEndIfDirective` → SHARED-STMT-003

How the tags were chosen:

- A "WithoutPrecedingBlankLine" test (and RH5006's deconstructing `foreach`, RH5009's `goto case` / `goto default`) gets `-001` plus the location ID (`-002`; `-003` for RH5019 and RH5021), because its markup sits on the keyword or first token.
- "WithPrecedingBlankLine", "AtStartOfBlock", "AfterCaseLabel", and "CommentLineDirectlyPrecedesIt" get `-001`, the same pattern as RH5001/RH5003/RH5010.
- Embedded-body tests and RH5009's `goto` after a label get SHARED-STMT-002, because in each one the statement's parent is not a block or switch section. Directive tests get SHARED-STMT-003.
- The 34 rule IDs are now all covered. `EveryRequirementOfAnEnforcedSpecIsCovered` passes.

### Left untagged on purpose

- RH5029: every anchor and indentation test from `VerifyDiagnosticWhenLocalDeclarationInsideObjectInitializerIsNotPrecededByBlankLine` through `VerifyFixAllCompensatesBothDiagnosticsOnSharedLabelLineWithoutDoubleCompensation`. They pin indentation at a switch label column plus the indent size, or inside an object initializer. Whether that is "indented like the statement it follows" (SHARED-STMT-004) is not evident, so none carries SHARED-STMT-004.
- RH5029 `VerifyDiagnosticOverIndentsWhenLabelSpansMultipleLinesAndPrecedingStatementSharesItsContinuationLine`: never tagged. thoenissen/Reihitsu#1000 holds it.
- RH5008 `VerifySameLineFixUsesReturnStatementSyntaxDepth` and `VerifyDiagnosticAnchorsOnLabelColumnPlusIndentSizeWhenPrecedingStatementSharesSwitchLabelLine`: same reason as the RH5029 anchor tests.
- RH5021:
  - `VerifyNoDiagnosticWhenExpressionStatementIsFollowedByLocalDeclarationWithoutBlankLine`, `VerifyNoDiagnosticForConsecutiveLocalDeclarationsWithInitializer`, `…WithoutInitializer`, and `VerifyNoDiagnosticForMixedLocalDeclarationBlockWithAndWithoutInitializer`.
  - In each, the expression statement that decides RH5021 is either preceded by a blank line or absent. The decisive case is already tagged on `VerifyNoDiagnosticWhenLocalDeclarationAndExpressionStatementAreAlreadySeparatedByBlankLine`.
- Every `FixAllCorrectsEveryDiagnostic` keeps the shared SHARED-FIX tags from its base class.

## Spec examples

| Example | Covered by |
|---|---|
| RH5004-001, RH5007-001, RH5017-001, RH5018-001 | No new test. Both halves of each example are existing tagged tests: the statement after a statement is reported on the keyword ("WithoutPrecedingBlankLine"), and the statement after a blank line is not ("WithPrecedingBlankLine"). The examples add no qualifier. |
| RH5006-001 | No new test. Plain and deconstructing `foreach` after a statement are the existing `VerifyDiagnosticForForeachStatementWithoutPrecedingBlankLine` and `VerifyDiagnosticForDeconstructionForeachStatementWithoutPrecedingBlankLine`. |
| RH5012-001 | No new test. The `continue` that is first in its block is `VerifyNoDiagnosticForContinueStatementAtStartOfBlock`, and the reported one is `VerifyDiagnosticForContinueStatementWithoutPrecedingBlankLine`. |
| RH5019-001 | No new test. `yield return` and `yield break` after a statement are the two existing "WithoutPrecedingBlankLine" tests. |
| RH5005-001, RH5008-001, RH5009-001, RH5013-001, RH5014-001, RH5015-001, RH5016-001, RH5019-002, RH5021-001, RH5021-002, RH5029-001 | One new test each, carrying the example verbatim (table above). |

The spec author and the skeptic already confirmed the examples not carried verbatim, with `scripts/apply-fix.sh` under LF and CRLF (`01-spec-author.md`, `02-skeptic.md`).

## Scenarios (bug fix)

_N/A._ Not a bug fix.

## Not covered, and why

- **Excluded by instruction or open question; no test in this run.**
  - `await using` / `await foreach`: thoenissen/Reihitsu#946.
  - A block comment on the statement's line: thoenissen/Reihitsu#972.
  - Deconstruction declarations for RH5021/RH5029: thoenissen/Reihitsu#1001.
  - The multi-line `case` label over-indent: thoenissen/Reihitsu#1000.
  - An attribute-prefixed statement: does not compile (CS7014, `04-skeptic.md`).
- **Line endings (CRLF).** The new tests run on LF only. SHARED-STMT-004's line-ending clause is shared behavior with one code owner, `StatementShouldBePrecededByABlankLineCodeFixProviderBase`, and the requirement is already covered by `VerifyInsertedBlankLineUsesDetectedCarriageReturnLineFeedEndOfLine` in the RH5001 class. No rule-specific requirement in this run mentions line endings, and the spec author ran every example under CRLF.
  - Proof of the single owner: `rg -l "StatementShouldBePrecededByABlankLineCodeFixProviderBase" src/Reihitsu.Analyzer.CodeFixes` lists 33 files: the base and the providers that derive from it, all 16 providers in scope among them.
- **Formatter parity (FC-013, FC-014).** The `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH50xx…FormatterTests.cs` parity classes already exist for all 16 rules. No formatter code changes in this lane, and the skeptic checked parity for the qualifier inputs (`02-skeptic.md`). Not duplicated.
- **FC-019, FC-005, FC-020, FC-021 (comment or directive in the gap).** These are shared behavior (SHARED-STMT-001, -003, -004). Every class already has a "CommentLineDirectlyPrecedesIt" test, and RH5008 and RH5029 have directive tests. Nothing in this run changes a gap.
- **FC-007, FC-008 (convergence, Fix All).** The verifier checks both for every new test with a fix, and each class's `GetFixAllScenario` covers several diagnostics in one document.
- **Do not reach this run, because no rewrite, guard, rename, or phase changes:** FC-001, FC-004, FC-006, FC-009 to FC-012, FC-015, FC-016, FC-018, FC-022 to FC-032, FC-036, FC-037. FC-015 (predicates duplicated between the analyzers and `BlankLineStatementSpacingRewriter`) is the skeptic's follow-up candidate, not a test gap.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5004|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5005|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5006|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5007|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5008|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5009|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5012|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5013|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5014|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5015|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5016|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5017|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5018|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5019|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5021|FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5029"
scripts/test.sh --project analyzer --filter "TestCategory=RH5029-001"
scripts/test.sh --project architecture
```

Results on this tree:

- Analyzer filter: 167 passed, 0 failed.
- Architecture: 72 passed, 0 failed, including `EveryRequirementOfAnEnforcedSpecIsCovered`.
- `scripts/format.sh` over the 16 changed files formatted 0 of 16.

## Follow-up candidates (out of scope)

_None new._
