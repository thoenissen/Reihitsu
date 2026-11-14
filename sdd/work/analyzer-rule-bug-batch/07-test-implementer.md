Role: test implementer
Commit: 35a3c0a
Verdict: tests written

Scope: thoenissen/Reihitsu#1708 only, per `00-lead.md` decision 12. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the decision recorded in `00-lead.md`.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Naming/RH4105MethodParameterCasingAnalyzerTests.VerifyUnnamedReferenceReceiverParameterIsNotReported` (`extension(ref int)`) | — | — | removed: the input does not compile (CS9305, modifiers on an unnamed receiver), so no caller analyzes it |
| `…RH4105….VerifyUnnamedReceiverParametersOfExtensionBlocksAreNotReported` | RH4105-004 | `Verify(source)` | passes (unchanged) |
| `…RH4105….VerifyUnnamedReceiverParameterOfAnExtensionBlockWithAStaticMemberIsNotReported` | RH4105-004 | `Verify(source)` | passes (unchanged) |
| `…RH4105….VerifyNamedReceiverParameterIsReportedNextToAnUnnamedOne` | RH4105-001, RH4105-004 | `Verify(source, fixed, …)` | passes (unchanged) |

## Scenarios (bug fix)

No new scenario. The modifier arm of the unnamed receiver is not reachable by compiling code; `03-test-implementer.md` listed `ref` as the representative modifier arm, and that arm no longer exists as a valid input.

## Not covered, and why

- Other modifiers on an unnamed receiver (`ref readonly`, `scoped`, `in`): the same CS9305 applies, so none compiles.
- Spec check: the RH4105-004 example (`sdd/specs/analyzer/RH4105.md`) names `extension(long)`, `extension<T>(T[])`, and `extension(int)` only; no `ref` receiver. Nothing to report against the spec.

## Commands

```shell
scripts/format.sh src/Reihitsu.Analyzer.Test/Naming/RH4105MethodParameterCasingAnalyzerTests.cs   # Formatted 0 of 1
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4105MethodParameterCasingAnalyzerTests"   # Passed: 51, Failed: 0
```
