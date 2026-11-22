Role: test implementer
Commit: 0a03a29
Verdict: tests written

Repair round 2 (test-only), for the auditor's finding 3 (`13-auditor.md`) and the lead's decision 6. The class is the accessible, non-public side of the guard in `RecordPositionalParameterUtilities.GetMember`. The tests cover every member the auditor probed: a protected member, an internal member in the same project, a private member of an enclosing record that a nested record can access, and the record's own private field. Expected values come from SHARED-CASING-014, the record-parameter bullet. When the record declares or inherits a member of the parameter's name, the compiler matches the parameter to that member. The parameter is reported when that member is not dictated. The member matched is the first one in the walk that the record can access. Each new test pins unchanged output: the parameter is reported. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

All tests are in `tests/Reihitsu.Analyzer.Test/Naming/RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests.cs`. In every test, `INamed` (`string name { get; }`) is a metadata reference, and `Person(string name) : INamed` is the dictated member behind the member under test. Intermediate records use a PascalCase parameter (`Other`), so only the parameter under test can be reported. I wrote one small test per member, not the single combined test the auditor suggested, so that a failure names the member kind.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ParameterMatchingAProtectedFieldOfAnIntermediateRecordInFrontOfAnInheritedDictatedPropertyIsReported` (`MidProtected { protected new string name = ""; }`, `BehindProtected(string name) : MidProtected(name)`) | SHARED-CASING-014, RH4120-001 (FC-003, FC-017, FC-044) | `Verify(source, onConfigure)` + reference, markup on `name` | passes |
| `ParameterMatchingAnInternalFieldOfAnIntermediateRecordOfTheSameProjectInFrontOfAnInheritedDictatedPropertyIsReported` (`MidInternal { internal new string name = ""; }` in the same project) | SHARED-CASING-014, RH4120-001 (FC-003, FC-017, FC-044) | same | passes |
| `ParameterOfANestedRecordMatchingAPrivateFieldOfItsEnclosingBaseRecordInFrontOfAnInheritedDictatedPropertyIsReported` (`Outer { private new string name = ""; public record Nested(string name) : Outer(name); }`) | SHARED-CASING-014, RH4120-001 (FC-003, FC-017, FC-044) | same | passes |
| `ParameterMatchingADeclaredPrivateFieldThatHidesAnInheritedDictatedPropertyIsReported` (`WithPrivateField(string name) : Person(name) { private new string name = name; }`) | SHARED-CASING-014, RH4120-001 (FC-003, FC-017) | same | passes |

There are no compile errors and no unrelated diagnostics. Private fields get a `Peek…()` reader, so CS0414 does not fire.

A mutation check confirmed that the tests catch what the finding describes. The production file was edited temporarily, and `git checkout` restored it. The run was the RH4120 test class, 45 tests.

| Mutant of the condition `compilation.IsSymbolAccessibleWithin(candidate, recordType)` | New tests failing (missing diagnostic) | Others |
|---|---|---|
| `candidate.DeclaredAccessibility == Accessibility.Public \|\| SymbolEqualityComparer.Default.Equals(candidate.ContainingType, recordType)` (the auditor's public-only hand model) | protected, internal same project, nested private | 42 pass |
| `compilation.IsSymbolAccessibleWithin(candidate, recordType.ContainingAssembly)` (assembly-scoped `within`) | protected, nested private, own private | 42 pass |

Every new test fails under at least one mutant. On the head, all 45 pass.

## Scenarios (bug fix)

_N/A. This is a behavior change, repair round._

## Not covered, and why

- A protected internal or private protected intermediate member: the compiler resolves both accessibility kinds through the same `IsSymbolAccessibleWithin` call. The protected and internal tests already decide each of the two parts, and neither mutant above separates them further. Not added.
- An accessible non-public member in another project (protected in a project reference): the inaccessible side across projects is pinned by the round-1 test. The protected side reads the same compiler result as the source case, because `rg -n "IsSymbolAccessibleWithin" src` finds one call site, `RecordPositionalParameterUtilities.GetMember`. Not added.
- A protected property instead of a field: the property and field arms meet in the same predicate before the accessibility check. The field arm is pinned by the round-1 field test, and the property arm by the round-1 private-property test. Not added.
- LF/CRLF, formatter, code fix, Fix All: no layout or fix path is touched.
- Remaining failure classes: as in `04-test-implementer.md`. This round does not change them.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests"
```

Current: 45 passed of 45.

## Follow-up candidates

_None._
