Role: test implementer
Commit: 2cb6341
Verdict: tests written

This follows up the published run. It covers the clarified RH6001-001, whose keyword list now includes `fixed` (`01-spec-author.md`, `squad/decisions/2026-10-31-line-start-fact-follow-ups.md`). The prompt carried no intended fix.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `src/Reihitsu.Analyzer.Test/Formatting/RH6001KeywordsMustBeSpacedCorrectlyAnalyzerTests.VerifyFixedKeywordGluedToParenthesisIsDetectedAndFixed` (new: `fixed(int* p = items)` in an `unsafe` method is reported on `fixed`, and the fix gives `fixed (int* p = items)`) | RH6001-001, RH6001-003, RH6001-004 | `Verify(source, fixed, AllowUnsafe, diag)` (iterative and Fix All) | green |
| `….VerifyFixedKeywordFollowedByStrayDocumentationCommentIsNotReported` (existing; the `RH6001-002` tag is added) | RH6001-002 | `Verify(source, AllowUnsafe)` | green |

These are tests for existing behavior, so both pass on the current code. `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH6001"` gives `Passed: 37, Failed: 0`. The architecture spec and coverage checks (`--filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"`) give `Passed: 51`.

## Not covered, and why

- **`fixed` in `VerifyEveryListedStatementKeywordIsDetectedAndFixed`.** I did not extend that test. It compiles without `unsafe`, so `fixed` gets its own test.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH6001"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"
```
