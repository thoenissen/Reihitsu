Role: test implementer
Commit: 178bde35af7ec9f4d5a96e20f36b38724fa1a31f
Verdict: tests written

Merge of `origin/main` (e014ac6) into `claude/squad-1926-97asq4`, completed as merge commit 178bde3 (parents c607a83, e014ac6). Nothing in the prompt suggested an intended fix or verdict, so there was nothing to ignore.

## Conflict resolution

One conflicted file: `tests/Reihitsu.Analyzer.Test/Formatting/RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.cs`. Both sides had appended tests at the same place, at the end of the `Tests` region. The resolution keeps every test from both sides, unchanged:

- this branch: `VerifyFixIsOfferedForACollectionCreationBehindACommentedPropertyInitializerEquals`, `VerifyFixIsOfferedForACollectionCreationBehindACommentedAssignmentOperator` (SHARED-FIX-010, FMT-UNSUP-003);
- main: `VerifyCodeFixKeepsTrailingCommaInFrontOfEndIfBehindActiveElseBranch` (FMT-STRUCT-020, SHARED-TRAIL-004).

No test from main needed changes. In main's RH5303 test, no trivia follows the `=` of `var list =`, so the input sits outside FMT-UNSUP-020. It has no switch arm (FMT-UNSUP-021), no attribute argument, and no using alias (FMT-UNSUP-022). Its directives sit between initializer elements, after each element's comma. `scripts/format.sh` changed nothing in the file. No other file conflicted, and no other test was changed.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5303 test class (85 tests) | as tagged | `Verify`, `VerifyCodeFixWithEndOfLine` | pass: passed 85/85 |
| Analyzer test classes that main changed (RH2001 trivia, RH5404, RH5409 to RH5416, RH5418, formatter-parity classes RH5409 to RH5412) | as tagged | as declared | pass: passed 442/442 |
| All RH2001 analyzer tests (main's RH2001 change and this run's RH2001-007 touch the same rule) | as tagged | as declared | pass: passed 319/319 |

## Not covered, and why
No new tests were written: this step only merges. The default test run is left to the validator.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~.RH2001"
```

## Follow-up candidates
_None._
