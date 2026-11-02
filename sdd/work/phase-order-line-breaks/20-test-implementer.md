Role: test implementer
Commit: see the commit that adds this handover (test changes and handover in one commit)
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This round carries out `00-lead.md` decision 33 (option A of `19-auditor.md` finding 6): tests that asserted an output no requirement or maintainer answer decides are removed. No production code touched.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| removed: `Reihitsu.Formatter.Test.Regression.LineBreaks.StatementBehindMultiLineCommentTests.ExpressionBehindCommentTailWithoutBlankLineRuleStaysOnCommentLine` | none (was an untagged characterization of undecided output) | `AssertRuleResult` | removed |
| removed: `…StatementBehindMultiLineCommentTests.TerminalBreakBehindCommentTailStaysOnCommentLine` | none (same) | `AssertRuleResult` | removed |
| removed: `Reihitsu.Formatter.Test.Unit.LineBreaks.LineBreakPhaseCommentTailStatementTests.StatementBehindCommentTailWithoutBlankLineRuleIsLeftAsWritten`, and its now unused private helper `AssertUnchanged` | none (same, phase alone) | `LineBreakPhase` alone | removed |
| `StatementBehindMultiLineCommentTests` type summary | — | — | narrowed: the clause "where none does, it stays on the comment's closing line" is dropped, since no test of the class asserts it any more |

## The other side of the condition stays covered

Decision 33 states the guard's other side (a pair without a blank-line rule behind a comment tail) remains exercised without asserting its output. Confirmed on this head:

- **Observer rows M8 and M9.** The fixture `StatementsBehindMultiLineCommentTail` in `src/Reihitsu.Formatter.Test/Unit/Pipeline/PhaseLineBreakOwnershipTests.cs` still holds `void M8() { A(); /* x⏎ */ B(); }` and `void M9(int x)` with `case 1: A(); /* x⏎ */ break;`. The fixture is a data row of `BlankLinePhaseChangesOnlyBlankLines` (FMT-PIPE-025), `BlankLinePhaseKeepsWhichTokensShareALine`, `PhasesAfterBlankLinePhaseKeepLineBreaksAndBlankLines`, and `PhasesAfterLineBreakPhaseKeepWhetherCommentsStartTheirLine` (FMT-PIPE-026). These assert phase ownership, not the final position of `B();` or `break`, so they constrain neither option of the follow-up's output question.
- **Core row.** `src/Reihitsu.Core.Test/BlankLineSpacingPolicyTests.cs`, `RequiresBlankLineBetweenStatementsReadsTheStatementRulesFromTheSyntax`, keeps `[DataRow("a++; /* x\n */ a--;", false)]` next to `[DataRow("a++; /* x\n */ return a;", true)]`: the policy's answer on both sides, independent of layout.
- The rule's side (a pair with a blank-line rule) is unchanged: 8 phase-alone rows and 7 tagged regression tests.

All three pass on this head (results under *Commands*).

## Scenarios (bug fix)

Not a bug-fix lane.

## Not covered, and why

- **The output of `A(); /* x⏎ */ B();` and of a terminal `break` behind a tail.** Deliberately unasserted: it is the output question of decision 28's follow-up (together with `19-auditor.md` finding 4's members). A test pinning either output would decide it.
- Every other entry of `15-test-implementer.md`'s *Not covered* stands unchanged.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~StatementBehindMultiLineCommentTests|FullyQualifiedName~LineBreakPhaseCommentTailStatementTests|FullyQualifiedName~PhaseLineBreakOwnershipTests"
scripts/test.sh --project core --filter "FullyQualifiedName~BlankLineSpacingPolicyTests"
scripts/test.sh --project architecture
```

On 35d7b9f plus these changes: formatter filter 40 passed (43 before, minus the 3 removed); core 62 passed; architecture 92 passed. `scripts/format.sh` over the two test files formatted 0 of 2.

Note: the working tree also held uncommitted changes to `src/Reihitsu.Core/BlankLineSpacingPolicy.cs` and `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakBlockRewriter.cs` that are not mine; they were present in the build of the runs above, and another role committed them as 29d27c9 (decision 34) before my commit. My commit holds only the two test files and this handover.
