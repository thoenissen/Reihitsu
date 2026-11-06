Role: test implementer
Commit: 0d7ddc2 (production head; tests corrected on top of 25bfd92)
Verdict: tests written

Correction round for lead decisions 11 and 12 (`00-lead.md`), on the two tests `04-implementer.md` disputed. The prompt carried no intended fix or verdict beyond the decisions recorded in `00-lead.md`.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4103MethodNameCasingAnalyzerTests.FixIsWithheldForANativeEntryPointTakenFromTheMethodName` | SHARED-FIX-003 | `Verify` with `FixedState`/`BatchFixedState.MarkupHandling = MarkupMode.Allow` | passes on 0d7ddc2 |
| `RH4005InterfaceNameCasingAnalyzerTests.FixRenamesANamespaceInterfaceToTheNameOfOneOfItsMethods` (was `NoFixIsOfferedWhenANamespaceInterfaceWouldHaveAMethodOfItsName`) | SHARED-CASING-009, SHARED-FIX-003 | `Verify` | passes on 0d7ddc2 |

- RH4103 (decision 11): configuration only. The expected source is unchanged: `{|RH4103:getpid|}` stays marked, `getppid` becomes `Getppid`. The default `MarkupHandling` (`IgnoreFixable`) dropped the remaining marker and expected 0 diagnostics in the fixed state; `MarkupMode.Allow` for the fixed and batch-fixed states makes the harness assert the withheld diagnostic, as `RH4107ProtectedFieldCasingAnalyzerTests` and `RH8204DoNotUsePlaceholderElementsAnalyzerTests` do. A rename of `getpid` would still fail the fixed-source comparison.
- RH4005 (decision 12): the input `namespace N { interface iOrder { void IOrder(); } }` now expects the fix offered, writing `interface IOrder { void IOrder(); }` — today's output on `main`. It is the other side of the compile-error condition: CS0542 does not apply to interfaces, so the rename adds no compile error and neither requirement withholds it. Both tags stay because the test checks that condition's negative side.

## Scenarios (bug fix)
_None new_ — this round corrects two tests; the reproductions of `03-test-implementer.md` stand.

## Not covered, and why
Nothing added beyond the two corrections the lead asked for.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"
# Passed: 517, Failed: 0
```
