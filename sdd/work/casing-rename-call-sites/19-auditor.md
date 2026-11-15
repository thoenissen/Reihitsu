Role: auditor
Commit: 07b4f2f308c35dd595298066c429b8b9e9c855f2
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `df00d3b` (`13-auditor.md`), decision 10 of `00-lead.md`. Head `07b4f2f` (checkout verified, clean tree). Base `origin/main` = `3b3639e` (`git ls-remote` matches the prompt), an ancestor of the head. The delta is what `14`–`18` describe and nothing more. It is the SHARED-CASING-009 interface paragraph, its Why, and its examples (141b218). It adds 11 tests in RH4102/RH4103/RH4109/RH4114 and converts 2 of them (e48cb14). In `CasingCodeFixProviderBase{T}.cs` (11d3dd8), `CountImplementedInterfaceMembers` is replaced by `HasInterfaceMemberOfNameInHierarchyAsync`, which is called before the rename. `scripts/lane-check.sh bug-fix` reports `confirmed`. Scope did not grow beyond the decision. No `TODO` and no tracker number among the added source lines.

Status of earlier findings:
- **#1, #2, #3 closed** (unchanged since `13-auditor.md`; R1, R2, and the a1/a2/a3 tests are untouched by the delta and pass per `16-implementer.md`).
- **#4 closed** for every member it listed: b1, b2, b4 (RH4103), b5 (RH4114), and b6 (RH4102) are withheld and tested, and b3 still holds. The class fails on a member that its enumeration did not name, the renamed member's type versus the other types of the rename cascade. That member is reported as #5.

Evidence: fixtures in the session scratchpad, outside the repository, run with `scripts/apply-fix.sh` on the head, under LF and CRLF with identical results. I checked run-time claims with `dotnet run` on before/after copies. I ran the #6 fixtures on `df00d3b` in a temporary worktree, which has since been removed.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 5 | high | `CasingCodeFixProviderBase{T}.cs:738` (call with `prepared.Value.Symbol` only), `:1118–1143` (`HasInterfaceMemberOfNameInHierarchyAsync`) | FC-001, FC-016, FC-006 | #4 | same requirement | The name check inspects only the hierarchy of the **reported** symbol. SHARED-CASING-009 says "a renamed method, property, or event … the type that declares the renamed member or a type … that derives from it". Every symbol of the rename cascade is renamed (SHARED-CASING-007), so the check has to cover the hierarchies of the other cascade members too. When a cascade member lies in another hierarchy whose interface has a member of the new name, the fix is offered and the run-time target of a call through that interface changes. **Failing members** (fix offered on the head): **e1** RH4103, a sibling implementer: `class A : IProcessor { public int process() => 0; }`, `interface IProcessor { int process(); }`, `interface IOther { int Process() => 1; }`, `class B : IProcessor, IOther { public int process() => 2; }`, fix on `A.process`. The cascade renames `B.process`, and `((IOther)new B()).Process()` returns 2 instead of 1 (checked with `dotnet run`). **e3** RH4103, the base of an override: `class Derived : Base { public override int process() => 3; }`, `class Base { public virtual int process() => 2; }`, `interface IOther { int Process() => 1; }`, `class Sibling : Base, IOther { }`, fix on `Derived.process`. The cascade renames `Base.process`, and `((IOther)new Sibling()).Process()` returns 2 instead of 1 (checked). **p1, p3** RH4114, the property shapes of e1 and e3 (`int Size => 1;` in `IOther`): fixed. **v1** RH4102, an event: `class A : INotify { public event EventHandler changed; }`, `class Root : IOther { public event EventHandler Changed; }`, `class B : Root, INotify, IOther { public event EventHandler changed; }`, fix on `A.changed`: fixed, and `IOther.Changed` on `B` maps to `B.Changed` afterwards. **Members that hold:** the fix on the interface member itself (e5, withheld through `FindImplementationsAsync`); the fix on an override whose base lists the interface (e4, `Derived.AllInterfaces` inherits it); e2 (the e1 method shape through `Root : IOther` with `B` re-listing it, withheld by another check); b1–b6. Axes: which cascade member's hierarchy holds the interface (the reported symbol's: holds; a sibling implementer's or an overridden base's: fails); member kind (method, property, event: all fail). The remarks of `RenameWithoutConflictAsync` ("The checks that depend on what the rename reaches take it from `GetRenameCascadeAsync`") no longer hold for this check (FC-031). Rare class. | Repeats #4 (and #2): refuse the class over the cascade. Run the same name check for every method, property, and event of `prepared.Value.Cascade` that a type declares, not only for the reported symbol. This is the same cheap check, and the spec text already demands it. Add tests for e1 and e3 at least. |
| 6 | medium | `CasingCodeFixProviderBase{T}.cs:1123` (`symbol.Kind is not (Method or Property or Event)`); SHARED-CASING-009, the interface paragraph's last sentence (`casing.md:559`) | FC-001, FC-002 | – | PR-introduced | The check admits every `SymbolKind.Method`, which includes a **local function** (RH4104). A local function's `ContainingType` is the enclosing type, so a local function whose PascalCase name is that of a member of an interface in the enclosing type's hierarchy loses its fix. A local function cannot implement an interface member, so no mapping can change. **Failing members** (RH4104, `no-fix-offered` on the head, `fixed` on `df00d3b` and so on `main`): **l1** `class Worker : System.IDisposable { void System.IDisposable.Dispose() { dispose(); void dispose() { } } }`; **l2** `interface IProcessor { int Process() => 1; }`, `class Worker : IProcessor { public int Run() { return process(); int process() => 2; } }`. **Holds:** l3, the same local function in a type with no interface (fixed). The spec paragraph lists the kinds it excludes (field, parameter, local variable, type, type parameter) but not local functions. The decision speaks of "a member", and the Why speaks of "a wrongly cased member", so the guard's span is wider than the decision it protects, and no test pins either side. | Narrow the guard to members of a type, for example by excluding `MethodKind.LocalFunction`, which is a cheap check. Name local functions among the excluded kinds in the spec paragraph, and add an RH4104 control (l2) that asserts the fix. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Interface name check (`HasInterfaceMemberOfNameInHierarchyAsync`) — which renamed symbols | `prepared.Value.Symbol` only | SHARED-CASING-009 "a renamed method, property, or event", read with SHARED-CASING-007 (every cascade symbol is renamed); every other check of `RenameWithoutConflictAsync` takes its region from the cascade (its remarks) | No: the other cascade members' hierarchies are not inspected (e1, e3, p1, p3, v1) | finding #5 |
| Interface name check — which symbol kinds | `SymbolKind.Method/Property/Event` with a `ContainingType` | The decision and Why: a type member that could implement an interface member | No: local functions are admitted (l1, l2) | finding #6 |
| Interface name check — which types | The containing type; for a class, `FindDerivedClassesAsync` (transitive by default, whole solution); for an interface, `FindImplementationsAsync` (transitive) and `FindDerivedInterfacesAsync`; for a struct, itself | The spec's "type that declares … or a type in the solution that derives from it; for an interface also implementers and inheriting interfaces", and `AllInterfaces` covers listed and inherited interfaces | Yes | holds |
| Interface name check — name match | `interfaceType.GetMembers(identifier)`, ordinal, any member kind | The spec's "a member of an interface already has" (the spec author's internal choice: any kind) | Yes | holds |
| R1 / R2 (unchanged) | As in `13-auditor.md` | – | – | holds; the interface arm was removed from the R2 condition and its comment updated |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether a rename meets an interface member in the type hierarchy | `HasInterfaceMemberOfNameInHierarchyAsync` (`:1118`), the only user of `FindDerivedClassesAsync`/`FindImplementationsAsync`/`FindDerivedInterfacesAsync` in non-test source; `CountImplementedInterfaceMembers` is gone | One owner |
| Which member implements an interface member | `InterfaceImplementationUtilities.cs:61` (region grouping, SHARED-REGGROUP-004) | A different decision; not a second owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4103 b1, b2, b4; RH4114 b5; RH4102 b6 (`NoFixIsOfferedWhenATypeDerivedFrom…`) | No fix, as a single fix or through Fix All, when a derived type lists an interface with a member of the new name | A fix offered | `Verify(src, src)` | adequate |
| RH4103/RH4114 converted controls (`…ThatTheTypeImplementsThroughItsBaseType`) | No fix when the type implements the interface only through its base | A fix offered | `Verify(src, src)` | adequate |
| RH4103 interface-as-declaring-type tests (`…ATypeImplementingTheMethodsInterface…`, `…AnInterfaceInheritingFromTheMethodsInterface…`) | No fix when an implementer or inheriting interface has an interface with the new name | A fix offered | `Verify(src, src)` | adequate |
| RH4103 `FixRenamesAMethodToTheNameOfAMethodOfAnInterfaceOutsideItsTypesHierarchy`; RH4109 `FixRenamesAFieldToTheNameOfAMethodOfAnInterfaceThatADerivedTypeImplements` | The fix renames outside the refused class, exact output | Withheld fix or another rewrite | `Verify(src, fixed)` | adequate; both sides of the name and kind boundaries (FC-003) for types and fields |
| Missing | e1, e3 (cascade member in another hierarchy); an RH4104 local-function control | – | – | tests required with #5 and #6 |

