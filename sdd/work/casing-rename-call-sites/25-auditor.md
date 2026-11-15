Role: auditor
Commit: 27789ab0dd7d56970ba9290837cd9280967a22e8
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `07b4f2f` (`19-auditor.md`), decisions 12 and 13 of `00-lead.md`. Head `27789ab` (checkout verified, clean tree). Base `origin/main` = `3b3639e` (`git ls-remote` matches the prompt), an ancestor of the head. The delta is what `20`–`24` describe and nothing more: the SHARED-CASING-009 interface paragraph, its Why and three examples (8938f16); 7 tests in RH4102/RH4103/RH4104/RH4114 (a5ffd78); in `CasingCodeFixProviderBase{T}.cs` (a4fc274), `HasInterfaceMemberOfNameInHierarchyAsync` takes `prepared.Value.Cascade` and skips local functions, with updated summaries and remarks. `scripts/lane-check.sh bug-fix` reports `confirmed`. Scope did not grow. No `TODO` and no tracker number among the added source lines.

Status of earlier findings:
- **#1, #2, #3, #4 closed** (untouched by the delta; their tests pass per `22-implementer.md`).
- **#5 closed** for every member it listed: e1, e3 (RH4103), p1, p3 (RH4114), v1 (RH4102) are withheld and tested; e2, e4, e5, b1–b6 still hold.
- **#6 closed** for the members it listed: l1, l2 (RH4104) are fixed and tested; l3 holds. The class of #6, the kind filter admitting a method symbol that cannot implement an interface member, now fails on a member its enumeration did not name, which taking the cascade newly brings in: the constructors of a renamed type. Reported as #7.

