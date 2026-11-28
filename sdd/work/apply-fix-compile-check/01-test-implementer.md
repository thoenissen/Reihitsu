Role: test implementer
Commit: 576f2cd671cf2eb14ff4c5969be36b10b85a292d (tests written on this head; the commit carrying them follows it)
Verdict: tests written

Tooling lane, no spec under `sdd/specs/` covers the fixture runner, so no test carries a `[Requirement]` tag. My prompt carried no intended fix or suspected cause. The issue's own example no longer reproduces through a real rule (maintainer comment on the issue, `00-lead.md`), so every test drives `FixtureRunner.RunAsync` with the existing stub doubles `TextReportingFakeAnalyzer` and `SourceTransformingFakeCodeFix`. No new helper was added.

Probe on this head (scratch test, deleted): under C# 10 the multi-line hole source has no parse diagnostic and one compilation error, CS8967; under C# 11 only CS5001. The fixture project compiles as `ConsoleApplication`, so every fixture without `Main` carries CS5001 in both the original and the result. `private Missing _first, _second;` reports CS0246 once; split into two declarations, it reports CS0246 twice with the same message.

## Tests

All in `tests/Reihitsu.Tooling.Test/Unit/FixtureRunnerCompilationTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RunAsyncReportsInvalidResultWhenTheFixBreaksALineInsideAnInterpolationHoleBeforeCSharp11` (LF, CRLF) | issue reproduction (CS8967); also asserts the result has no parse error under C# 10 | stub analyzer + stub fix, `FixtureRunner.RunAsync` | fails: `Fixed` instead of `InvalidResult` |
| `RunAsyncReportsFixedWhenTheFixBreaksALineInsideAnInterpolationHoleFromCSharp11` (C# 11, Latest) | other side of the version condition; result compiles | same | passes |
| `RunAsyncReportsInvalidResultWhenTheFixIntroducesAnUndefinedName` | any new compile error (CS0103), not only version errors | same | fails: `Fixed` instead of `InvalidResult` |
| `RunAsyncReportsFixedWhenTheFixIntroducesOnlyAWarning` | severity condition: a new warning (CS0219) is no error | same | passes |
| `RunAsyncReportsNoDiagnosticForAFixtureThatDoesNotCompile` | a fixture's own compile errors are no `parse-error` (stable behavior) | same | passes |
| `RunAsyncReportsFixedWhenTheFixKeepsACompileErrorTheFixtureAlreadyHad` | "errors the original did not have": the fixture's CS0246 stays | same | passes |
| `RunAsyncReportsFixedWhenTheFixRepeatsACompileErrorTheFixtureAlreadyHad` | the same error (same ID and message) reported more often after a field split is not new | same | passes |
| `RunAsyncReportsInvalidResultWhenTheFixAddsAnotherCompileErrorToAFixtureThatDoesNotCompile` | a new error of another ID (CS0103) on a fixture that already has CS0246 | same | fails: `Fixed` instead of `InvalidResult` |
| `RunAsyncReportsInvalidResultWhenTheFixAddsAnErrorWithTheSameIdForAnotherName` | a new CS0246 for another name next to the fixture's CS0246: comparing by ID alone is not enough | same | fails: `Fixed` instead of `InvalidResult` |

Observed on this head: 5 fail, each with `Expected: InvalidResult, actual: Fixed`; 6 pass. The full tooling suite: 233 pass, only these 5 fail.

Two of these tests pin design choices the lead can override. Together, the "repeats" and "same ID for another name" tests require comparing the errors as a set of (ID, message), not as a multiset and not by ID alone. The reason for the "repeats" test: a field split of an unknown type is a real RH7101 fixture shape, and it would otherwise come out as `invalid-result`.

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Stub fix writes the object creation onto several lines inside a `$"` hole, `--lang-version 10` | LF | `FixtureRunner.RunAsync` | reproduced | `Fixed` vs `InvalidResult` |
| same | CRLF | same | reproduced | `Fixed` vs `InvalidResult` |
| Issue's example through a real rule (RH5301) | — | `apply-fix.sh` | not reachable | the rule no longer reports there (maintainer comment), so no real-rule end-to-end test is possible |

## Not covered, and why

- No end-to-end `CodeFixRunCommand` / `apply-fix.sh` test of the `invalid-result` token. `CodeFixRunCommand` resolves only real IDs through `CodeFixTargetResolver`, and no shipped fix produces a result that compiles worse. `FixtureOutcome.InvalidResult` already maps to the token and to `ExitCodes.Error` in `DescribeOutcome` and `GetExitCode` (`src/Reihitsu.Tooling/CodeFixRunCommand.cs`), and those are unchanged.
- Multi-iteration timing: whether a new compile error is checked after every iteration or only on the final result is not pinned. The issue does not decide it, and both would pass these tests.
- Failure-class catalog: the analyzer, formatter, code-fix, and unsupported-shape classes cannot reach the tooling runner. This lane changes no shipped output.
- The existing parse-error test `FixtureRunnerTests.RunAsyncReportsInvalidResult` stays and must keep passing.

## Commands

```shell
scripts/test.sh --project tooling --filter "FullyQualifiedName~Reihitsu.Tooling.Test.Unit.FixtureRunnerCompilationTests"
scripts/test.sh --project tooling
```

## Follow-up candidates

_None._ For the implementer, inside this run: the `invalid-result` row in `scripts/README.md` ("produced source that no longer parses"), the exit-code paragraph below it, the `FixtureOutcome.InvalidResult` doc comment, and the `RunAsync` `languageVersion` parameter doc all describe the parse-only check.
