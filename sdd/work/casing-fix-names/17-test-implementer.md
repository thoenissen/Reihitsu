Role: test implementer
Commit: a2f386b
Verdict: tests written

Structural repair for auditor `16-auditor.md` finding 3 (#1434), under lead decision 4. SHARED-CASING-014 bullet 2 and RH4116-001 are unchanged, and every expected value comes from them. A positional parameter's tuple element names are dictated when the property the compiler declares for it implements or overrides, directly or through other levels, a member declared outside the compilation. They are reported otherwise. A type argument written in this compilation is reported where it is written, and only once. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests
All tests are in `tests/Reihitsu.Analyzer.Test/Naming/RH4116TupleElementCasingAnalyzerTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `TupleElementNamesOfARecordPositionalParameterWhosePropertyImplementsAReferencedAssemblyPropertyAreNotReported` (record class, implicit implementation of `INamed.Pair`; auditor's (a)) | SHARED-CASING-014, RH4116-001 | analyzer `Verify` + `CompiledAssemblyReference` | fail: 2 diagnostics, expected 0 |
| `TupleElementNamesOfARecordStructPositionalParameterWhosePropertyImplementsAReferencedAssemblyPropertyAreNotReported` (`record struct` and `readonly record struct`; auditor's (b)) | SHARED-CASING-014, RH4116-001 | same | fail: 4, expected 0 |
| `TupleElementNamesOfARecordPositionalParameterWhosePropertyOverridesAReferencedAssemblyPropertyAreNotReported` (the synthesized property overrides the abstract property of a referenced record directly, which is auditor's (c), and through a source record's `abstract override`, which is the "any number of levels" axis) | SHARED-CASING-014, RH4116-001 | same | fail: 4 (`Direct` and `Leaf`), expected 0. `Middle`'s explicit property is already not reported |
| `TupleElementNamesNestedInTheTypeOfADictatedRecordPositionalParameterAreNotReported` (the tuple as a type argument, as the element type of an array, and inside another tuple) | SHARED-CASING-014, RH4116-001 | same | fail: 8, expected 0 |
| `TupleElementNamesSuppliedByATypeArgumentAreReportedWhereWrittenAndNotInARecordPositionalParameter` (`record H((int first, int second) Value) : IHolder<(int first, int second)>`; reported in the base list only) | SHARED-CASING-014, RH4116-001 | same | fail: 4, expected 2 (base list only) |
| `TupleElementNamesOfARecordPositionalParameterThatImplementsNothingAreReportedNextToADictatedOne` (record and record struct, each with one dictated and one non-implementing positional parameter, in both orders) | SHARED-CASING-014, RH4116-001 | same | fail: 8, expected 4. The 4 extra are the dictated `Pair` names, and the non-implementing `Range` names are reported as expected |
| `TupleElementNamesOfAPositionalParameterThatIsNotDictatedAreReported` (the other side, FC-003: implements a source interface as a record and as a record struct; overrides a source abstract record; implements nothing as a record and as a record struct; a class primary-constructor parameter, for which no property is declared) | SHARED-CASING-014, RH4116-001 | analyzer `Verify` | pass |

Each failing test fails only on extra RH4116 diagnostics, located on the dictated positional parameter's element names. The verifier reports no CS diagnostic, so every input compiles. Whole class: 6 failed, 18 passed. All 17 existing tests still pass.

## Scenarios (bug fix)
_N/A: this is a structural repair for an audit finding. The failing tests above reproduce it._

## Not covered, and why
- **Another project of the solution instead of a referenced assembly.** Whether a member is dictated is decided by `CasingAnalyzerBase.IsDictatedMember` for every member kind (`rg -n "IsDictatedMember" src`, as the auditor's policy-ownership table shows). The record case changes only which symbol RH4116 asks about, not how that symbol is judged. The existing RH4116 tests use only `CompiledAssemblyReference` for the same reason.
- **A record that declares the positional member explicitly** (`record R((int first, int second) Pair) : INamed { public (int first, int second) Pair { get; } = Pair; }`). The compiler then declares no property for the parameter. The explicit property's type is already covered by the property branch. Whether the parameter's own tuple names must repeat the property's is a compiler rule (CS8866 type matching) that I did not probe. The decision covers the generated property only. No test is written, and it is not a finding against the spec.
- **A derived record whose positional parameter matches a non-abstract inherited property.** No property is declared for the parameter, so the same reasoning applies as for the explicit-member case above.
- **Explicit implementation.** A synthesized positional property cannot be an explicit interface implementation.
- **FC-004/005/019–021/026, line endings, layout, Fix All, rename.** RH4116 has no code fix, and the change only affects reporting.
- **Other failure classes.** These are as dismissed in `16-auditor.md`. FC-016/FC-024 (missed sibling kind) and FC-003 (both sides) are the classes these tests cover.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4116TupleElementCasingAnalyzerTests"
```

## Follow-up candidates
_None_ beyond the RH4120 candidate already in `00-lead.md`, which has the same cause on the parameter's name.
