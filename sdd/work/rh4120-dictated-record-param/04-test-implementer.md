Role: test implementer
Commit: 3c509ca
Verdict: tests written

Behavior change; tests derived from SHARED-CASING-014 (record-parameter bullet and boundary sentence, as of 8d09179), RH4120-001, the skeptic's case table (02), and the maintainer's answers 1A/2B in `00-lead.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. All tests are in `tests/Reihitsu.Analyzer.Test/Naming/RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests.cs`. A referenced assembly is built with `CompiledAssemblyReference.CreateAsync` (metadata reference), another project with `TestState.AdditionalProjects`, as in the RH4116 and RH4103 tests.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ParameterWhosePropertyImplementsAReferencedAssemblyInterfacePropertyIsNotReported` (`Person : INamed`, skeptic 1) | SHARED-CASING-014, RH4120-001 | `Verify(source, onConfigure)` + reference | fails: 1 RH4120 reported, 0 expected |
| `ParameterOfEveryRecordKindWhosePropertyImplementsAReferencedAssemblyInterfacePropertyIsNotReported` (`record class`, `record struct`, `readonly record struct`, skeptic 2) | SHARED-CASING-014, RH4120-001 | same | fails: 3 reported, 0 expected |
| `ParameterWhosePropertyOverridesAReferencedAssemblyAbstractPropertyIsNotReported` (`Item : MetaRecord`, answer 1A, skeptic 3) | SHARED-CASING-014, RH4120-001 | same | fails: 1 reported |
| `ParameterWhosePropertyOverridesAReferencedAssemblyPropertyThroughASourceRecordIsNotReported` (`Leaf : Middle : MetaRecord`, skeptic 4) | SHARED-CASING-014, RH4120-001 | same | fails: 1 reported |
| `ParameterWhosePropertyImplementsAReferencedAssemblyPropertyThroughASourceInterfaceIsNotReported` (`ILocal : INamed`, skeptic 5) | SHARED-CASING-014, RH4120-001 | same | fails: 1 reported |
| `ParameterWhosePropertyImplementsAReferencedAssemblyGenericInterfacePropertyIsNotReported` (`IGen<string>`, skeptic 6) | SHARED-CASING-014, RH4120-001 | same | fails: 1 reported |
| `ParameterWhosePropertyImplementsAReferencedAssemblyDefaultInterfacePropertyIsNotReported` (`INamed.name => "default"`, skeptic premise note) | SHARED-CASING-014, RH4120-001 | same | fails: 1 reported |
| `ParameterOfAPartialRecordWhoseOtherPartImplementsAReferencedAssemblyInterfaceIsNotReported` (two documents, FC-024, skeptic 7) | SHARED-CASING-014, RH4120-001 | same + second source | fails: 1 reported |
| `ParameterWhosePropertyImplementsOrOverridesAPropertyOfAnotherProjectIsNotReported` (`INamed`, `MetaRecord` in project `Contracts`, skeptic 8) | SHARED-CASING-014, RH4120-001 | `Verify(source, onConfigure)` + `AdditionalProjects` | fails: 2 reported |
| `OtherParametersOfARecordWithADictatedParameterAreReportedAndFixed` (`Person(string name, string other, int age) : INamed`, skeptic 9; fix and Fix All rename `other`/`age` only) | SHARED-CASING-014, RH4120-001, RH4120-003 | `Verify(source, fixedSource, onConfigure)` | fails: 3 reported, 2 expected |
| `ParameterMatchingAnInheritedDictatedPropertyIsNotReported` (`Derived : Person(name)`, `Leaf : Derived(name)`, answer 2B, skeptic 13) | SHARED-CASING-014, RH4120-001 | `Verify(source, onConfigure)` + reference | fails: 3 reported |
| `ParameterMatchingADeclaredDictatedPropertyIsNotReported` (`Annotated`, record struct `AnnotatedPoint`, answer 2B, skeptic 14) | SHARED-CASING-014, RH4120-001 | same | fails: 2 reported |
| `ParameterWhosePropertyImplementsASourceInterfacePropertyIsReported` (`Local : ILocal`, skeptic 10) | SHARED-CASING-014, RH4120-001 | `Verify(source)` | passes |
| `ParameterWhosePropertyImplementsAReferencedAssemblyPropertyOnlyForADerivedRecordIsReported` (`Holder`, `Sub`, skeptic 11) | SHARED-CASING-014, RH4120-001 | `Verify(source, onConfigure)` + reference | passes |
| `ParameterOfARecordThatImplementsTheReferencedAssemblyPropertyExplicitlyIsReported` (`Explicit`, skeptic 12, FC-024) | SHARED-CASING-014, RH4120-001 | same | passes |
| `ParameterMatchingAnInheritedPropertyOfAReferencedAssemblyThatIsNotDictatedIsReported` (`Inherited : MetaPerson(name)`, `Virtual : MetaVirtual` with a non-abstract virtual `title`, `NamedInMeta : MetaNamed(name)` where `MetaNamed.name` implements `INamed.name` inside the referenced assembly; option 2C not chosen, skeptic 15) | SHARED-CASING-014, RH4120-001 | same | passes |
| `NoFixIsOfferedForEitherOfTwoMatchingPositionalParametersOfABaseAndADerivedRecord` (existing; tag added, assertion unchanged — pins RH4120-001's new `B`/`D` example) | RH4120-001, RH4120-003, SHARED-FIX-003 | `Verify(source, fixedSource)` | passes |
| `NoFixIsOfferedWhenTheRenameWouldSplitTheParameterFromItsDeclaredProperty` (existing; tag added, assertion unchanged — pins RH4120-001's new `P` example) | RH4120-001, SHARED-CASING-007, SHARED-FIX-003 | same | passes |

Every failing test fails on the diagnostic count only (the exempted parameter is reported today); none fails to compile or on an unrelated diagnostic. In `OtherParametersOfARecordWithADictatedParameterAreReportedAndFixed` the extra diagnostic is `name` at (3,29). A throwaway probe (not committed) confirmed that the code-fix verifier's fixed state inherits the `AdditionalReferences` of the test state, so the fixed code of that test compiles against the referenced assembly.

Note for the implementer and checker: `NamedInMeta` follows the definition in SHARED-CASING-014 literally — `MetaNamed.name` implements `INamed.name`, but both are declared in the same (referenced) compilation, so `MetaNamed.name` is not dictated and `NamedInMeta.name` stays reported. Asking `IsDictatedMember` about the matching member, rather than checking only where it is declared, gives that result.

## Scenarios (bug fix)

_N/A — behavior change._

## Not covered, and why

- Existing tests whose output the change doc changes: `rg RH4120 tests` finds only this test file; no existing test has a positional parameter whose member is dictated (none uses a referenced assembly or another project), so no existing assertion changes. `NoFixIsOfferedWhenTheParametersPropertyWouldImplementAnInterfaceProperty` (source interface, SHARED-CASING-009) stays as it is: its interface is in source.
- RH4120-001 `B`/`D`/`P` example: pinned by the two existing tests above (tag added), no new test.
- `_`, PascalCase parameters, other constructors (skeptic 16): pinned by `DiscardsAndParametersOfOtherConstructorsAndMembersAreNotChecked` and `VerifyNoDiagnosticsForPascalCaseRecordPrimaryConstructorParameter`.
- LF/CRLF, formatter parity, idempotency: no layout, no formatter path; the change only removes diagnostics.
- FC-018 (references through the Renamer), FC-007/FC-008 (convergence, Fix All): the fix is unchanged; `OtherParametersOfARecordWithADictatedParameterAreReportedAndFixed` runs the fix and Fix All over two reported parameters next to an exempted one, which also renames references in the record body; the batch scenario `GetFixAllScenario` stays.
- FC-023 (`@name`, Unicode): the decision is semantic (the symbol), not textual; no separate path. Skeptic found no separate behavior.
- FC-036: the run removes diagnostics and changes no rename.
- FC-004, FC-005, FC-019–FC-022, FC-025–FC-029, FC-038–FC-043, FC-010–FC-014: no trivia, layout, formatter, or precheck change.
- FC-015/FC-016/FC-044 are implementation concerns (one owner for "dictated" and for parameter-to-member mapping, read compiler results); the tests above exercise the compiler-computed relations (implicit, override, chain, generic, interface inheritance, explicit implementation, partial, default interface member) so a modelled guard would fail one of them.
- An explicit implementation in the body together with a body property of the parameter's name, and a matching member that is a field: no candidate in the handovers, and a field cannot implement a property; not added.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests"
scripts/test.sh --project architecture
```

Current: RH4120 class 12 failed / 26 passed (the 12 listed as failing above); architecture 198 passed.

## Follow-up candidates

_None._
