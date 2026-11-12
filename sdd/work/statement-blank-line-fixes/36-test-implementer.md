Role: test implementer
Commit: e1c49b3 (tests; the handover commit follows it)
Verdict: tests written

Decision #35 (auditor 35, finding #9, mutants D and E). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the two tests the lead asked for, which match 35-auditor's suggested change. No production code was touched or committed: `git diff --exit-code src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` was clean against HEAD before the commit.

## Tests
Both are in `src/Reihitsu.Analyzer.Test/Formatting/RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests.cs`, directly after `VerifyNoCodeFixWhenOnlySectionStatementBelowLabelContinuationStandsBehindTailOfMultiLineComment`. Both pass on the head, as expected: the code already holds (35-auditor, H01/H02).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyMovedStatementTakesColumnOfSectionStatementAsWrittenWhenLabelSpansMultipleLines` (finding #9 a, H01: `case 1 when⏎    value > 0: Method(1); var y = 1;⏎      break;`, `break;` two spaces deeper than one level below `case`, inside an object initializer) | SHARED-STMT-004 | `Verify(source, fixed)`, exact text, 1 incremental / 1 Fix All iteration | pass: `var y = 1;` moves to a new line behind one blank line, at `break`'s column exactly (two spaces deeper than the canonical column) |
| `VerifyNoCodeFixWhenOnlySectionStatementBelowLabelContinuationStandsBehindTailOfMultiLineCommentStartingItsOwnLine` (finding #9 b, H02: `… var y = 1;⏎    /* c⏎            d */ break;`, comment in the leading trivia of `break`, inside an object initializer) | SHARED-STMT-004 | `VerifyNoCodeFixIsOffered` | pass: diagnostic reported, no action |

Mutation check (temporary edits to `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs`, each reverted with `git checkout` and confirmed with `git diff --exit-code`; focused RH5029 class run, 52 tests):

| Mutant | Edit | Result |
|---|---|---|
| E — label-derived column | line 236: `GetLineFromPosition(lineStartingStatement.SpanStart))` → `GetLineFromPosition(label.SpanStart)) + "    "` | 1 failed: `VerifyMovedStatementTakesColumnOfSectionStatementAsWrittenWhenLabelSpansMultipleLines` (only failure); diff shows `var y = 1;` two columns left of the expected line |
| D — Core helper at the fallback | line 232: `StartsLineBehindComments(sourceText, statement.GetFirstToken())` → `SyntaxTokenPositionUtilities.IsFirstOnLine(statement.GetFirstToken())` | 1 failed: `VerifyNoCodeFixWhenOnlySectionStatementBelowLabelContinuationStandsBehindTailOfMultiLineCommentStartingItsOwnLine` (only failure); "No code fix may be offered for the diagnostic" |

With 32's mutants A, B, C, every member of the fallback's axis list in 35-auditor #9 is now pinned by a test that fails on its own mutant.

## Scenarios (bug fix)
Not applicable: test gaps for an existing fix; no new bug report.

## Not covered, and why
- CRLF variants: LF only, like their neighbours. The code reads trivia kinds and line positions from `SourceText`; 35-auditor ran H01 and H02 under CRLF with the same results.
- RH3208/RH5002 counterparts: not asked for by decision #35; they reach the same `TryGetListIndentation` through the shared owner, so the mutants are pinned there too.
- Fix All across several diagnostics: test (a) has one diagnostic; its Fix All iteration count is asserted by `Verify`. Test (b) has no action to batch.
- Failure classes: as in 35-auditor; this step adds tests only.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests"
```
Result on the head: 52 passed, 0 failed.
