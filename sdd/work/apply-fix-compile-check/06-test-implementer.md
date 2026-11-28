Role: test implementer
Commit: 1cdda96 (tests written on this head; the commit carrying them follows it)
Verdict: tests written

Repair round 1, tooling lane, no spec covers the fixture runner, so no test carries a `[Requirement]` tag. My prompt carried no intended fix or suspected cause. The rule under test is `00-lead.md` decision 2: a result is invalid when, for some error ID, its number of distinct messages exceeds the fixture's (an ID the fixture lacks counts from zero). All new tests use the existing stubs `TextReportingFakeAnalyzer` and `SourceTransformingFakeCodeFix` through a new private helper `RunReplacingFixAsync` (reports while the source contains a text, the fix replaces that text).

## Tests

New, in `tests/Reihitsu.Tooling.Test/Unit/FixtureRunnerCompilationTests.cs`:

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RunAsyncReportsFixedWhenTheFixRenamesTheContainingTypeOfAMemberAnExistingErrorNames` | audit #1, RH4002 member (CS0161 `'sample.M()'` becomes `'Sample.M()'`); FC-044 nearest | `RunReplacingFixAsync` → `FixtureRunner.RunAsync` | fails: `InvalidResult` instead of `Fixed` |
| `RunAsyncReportsFixedWhenTheFixRenamesTheMemberAnExistingErrorNames` | audit #1, RH4103 member (CS0161, member's own name) | same | fails: `InvalidResult` instead of `Fixed` |
| `RunAsyncReportsFixedWhenTheFixRenamesTheParameterAnExistingErrorNames` | audit #1, RH4105 member (CS0177, parameter name) | same | fails: `InvalidResult` instead of `Fixed` |
| `RunAsyncReportsInvalidResultWhenTheFixRenamesATypeAndAddsAnErrorWithTheSameId` | other side of decision 2 on the rename path: CS0161 goes from one distinct message to two | same | passes (and must keep passing) |
| `RunAsyncReportsInvalidResultWhenTheFixReplacesAnExistingErrorWithOneOfAnotherId` | decision 2 counts per ID, not in total: CS0246 dropped, CS0103 added, total distinct errors unchanged | same | passes (and must keep passing) |

Observed on this head: the three rename tests fail with `expected: Fixed, actual: InvalidResult`; the full tooling suite has 240 pass, only these 3 fail.

Existing tests checked against decision 2 — every one still states what it requires, assertions unchanged:

| Existing test | Under decision 2 |
|---|---|
| `...BreaksALineInsideAnInterpolationHoleBeforeCSharp11` (LF, CRLF) | CS8967 counts from zero → invalid |
| `...FromCSharp11` (11, Latest) | no new error → fixed |
| `...IntroducesAnUndefinedName` | CS0103 from zero → invalid |
| `...IntroducesOnlyAWarning` | warnings not counted → fixed |
| `RunAsyncReportsNoDiagnosticForAFixtureThatDoesNotCompile` | unaffected |
| `...KeepsACompileErrorTheFixtureAlreadyHad` | CS0246 stays at one message → fixed |
| `...RepeatsACompileErrorTheFixtureAlreadyHad` | CS0246 repeated, still one distinct message → fixed (rules out a multiset count) |
| `...AddsAnotherCompileErrorToAFixtureThatDoesNotCompile` | CS0103 from zero → invalid |
| `...AddsAnErrorWithTheSameIdForAnotherName` | CS0246 one → two distinct messages → invalid (rules out an ID-only set) |

Text changes (comments only):
- Class summary of `FixtureRunnerCompilationTests` restated as the per-ID distinct-message count, renames included.
- Summary of `...AddsAnErrorWithTheSameIdForAnotherName` restated as "two distinct messages where the fixture had one".
- Audit #2: `FixtureRunnerTests.HasSyntaxErrorsDetectsUnparseableSource` summary now says only that it is the check that classifies a fixture as a parse error.

## Scenarios (bug fix)

_Not a bug-fix reproduction round; the rename members are audit finding #1, reproduced by the three failing tests above._

## Not covered, and why

- Renames of a local, a type parameter, or a document (RH4001): every rename member reaches the same comparison in `FixtureRunner.RunAsync` (`resultErrors` against `originalErrors` from `GetCompileErrorsAsync`); the arms differ only in the message text, so the three members above cover the axes the audit names (the symbol itself, and the symbol as the container of a displayed member). A document rename changes only the path, which no compiler message embeds (audit, Guard scope).
- CRLF variants of the rename tests: compiler messages carry no line endings or positions, and the layout is not involved; the CRLF arm of the comparison is already exercised by the interpolation-hole test.
- A rename that also drops the error, or merges two errors into one: the count falls, which is the fixed side already covered by the keep/rename tests.
- The analyzer, formatter, code-fix, and unsupported-shape failure classes cannot reach the tooling runner.

## Commands

```shell
scripts/test.sh --project tooling --filter "FullyQualifiedName~Reihitsu.Tooling.Test.Unit.FixtureRunnerCompilationTests"
scripts/test.sh --project tooling
```

## Follow-up candidates

_None._ For the implementer, inside this run: the inline comment at `src/Reihitsu.Tooling/FixtureRunner.cs` above the `IsSubsetOf` check and the `GetCompileErrorsAsync` summary describe the (ID, message) set of decision 1, and `scripts/README.md` should be checked for the same wording.
