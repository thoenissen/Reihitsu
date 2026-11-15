Role: test implementer
Commit: 2f0a416
Verdict: tests written

## Scope

Conflict resolution only: the merge of `origin/main` (`81eabeb`) into the branch, where `src/Reihitsu.Analyzer.Test/Formatting/RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests.cs` conflicted (`16-auditor.md`). No test was written or changed.

## Resolution

Both sides kept unchanged. The file is the branch's version (stage 2) with `main`'s four added tests inserted before `#endregion // Tests`, byte for byte as on `main`:

- `FixMovesLeadingCommaBehindPreviousArgument` (RH5102-003, FMT-BREAK-012)
- `FixMovesLeadingCommaOfListWrappedBehindOpeningParenthesis` (RH5102-003, FMT-BREAK-012)
- `FixMovesLeadingCommasOfSeveralArgumentListsInOneDocument` (RH5102-003, FMT-BREAK-012)
- `VerifyCommentHeldLeadingCommaWithOneArgumentPerLineIsNotReported` (RH5102-001)

The branch's `VerifyConditionalDirectiveInAGapKeepsTheListFromBeingReportedButNotOneInsideAnArgument` and `VerifyConditionalDirectiveWithAlternativesInFrontOfTheFirstArgumentKeepsTheListFromBeingReported` (RH5102-004) stay as they were. `diff` against stage 2 shows only the inserted block; test methods: branch 22, `main` 24, merged 26 (22 + 4). Encoding (BOM, LF, no final newline) kept as on the branch. `scripts/format.sh` on the file: formatted 0 of 1.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5101\|FullyQualifiedName~RH5102"` on the merged tree | RH5101/RH5102 both sides | — | Passed 59 of 59, failed 0 |

No test of either side fails against the merged code; no assertion was changed.
