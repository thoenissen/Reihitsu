Role: test implementer
Commit: (this commit; parent 2ea0336, the spec author's commit for decision #28)
Verdict: tests written

Scope: architecture decisions #28 and #29 of `00-lead.md` (auditor 23, findings #5 and #6). I touched no production code. My prompt named the decisions' asks and no intended fix or suspected cause beyond them. Expected values come from SHARED-STMT-004 and SHARED-AFTERBRACE-002 as committed in 2ea0336.

## Tests
Files:
- `src/Reihitsu.Analyzer.Test/Formatting/RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests.cs`
- `src/Reihitsu.Analyzer.Test/Formatting/RH5002IfStatementsShouldBePrecededByABlankLineAnalyzerTests.cs`

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH3208 `VerifyNoCodeFixWhenLocalFunctionStartsOnLastLineOfMultiLineCommentInsideObjectInitializer` | SHARED-AFTERBRACE-002 | `VerifyNoCodeFixIsOffered` | fails until refused (auditor 23 cell a) |
| RH3208 `VerifyNoCodeFixWhenLocalFunctionStartsOnWrappedArgumentLineInsideObjectInitializer` | SHARED-AFTERBRACE-002 | `VerifyNoCodeFixIsOffered` | fails until refused (cell b) |
| RH3208 `VerifyNoCodeFixWhenLocalFunctionStartsOnLineOfNestedUnbracedStatementInsideObjectInitializer` | SHARED-AFTERBRACE-002 | `VerifyNoCodeFixIsOffered` | fails until refused (cell c) |
| RH3208 `VerifyNoCodeFixWhenLocalFunctionStartsOnLastLineOfMultiLineCommentInSwitchSectionInsideObjectInitializer` | SHARED-AFTERBRACE-002 | `VerifyNoCodeFixIsOffered` | fails until refused (cell d) |
| RH3208 `VerifyNoCodeFixWhenLocalFunctionStartsOnLineOfClosingBraceOfEarlierBlockInsideObjectInitializer` | SHARED-AFTERBRACE-002 | `VerifyNoCodeFixIsOffered` | fails until refused (`}`-led line, which the spec now refuses) |
| RH3208 `VerifyNoCodeFixForSpecExampleWhereLocalFunctionStartsOnWrappedArgumentLine` | SHARED-AFTERBRACE-002 | `VerifyNoCodeFixIsOffered` | fails until refused (the new spec example, verbatim) |
| RH3208 `VerifyNoCodeFixForSpecExampleWhereOnlyAnotherSectionsStatementStartsItsOwnLine` | SHARED-AFTERBRACE-002 | `VerifyNoCodeFixIsOffered` | passes (decision #29: the label-continuation example verbatim, with `default: return 0;`) |
| RH3208 `VerifyStatementBehindCommentTakesColumnOfLocalFunctionStartingItsLineInsideObjectInitializer` | SHARED-AFTERBRACE-002 | `Verify(source, fixed, 1/1 iterations)` | passes (holding member: the preceding statement starts the line; `M();` at its column 12) |
| RH3208 `VerifyStatementBehindCommentTakesColumnOfEarlierStatementStartingItsLineInsideObjectInitializer` | SHARED-AFTERBRACE-002 | `Verify(source, fixed, 1/1 iterations)` | passes (holding member: `var a = value;` starts the line; `M();` at column 12) |
| RH3208 `VerifyStatementBehindCommentTakesSectionColumnWhenLocalFunctionSharesCaseLabelLineInsideObjectInitializer` | SHARED-AFTERBRACE-002 | `Verify(source, fixed, 1/1 iterations)` | passes (holding member: `case 1:` starts the line; `M();` one level deeper, in `break;`'s column) |
| RH5029 `VerifyNoCodeFixWhenPrecedingStatementStartsOnLastLineOfMultiLineCommentInsideObjectInitializer` | SHARED-STMT-004 | `VerifyNoCodeFixIsOffered` | fails until refused |
| RH5029 `VerifyNoCodeFixWhenPrecedingStatementStartsOnWrappedArgumentLineInsideObjectInitializer` | SHARED-STMT-004 | `VerifyNoCodeFixIsOffered` | fails until refused |
| RH5029 `VerifyNoCodeFixWhenPrecedingStatementStartsOnLineOfNestedUnbracedStatementInsideObjectInitializer` | SHARED-STMT-004 | `VerifyNoCodeFixIsOffered` | fails until refused |
| RH5029 `VerifyNoCodeFixWhenPrecedingStatementStartsOnLastLineOfMultiLineCommentInSwitchSectionInsideObjectInitializer` | SHARED-STMT-004 | `VerifyNoCodeFixIsOffered` | fails until refused |
| RH5029 `VerifyNoCodeFixWhenPrecedingStatementStartsOnLineOfClosingBraceOfEarlierBlockInsideObjectInitializer` | SHARED-STMT-004 | `VerifyNoCodeFixIsOffered` | fails until refused |
| RH5029 `VerifyNoCodeFixWhenOnlyAnotherSectionsStatementStartsItsOwnLine` | SHARED-STMT-004 | `VerifyNoCodeFixIsOffered` | passes (decision #29 variant: two label-continuation sections plus `default:` whose `return;` starts its own line) |
| RH5002 `VerifyNoCodeFixForSpecExampleStatementStartingOnLastLineOfBlockCommentInsideObjectInitializer` | SHARED-STMT-004 | `VerifyNoCodeFixIsOffered` | fails until refused (the new SHARED-STMT-004 example, verbatim) |

Every failing test fails for the right reason on 2ea0336. `Verify(source)` passes, so exactly the marked diagnostic(s) are reported. Then `Assert.IsEmpty(actions)` fails with "No code fix may be offered for the diagnostic at [..]". Full run: 12 failed, 135 passed in the three classes.

Holding members of the statement base already have fix tests in RH5029, which must keep their fix:
- the preceding statement starts the line: `VerifyDiagnosticWhenLocalDeclarationInsideObjectInitializerIsNotPrecededByBlankLine`
- an earlier statement of the list: `VerifyDiagnosticKeepsSiblingColumnWhenPrecedingLineStartsWithEarlierSibling`
- the label's first token: `VerifyDiagnosticAnchorsOnLabelColumnPlusIndentSizeWhenPrecedingStatementSharesSwitchLabelLine`

The layout of the converted body in the three RH3208 holding tests is not stated by SHARED-AFTERBRACE-002. For example, `{` under `int` when `int M()` stands behind `var a = value;`. It is head's output from the existing body conversion, which this run does not change. I confirmed it under LF and CRLF with `scripts/apply-fix.sh RH3208`. Only `M();`'s column, the blank line, and the trimmed `b */` line come from the spec.

## Conflict for the lead: existing tests versus the committed spec wording
SHARED-STMT-004 (2ea0336) gives a column only when the first token of a statement or label of the section starts the line "with nothing but whitespace in front of it". It refuses "a comment of any kind", and it scopes statements and labels to "the same block or switch section" / "its switch section". Decision #28 and auditor 23 call "a label's first token or trivia in front of it" a holding member. Four existing RH5029 fix tests that earlier implementers worked against expect a fix in cells that this wording refuses. I did not change them (role: *Must not*). Each needs a decision: keep the test and narrow the spec wording, or rewrite the test to "no code fix offered".

| Existing RH5029 test | Anchor line | Spec wording says |
|---|---|---|
| `VerifyDiagnosticKeepsSiblingColumnWhenPrecedingLineStartsWithComment` (`/* note */ Method(1); var y = 1;`) | led by a single-line comment in the preceding statement's own leading trivia | refused ("a comment of any kind") |
| `VerifyDiagnosticAnchorsOnLabelColumnPlusIndentSizeWhenCommentPrecedesTheLabelOnItsLine` (`/* note */ case 0: Method(1); var y = 1;`) | comment in front of the label | refused |
| `VerifyDiagnosticAnchorsOnLabelColumnPlusIndentSizeWhenLineStartsWithAnotherSectionsLabel` (`case 1: break; case 0: Method(1); var y = 1;`) | another section's label | refused if "its switch section" means the target's section |
| `VerifyDiagnosticDoesNotCompensateWhenLineStartsWithAnEarlierSiblingSectionsStatement` (`break; case 0: Method(1); var y = 1;`) | a statement of another section | refused ("a statement of the same block or switch section") |

If the implementer follows the spec literally, these four go red. Two other readings keep some of them:
- Keying on the line's first token, with leading trivia allowed in front, keeps the first two.
- Also admitting statements and labels of any section of the same switch keeps the last two. That is today's behavior, through `IsWithinLabelContinuation` and `ComputeAnchorDerivedIndentation`.

Under both readings together, every new test above still fails today and passes afterwards. In each new failing test, the line begins inside a construct that started on an earlier line, or with a token that is not the first token of any statement or label.

## Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH3208 cells a–d (auditor 23) | LF, CRLF | RH3208 code fix (`apply-fix.sh`) | reproduced | `M();` at columns 20 / 24 / 16 / 24 (the leader's column) vs no fix offered |
| RH5029 cells a–d | LF, CRLF | RH5029 code fix (`apply-fix.sh`) | reproduced | `var` at the leader's column (20 / 16 / 16 / 24) vs no fix offered |
| RH5029 two sections plus `default:` | LF, CRLF | RH5029 code fix | holds | no fix offered |
| RH3208 holding members h1–h3 | LF, CRLF | RH3208 code fix | holds | `M();` at columns 12 / 12 / 20, as specified |

## Not covered, and why
- CRLF variants of the refusal tests: the decision is whether an action is registered, and it is taken on syntax. No text is produced, so the line ending cannot change it. `apply-fix.sh` ran every fixture under both endings with the same results.
- Fix All and convergence on the refusal tests: no action is offered. `VerifyNoCodeFixIsOffered` checks every reported diagnostic, including both diagnostics of the decision #29 tests.
- Base rules other than RH5029 and RH5002: they all reach the owner through `StatementShouldBePrecededByABlankLineCodeFixProviderBase` → `BlankLineCodeFixUtilities.CanInsertBlankLineBefore` (decision #16). RH5002 carries the spec example.
- RH5030: it cannot reach the rebuild path (auditor 23).
- Anonymous-object variant: the owner keys on `HasAnchorScopeAncestor`, which is the same branch for both. The existing `…InsideAnonymousObject` test covers that branch.
- RH5103 tests with comment-led lines (`RH5103…Tests.cs:312/376/566`): RH5103's fix does not call `BlankLineCodeFixUtilities`, so it is outside this change.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests|FullyQualifiedName~RH5002IfStatementsShouldBePrecededByABlankLineAnalyzerTests"
```