## Failure classes dismissed
The delta replaced one guard. The dismissals of `07-auditor.md` and `13-auditor.md` still hold for everything it does not reach (FC-004/005/007–014/018–023/025–030/032–035/037–043). Re-checked for the delta:
- FC-003: types (in and out of the hierarchy) and fields have tests on both sides. Local functions do not (#6).
- FC-015: one owner (see the policy table).
- FC-017: see the assertion table.
- FC-024: an explicit interface implementation is not reported by a casing rule. An explicit implementation in the hierarchy only makes the name check refuse more (accepted by decision 10). Partial members keep the same containing type, so the cascade adds no hierarchy there.
- FC-031: the summary of `HasInterfaceMemberOfNameInHierarchyAsync` and its call-site comment describe the new code. The summary and remarks of `RenameWithoutConflictAsync` overstate the region the check covers (part of #5).
- FC-036: unchanged. The check only withholds.
- FC-042: no shape is declared unsupported in this run.
- Not dismissed: FC-001, FC-002, FC-006, FC-016 (#5, #6).

## Architecture findings
#5 is the third failure of the same decision, whether a casing rename changes which member implements an interface member. It fails between the same components, the casing rename guard and the compiler's interface mapping (#2: the own type's mapping; #4: derived types; #5: the hierarchies of the other cascade members). This time the cause is structural in a different way. Every other safety check of `RenameWithoutConflictAsync` takes its region from the rename cascade, as its remarks state, but the new check is keyed on the reported symbol. The refusal by name that decision 10 chose is sound. Its input has to be the cascade, as SHARED-CASING-007 defines the reach of every check.

## Hints
- `HasInterfaceMemberOfNameInHierarchyAsync` runs `FindDerivedClassesAsync` for every reported method, property, or event of a class before the rename, also when the rename would be refused later. It costs one solution-wide search per fix computation, which is cheap next to the renamer. This is not a finding.
- The `RH2001` renamer follow-up candidate in `00-lead.md` is unchanged.
