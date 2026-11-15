Role: auditor
Commit: df00d3b3b163c1308efec0a468f67e22301d153e
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `62b92c4` (`07-auditor.md`). Head `df00d3b` (checkout verified, clean tree). Base `origin/main` = `3b3639e` as the prompt states, and it is an ancestor of the head. The merge of `main` brings no change to the casing code, `sdd/specs/shared/`, or `squad/failure-classes.md`. The run's diff against `3b3639e` touches only `sdd/specs/shared/casing.md`, `CasingCodeFixProviderBase{T}.cs`, and the Naming tests. `scripts/lane-check.sh bug-fix` reports `confirmed — claimed bug-fix, diff needs bug-fix`. The repair is what `08`–`12` describe: SHARED-CASING-009 gained the three classes (b58ee29), 12 tests (c8650bb), and a production change of +41/−17 in one file (8c7e443). That change makes the R1 guard take `GetOldNames` and adds `CountImplementedInterfaceMembers`. Scope did not grow. No `TODO` and no tracker number among the added source lines.

Status of earlier findings:
- **#1 closed.** `RewritesOtherCodeAsync` now takes its names from `GetOldNames(cascade, renamedCascade)` and no longer builds its own set. c4, c6, c7, and d1 have tests and are withheld. c1 (adjacent `Value + value`) now withholds whether the differ merges the spans or not: with `oldNames = {Value}`, a merged span holds 1 old-name identifier out of 3 tokens, and a separate span holds 0. I traced the cascade members that still have to pass: the other part's differently spelled parameter (`other`, an old name), and the unrenamed type parameter of the other part (retained, and no rewrite reaches it). I found no remaining spelling-based member.
- **#2 closed** for every member it listed: a3 (method) is withheld and tested, and so are the property and event variants it named as not run. The indexer has no casing rule. `dynamic` stays out of scope (`08-spec-author.md`). A member its enumeration did not name fails. That member is reported as #4.
- **#3 closed.** SHARED-CASING-009 records the class, and a1 and a2 are tested. The qualifying control (RH4002 `FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture`) still holds.

