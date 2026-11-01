Role: test implementer
Commit: 50a5b05 (base); this handover's commit carries the removal
Verdict: tests written

Scope: repair round 1 of thoenissen/Reihitsu#1497, decision row 8 in `00-lead.md`, which applies maintainer answer 8. I read `00-lead.md` (*Maintainer answers* item 8, *Decisions taken* row 8) and `11-test-implementer.md`. The prompt asked me to remove tests and named no fix or verdict to aim for, so there was nothing to ignore. No production code changed, and no kept test changed.

## Removed

Each of these tests uses a trailing block comment that spans lines between two using directives. Maintainer answer 8 calls that shape unwanted, and the run pins no output for it.

| File | Test | Was for |
|---|---|---|
| `src/Reihitsu.Formatter.Test/Unit/UsingDirectives/UsingDirectivePhaseCommentBlankLineTests.cs` | HeaderBlankLineAndWhitespaceOnlyLineBecomeOneBlankLineBehindATrailingCommentSpanningLines | finding 1 (e), FMT-USING-010 |
| same | BlankLineAboveCommentIsKeptBehindATrailingCommentSpanningLines | finding 1 (b), FMT-USING-010 |
| same | DirectiveWithoutCommentGetsNoLineBreakBehindATrailingCommentSpanningLines | FMT-USING-005 pin |
| `src/Reihitsu.Analyzer.Test/Formatting/RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzerTests.cs` | CodeFixWritesOneBlankLineForHeaderBlankLineAndWhitespaceOnlyLineBehindATrailingCommentSpanningLines | finding 1 (e), RH7207-004 |
| same | CodeFixKeepsBlankLineAboveCommentBehindATrailingCommentSpanningLines | finding 1 (b), RH7207-004 |
| `src/Reihitsu.Core.Test/UsingDirectiveOrderingUtilitiesTests.cs` | HasBlankLineAboveIsFalseBehindATrailingCommentSpanningLines | audit finding 2, SHARED-USING-008 |
| same | IsLedByCommentIsFalseForCommentTrailingThePredecessor | audit finding 2, SHARED-USING-011 |

Neither Core test is the only test of a branch:

- **`HasBlankLineAbove`**: the spanning-comment input has a predecessor whose trailing trivia ends no line (count 0), and its directive's leading trivia is empty (`firstSignificantTriviaIndex < 0`). `HasBlankLineAboveIsFalseForDirectiveSharingItsPredecessorsLine` (`using System; using System.Linq;`) covers exactly the same two branches. Of the remaining branches, the predecessor's line break (count 1) is covered by `…IsFalseForOneLineBreakFromThePredecessor`, a leading comment (`>= 0`) by `…IsTrueForBlankLineAboveTheLeadingComment`, and both outcomes of `>= 2` by several tests.
- **`IsLedByComment`**: its input also has empty leading trivia, which is the `< 0` branch. `IsLedByCommentIsFalseWithoutLeadingTrivia` covers that branch too.

## Kept

- All other Core tests for `IsLedByComment` and `HasBlankLineAbove` from audit finding 2. None of their inputs contains a trailing comment that spans lines.
- The RH5020 tests that contain `b */`. They predate the run (`git diff b27f329` shows that the run added only the two same-group comment tests there, and neither uses this shape).

Helpers: every private helper in the phase test file (`AssertResult`, `AssertUnchanged`, `Apply`, `Normalize`, `Describe`, `_lineEndings`) still has callers. The RH7207 and Core removals used only shared base helpers, so no helper lost its last caller.

## Tests

No test was added. The kept tests are as listed in `10-test-implementer.md` and `11-test-implementer.md`, minus the removals above.

## Not covered, and why

- The spanning trailing-comment shape between using directives: per maintainer answer 8 and decision row 8, it gets no test and no follow-up.

## Commands

```shell
scripts/format.sh <the three test files>   # formatted 0 of 3
scripts/test.sh --project formatter
scripts/test.sh --project analyzer
scripts/test.sh --project core
scripts/test.sh --project architecture
```

Results: formatter 3744/3744 (3 fewer), analyzer 6383/6383 (2 fewer), core 563/563 (2 fewer), architecture 92/92.
