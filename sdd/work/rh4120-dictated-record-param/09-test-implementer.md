Role: test implementer
Commit: 2bf27de
Verdict: tests written

Repair round 1, for the auditor's findings 1 and 2 (`08-auditor.md`) and the lead's decision 4. Expected values come from SHARED-CASING-014 (the record-parameter bullet: when the compiler declares no property because the record inherits a member of the parameter's name, the parameter MUST NOT be reported when *that member* is dictated; the member the compiler matches is the one it can access) and the maintainer's answer 2B. The prompt carried no intended fix, suspected cause, or hoped-for verdict. All tests are in `tests/Reihitsu.Analyzer.Test/Naming/RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests.cs`; `INamed` is a metadata reference (`CompiledAssemblyReference.CreateAsync`) in every test. Intermediate records use PascalCase parameters (`Other`) so that only the parameter under test can be reported.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ParameterMatchingAnInheritedDictatedPropertyBehindAnInaccessibleMemberOfAnIntermediateRecordIsNotReported` (`Person(string name) : INamed`; `MidField` with a private field `name`, `MidProperty` with a private property `name`; `BehindField(string name) : MidField(name)`, `BehindProperty(string name) : MidProperty(name)`; finding 1, source) | SHARED-CASING-014, RH4120-001 (FC-044, FC-001) | `Verify(source, onConfigure)` + reference | fails: 2 reported (`BehindField.name` at 19,34, `BehindProperty.name` at 21,37), 0 expected |
| `ParameterMatchingAnInheritedDictatedPropertyBehindAnInaccessibleMemberOfAnIntermediateRecordOfAnotherProjectIsNotReported` (project `Library` holds `Person : INamed`, `MidPrivate` with a private field `name`, `MidInternal` with an internal field `name`; test project: `BehindPrivate(string name) : MidPrivate(name)`, `BehindInternal(string name) : MidInternal(name)`; finding 1, another project) | SHARED-CASING-014, RH4120-001 (FC-044, FC-001) | same + `AdditionalProjects["Library"]` with the reference, project reference | fails: 2 reported (1,36 and 3,37), 0 expected |
| `ParameterMatchingADeclaredFieldThatHidesAnInheritedDictatedPropertyIsReported` (`WithField(string name) : Person(name) { public new string name = name; }`; finding 2, the `or IFieldSymbol` arm) | SHARED-CASING-014, RH4120-001 (FC-003, FC-017) | same, markup on `name` | passes (pins today's behavior; the field the record declares is matched and is not dictated) |

Both failing tests fail on the diagnostic set only: no compile error, no unrelated diagnostic. The failure lists exactly the parameters behind the inaccessible members, each member kind failing on its own line (private field, private property, private field in another project, internal field in another project). The internal case in another project, which the auditor traced but did not run, fails as traced.

## Scenarios (bug fix)

_N/A — behavior change, repair round._

## Not covered, and why

- An internal member of an intermediate record in the same project: the record can access it, so the compiler matches it (a non-dictated field or property) and the parameter is reported like `WithField`; it is the accessible side of the same condition, which the field test and `ParameterMatchingAnInheritedPropertyOfAReferencedAssemblyThatIsNotDictatedIsReported` already cover for the "matched member is not dictated" outcome. Not added.
- Protected or public intermediate members, private methods or events, members the record itself declares whatever their accessibility, generic source base, `new` property hiding a dictated one: the auditor ran these as holding; the declared-member side is pinned by `ParameterMatchingADeclaredDictatedPropertyIsNotReported` and the new field test, the accessible-inherited side by `ParameterMatchingAnInheritedDictatedPropertyIsNotReported`. No new tests.
- An inaccessible member in a metadata-reference intermediate record: the auditor ran it as not reported today (metadata import hides private members), so it pins nothing the source and project tests do not; not added.
- LF/CRLF, formatter, code fix, Fix All: no layout or fix path changes; the repair only changes which member decides the diagnostic.
- Remaining failure classes: as in `04-test-implementer.md`, unchanged by this round.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests"
```

Current: 2 failed (the two listed above) / 39 passed of 41.

## Follow-up candidates

_None._
