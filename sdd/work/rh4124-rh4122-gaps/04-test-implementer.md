Role: test implementer
Commit: a5f5753
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing to ignore.

Behavior-change lane: tests derived from the examples of RH4124-003 and RH4122-004 and from the skeptic's candidate inputs (02-skeptic.md). Every positive test fails today with only missing diagnostics ("Mismatch between number of diagnostics returned, expected N actual M", where M counts only the already-reported RH4122-001 member parameters); every negative test passes today, as it must. No compiler errors in any fixture.

## Tests

RH4124 — `tests/Reihitsu.Analyzer.Test/Naming/RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzerTests.cs`; RH4122 — `tests/Reihitsu.Analyzer.Test/Naming/RH4122SingleLetterParameterNamesAnalyzerTests.cs`. All use `Verify(source)` with `{|RHxxxx:…|}` markup, which asserts the exact diagnostic set and spans.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SingleLetterQueryRangeVariablesAreReported` (spec example 1 verbatim; holds the issue's shape `from x in values let y = x * 2 select y`, FC-034; all five clause kinds, FC-016) | RH4124-003 | `Verify` | Fails: expected 7, actual 0 |
| `QueryRangeVariablesWithLongerNamesAreNotReported` (spec example 2) | RH4124-003 | `Verify` | Passes |
| `UsesOfQueryRangeVariablesInLaterClausesAreNotReported` (uses in `where`, `orderby`, `let`, `select`; FC-002 "declares") | RH4124-003 | `Verify` | Fails: expected 2, actual 0 |
| `SingleLetterRangeVariableOfExplicitlyTypedFromClauseIsReported` | RH4124-003 | `Verify` | Fails: expected 1, actual 0 |
| `SingleLetterRangeVariableOfSecondFromClauseIsReported` | RH4124-003 | `Verify` | Fails: expected 1, actual 0 |
| `SingleLetterRangeVariableOfGroupContinuationIsReported` | RH4124-003 | `Verify` | Fails: expected 1, actual 0 |
| `DiscardRangeVariablesAreNotReported` (`_` in `from`, `let`, `join`, join `into`, continuation `into`; FC-003, FC-023) | SHARED-SINGLE-001, RH4124-003 | `Verify` | Passes |
| `SingleLetterRangeVariableIsOneCharacterOtherThanUnderscore` (`X`, `@b`, `é` reported; `ab` not; FC-003, FC-023) | SHARED-SINGLE-001, RH4124-003 | `Verify` | Fails: expected 3, actual 0 |
| `SingleLetterRangeVariablesAreReportedInEveryEnclosingScope` (field initializer, expression-bodied property, nested query, query in a lambda, query in a local function; FC-027) | RH4124-003 | `Verify` | Fails: expected 5, actual 0 |
| `SingleLetterExtensionBlockReceiversAreReported` (spec example 1 verbatim, incl. `this int s`) | RH4122-004 | `Verify` | Fails: expected 3, actual 1 (`s`) |
| `UnnamedAndLongerNamedExtensionBlockReceiversAreNotReported` (spec example 2; FC-002, FC-003) | RH4122-004 | `Verify` | Passes |
| `UnnamedReceiverOfGenericExtensionBlockIsNotReported` (`extension<T>(T[])`; the unnamed side of the generic form) | RH4122-004 | `Verify` | Passes |
| `ExtensionBlockReceiverIsReportedOncePerBlock` (block with two properties, an instance and a static method; pins "one diagnostic per extension block" against a symbol walk over synthesized implementation methods) | RH4122-004 | `Verify` | Fails: expected 3, actual 2 (`a`, `b`) |
| `SingleLetterReceiversWithModifiersAndAttributesAreReported` (`ref`, `in`, `[AllowNull]`, `int?`; FC-016) | RH4122-004 | `Verify` | Fails: expected 4, actual 0 |
| `SingleLetterReceiverIsOneCharacterOtherThanUnderscore` (`D`, `@o`, `é` reported; `value`, `_` not; FC-003, FC-023) | SHARED-SINGLE-001, RH4122-004 | `Verify` | Fails: expected 3, actual 0 |
| `ExtensionBlockReceiversInEachPartOfPartialClassAreReported` (FC-024) | RH4122-004 | `Verify` | Fails: expected 2, actual 0 |

Existing tests that assert a changed output: none. The RH4124 title/message change (answer 1B) is asserted only through the resource's `MessageFormat` in the RH4124 tests and through `AnalyzerPackageMetadataTests` (resource title, rule-page H1, README row), which the spec author ran green after the text change. `rg "anonymous method parameter names should not" tests/` finds nothing; `rg -l "RH4124|RH4122" tests/` finds only the two test classes. All 29 pre-existing tests of the two classes pass.

`scripts/test.sh --project architecture`: 198 passed — `EveryRequirementOfAnEnforcedSpecIsCovered` is green now that RH4122-004 and RH4124-003 carry tagged tests.

## Scenarios (bug fix)

Not a bug fix; the skeptic reproduced the premise for both rules (02-skeptic.md), and the positive tests above fail on exactly that gap.

## Not covered, and why

- Skeptic's `extension(string t)` with an extension indexer `this[int i]`: left out; the once-per-block property is pinned by `ExtensionBlockReceiverIsReportedOncePerBlock` with four members of three kinds, and member parameters belong to RH4122-001, which existing tests pin.
- Skeptic's `[NotNull] string? s`: does not compile cleanly in the test harness (CS8777, `s` must be non-null on exit); replaced by `[AllowNull] string s` plus a separate `int? n`, which keep the attribute and the nullable type.
- RH4123, RH4125, RH4126 staying silent on range variables and receivers: their analyzers are not changed by this run and their registrations (local-function parameters, local declarations, foreach) cannot reach a query clause or an extension receiver; the skeptic observed none reports them.
- Code-fix classes (FC-007, FC-008, FC-009, FC-018, FC-030, FC-036): neither rule has a code fix, and none is added.
- Formatter classes (FC-004, FC-005, FC-010 to FC-014, FC-019 to FC-022, FC-025, FC-026, FC-028, FC-029, FC-038 to FC-043): no formatter phase, token gap, or line join is involved; names are not judged by the formatter.
- Line endings: not a layout change; diagnostics on identifiers are independent of line endings.
- FC-015, FC-044: the single-letter decision stays in the shared `SingleLetterIdentifierUtilities`; the implementer's diff is where a private copy would show, which the auditor checks.
- FC-031: the analyzer `<summary>` widening is the implementer's (03-spec-author.md).

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH4122SingleLetterParameterNamesAnalyzerTests|FullyQualifiedName~RH4124SingleLetterLambda"
scripts/test.sh --project architecture
```

## Follow-up candidates

_None._
