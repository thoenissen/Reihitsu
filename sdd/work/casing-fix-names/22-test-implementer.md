Role: test implementer
Commit: a7305e2
Verdict: tests written

Lead decisions 6 and 8 cover auditor `21-auditor.md` finding 5 (structural repair 2) and finding 7 (repair round 3) for #1434. SHARED-CASING-014 bullet 2 and RH4116-001 are unchanged, and every expected value comes from them. A positional parameter's tuple element names are dictated when the `Deconstruct` the compiler declares for the record implements a member declared outside the compilation. They stay reported when that `Deconstruct` implements a source member or nothing, and when a referenced-assembly member is implemented only by `Equals(R)` through `IEquatable<R>`. An indexer's type and parameter types, and an event's type with accessors, are dictated under the same terms. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests
All tests are in `tests/Reihitsu.Analyzer.Test/Naming/RH4116TupleElementCasingAnalyzerTests.cs`. Each is tagged `[Requirement("SHARED-CASING-014")]` and `[Requirement("RH4116-001")]`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `TupleElementNamesOfARecordPositionalParameterWhoseDeconstructImplementsAReferencedAssemblyDeconstructAreNotReported` (record class; auditor's `IDecon` probe) | SHARED-CASING-014, RH4116-001 | analyzer `Verify` + `CompiledAssemblyReference` | fail: 2 diagnostics, expected 0 |
| `TupleElementNamesOfARecordStructPositionalParameterWhoseDeconstructImplementsAReferencedAssemblyDeconstructAreNotReported` (`record struct` and `readonly record struct`, two-parameter `Deconstruct` with a non-tuple first parameter) | SHARED-CASING-014, RH4116-001 | same | fail: 4, expected 0 |
| `TupleElementNamesNestedInTheTypeOfARecordPositionalParameterWhoseDeconstructIsDictatedAreNotReported` (type argument, array element type, tuple inside a tuple; record, record struct, readonly record struct) | SHARED-CASING-014, RH4116-001 | same | fail: 24, expected 0 |
| `TupleElementNamesOfARecordPositionalParameterWhoseDeconstructImplementsASourceDeconstructAreReported` (other side, FC-003: the generated `Deconstruct` implements a source interface; record, record struct, readonly record struct) | SHARED-CASING-014, RH4116-001 | analyzer `Verify` | pass |
| `TupleElementNamesOfARecordPositionalParameterAreReportedWhenTheRecordDeclaresItsOwnDeconstruct` (the declared `Deconstruct` implements the referenced one, so its out parameter is not reported and the positional parameter is) | SHARED-CASING-014, RH4116-001 | `Verify` + reference | pass |
| `TupleElementNamesOfARecordPositionalParameterAreReportedAlthoughTheGeneratedEqualsImplementsIEquatable` (`: IEquatable<R>` written explicitly on record, record struct, and readonly record struct; guards against an "any generated member" filter) | SHARED-CASING-014, RH4116-001 | analyzer `Verify` | pass |
| `TupleElementNamesInTheSignatureOfAnIndexerThatImplementsAReferencedAssemblyIndexerAreNotReported` (implicit and explicit; type, tuple parameter, tuple nested in a `List<>` parameter) | SHARED-CASING-014, RH4116-001 | `Verify` + reference | pass |
| `TupleElementNamesInTheSignatureOfAnIndexerThatIsNotDictatedAreReported` (in one class next to the dictated indexer: an indexer implementing a source interface indexer and one implementing nothing; the source interface is reported too) | SHARED-CASING-014, RH4116-001 | `Verify` + reference | pass |
| `TupleElementNamesOfAnEventWithAccessorsThatImplementsAReferencedAssemblyEventAreNotReported` (implicit and explicit, `add`/`remove`) | SHARED-CASING-014, RH4116-001 | `Verify` + reference | pass |
| `TupleElementNamesOfAnEventWithAccessorsThatIsNotDictatedAreReported` (in one class next to the dictated event: one implementing a source interface event and one implementing nothing) | SHARED-CASING-014, RH4116-001 | `Verify` + reference | pass |

The three failing tests fail only on extra RH4116 diagnostics. The run output names RH4116 39 times and no CS diagnostic, so every input compiles. The counts match the element names on the dictated positional parameters exactly. Whole class: 3 failed, 31 passed. All 24 earlier tests still pass. Finding 7's tests pass on the current code, as the auditor's probes predicted, and they pin both sides of the new path.

## Scenarios (bug fix)
_N/A: this is a structural repair and coverage repair for audit findings. The failing tests above reproduce finding 5._

## Not covered, and why
- **The "nothing implemented" side of `Deconstruct`.** Every record has a generated `Deconstruct` that implements nothing. `TupleElementNamesOfAPositionalParameterThatIsNotDictatedAreReported` (`Plain`, `PlainStruct`) and the new `IEquatable` test already pin this.
- **A class or struct primary constructor.** The compiler declares no `Deconstruct` for one. `TupleElementNamesOfAPositionalParameterThatIsNotDictatedAreReported` (`PrimaryConstructor`) already pins it.
- **A `Deconstruct` dictated through a base record or a derived record.** The generated `Deconstruct` is not virtual, so it can only implement an interface member. The implicit-implementation tests cover that.
- **Other synthesized members** (`Equals`, `GetHashCode`, `PrintMembers`, `<Clone>$`, copy constructor, `EqualityContract`). Per `21-auditor.md`, none of them carries a positional parameter's type. Only `Equals(R)` implements a referenced member, and the `IEquatable` test covers it.
- **Another project of the solution instead of a referenced assembly.** `CasingAnalyzerBase.IsDictatedMember` judges every symbol, and the repair changes only which symbols are asked. This is the same reasoning as in `17-test-implementer.md`.
- **Finding 6 (shared mapping).** It changes internal structure, not output. The record-property tests from `17` and the `Deconstruct` tests above pin RH4116's side. For the rename-safety side, `CasingRenameSafetyUtilities`' existing code-fix tests stay authoritative, and I wrote no new test.
- **FC-004/005/019–021/026, line endings, Fix All, rename.** RH4116 has no code fix, and the change only affects reporting.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4116TupleElementCasingAnalyzerTests"
```

## Follow-up candidates
_None._ The RH4120 candidate already in `00-lead.md` is the only one.
