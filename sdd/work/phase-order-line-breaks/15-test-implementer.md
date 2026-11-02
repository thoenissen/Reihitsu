Role: test implementer
Commit: see the commit that adds this handover (tests and handover in one commit)
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This round carries out the test parts of `00-lead.md` decisions 28 (option B of `14-auditor.md` finding 1) and 31 (direct Core tests for the public `BlankLineSpacingPolicy` members). No production code touched.

Expected values: every full-pipeline expected output is `main`'s output (decision 28). I computed it by running `main`'s own CLI (`scripts/format.sh` in a worktree of `origin/main` aec8dc6) on a fixture holding every member `14-auditor.md` lists, under LF and CRLF (identical apart from the line ending, stable on a second pass), and then ran the new regression class itself on that `main` worktree: 9 passed. The phase-alone and observer expectations come from FMT-BREAK-006 read with SHARED-COMMENT-012 ("the formatter MUST break the line in front of the token") and FMT-PIPE-025 ("the blank-line phase adds and removes only blank lines"), not from code.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Reihitsu.Formatter.Test.Unit.LineBreaks.LineBreakPhaseCommentTailStatementTests.StatementBehindCommentTailStartsTheLineBelowIt` ×6 (return behind expression; return behind `;`; statement behind `}`; `break` in a loop; expression behind local declaration; local declaration behind expression) | FMT-BREAK-006, SHARED-COMMENT-012 | `LineBreakPhase` alone, LF and CRLF; the statement's line exists and the line directly above is exactly `*/` (no blank line) | fail (the line-break phase leaves the statement behind `*/`) |
| `…LineBreakPhaseCommentTailStatementTests.ReturnBehindCommentTailOfEmptyStatementInSwitchSectionStartsTheLineBelowIt` | FMT-BREAK-006, SHARED-COMMENT-012 | same | fail |
| `…LineBreakPhaseCommentTailStatementTests.ReturnBehindCommentTailInSwitchSectionStartsTheLineBelowIt` | FMT-BREAK-006, SHARED-COMMENT-012 | same | fail |
| `…LineBreakPhaseCommentTailStatementTests.StatementBehindCommentTailWithoutBlankLineRuleIsLeftAsWritten` | none (characterization, the other side of the condition; FC-003) | `LineBreakPhase` alone, unchanged, LF and CRLF | pass |
| `Reihitsu.Formatter.Test.Regression.LineBreaks.StatementBehindMultiLineCommentTests` — 7 tests, one per failing member of finding 1 (`ReturnBehindCommentTail…`, `ReturnBehindCommentTailOfEmptyStatement…`, `StatementBehindCommentTailOfClosingBrace…`, `BreakInLoopBehindCommentTail…`, `ExpressionBehindCommentTailOfLocalDeclaration…`, `ReturnBehindCommentTailOfEmptyStatementInSwitchSectionIsBraced`, `ReturnBehindCommentTailInSwitchSectionIsBraced`) | FMT-BREAK-006, SHARED-COMMENT-012 | `AssertRuleResult(input, expected)` (exact output, second pass, phase stability, LF and CRLF) | pass (output as on `main`; guards that option B keeps it) |
| `…StatementBehindMultiLineCommentTests.ExpressionBehindCommentTailWithoutBlankLineRuleStaysOnCommentLine`, `…TerminalBreakBehindCommentTailStaysOnCommentLine` | none (characterization of the two pre-existing members left to the follow-up; they pin that option A is not shipped by accident) | `AssertRuleResult` | pass |
| `Reihitsu.Formatter.Test.Unit.Pipeline.PhaseLineBreakOwnershipTests.BlankLinePhaseChangesOnlyBlankLines` ×6 (every fixture) | FMT-PIPE-025 | observer; the blank-line phase's output and input, blank lines removed and lines right-trimmed, are equal; LF and CRLF | fail for `StatementsBehindMultiLineCommentTail` (the blank-line phase writes the line break behind `*/`); pass for the 5 existing fixtures |
| New fixture `StatementsBehindMultiLineCommentTail` added to `BlankLinePhaseKeepsWhichTokensShareALine`, `PhasesAfterBlankLinePhaseKeepLineBreaksAndBlankLines` (FMT-PIPE-025) and `PhasesAfterLineBreakPhaseKeepWhetherCommentsStartTheirLine` (FMT-PIPE-026) | FMT-PIPE-025 / FMT-PIPE-026 (existing tags) | observer | pass (the physical-line measure cannot see finding 1, as the auditor noted; the new test above does) |
| `Reihitsu.Core.Test.BlankLineSpacingPolicyTests.RequiresBlankLineBehindClosingBraceDependsOnTheLastTokenOfThePrecedingStatement` ×2 | SHARED-BLANK-003 | direct | pass |
| `…RequiresBlankLineBehindClosingBraceReturnsFalseForTerminalSwitchSectionBreak` | SHARED-BLANK-004 | direct | pass |
| `…RequiresBlankLineForStatementKindReturnsTrueForJumpAndCompoundStatements` ×7 | FMT-BLANK-001 | direct | pass |
| `…RequiresBlankLineForStatementKindSeparatesLocalDeclarations` ×5 | FMT-BLANK-002 | direct | pass |
| `…RequiresBlankLineForStatementKindSeparatesYieldStatements` ×5 | FMT-BLANK-003 | direct | pass |
| `…RequiresBlankLineForStatementKindExemptsOnlyTheTerminalSwitchSectionBreak` ×2 | FMT-BLANK-004, SHARED-BLANK-004 | direct | pass |
| `…RequiresBlankLineForStatementKindReturnsFalseForStatementsNoRuleSeparates` ×5 | none (no rule; the other side) | direct | pass |
| `…RequiresBlankLineBetweenStatementsReadsTheStatementRulesFromTheSyntax` ×12 (incl. existing blank line, same line, and a multi-line comment in the gap: the result does not depend on layout) | FMT-BREAK-006 | direct | pass |
| `…RequiresBlankLineBetweenStatementsExemptsTheTerminalSwitchSectionBreak` ×4 | FMT-BREAK-006, SHARED-BLANK-004 | direct | pass |
| `…RequiresBlankLineBetweenStatementsSeparatesBreakInsideBlockOfSwitchSection` | FMT-BREAK-006, SHARED-BLANK-004 | direct | pass |

Every new failing test fails for the finding's reason: the phase-alone tests report "No line of the phase output … starts with '<statement>'" (the statement is still behind `*/`), and the observer test reports the blank-line phase's non-blank lines grew (79 → 86 for the fixture). The Core tests pin the current public contract; they pass and must keep passing through decision 29's consolidation (finding 2), which is the point of decision 31.

## Scenarios (bug fix)

Not a bug-fix lane.

## Not covered, and why

- **Comment-tail members inside the switch-brace phase's inserted block measured at that phase.** Decision 28 places the line break in the line-break phase, which runs before the switch-brace phase; the switch-section tests run the line-break phase alone on the unbraced section and the full pipeline on the braced output, which covers both ends.
- **Directive in the gap.** A directive ends its line, so a statement behind it starts its line already and the comment-tail condition cannot arise; `PhaseLineBreakOwnershipTests` and the existing FMT-BLANK-005 tests cover directives.
- **A `/** */` comment instead of `/* */`.** Both are `MultiLineCommentTrivia`/documentation trivia read by the same SHARED-COMMENT-012 line-start measure; the auditor's matrix lists only `/* */`, and the pipeline output for `/** */` was not checked against `main`, so it is not asserted.
- **Option A's members** (`A(); /* x⏎ */ B();`, terminal `break` behind a tail) are only characterized, untagged, as on `main`; their change is the follow-up's output question (decision 28).
- **FC catalog.** Idempotency and CRLF are inside `AssertRuleResult` and every phase-alone/observer helper. No code fix, naming, or literal is involved. Finding 2 (FC-015) and finding 3 (FC-031) are behavior-neutral; decision 31's Core tests guard finding 2's refactor.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~StatementBehindMultiLineCommentTests|FullyQualifiedName~LineBreakPhaseCommentTailStatementTests|FullyQualifiedName~PhaseLineBreakOwnershipTests"
scripts/test.sh --project core --filter "FullyQualifiedName~BlankLineSpacingPolicyTests"
scripts/test.sh --project architecture
```

On 1678805 plus these tests: formatter filter 34 passed, 9 failed (the 8 phase-alone rows and `BlankLinePhaseChangesOnlyBlankLines(StatementsBehindMultiLineCommentTail)`); core 62 passed; architecture 92 passed. `scripts/format.sh` over the four test files formatted 2 of 4.
