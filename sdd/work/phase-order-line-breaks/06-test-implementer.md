Role: test implementer
Commit: 5e91bcd
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This round carries out `00-lead.md` decision 17 (point 1 of `04-test-implementer.md`). Expected values come from FMT-BREAK-036, maintainer answer 1, and RH5104-001; RH5104, its fix, and its rule page are unchanged. No production code touched.

File: `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5104CommentsMustBeOnTheirOwnLineFormatterTests.cs`.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5104CommentsMustBeOnTheirOwnLineFormatterTests.VerifyFormatterKeepsCommentBehindIfConditionAndItStaysReported` (replaces `VerifyFormatterFixesViolation`, same input) | RH5104-001, FMT-BREAK-036 | local `VerifyFormatterKeepsReportedCode`: `Verify(source, RH5104)`, then `FormattingPipeline.Execute` asserted to return the input unchanged on two passes, under LF and CRLF | fail |

`VerifyFormatter` on `FormatterTestsBase<TAnalyzer>` cannot express this case: it always asserts that the formatted output is analyzer-clean. The test therefore uses a private helper in its own class, following the pattern of `RH5029…FormatterTests.VerifyFormatterLeavesCodeUnchanged`. Because the formatted output is asserted equal to the input, the RH5104 diagnostic verified on the input is also the diagnostic on the formatter's output.

Observed on 5e91bcd: the diagnostic assertion passes. The formatter assertion fails on the issue's own difference: the formatter moves `// Compare the values.` above the `if` (the retired FMT-BREAK-005 move), where the test expects it to stay behind `if (left == right)`. There is no compile or helper failure. `VerifyFormatterKeepsTwoCommentsOnALineWithoutCodeAndTheyAreNotReported` still passes.

## Scenarios (bug fix)

Not a bug-fix lane.

## Not covered, and why

- **Other heads (`while`, `else if`, `foreach`, …) on the RH5104 surface.** FMT-BREAK-036's formatter tests (`BlockStatementHeadCommentTests`, 04-test-implementer) cover every head. RH5104 is unchanged, and RH5104-001's existing analyzer tests cover reporting a trailing comment. The parity file only had the `if` case, which this round rewrites.
- **Code-fix path.** RH5104's fix is unchanged and not part of this decision.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5104CommentsMustBeOnTheirOwnLineFormatterTests"
scripts/test.sh --project architecture
```

The architecture run shows 1 failure, `OwnershipMapMatchesRuntimePhaseOrder`. It was already failing before this round (04-test-implementer), and `RequirementCoverageTests` passes.
