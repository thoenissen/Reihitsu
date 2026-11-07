Role: test implementer
Commit: 03cdad7 (base); this handover's commit carries the change
Verdict: tests written

Scope: `00-lead.md` decision 11 only. The prompt asked for a harness correction and carried no intended fix or verdict beyond it.

## Dispute verified

On HEAD `a9fbe8c` (production code of `bd592fd`), the three tests fail with "Mismatch between number of diagnostics returned, expected "0" actual "1"" (RH2005 test, RH2001 type-parameter test) and "actual "2"" (RH2001 Fix All test), raised from `VerifyDiagnosticsAsync` on the fixed state. `Microsoft.CodeAnalysis.Testing` drops `{|RH2005:…|}`/`{|RH2001:…|}` markup of a fixable diagnostic ID in the fixed state unless `MarkupHandling` is `MarkupMode.Allow`, so the expected remaining diagnostics were never registered. The implementer's reading is correct: the code outputs already matched; only the harness setting was missing.

## Change

Each of the three tests now passes the repository's existing setup (as in `RH8204DoNotUsePlaceholderElementsAnalyzerTests.VerifyFixAllLeavesPlaceholderWithoutEndTagUnchanged`) through the `onConfigure` overload of `Verify`: `test.FixedState.MarkupHandling = MarkupMode.Allow;` and `test.BatchFixedState.MarkupHandling = MarkupMode.Allow;`, with the same comment. `RH2005FieldsMustBePrivateAnalyzerTests.cs` gains `using Microsoft.CodeAnalysis.Testing;`. No source, fixed source, or expected diagnostic changed.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH2005FieldsMustBePrivateAnalyzerTests.VerifyFixAllSkipsTheFieldReadOutsideItsType` | unchanged tags | `Verify(source, fixedSource, onConfigure, expected)` | pass |
| `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.VerifyFixIsOfferedOnlyForTheTypeParameterConstrainedToAReferenceType` | unchanged tags | `Verify(source, fixedSource, onConfigure)` | pass |
| `RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.VerifyFixAllSkipsThePropertiesWhoseFixIsWithheld` | unchanged tags | `Verify(source, fixedSource, onConfigure)` | pass |

## Not covered, and why

Nothing new: the change is a harness setting on existing tests; asserted outputs are unchanged.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2005FieldsMustBePrivateAnalyzerTests|FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"
```

Result: 80 passed, 0 failed.
