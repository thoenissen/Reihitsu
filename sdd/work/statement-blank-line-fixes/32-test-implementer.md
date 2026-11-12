Role: test implementer
Commit: 61ff74a (tests written on this head; the handover commit follows it)
Verdict: tests written

Repair round 2 for decision #34 (auditor 31, findings #7 and #8). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the three tests the lead asked for, which match 31-auditor's suggested changes. No production code was touched or committed.

## Tests
All three are in `src/Reihitsu.Analyzer.Test/Formatting/RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests.cs`, directly after `VerifyNoCodeFixWhenOnlyAnotherSectionsStatementStartsItsOwnLine`. All pass on the head, as expected: the code already holds (31-auditor, F02/F03/F13).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyNoCodeFixWhenPrecedingStatementStartsOnLastLineOfMultiLineCommentStartingItsOwnLineInsideObjectInitializer` (finding #7, F13: `Method(0);⏎/* x⏎        y */ Method(1); var y = 1;`, comment in the leading trivia of `Method(1)`) | SHARED-STMT-004 | `VerifyNoCodeFixIsOffered` | pass: diagnostic reported, no action |
| `VerifyMovedStatementTakesColumnOfSectionStatementLineStartingWithCommentWhenLabelSpansMultipleLines` (finding #8, F02: `case 1 when⏎    value > 0: Method(1); var y = 1;⏎    /* c */ break;`) | SHARED-STMT-004 | `Verify(source, fixed)`, exact text, 1 incremental / 1 Fix All iteration | pass: `var y = 1;` moves to a new line behind one blank line, at the column of the `/* c */ break;` line (the column of `/*`, which is the line's first non-whitespace character per SHARED-STMT-004) |
| `VerifyNoCodeFixWhenOnlySectionStatementBelowLabelContinuationStandsBehindTailOfMultiLineComment` (finding #8, F03: `… var y = 1; /* c⏎    d */ break;`) | SHARED-STMT-004 | `VerifyNoCodeFixIsOffered` | pass: diagnostic reported, no action |

Mutation check (temporary edits to `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs`, each reverted with `git checkout`, none committed; focused RH5029 class run, 50 tests):

| Mutant | Edit | New test that fails |
|---|---|---|
| A — strict fallback | line 232: `StartsLineBehindComments(...)` → `SyntaxTokenPositionUtilities.StartsLine(statement.GetFirstToken())` | `VerifyMovedStatementTakesColumnOfSectionStatementLineStartingWithCommentWhenLabelSpansMultipleLines` (only failure) |
| B — trivia-blind fallback | line 232: predicate → `true` | `VerifyNoCodeFixWhenOnlySectionStatementBelowLabelContinuationStandsBehindTailOfMultiLineComment` (plus three existing continuation tests) |
| C — Core helper for the line starter | line 223: `StartsLineBehindComments(sourceText, lineStartToken)` → `SyntaxTokenPositionUtilities.IsFirstOnLine(lineStartToken)` | `VerifyNoCodeFixWhenPrecedingStatementStartsOnLastLineOfMultiLineCommentStartingItsOwnLineInsideObjectInitializer` (only failure) |

So both sides of the comment-tolerant fallback (#8) and the leading-trivia placement of a comment tail (#7) are now pinned, each by a test that fails on its own mutant.

## Scenarios (bug fix)
Not applicable: repair round for test gaps; no new bug report.

## Not covered, and why
- RH3208 counterpart of #7 (optional per 31-auditor): not asked for by decision #34. RH3208 reaches the same `CanInsertBlankLineBefore` → `TryGetListIndentation` → `StartsLineBehindComments` call chain, so mutant C is pinned through the shared owner.
- CRLF variants: the three tests are LF only, like their neighbours. The changed code reads trivia kinds and line positions from `SourceText`; 31-auditor ran F02, F03 and F13 under CRLF with the same results.
- Fix All: test 2 has one diagnostic; Fix All iteration count is asserted by `Verify`. The refusal tests have no action to batch.
- Failure classes: as in 31-auditor; this round adds tests only.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests"
```
Result on the head: 50 passed, 0 failed.
