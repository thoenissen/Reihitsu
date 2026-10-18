Role: test implementer
Commit: b8dba74
Verdict: tests written

Repair round after `07-spec-author.md`. RH4122-003 in `sdd/specs/analyzer/RH4122.md` now lists `readonly record struct` among the record forms whose primary constructor parameters are not checked. Production code is unchanged, and the new test passes against it. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

How it was established:
- **No existing coverage.** `rg "readonly record struct" src/Reihitsu.Analyzer.Test/Naming/RH4122*.cs` finds nothing, so no existing RH4122 test covers the form.
- **New test.** It sits next to `RecordLocalFunctionLambdaAndAnonymousMethodParametersAreNotChecked` and checks both sides of the boundary:
  - the parameters of two `readonly record struct` declarations are not reported: `X`, `y`, `a`, `b`;
  - the parameter of a `readonly struct` primary constructor is reported: `w`.
- **Check that the test can fail.** I turned `readonly record struct Pair(int a, int b)` into `readonly struct Pair(int a, int b)`. The test then failed with "expected 1 actual 3" diagnostics. I restored the file before the runs below.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4122SingleLetterParameterNamesAnalyzerTests.ReadonlyRecordStructPrimaryConstructorParametersAreNotChecked` | RH4122-003; FC-003, both sides of the record / non-record boundary | `Verify(source)` | pass |

## Scenarios (bug fix)
_N/A._ No bug fix in this lane.

## Not covered, and why
- **RH4121-004.** It changed in wording only, and its example is unchanged. Its existing tagged test `RH4121TypeParameterNameCasingAnalyzerTests.FixIsWithheldWhenNoUsableNameCanBeDerived` still covers it.
- **SHARED-CASING-008.** It is newly listed under RH4121's `Uses` and is already covered by the tests of the other casing rules. `FixIsWithheldWhenNoUsableNameCanBeDerived` checks the `_` case through RH4121 as well; it is not tagged SHARED-CASING-008, because it exists for RH4121-004.
- **No open question pinned.** None of the filed questions is asserted.

## Commands
```shell
scripts/test.sh --project analyzer --configuration Debug --filter "FullyQualifiedName~Naming.RH4122"   # 15 passed
scripts/test.sh --project architecture --configuration Debug                                            # 72 passed
```
`scripts/format.sh` on the changed test file: 0 of 1 reformatted.
