Role: test implementer
Commit: see the commit that adds this file (parent dbeb6b63)
Verdict: tests written

Repair round 2, `00-lead.md` row 26, the test part: `28-auditor.md` finding #7. No test covered the second layout of `FormatSplitFieldInContext`, the one that lays the context out again from the target's document column when the first field keeps the target's line (FMT-PIPE-017, answers 3a/3b). My prompt named the work and carried no intended fix, suspected cause, or hoped-for verdict. No production code was touched. Another agent was editing `src/Reihitsu.Formatter/ReihitsuFormatter.cs` (finding #8) at the same time. I did not stage that file.

Expected values come from FMT-PIPE-029 and FMT-PIPE-017, not from patched code (FC-035):

- The first field keeps the target's line and column behind the `*/` of a comment that spans lines. That is SHARED-COMMENT-012, and the gap holds no documentation comment.
- The column counts each character in front of it, a tab as one column (answer 3b):
  - g02: `   b */      private` puts `private` in column 13.
  - The tab variant: `   b */→→private` puts it in column 9.
- The wrapped line is laid out from that column. The formatter puts the operator under the operand, 17 columns behind `private`, so `+ 2` lands in column 30 for g02 and column 26 for the tab variant.
- The later field stands at the member column. No blank line is written between the fields.

I checked these values against document-level formatting. `scripts/format.sh` over `    private int _x = 1 +⏎        2, _y;` gives `+ 2` 17 columns behind `private`, then `    private int _y;` with no blank line. The g02 shape with one space behind `*/` gives the same. The fixtures were in the scratchpad and are removed.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ReihitsuFormatterFieldSplitTests.WrappedLineOfAFirstFieldBehindACommentThatSpansLinesIsLaidOutFromTheTargetsColumn` (g02) | FMT-PIPE-029, FMT-PIPE-017; FC-017, FC-003 | `AssertSplitsTarget` (whole text, LF and CRLF, second pass) | passes |
| `ReihitsuFormatterFieldSplitTests.WrappedLineOfAFirstFieldBehindACommentThatSpansLinesAndTabsCountsEachTabAsOneColumn` (g02 with two tabs, answer 3b) | FMT-PIPE-029, FMT-PIPE-017; FC-017, FC-003 | same | passes |
| `RH7101DoNotCombineFieldsAnalyzerTests.FixLaysOutTheWrappedLineOfAFirstFieldBehindACommentThatSpansLinesFromTheDeclarationsColumn` (g02) | RH7101-004; FC-017, FC-003 | `Verify` + `AssertSingleFixUnderBothLineEndings` (exact text, one action, LF and CRLF) | passes |
| `RH7101DoNotCombineFieldsAnalyzerTests.FixCountsEachTabInFrontOfAFirstFieldBehindACommentThatSpansLinesAsOneColumn` (tab variant) | RH7101-004; FC-017, FC-003 | same | passes |

## Watched failures (mutation)

I made a scratch worktree at dbeb6b63 holding these test files and changed nothing else of the branch. In it, the condition of the second `FormatContext` call (`if (mayShareLineWithPrecedingMember && sharesLineWithPrecedingMember == false)`) was made always false. The worktree is removed.

- **Formatter** (`ReihitsuFormatterFieldSplitTests`): 2 failed, 45 passed. The 2 failures are exactly the new tests, each at "Unexpected first pass under LF line endings". In each, `+ 2` stands in column 25 instead of 30 (g02) or 26 (tabs). Column 25 is the layout from the column the context's own layout gives `private`, one space behind `*/`.
- **Analyzer** (`RH7101DoNotCombineFieldsAnalyzerTests`): 2 failed, 80 passed. The 2 failures are exactly the new tests, in `Verify`'s fix comparison, with the same column-25 diff.
- The tab variant tells all three readings apart:
  - the specified one, each tab one column: 26;
  - a tab at the preparation phase's width: 32;
  - no second layout: 25.

## Not covered, and why

- **CRLF under the mutation.** Each test stops at its first LF assertion. On the head, the CRLF arm of every new test passes against the exact text. The branch reads no line ending; it reads only the front-gap answer and the document column.
- **A one-space or one-tab gap** (g01, g03, g05, g07 of 28). The context's own layout already puts `private` one column behind `*/` there, so both layouts agree and the branch cannot be observed. The auditor found no difference for them.
- **The formatter-layout helper** (`AssertFormattingAddsOnlyBlankLines`). Document-level formatting collapses the spaces behind `*/` to one. The fix keeps them by FMT-PIPE-017/018. So the helper does not apply, as for the holding tests of 22 and 25.
- **Fix All.** Each input has one diagnostic, and Fix All is unchanged.
- **The other side of the branch** (the first field starts the next line, so the first layout is final). The A–D "moves" tests of 22 already cover it.
- **Failure classes.** This step only adds tests. The classes that 28 dismissed stay dismissed. FC-017 and FC-003 are the classes of #7, and these tests close them.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterFieldSplitTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH7101DoNotCombineFieldsAnalyzerTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~Sdd|FullyQualifiedName~Comment"
```

Results on dbeb6b63 plus these tests (Release). The working tree also held the other agent's uncommitted edit of `ReihitsuFormatter.cs`. Formatter: 47 passed. Analyzer: 82 passed. Architecture: 62 passed.
