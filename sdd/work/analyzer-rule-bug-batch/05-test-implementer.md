Role: test implementer
Commit: 73954f3
Verdict: tests written

Alignment round after `04-spec-author.md`: one existing test aligned with the corrected RH5602-001 example. No other test changed. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `src/Reihitsu.Analyzer.Test/Formatting/RH5602CodeMustNotContainTrailingWhitespaceAnalyzerTests.VerifyEveryRunOfTheRequirementExampleIsReported` | RH5602-001, RH5602-002, SHARED-DIAG-003 | `Verify(source)` | fails: 5 of 11 reported (every run behind a literal's end and in the interpolation hole missing); no compiler diagnostic |

Changes to the test: the source is now the corrected RH5602-001 example verbatim (`string p = @"a··`, `string q = """`); the `CompilerDiagnostics.None` configuration, its comment, and the `using Microsoft.CodeAnalysis.Testing;` that only it needed are removed, so compiler errors are verified again. Tags unchanged.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| RH5602-001 example | LF | analyzer | reproduced | 5 diagnostics (lines 2–6) vs 11; the example compiles |

## Not covered, and why

Nothing new: the matrix in `03-test-implementer.md` (#1074 section) is unchanged.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5602CodeMustNotContainTrailingWhitespaceAnalyzerTests.VerifyEveryRunOfTheRequirementExampleIsReported"
```
