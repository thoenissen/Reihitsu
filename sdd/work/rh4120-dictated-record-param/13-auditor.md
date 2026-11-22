Role: auditor
Commit: 7c131eba4d5ce6011b2838d4a3b59308d532ad42
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `3b066662dc49e2abcd3964664c6937adf78bed0c` (audit 1, `08-auditor.md`). Head `7c131eba4d5ce6011b2838d4a3b59308d532ad42` is checked out, and the tree is clean. Base `origin/main` is `1788f94d265c4e13052d5145404ae3ffd21e6acc`, which matches the prompt and is an ancestor of the head. The repair diff matches the handovers 09–12. `GetMember` takes the `Compilation` and skips a property or field candidate for which `IsSymbolAccessibleWithin(candidate, recordType)` is false. RH4120 passes `semanticModel.Compilation`. There are three new tests and the work files. Nothing else changed, and the spec and the change doc are untouched, so scope did not grow. `scripts/lane-check.sh behavior-change`: confirmed.

One probe ran in a throwaway worktree under the session scratchpad, which has since been removed. It was an extra MSTest method in `Reihitsu.Analyzer.Test`, run with `scripts/test.sh --project analyzer --filter`, first against the head and then against a mutant of the guard (see finding 3).

Previous findings:
- **#1: closed.** Every member it listed is now tested and holds: a private field and a private property of an intermediate source record, and a private field and an internal field of an intermediate record of another project.
- **#2: closed.** `ParameterMatchingADeclaredFieldThatHidesAnInheritedDictatedPropertyIsReported` pins the `or IFieldSymbol` arm. Without that arm, `WithField`'s only member named `name` is skipped, the walk reaches the dictated `Person.name`, and the expected diagnostic disappears.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | medium | `tests/Reihitsu.Analyzer.Test/Naming/RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests.cs` (guard in `RecordPositionalParameterUtilities.GetMember`) | FC-003, FC-017 | #2 (the same filter in `GetMember`, which decides the member the parameter stands for; tested on one side only) | PR-introduced | The repaired condition is tested only on its inaccessible side (private, or internal in another project) and on public members. No test has a member that the record can access but that is not public. An inherited member of that kind must stop the walk at a non-dictated member in front of a dictated one. The guarantee is that the code skips exactly what the compiler skips, and today's tests do not hold it. Mutant: `candidate.DeclaredAccessibility == Accessibility.Public \|\| SymbolEqualityComparer.Default.Equals(candidate.ContainingType, recordType)`, which is the hand model that FC-044 warns against. With this mutant, all 41 committed RH4120 tests pass. A probe fails on it and passes on the head. In the probe, `INamed` comes from a metadata reference and `Person(string name) : INamed`, and four inputs are each reported by the compiler's match and by the head: `MidProtected(string Other) : Person(Other) { protected new string name = ""; }` with `BehindProtected(string name) : MidProtected(name)`; the same with `internal new` in the same project; `Outer(string Other) : Person(Other) { private new string name = ""; public record Nested(string name) : Outer(name); }` (a private member that the nested record can access); and a record that declares `private new string name = name;` itself. There were no compile errors. **Members that hold** (committed tests): public inherited (`ParameterMatchingAnInheritedDictatedPropertyIsNotReported`), public declared (the field test, `ParameterMatchingADeclaredDictatedPropertyIsNotReported`), and the inaccessible ones (the two new tests). | Add one test, tagged SHARED-CASING-014 and RH4120-001, that reports the parameter behind a protected member of an intermediate source record. Add the nested-record case (a private member that the record can access) to the same test, so that both a public-only model and an assembly-scoped `within` fail. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which member a positional parameter stands for (`GetMember`) | Properties and fields of the parameter's `ValueText` name, in the record and then in each base type. The first one with `IsSymbolAccessibleWithin(candidate, recordType)` is taken | The compiler's positional match. A member the record declares is taken whatever its accessibility; otherwise the first member found while walking the base types that the record can access (the hidden-member lookup with its accessibility check) | Agrees on every input traced or run. Declared members of any accessibility: `IsSymbolAccessibleWithin` of the record's own member within the record is true. Protected, same-project internal, and private members of an enclosing record (for a nested record): accessible, so taken (probe). Private members, and internal members of another assembly without `InternalsVisibleTo`: skipped (tests). The project-reference and metadata-reference paths now agree, because both read the same compiler accessibility result. One theoretical difference: the compiler checks `OriginalDefinition`s, and the analyzer checks the constructed member of `Base<T>`, so the type arguments' accessibility enters. A type argument of a base type that the record cannot access is a CS0060 or CS0122 error in the chain, so no caller reaches it | ok; finding 3 is about the tests, not the guard |
| Can `IsSymbolAccessibleWithin` throw (AD0001)? | `within` is the record's declared symbol from the same compilation. A candidate comes only from a type reachable through `BaseType` | Roslyn requires both symbols to come from the compilation or its references | A base type in an assembly that is not referenced is a missing-metadata type with no members, so it yields no candidate. Retargeted symbols of a project reference are the symbols that reference binds to. The project-reference tests (one existing, one new) exercise this path without AD0001 | ok |
| Short-circuit | `candidate is IPropertySymbol or IFieldSymbol && …`: the `is` pattern binds tighter than `&&`, so the accessibility check runs only on properties and fields | – | – | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The positional parameter and the member it stands for | `rg -n "GetMember\(\|IsSymbolAccessibleWithin" src`: `RecordPositionalParameterUtilities.GetMember` (one caller, `RH4120RecordPrimaryConstructorParameterCasingAnalyzer.IsDictatedName`). `IsSymbolAccessibleWithin` appears nowhere else in `src` | one owner |
| A member is dictated | `CasingAnalyzerBase.IsDictatedMember`, unchanged since audit 1 | one owner |
| Accessibility | Read from the compiler, not modelled | ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ParameterMatchingAnInheritedDictatedPropertyBehindAnInaccessibleMemberOfAnIntermediateRecordIsNotReported` | An inaccessible private field or private property of an intermediate source record does not stop the walk before the dictated member | Any RH4120 diagnostic (two before the repair) | `Verify(source, onConfigure)` compares the exact diagnostic set, including compiler errors | ok |
| `…OfAnotherProjectIsNotReported` | The same for a private field and an internal field of an intermediate record in a project reference, which is the IDE path | Any RH4120 diagnostic (two before the repair) | the same, with `AdditionalProjects["Library"]` and the Meta reference on both projects | ok |
| `ParameterMatchingADeclaredFieldThatHidesAnInheritedDictatedPropertyIsReported` | A field that the record declares is the matched member, and it is not dictated | A missing diagnostic (it disappears if the field arm is dropped) | `Verify` with markup | ok, closes #2 |
| The accessible, non-public side of the new condition | An accessible inherited member stops the walk, whatever its declared accessibility | – (no test) | – | finding 3 |

## Failure classes dismissed
- FC-001, FC-044: the guard now reads the compiler's accessibility result, and its span is traced in *Guard scope*. The remaining model, the base-type walk by name, was decided in `00-lead.md` decision 4. No input diverges on the members traced and probed.
- FC-002: SHARED-CASING-014's "declares or inherits a member of the parameter's name, that member" is read together with its *Why*, "matches the parameter to that member", which selects the member the compiler matches. See *Hints*.
- FC-015, FC-016: one owner (see *Policy ownership*).
- FC-023: `ValueText` is unchanged in both lookups.
- FC-031: the summary and remarks of `GetMember`, its new `<param>`, and the remarks of `IsDictatedName` describe the new code accurately. There is no `TODO`, no commented-out code, and no tracker number.
- FC-032, FC-035: the expected values come from SHARED-CASING-014 and answer 2B, not from patched code.
- FC-036, FC-018, FC-007–FC-009, FC-030: the code fix and the rename-safety path are unchanged. The repair only decides whether a diagnostic is reported.
- FC-004–FC-006, FC-010–FC-014, FC-019–FC-022, FC-024–FC-029, FC-033, FC-034, FC-037–FC-043, and the tab-width matrix: the repair touches no trivia, layout, formatter, precheck, unsupported-shape, or partial-declaration path. Partial records are unchanged: `IsSymbolAccessibleWithin` is true for every member of the record's own merged type.

## Architecture findings
_None._ Audit 1's structural point, that `GetMember` replays the compiler's positional match, is decided in `00-lead.md` decision 4. Its known divergence, accessibility, now reads the compiler's result, and no further divergence was found. Finding 3 concerns the tests that pin that reading.

## Hints
- SHARED-CASING-014's record bullet says "when the compiler declares none because the record declares or inherits a member of the parameter's name, that member". In C# terms a private member of a base record is inherited but not accessible, so with the repaired inputs the record "inherits" two members of that name. The *Why* paragraph ("matches the parameter to that member") resolves this as the matched member, and the spec checker read it the same way. A spec author could make the bullet say "the member the compiler matches", but the text as written does not contradict the code.
- `00-lead.md`, *Decisions taken* row 2 still carries the superseded "stays reported". This is unchanged from audit 1, and the PR summary should not carry it over.

## Follow-up candidates
_None._