Evidence: fixtures in the session scratchpad, outside the repository, run with `scripts/apply-fix.sh` on the head, and in temporary worktrees on `07b4f2f` and `3b3639e` (removed since). Head results are identical under LF and CRLF. One run-time claim checked with `dotnet run` on before/after copies.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 7 | high | `CasingCodeFixProviderBase{T}.cs:1128–1130` (the kind filter in `HasInterfaceMemberOfNameInHierarchyAsync`), reached from `:739` with `prepared.Value.Cascade` | FC-001, FC-002 | #6 | PR-introduced | The cascade of a renamed class, struct, record, or record struct holds its constructors (`GetRenameCascadeAsync` reads it from `FindReferencesAsync`, which cascades a type to its constructors). A constructor is `SymbolKind.Method`, is not a local function, and has the type as `ContainingType`, so the check now runs for every type rename and withholds it when the new type name is the name of a member of an interface in the type's hierarchy. A constructor cannot implement an interface member and a type is not an interface member, so no mapping can change. SHARED-CASING-009 says "A renamed field, parameter, local variable, local function, type, or type parameter is not withheld by this paragraph", so the head breaks that clause. **Failing members** (`no-fix-offered` on the head, `fixed` on `07b4f2f` and on `main`): **t1** RH4002, own type: `public interface IHasProcessor { int Processor => 1; }` `public class processor : IHasProcessor { }`; **t2** RH4002, derived type: the same interface, `public class processor { }`, `public class Derived : processor, IHasProcessor { }`; **t4** RH4010: `public record processor : IHasProcessor;`; **t5** RH4003: `public struct processor : IHasProcessor { }`; **t7** RH4011: `public record struct processor : IHasProcessor;`. **Members that hold:** c1–c5, the same shapes with the interface member renamed to `Other` (fixed on the head); **t6** RH4006 `public delegate void clone();` (fixed; `ICloneable.Clone` is not reached); the local functions of #6 (l1, l2, l3). Axes: which method kind of the cascade is admitted (ordinary method, accessor: correct; local function: excluded by decision 13; instance constructor, implicit or declared: fails); which type kind is renamed (class, record, struct, record struct: fail; delegate, enum, interface without static constructor: hold); whose hierarchy holds the interface (own type, derived type: both fail). No test of the run covers a type rename next to an interface member of the new name, so the analyzer suite stays green. Rare class. | Repeats #6: narrow the kind filter once, as an allow-list instead of excluding one more kind — admit only methods of `MethodKind.Ordinary` (plus properties and events), which drops constructors, static constructors, destructors, accessors (their property or event is in the cascade itself), operators, and local functions in one cheap check. Add an RH4002 control (t1 or t2) that asserts the fix, and adjust the comment and remarks that name local functions as the only exempt kind. |
| 8 | medium | SHARED-CASING-009, interface paragraph, last sentence (`casing.md:559`); no RH4120 test | FC-017, FC-031 | – | same requirement | The cascade of a positional record parameter (RH4120) holds the property the compiler declares for it. Since the check takes the cascade, it withholds the fix when the record's hierarchy implements an interface with a member of the new name: `public interface INamed { string Name => "default"; }` `public record Person(string name) : INamed;` is `no-fix-offered` on the head, `fixed` on `07b4f2f` and on `main`. The head is right in substance: after the rename the declared property `Name` implements `INamed.Name`, and `((INamed)new Person("x")).Name` prints `default` before and `x` after (checked with `dotnet run`), so `main`'s fix changes run-time behavior (SHARED-FIX-003), and decision 12's "every method, property and event of the cascade" covers the property. But the spec's last sentence says "A renamed … parameter … is not withheld by this paragraph", which this input contradicts, and no test pins the withholding, so the next narrowing of the filter (for example #7's) could drop it unnoticed. Control c3 (interface member `Other`) is fixed on the head. | Spec: say that a renamed symbol of another kind is withheld when its cascade holds a method, property, or event this paragraph covers, for example the property the compiler declares for a positional record parameter; keep the kind list for the rest. Add an RH4120 test for the `Person` input asserting no fix (`Verify(src, src)`), tagged SHARED-CASING-009 and SHARED-FIX-003. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Interface name check: which renamed symbols | Every symbol of `prepared.Value.Cascade` | SHARED-CASING-009 ("a method, property, or event of the rename cascade"), SHARED-CASING-007; every other check of `RenameWithoutConflictAsync` takes the cascade | Yes | holds (#5 closed) |
| Interface name check: which symbol kinds | `SymbolKind.Method/Property/Event` except `MethodKind.LocalFunction`, with a `ContainingType` | The spec's exempt kinds (field, parameter, local variable, local function, type, type parameter); the decision: a symbol that can implement an interface member | No: constructors in a type's cascade are admitted (t1, t2, t4, t5, t7); a parameter's cascade property is admitted, which the spec text does not say (#8) | findings #7, #8 |
| Interface name check: which types, name match | As in `19-auditor.md` (containing type; derived classes; for an interface, implementations and derived interfaces; `GetMembers(identifier)`, any kind) | As in `19-auditor.md` | Yes | holds, unchanged |
| R1 / R2 and the other checks | Unchanged | – | – | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether a rename meets an interface member in the type hierarchy | `HasInterfaceMemberOfNameInHierarchyAsync` only (`rg FindDerivedClassesAsync\|FindImplementationsAsync\|FindDerivedInterfacesAsync src`, non-test) | One owner |
| Which symbols can implement an interface member (the kind filter) | The filter at `:1128–1130`; `MethodKind.LocalFunction` elsewhere (`IsCompilerLookupMethodRename`, `ExcludedCodeUtilities.cs:156`, RH4122/RH4123) answers other questions | One owner; its span is #7 |
| The rename cascade | `GetRenameCascadeAsync` only | One owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4103 e1, e3; RH4114 p1, p3; RH4102 v1 (`NoFixIsOfferedWhenAnotherImplementer…`, `…ATypeDerivedFromTheOverridden…`) | No fix, single or Fix All, for any reported member of a cascade in which another member's hierarchy holds an interface with the new name | A fix offered on any span (in e3 the `Derived` span is the one the own-hierarchy check misses) | `Verify(src, src)` | adequate |
| RH4104 l1, l2 (`FixRenamesALocalFunctionToTheNameOf…`) | A local function is renamed although its enclosing type's interfaces have the new name; exact output | Withheld fix or another rewrite | `Verify(src, fixed)` | adequate |
| Missing | A type rename next to an interface member of the new name (#7); the RH4120 positional parameter (#8) | – | – | tests required with #7 and #8 |

## Failure classes dismissed
The delta changed one guard's input and kind filter. The dismissals of `07-`, `13-`, and `19-auditor.md` hold for everything it does not reach (FC-004/005/007–014/018–023/025–030/032–035/037–043). Re-checked for the delta:
- FC-003: both sides of the cascade boundary (e1/e3 withheld, `Job` and `FixRenamesAMethodThatADerivedTypeUsesToImplementASourceInterface` fixed) and of the local-function boundary (l1/l2 fixed, RH4103 tests withheld) are tested; the type-kind side is not (#7).
- FC-006, FC-016: the reported symbol versus the other cascade members is closed (#5); the other members of the cascade are now the input.
- FC-015: one owner (see the policy table).
- FC-024: explicit interface implementations are not reported by a casing rule; when one is in the cascade, its type is an implementer of the interface the interface member's own hierarchy already covers. Partial members share their containing type.
- FC-031: the summaries and remarks of `RenameWithoutConflictAsync` and `HasInterfaceMemberOfNameInHierarchyAsync` describe the cascade-wide check; the remark "A local function implements no interface member, so it adds no hierarchy" is true but no longer the only exemption the filter needs (#7). The spec's kind list is contradicted by RH4120 (#8).
- FC-036: the check only withholds.
- FC-042: no shape is declared unsupported in this run.
- Not dismissed: FC-001, FC-002 (#7), FC-017, FC-031 (#8).

## Architecture findings
#7 is the second failure of the decision "which symbols of a rename can implement an interface member", between the same components as #6: the casing guard's kind filter and the compiler's interface mapping. Its cause is structural: the filter is a deny-list of `SymbolKind`s plus one excluded `MethodKind`, while the set it filters, the rename cascade, is whatever Roslyn's reference engine cascades to. Every widening of the input (decision 12 brought in constructors and a positional parameter's property) exposes another kind the deny-list did not name. An allow-list of the kinds that can implement an interface member (ordinary methods, properties, events) states the decision once and does not depend on what the cascade happens to hold.

## Hints
- The `RH2001` renamer follow-up candidate in `00-lead.md` is unchanged.
