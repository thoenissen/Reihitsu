Role: test implementer
Commit: 490fbb5 (tests written on this head; the commit carrying them follows it)
Verdict: tests written

Architecture decision 3 of `00-lead.md`, tooling lane; no spec covers the fixture runner, so no test carries a `[Requirement]` tag. My prompt carried no intended fix or suspected cause. The rule under test is decision 3: a fix result is invalid when it carries an error ID the fixture did not have; message text is not compared.

## Tests

All in `tests/Reihitsu.Tooling.Test/Unit/FixtureRunnerCompilationTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RunAsyncReportsFixedWhenTheFixRenamesOneOfSeveralSameNamedSymbolsPerIteration` (new) | audit #3, FC-044: two methods each with an unassigned `out int Value` (CS0177 twice, one message); the stub fix renames the first occurrence per iteration, so after iteration 1 CS0177 shows `value` and `Value` | stub `TextReportingFakeAnalyzer` + inline `SourceTransformingFakeCodeFix` (first occurrence only), `FixtureRunner.RunAsync`, LF, `Latest` | fails on this head: `expected: Fixed, actual: InvalidResult`; asserts `Fixed`, 2 iterations, and the final source with both parameters renamed |
| `RunAsyncReportsInvalidResultWhenTheFixAddsAnErrorWithTheSameIdForAnotherName` | removed, as decision 3 says: it required a per-name distinction | — | — |
| `RunAsyncReportsInvalidResultWhenTheFixRenamesATypeAndAddsAnErrorWithTheSameId` | removed, as decision 3 says | — | — |

Remaining tests checked against decision 3; assertions unchanged:

| Existing test | Under decision 3 |
|---|---|
| `...BreaksALineInsideAnInterpolationHoleBeforeCSharp11` (LF, CRLF) | CS8967 new ID → invalid |
| `...FromCSharp11` (11, Latest) | no new ID → fixed |
| `...IntroducesAnUndefinedName` | CS0103 new ID → invalid |
| `...IntroducesOnlyAWarning` | warnings not counted → fixed |
| `RunAsyncReportsNoDiagnosticForAFixtureThatDoesNotCompile` | unaffected |
| `...KeepsACompileErrorTheFixtureAlreadyHad` | CS0246 existing → fixed |
| `...RepeatsACompileErrorTheFixtureAlreadyHad` | CS0246 repeated → fixed |
| `...AddsAnotherCompileErrorToAFixtureThatDoesNotCompile` | CS0103 new next to CS0246 → invalid |
| three single-symbol rename tests (containing type, member, parameter) | existing ID → fixed; they pass on this head and must keep passing |
| `...ReplacesAnExistingErrorWithOneOfAnotherId` | CS0103 new ID → invalid |

Comment changes only:
- Class summary restated for the ID-only rule ("an error ID the fixture did not have"; existing IDs count as fixed whatever names their messages show).
- `...RenamesTheContainingTypeOfAMemberAnExistingErrorNames` summary: "the fixture already had the error's ID" instead of "keeps one distinct message".
- `...ReplacesAnExistingErrorWithOneOfAnotherId` summary: "CS0103 is an ID the fixture did not have" instead of the per-ID distinct-message count.
- `FixtureRunnerTests` needs no change (`HasSyntaxErrorsDetectsUnparseableSource` was fixed in round 1; `RunAsyncReportsInvalidResult` is the parse-error path).

Observed on this head: full tooling suite 242 tests, 241 pass, only the new test fails, for the reason above.

## Scenarios (bug fix)

_Not a bug-fix round._

## Not covered, and why

- The other members of audit #3 (RH4115 locals, RH4104 local functions, RH4002 types in two namespaces): each reaches the same comparison in `FixtureRunner.RunAsync` (result error IDs against the fixture's from `GetCompileErrorsAsync`); they differ only in the message text, which decision 3 stops reading. One member suffices.
- A fix that replaces an existing error with one of the same ID for another name (CS0246 `Missing` → `Other`) is `fixed` under decision 3, the accepted limit; no test pins that limit, since the decision drops the per-name tests rather than inverting them, and the keep/repeat/rename tests already cover the "existing ID" side of the condition.
- CRLF for the new test: compiler messages and IDs carry no line ending; the CRLF arm of the comparison is exercised by the interpolation-hole test.
- Analyzer, formatter, code-fix, and unsupported-shape failure classes cannot reach the tooling runner.

## Commands

```shell
scripts/test.sh --project tooling --filter "FullyQualifiedName~Reihitsu.Tooling.Test.Unit.FixtureRunnerCompilationTests"
scripts/test.sh --project tooling
```

## Follow-up candidates

_None._ For the implementer, inside this run: the `GetCompileErrorsAsync` summary and the inline comment above the check in `src/Reihitsu.Tooling/FixtureRunner.cs`, the `FixtureOutcome.InvalidResult` summary, the `invalid-result` row of `scripts/README.md`, and the `RunAsync` `languageVersion` doc (audit #4) describe the per-ID distinct-message count.