Evidence: fixtures in the session scratchpad, outside the repository, run with `scripts/apply-fix.sh` on the head. LF and CRLF gave identical results. I checked the run-time claims with `dotnet run` on before/after copies.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 4 | high | `CasingCodeFixProviderBase{T}.cs:1108–1118` (`CountImplementedInterfaceMembers`, used at `:766`); `sdd/specs/shared/casing.md` SHARED-CASING-009, interface paragraph (line 559, "When the type does not list the interface itself, the member that implements it today keeps implementing it, and this does not withhold the fix") | FC-016, FC-001, FC-006 | #2 | same requirement | The interface check reads the interface mapping of the renamed member's **own** type only. A type **derived** from it that lists the interface maps the interface member to the most derived public member of that name, and after the rename that is the renamed member. The run-time target of a call through the interface changes with no source reference changing (SHARED-FIX-003), and the fix is offered. **Failing members** (fix offered on the head; run time checked before vs after): **b1** RH4103: `interface IProcessor { int Process(); }`, `class Root { public int Process() => 1; }`, `class Base : Root { public int process() => 2; }`, `class Derived : Base, IProcessor { }`. Renamed to `Process`, `((IProcessor)new Derived()).Process()` returns 2 instead of 1. **b2** the same with a default implementation `int Process() => 1;` in the interface and no `Root`: 1 → 2. **b4** like b1 with `Root : IProcessor` and `Derived` re-listing it: 1 → 2. **b5** RH4114 property, the shape of b1 (`int Size { get; }`): 1 → 2. **b6** RH4102 event, the shape of b1: fixed, same mapping mechanism. **Members that hold:** a3 and its property and event variants (own type lists the interface); **b3** a default interface implementation on the renamed member's own type (`class Derived : IProcessor { public int process() => 2; }`), withheld; the controls `FixRenames…WhenTheTypeDoesNotListTheInterface` (no type re-lists the interface). Axes: which type lists the interface (own: holds; derived: fails); what implements the interface member today (inherited class member, default interface implementation, base re-implementation); member kind (method, property, event). The spec states the derived case as "does not withhold", and that statement contradicts the unchanged SHARED-FIX-003. Rare class: a wrongly cased member whose correct name meets an interface member that a derived type implements. | Repeats #2, so this is an architecture finding (`squad/README.md`): refuse the class. One way is to compare the implementation count over the containing type **and every type derived from it** in the solution, before and after the rename. Another is to withhold when a type derived from the containing type lists an interface with a member of the new name. Narrow the spec's "does not list the interface" counter-statement to the case where no derived type lists it either. Add tests for b1 and b2 at least. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| R1: the renamer rewrote code other than cascade references (`RewritesOtherCodeAsync`) | Every `TextChange` of every changed document. A token counts as a cascade reference when it is an identifier whose `ValueText` is in `GetOldNames(cascade, renamedCascade)` | `GetOldNames` (`:403`), the single owner of the cascade's old names; SHARED-CASING-009 matches by symbol, not spelling | Yes: one definition, which excludes retained and unrenamed spellings. A spelling match can still over-approximate only toward withholding | holds (#1 closed) |
| R2: reference count before vs after (`CountReferenceLocationsAsync`) | Unchanged from the first audit | SHARED-FIX-003 | Unchanged: partial by design, now complemented by the interface check | holds for the members it covers |
| Interface mapping: count of interface members the symbol implements (`CountImplementedInterfaceMembers`) | `symbol.ContainingType.AllInterfaces`, mapped with that type's `FindImplementationForInterfaceMember` | The compiler's interface mapping of **every** type that inherits the renamed member (SHARED-FIX-003; SHARED-CASING-009, interface paragraph) | No: derived types that list an interface are not inspected (b1, b2, b4, b5, b6) | finding #4 |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The cascade's old names | `GetOldNames` (`CasingCodeFixProviderBase{T}.cs:403`), computed once at `:758` and reused by R1, the caller-argument check, and the excluded-code check | One owner; the private copy is gone (#1 closed) |
| Which member implements an interface member | `CountImplementedInterfaceMembers` (`:1108`); `InterfaceImplementationUtilities` (`src/Reihitsu.Analyzer/Core/InterfaceImplementationUtilities.cs:61`) | Different decisions (rename safety vs region grouping), and both read the compiler's mapping directly; ok |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4105 c4, c6, c7, c1; RH4121 d1 (`NoFixIsOfferedWhen…SpelledLikeThe…OfTheOtherPart…`) | No fix, single fix and Fix All, when a non-cascade reference spelled like a retained cascade name is rewritten | A fix offered or any rewrite | `Verify(src, src)` | adequate; the other side is pinned by the existing RH4105/RH4121 partial-method controls |
| RH4103 a1, a2 (`…TurnACallOfTheRenamedExtensionMethodIntoAStaticCall`, `…CastAnArgumentOfACallOfTheRenamedMethod`) | No fix when a cascade reference would be rewritten beyond its name | A fix offered | `Verify(src, src)` | adequate; the qualifying side is pinned by RH4002 `FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture` |
| RH4103/RH4114/RH4102 `NoFixIsOfferedWhenTheNewNameWouldChangeWhich…ImplementsAnInterface…` | No fix when the own type's interface mapping changes | A fix offered | `Verify(src, src)` | adequate for the own-type member |
| RH4103/RH4114 `Fix…HidesAnInheritedInterfaceImplementationWhenTheTypeDoesNotListTheInterface` | The fix renames when no mapping changes | Withheld fix or another rewrite | `Verify(src, fixed)`, exact output | adequate; both sides of the own-type boundary (FC-003) |
| Missing | b1/b2 (derived type lists the interface) | – | – | tests required with #4 |

## Failure classes dismissed
The repair changed only the R1 name source and added the interface check, so the first audit's dismissals still hold (`07-auditor.md`): FC-004/005/019/020/021, FC-010–014/028–030/032, FC-038–043, FC-022, FC-023, FC-025/027, FC-026, FC-007–009, FC-018, FC-033–035, and FC-037. Re-checked for the delta:
- FC-031: the summaries of `RenameWithoutConflictAsync`, `RewritesOtherCodeAsync`, and `CountImplementedInterfaceMembers`, and the inline comment at `:760`, describe the new code. The remark that the count "stays the same" when the type does not list the interface is true for the own type, and #4 covers the derived case.
- FC-015: resolved for old names (see the policy table).
- FC-024: partial methods and type parameters are covered by c4–d1. Explicit interface implementations are not reported by a casing rule.
- FC-036: `nameof` (c7) is covered. Reflection, serialization, and `dynamic` stay out of scope.
- FC-017: see the assertion table.
- Not dismissed: FC-016, FC-001, FC-006 (#4).

## Architecture findings
#4 repeats the class of #2: the same decision (does the rename change which member implements an interface member) fails between the same components (the casing rename guard and the compiler's interface mapping). It now fails on a member that the enumeration of #2 did not name, a type derived from the renamed member's type that lists the interface. `squad/README.md` directs a repeat to be refused as a class rather than repaired member by member. The spec's counter-statement in SHARED-CASING-009 states the failing member as allowed, so the refusal needs the spec author as well.

## Hints
- `CountImplementedInterfaceMembers` also runs for parameters, type parameters, locals, and nested types. Their `ContainingType` is the enclosing type, so the count is always 0 on both sides. It is correct, but the work is wasted on every such candidate. It is cheap next to the reference searches, so this is not a finding.
- Not run: b1 with a source call such as `new Derived().Process()`. That call rebinds from `Root.Process` to `Base.Process`, so R2 would most likely withhold it already. The finding concerns the input with no such call.
