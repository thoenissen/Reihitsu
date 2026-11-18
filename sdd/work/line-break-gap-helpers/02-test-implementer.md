Role: test implementer
Commit: 672a136 (tests written and run on this head; committed on top of it)
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Both issues ask for a refactor with no change of formatter output, so no reproduction exists. Every test below is a characterization test of today's output or of today's helper behavior, and every one passes on `672a136`. Expected outputs were taken from `scripts/format.sh` over scratch fixtures on this head (second run: "Formatted 0 of 22 file(s)"), not from a patched copy. `AssertRuleResult` checks each full-pipeline test under LF and CRLF, a second pass, and the phase-stability check.

New files:
- `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ChainGapJoinTests.cs` (#1640)
- `src/Reihitsu.Formatter.Test/Regression/LineBreaks/LineBreakInFrontOfTokenTests.cs` (#1651)
- `src/Reihitsu.Formatter.Test/Unit/LineBreaks/LineBreakTriviaUtilitiesTests.cs` (extended: `EndLineInFrontOf`, `JoinGaps`)

## thoenissen/Reihitsu#1640

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ChainGapJoinTests.EveryJoinKindOfOneChainIsClosedInOneLayout` (cell G) | FMT-BREAK-019, FMT-BREAK-020; FC-015, FC-026 | `AssertRuleResult` | pass |
| `ChainGapJoinTests.TokenJoinedOnItsLeadingSideEndsItsLineOnItsTrailingSide` (cell H) | FMT-BREAK-019, FMT-BREAK-020; FC-026 | `AssertRuleResult` | pass |
| `ChainGapJoinTests.OperatorBehindMultiLineCommentTailIsJoinedInsideAndBrokenInFront` (cell I) | FMT-BREAK-019, SHARED-COMMENT-012; FC-026 | `AssertRuleResult` | pass |
| `ChainGapJoinTests.ChainWrappedOnlyInsideALinkOperatorStaysWrapped` (cell J) | FMT-BREAK-019; layout read before the joins | `AssertRuleResult` | pass |
| `ChainGapJoinTests.PrefixLinkWrappedOnlyInsideItsOperatorStartsItsLine` (cell K) | FMT-BREAK-019; layout read before the joins | `AssertRuleResult` | pass |
| `ChainGapJoinTests.ConditionalElementAccessesWithoutLinksAreJoined` (cell L, `JoinConditionalElementAccesses`) | none (characterization) | `AssertRuleResult` | pass |
| `ChainGapJoinTests.LineBreakInsertedBehindBlockCommentAndBehindWhitespace` (cell B) | FMT-BREAK-019, FMT-BREAK-029 | `AssertRuleResult` | pass |
| `ChainGapJoinTests.SingleLineCommentInLinkGapKeepsItsLine` (later link and link 0) | FMT-BREAK-001; FC-019 | `AssertRuleResult` | pass |
| `ChainGapJoinTests.SingleLineCommentInsideLinkOperatorKeepsItsLine` | FMT-BREAK-001; FC-019 | `AssertRuleResult` | pass |
| `ChainGapJoinTests.SingleLineCommentBetweenDotAndMemberNameKeepsItsLine` | FMT-BREAK-001; FC-019 | `AssertRuleResult` | pass |
| `ChainGapJoinTests.DirectiveInLinkGapKeepsItsLine` | FMT-BREAK-001; FC-020, FC-021 | `AssertRuleResult` | pass |
| `ChainGapJoinTests.DirectiveInsideLinkOperatorKeepsItsLine` | FMT-BREAK-001; FC-020 | `AssertRuleResult` | pass |
| `ChainGapJoinTests.DirectiveInFrontOfFirstLinkKeepsItsLine` | FMT-BREAK-001; FC-020 | `AssertRuleResult` | pass |
| `ChainGapJoinTests.DirectiveBetweenDotAndMemberNameKeepsItsLine` | FMT-BREAK-001; FC-020 | `AssertRuleResult` | pass |
| `ChainGapJoinTests.ChainInsideArgumentOfOuterChainIsJoinedOnItsOwn` | FMT-BREAK-019, FMT-BREAK-020; FC-027 | `AssertRuleResult` | pass |
| `LineBreakTriviaUtilitiesTests.JoinGapsWithoutGapsReturnsSameNode` | none; FC-003 (empty side) | unit | pass |
| `LineBreakTriviaUtilitiesTests.JoinGapsJoinsGapWithMinimalSeparator` | FMT-BREAK-031 | unit | pass |
| `LineBreakTriviaUtilitiesTests.JoinGapsWritesSpaceWhereTokensWouldMerge` (CRLF input) | FMT-BREAK-031; FC-010 | unit | pass |
| `LineBreakTriviaUtilitiesTests.JoinGapsJoinsBothSidesOfOneToken` | FMT-BREAK-031 | unit | pass |
| `LineBreakTriviaUtilitiesTests.JoinGapsResultDoesNotDependOnGapOrder` | none; locks the precondition decision 1 asks the summary to state | unit | pass |
| `LineBreakTriviaUtilitiesTests.JoinGapsLeavesOtherGapsUnchanged` | none | unit | pass |

The `JoinGaps` unit tests call today's signature `JoinGaps(node, gaps)`. Decision 1 lets the implementer make it compose with edits it does not make. An added overload or parameter keeps these tests; a changed signature of this overload goes back through the lead.

## thoenissen/Reihitsu#1651

### Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LineBreakInFrontOfTokenTests.LaterCallBehindMultiLineCommentTailStartsItsOwnLine` (cell A; the chain's comment-tail branch must stay) | FMT-BREAK-019, SHARED-COMMENT-012 | `AssertRuleResult` | pass |
| `ChainGapJoinTests.LineBreakInsertedBehindBlockCommentAndBehindWhitespace` (cell B, listed above) | FMT-BREAK-019, FMT-BREAK-029 | `AssertRuleResult` | pass |
| `ChainGapJoinTests.OperatorBehindMultiLineCommentTailIsJoinedInsideAndBrokenInFront` (cell I, listed above) | FMT-BREAK-019, SHARED-COMMENT-012 | `AssertRuleResult` | pass |
| `LineBreakInFrontOfTokenTests.ArgumentBehindWhitespaceStartsItsOwnLine` (cell M, axis b) | FMT-BREAK-012 | `AssertRuleResult` | pass |
| `LineBreakInFrontOfTokenTests.ArgumentBehindBlockCommentStartsItsOwnLine` (axis c) | FMT-BREAK-012 | `AssertRuleResult` | pass |
| `LineBreakInFrontOfTokenTests.ArgumentAlreadyStartingItsLineKeepsItsLineBreak` (axis a, other side) | FMT-BREAK-012, FMT-BREAK-030 | `AssertRuleResult` | pass |
| `LineBreakInFrontOfTokenTests.ConstraintBehindWhitespaceStartsItsOwnLine` (cell N, axis b) | FMT-BREAK-037 | `AssertRuleResult` | pass |
| `LineBreakInFrontOfTokenTests.ConstraintBehindBlockCommentStartsItsOwnLine` (axis c) | FMT-BREAK-037 | `AssertRuleResult` | pass |
| `LineBreakInFrontOfTokenTests.ConstraintAlreadyStartingItsLineKeepsItsLineBreak` (axis a, other side) | FMT-BREAK-037, FMT-BREAK-030 | `AssertRuleResult` | pass |
| `LineBreakTriviaUtilitiesTests.EndLineInFrontOfAppendsLineBreakBehindWhitespace` (branch: no line break; whitespace kept, decision 4) | FMT-BREAK-029 | unit | pass |
| `LineBreakTriviaUtilitiesTests.EndLineInFrontOfUsesRequestedEndOfLine` (CRLF) | none; FC-010 | unit | pass |
| `LineBreakTriviaUtilitiesTests.EndLineInFrontOfAppendsLineBreakBehindBlockComment` | none; FC-019 | unit | pass |
| `LineBreakTriviaUtilitiesTests.EndLineInFrontOfKeepsTrailingLineBreak` (branch: line break in trailing trivia) | FMT-BREAK-030 | unit | pass |
| `LineBreakTriviaUtilitiesTests.EndLineInFrontOfKeepsLineBreakBehindEndOfLineComment` | FMT-BREAK-030; FC-019 | unit | pass |
| `LineBreakTriviaUtilitiesTests.EndLineInFrontOfKeepsLineBreakInLeadingTriviaOfToken` (branch: line break in the token's leading trivia) | FMT-BREAK-030 | unit | pass |
| `LineBreakTriviaUtilitiesTests.EndLineInFrontOfKeepsLineBreakInFrontOfMultiLineComment` (axis d: the helper has no comment-tail branch) | none (characterization) | unit | pass |

`EndLineInFrontOfKeepsLineBreakInFrontOfMultiLineComment` locks decisions 3 and 6: the comment-tail branch stays in the chain and does not move into the helper in this run, because moving it would change binary and ternary output (skeptic cells C, D, R, U). Follow-up candidate 1 in `00-lead.md` is expected to flip this test.

### Existing tests the change may touch
Final output is locked; only the line-break phase's own output may change at the migrated sites (decision 4: the whitespace in front of the inserted line break stays for the cleanup phase). These files run the line-break phase or its rewriters alone and can lock stripped text: `Unit/LineBreaks/LineBreakRewriterTests.cs`, `Unit/LineBreaks/JoinSeparatorPhaseTests.cs`, `Unit/LineBreaks/LineBreakPhase*Tests.cs`, `Integration/LineBreakPhaseIntegrationTests.cs`, `Integration/PhaseCombinationTests.cs`, `Regression/LineBreaks/DirectiveJoinTests.cs`, `CommentJoinTests.cs`, `LeadingTriviaMultiLineDetectionTests.cs`, `ComplexElementInitializerLineBreakTests.cs` (all under `src/Reihitsu.Formatter.Test/`). Which of them change cannot be known before the change; I changed none. The implementer reports each one that changes and confirms that the change is only whitespace in front of a line break that the cleanup phase removes.

## Scenarios (bug fix)
_N/A_: no defect is reported. Both issues are refactors.

## Not covered, and why
- **Coverage overlap.** I did not prove every cell uncovered by the existing chain tests (`ChainLinkLayoutTests`, `ConditionalAccessWrapBetweenOperatorTokensTests`, `ChainSplitMemberNameRejoinTests`, `ChainLinkBehindMultiLineCommentTests` and others). Cell A is close to `ChainLinkBehindMultiLineCommentTests` but varies the link (first call behind link 0 rather than a later one). Overlap costs nothing and keeps each cell named.
- **Binary and ternary callers of `EndLineInFrontOf`.** Their output must stay, and the helper's decision stays as it is (decision 4, option a). Existing binary and ternary tests cover the callers. Skeptic cells C, D, R and U (comment tail) are deliberately not locked at the formatter level: today's output there violates SHARED-COMMENT-012, and the follow-up fixes it. The helper-level test above guards the run against moving the branch.
- **`CollapseSeparatorsToPreviousElementLine`** is optional in this run (skeptic) and already shared. Its nine callers are covered by the existing list and initializer tests.
- **Prepend family** (`PrependEndOfLine`/`MoveTokenToNewLine`): out of scope (decision 3).
- **Code-fix surface** (RH5112, RH5201, RH5048 format chains through the formatter): the same `ReihitsuFormatter` path. A fix result changes only if the formatter's output changes, and the full-pipeline tests lock that. No code-fix test added.
- **Failure classes not applicable:** naming, Renamer, analyzer parity, unsupported shapes (FC-038 to FC-043; no shape is declared, per escalation 10 answer A), Fix All convergence (no code-fix change).

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ChainGapJoinTests|FullyQualifiedName~LineBreakInFrontOfTokenTests|FullyQualifiedName~LineBreakTriviaUtilitiesTests"
```
Result on `672a136`: 41 passed, 0 failed.

## Follow-up candidates
1. `ChainGapJoinTests.SingleLineCommentBetweenDotAndMemberNameKeepsItsLine` and `DirectiveBetweenDotAndMemberNameKeepsItsLine` lock today's output, where the member name behind the comment or directive (`C()`) lands at the statement's indentation (column 8), not under the chain's other links. I have not checked this column against FMT-INDENT or SHARED-CHAIN-007. If it is wrong, it is a separate indentation defect: the run only needs the join to stay refused. A follow-up on that column would rewrite these two expectations.
