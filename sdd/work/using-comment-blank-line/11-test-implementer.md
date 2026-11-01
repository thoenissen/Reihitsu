Role: test implementer
Commit: 426bf24 (base); this handover's commit carries the removal
Verdict: tests written

Scope: repair round 1 of thoenissen/Reihitsu#1497, after maintainer answer 8. Read: `00-lead.md` (*Maintainer answers* item 8, *Decisions taken* rows 4–7), `10-test-implementer.md` (`fd86fdd`), `09-auditor.md`. The prompt asked me to remove the tests that pin a different output for a trailing block comment spanning lines between two directives. It named no fix or verdict to aim for, so there was nothing to ignore. No production code changed and no kept test changed.

## Removed

I removed the 13 tests that failed on the current code in `fd86fdd`. Each one pinned an output for audit finding 1 member (d), decision 5, or decision 6, and maintainer answer 8 withdrew all three:

| File | Test | Was for |
|---|---|---|
| `src/Reihitsu.Formatter.Test/Unit/UsingDirectives/UsingDirectivePhaseCommentBlankLineTests.cs` | HeaderBlankLineCountsForTheCommentThatMovesBehindATrailingCommentSpanningLines | finding 1 (d) |
| same | CommentOnALineOfItsOwnStaysThereBehindATrailingCommentSpanningLines | decision 5 |
| same | NoTrailingWhitespaceIsLeftWhenTheHeaderBlankLineIsKeptBehindATrailingCommentSpanningLines | decision 6 |
| same | NoTrailingWhitespaceIsLeftWhenTheBlankLineAboveACommentIsKeptBehindATrailingCommentSpanningLines | decision 6 |
| same | NoTrailingWhitespaceIsLeftWhenACommentOnALineOfItsOwnIsPlacedBehindATrailingCommentSpanningLines | decision 6 |
| `src/Reihitsu.Analyzer.Test/Formatting/RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzerTests.cs` | CodeFixKeepsHeaderBlankLineAboveCommentThatMovesBehindATrailingCommentSpanningLines (+ …UnderCarriageReturnLineFeed) | finding 1 (d) |
| same | CodeFixKeepsCommentOnALineOfItsOwnBehindATrailingCommentSpanningLines (+ …UnderCarriageReturnLineFeed) | decision 5 |
| same | CodeFixLeavesNoTrailingWhitespaceWhenItKeepsTheHeaderBlankLineBehindATrailingCommentSpanningLines | decision 6 |
| same | CodeFixLeavesNoTrailingWhitespaceWhenItKeepsTheBlankLineAboveACommentBehindATrailingCommentSpanningLines | decision 6 |
| same | CodeFixLeavesNoTrailingWhitespaceWhenACommentOnALineOfItsOwnIsPlacedBehindATrailingCommentSpanningLines | decision 6 |
| `src/Reihitsu.Analyzer.Test/Ordering/RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests.cs` | CodeFixKeepsCommentOnALineOfItsOwnBehindATrailingCommentSpanningLines | decision 5 |

After the removal, every private helper in the phase test file still has callers.

## Tests

These are the tests kept from `fd86fdd`. All of them pass on the current code:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| HeaderBlankLineAndWhitespaceOnlyLineBecomeOneBlankLineBehindATrailingCommentSpanningLines — member (e) | FMT-USING-010 | phase alone (`AssertResult`, LF + CRLF) | passes |
| BlankLineAboveCommentIsKeptBehindATrailingCommentSpanningLines — member (b) | FMT-USING-010 | phase alone | passes |
| DirectiveWithoutCommentGetsNoLineBreakBehindATrailingCommentSpanningLines | FMT-USING-005 | phase alone | passes |
| CodeFixWritesOneBlankLineForHeaderBlankLineAndWhitespaceOnlyLineBehindATrailingCommentSpanningLines — (e) | RH7207-004 | `Verify` | passes |
| CodeFixKeepsBlankLineAboveCommentBehindATrailingCommentSpanningLines — (b) | RH7207-004 | `Verify` | passes |
| Core `IsLedByComment…` / `HasBlankLineAbove…` tests (audit finding 2), listed in `10-test-implementer.md` | SHARED-USING-008 / SHARED-USING-011 / untagged mechanism tests | direct | pass |

Members (b) and (e) also contain the spanning trailing comment. They stay because they pass on the current code, so they pin today's output and do not ask for special handling. If the maintainer's "no special handling" should also free that output, those 4 tests are the ones to drop.

## Not covered, and why

- Finding 1 (d), decision 5, decision 6: the maintainer withdrew them (answer 8), so they get no test and no follow-up.
- Everything else is unchanged from `10-test-implementer.md`.

## Note

The test runs included a working-tree change I did not make: `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Utilities/UsingLeadingTriviaBuilder.cs` drops `using System.Linq;`, which is audit finding 3. While I worked, the cleaner committed it as `7d3162c`, so this commit sits on top of it and contains only the test files and this handover.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~UsingDirectivePhaseCommentBlankLineTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzerTests|FullyQualifiedName~RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests"
scripts/test.sh --project core --filter "FullyQualifiedName~UsingDirectiveOrderingUtilitiesTests"
```

Results:

- formatter filter: 23/23 passed.
- analyzer filter: 97/97 passed.
- core filter: 40/40 passed.
- Full projects: formatter 3747/3747, analyzer 6385/6385, core 565/565, architecture 92/92.
