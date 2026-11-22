Role: auditor
Commit: 3b066662dc49e2abcd3964664c6937adf78bed0c
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Full audit. Head `3b066662dc49e2abcd3964664c6937adf78bed0c` (checked out; tree clean). Base `origin/main` = `1788f94d265c4e13052d5145404ae3ffd21e6acc` (matches the prompt's `ls-remote`), an ancestor of the head. The diff matches the handovers: the change doc, SHARED-CASING-014 and RH4120-001, `RecordPositionalParameterUtilities.GetMember`, the `IsDictatedName` override of RH4120, and 16 new or re-tagged tests. `scripts/lane-check.sh behavior-change`: confirmed.

Probes ran in a throwaway worktree under the session scratchpad, which has been removed. They were extra MSTest methods in `Reihitsu.Analyzer.Test`, run with `scripts/test.sh --project analyzer --filter`.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Analyzer/Core/RecordPositionalParameterUtilities.cs` `GetMember` | FC-044, FC-001 | – | PR-introduced | `GetMember` models the compiler's match from a positional parameter to an existing member, but ignores accessibility. The compiler skips an inherited member the record cannot access and matches the next accessible one further up. `GetMember` stops at the first property or field it sees. Counterexample, with `INamed { string name { get; } }` in a referenced assembly: `public record Person(string name) : INamed; public record Mid(string other) : Person(other) { private string name = ""; public string Peek() => name; } public record R(string name) : Mid(name);`. The compiler declares no property for `R.name`; `R` inherits `Person.name` with no error, and `FindImplementationForInterfaceMember` gives `Person.name`. SHARED-CASING-014 (2B bullet) says `R.name` MUST NOT be reported, but `GetMember` returns `Mid`'s private field, so RH4120 reports `R.name` (probe: 2 diagnostics where 1 was expected). **Failing members:** a private field and a private property in an intermediate source record, both run. A private field in an intermediate record of another project of the solution, run: reported, while the same record as a metadata reference is not reported. So the IDE and a command-line build disagree, against SHARED-CASING-014's stated reason for the compilation boundary. An `internal` member of an intermediate record in another project fails in the same way: traced, not run, because a compilation reference exposes internal members and a metadata reference with the default import options does not. **Members that hold:** a private method or event (both sides skip it); a protected or public member (both sides take it); any member the record itself declares (the compiler matches declared members whatever their accessibility); a generic source base `Base<T>(string name) : INamed` with `G(string name) : Base<int>(name)` (not reported, run); a `new` property hiding a dictated one (reported, as the spec requires, run). The outcome on the failing members equals `main`'s (reported, no fix). The input is rare: the private or internal member violates RH4106, RH4111, RH4108, or RH4113. | Choose the member by the accessibility the compiler computes, not a model of it. In the base-type loop, skip a candidate the record cannot access (`Compilation.IsSymbolAccessibleWithin(candidate, recordType)`, from `semanticModel.Compilation`). This is a one-condition cheap check that reads a compiler result, and it makes the IDE and the command line agree. Add a test for the private-field member. If the lead prefers refusing the class, the code stays and SHARED-CASING-014 must say that such a parameter is reported; today it says MUST NOT. |
| 2 | low | `tests/Reihitsu.Analyzer.Test/Naming/RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests.cs` | FC-003, FC-017 | – | PR-introduced | The `or IFieldSymbol` arm of `GetMember` decides the outcome when a field of the parameter's name stands between the record and a dictated property, but no test exercises it. Example: `public record F(string name) : Person(name) { public new string name = name; }` with `Person : INamed` from case 1. The compiler matches the field, which is not dictated, so `F.name` is reported; a probe confirms today's code does this. If the arm is dropped, `GetMember` walks on to `Person.name`, `F.name` stops being reported, and every committed test stays green. | Add one test for the field case, `F.name` reported, tagged SHARED-CASING-014 and RH4120-001. Or fold it into the test that finding 1 adds. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Is the positional parameter's name dictated (RH4120 `IsDictatedName`) | The symbol `GetMember` returns: the first property or field named after the parameter, in the record and then its base types, whatever its accessibility | The compiler's positional member match: a declared member of the record whatever its accessibility, else the first accessible member found while walking the base types | Agrees for a synthesized property (the record's only member of that name), for a declared member, and for an accessible inherited one. Disagrees when an inaccessible member of the name stands between the record and the matched member | Finding 1 |
| Which member is the "declared property" (synthesized case) | `GetMember` finds the record's member first; `GetDeclaredProperty` matches by the declaring syntax span | `GetDeclaredProperty`, used by `CasingRenameSafetyUtilities.GetPositionalPropertyFlagsAsync` and by RH4116 through `GetDeclaredMembers` | Agree: when the compiler declares a property, it is the record's only member of that name | ok |
| What makes a member dictated | `CasingAnalyzerBase.IsDictatedMember`, unchanged | Same method, shared with RH4102–RH4104, RH4112–RH4114, and RH4116 | Single owner, reused | ok |
| Which parameters are checked | `GetLocations`, unchanged (record parameter list, not `_`) | RH4120-001/-002 | RH4120-001 now states this, matching the code | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| A member is dictated | `rg -n "IsDictated" src`: `CasingAnalyzerBase.IsDictatedMember` is the only implementation. `IsDictatedName` overrides in RH4102, RH4103, RH4104, RH4112, RH4113, RH4114, RH4116, and RH4120 call it | one owner |
| Positional parameter to the member it stands for | `rg -n "GetMembers\(parameter\.Identifier\|BaseType\)" src`: `RecordPositionalParameterUtilities.GetMember` (new), `GetDeclaredProperty`, and `GetDeclaredMembers`. No private copy in `Reihitsu.Analyzer.CodeFixes` or `Reihitsu.Core`. `RegionCategoryUtilities` walks base types for another decision | one owner class. Finding 1 concerns its fidelity, not duplication |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| 8 "…IsNotReported" tests for outcome 1 (interface, every record kind, override, override through a source record, a source interface extending a Meta one, generic, default interface member, partial across documents) | The parameter whose synthesized property is dictated is not reported | Any RH4120 diagnostic in the source | `Verify(source, onConfigure)` compares the exact diagnostic set; the reference is attached via `AdditionalReferences` | ok |
| `ParameterWhosePropertyImplementsOrOverridesAPropertyOfAnotherProjectIsNotReported` | Same, for a project reference | Any RH4120 diagnostic | same, with `AdditionalProjects` | ok; covers outcome 1 only (see Hints) |
| `ParameterMatchingAnInheritedDictatedPropertyIsNotReported`, `ParameterMatchingADeclaredDictatedPropertyIsNotReported` | 2B: a parameter that matches a dictated inherited (one and two levels) or declared member is not reported | Any RH4120 diagnostic | same | ok; the field arm is untested (finding 2) |
| `OtherParametersOfARecordWithADictatedParameterAreReportedAndFixed` | Only the dictated parameter is exempt; the fix and Fix All rename the others, including references | Wrong diagnostic set, or a fixed source that differs | `Verify(source, fixedSource, onConfigure)` (single fix and Fix All) | ok |
| 4 "…IsReported" counterparts (source interface, implementation only for a derived record, explicit implementation, an inherited Meta member that is not dictated: `Inherited`, `Virtual`, `NamedInMeta`) | The exemption does not reach a member that is not dictated | A missing RH4120 diagnostic | `Verify` with markup | ok |
| 2 existing tests re-tagged RH4120-001 | Example `B`/`D`/`P` is reported | A missing diagnostic | unchanged | ok |

## Failure classes dismissed
- FC-001, FC-044: reachable. See finding 1.
- FC-002: "the property the compiler declares" and "declares or inherits a member" are explicit conditions; the 2B qualifier "the compiler declares none" is what `GetMember` replaces with a lookup, and its gap is finding 1.
- FC-003: both sides of the change are tested (exempted cases and their counterparts). The field arm is finding 2.
- FC-015, FC-016: one owner each (see Policy ownership). RH4116 already uses the same helpers for the synthesized case.
- FC-017: see Assertion adequacy.
- FC-023: `ValueText` is used in both `GetLocations` and `GetMember`, so `@name` gives the same lookup.
- FC-024: partial records are tested across two documents, and explicit implementation is tested. Static interface members cannot match a positional parameter without a compile error (CS8866).
- FC-031: the summaries and remarks of `GetMember` and the class, and the `IsDictatedName` remarks, describe the new code accurately.
- FC-036, FC-018, FC-007, FC-008, FC-009, FC-030: the code fix is unchanged. A parameter that is not reported gets no fix, and the fix for the other parameters is tested.
- FC-032, FC-035: the expected values come from the spec examples and the maintainer's answers, not from patched code. No formatter change can flip these results.
- FC-004, FC-005, FC-006, FC-010–FC-014, FC-019–FC-022, FC-025–FC-029, FC-033, FC-034, FC-037–FC-043, and the tab-width matrix: no trivia, layout, formatter, precheck, or unsupported-shape path is changed. The change only removes diagnostics.

## Architecture findings
Finding 1 is structural: a component predicts another's behavior. `RecordPositionalParameterUtilities.GetMember` replays the compiler's lookup rule for positional members, which the compiler applies in `AddSynthesizedRecordMembersIfNecessary` and exposes through no API. The two disagree for an inaccessible member of the parameter's name in an intermediate base record. The same input then gets different diagnostics in the IDE (project reference: private and internal members visible) and in a command-line build (metadata reference: public import). The compiler does expose accessibility (`IsSymbolAccessibleWithin`), so the known divergence can be closed by reading that result; this stays within the component the run changes. Whether that suffices or the class is refused is the lead's decision.

## Hints
- `00-lead.md`, *Decisions taken* row 2 still says `Derived(string name) : Person(name)` "stays reported", which the maintainer's answer 2B superseded. This is the lead's working file, but the PR summary should not carry it over.
- `ParameterWhosePropertyImplementsOrOverridesAPropertyOfAnotherProjectIsNotReported` covers only the synthesized-property case for another project. The spec's "the same holds when … declared in another project" also covers the 2B paths. Their dictated decision goes through the same `IsDictatedMember` path, so this is low risk, but finding 1 shows that the project-reference path can differ from the metadata path in member lookup.
- RH4116 (tuple element names) does not follow answer 2B. A tuple-typed parameter that matches an inherited dictated member still has its element names reported, because `GetDeclaredMembers` yields only the synthesized property. This is uncertain: renaming the tuple elements likely still compiles, because the positional match ignores tuple names, so it may be correct as it is. It is outside the run and is not a finding.

## Follow-up candidates
_None._
