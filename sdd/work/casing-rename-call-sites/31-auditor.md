Role: auditor
Commit: 067e3dca95cc7d3904dd94e71ff7eefdb2c2a2b0
Verdict: pass, text cleanup

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

Repair delta against `27789ab` (`25-auditor.md`), decisions 14 and 15 of `00-lead.md`; repair round 3. Head `067e3dc` (checkout verified, clean tree). Base `origin/main` = `3b3639e` (`git ls-remote` matches the prompt), an ancestor of the head. The delta is what `26`–`30` describe and nothing more: the SHARED-CASING-009 interface paragraph, its Why and two examples (861e6b8); five tests in RH4002, RH4003, RH4120 (ff34514); in `CasingCodeFixProviderBase{T}.cs` (8847090) the kind filter of `HasInterfaceMemberOfNameInHierarchyAsync` becomes `symbol is not (IPropertySymbol or IEventSymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary })`, plus summaries, remarks, and the call-site comment. `scripts/lane-check.sh bug-fix` reports `confirmed`. Scope did not grow; the spec change is the clarification decisions 14 and 15 ask for. No `TODO` and no tracker number among the added lines.

Status of earlier findings:
- **#1–#6 closed** (untouched by the delta except #6's local-function condition, which the allow-list subsumes; RH4104 l1/l2 stay green per `28-implementer.md`).
- **#7 closed** for every member it listed: t1, t2 (RH4002), t5 (RH4003) are fixed and tested; t4 (RH4010, also as a positional record `processor(int Value)`) and t7 (RH4011) are fixed on the head (fixtures below). The class beyond the listed members holds too: a class with a declared constructor, static constructor, finalizer, user-defined operator, and conversion operator (k1) is fixed, every one of them renamed with the type.
- **#8 closed**: the spec now states that a positional record parameter is withheld through its property, and RH4120 `NoFixIsOfferedWhenTheParametersPropertyWouldImplementAnInterfaceProperty` pins it next to its fixed control. Variants hold: r1 (`Person(string name) : Base(name), INamed`) and r2 (`Child(...) : Person(...), INamed`, the property inherited by a derived record that lists the interface) are withheld.

Evidence: fixtures in the session scratchpad (`a31/`), outside the repository, run with `scripts/apply-fix.sh` on the head; every result identical under LF and CRLF.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 9 | low | `sdd/specs/shared/casing.md`, SHARED-CASING-009 Why: "A constructor, finalizer, operator, or local function belongs to no interface mapping" | FC-031 | – | same requirement | Text only. Since C# 11 a user-defined operator can implement a `static abstract` operator of an interface (`interface IAdd<T> where T : IAdd<T> { static abstract T operator +(T a, T b); }`, implemented by `public static C operator +(C a, C b)`), so the sentence is wrong for operators. The conclusion it supports still holds: an operator implements by its operator token, which no rename changes, so not checking it withholds nothing that should be withheld. The code remark says "kinds that can implement an interface member by their name", which is accurate. Cannot reach compiled behavior, diagnostics, or rule metadata. | Reword, for example: "A constructor, finalizer, or local function belongs to no interface mapping, and an operator implements an interface operator by its operator token, which a rename does not change". |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Interface name check: which symbols of the cascade | Properties, events, `MethodKind.Ordinary` methods of `prepared.Value.Cascade` with a containing type | SHARED-CASING-009 as clarified (properties, events, ordinary methods; every other kind named as not checked); the decision "a symbol that can implement an interface member by its name" | Yes. Constructors and the other type members renamed with a type: not checked (t1, t2, t4, t5, t7, k1 fixed). Accessors: their property or event is in the cascade. Explicit method implementations: covered through the interface member's hierarchy, which `FindImplementationsAsync` makes transitive — x1 (explicit implementer `C`, `D : C, IBar` with `IBar.Process`) and x2 (rename of an implicit implementer while another class implements explicitly and its derived class lists `IBar`) are withheld; control x3 (`IBar.Other`) is fixed. Positional record parameter: through its property (RH4120 test, r1, r2) | holds (#7, #8 closed) |
| Interface name check: which types, name match | Unchanged since `19-auditor.md` | Unchanged | Yes | holds |
| R1 / R2 and the other checks of `RenameWithoutConflictAsync` | Unchanged | – | – | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which symbols can implement an interface member by name | The condition at `CasingCodeFixProviderBase{T}.cs:1132` only. `rg MethodKind` in the file finds `:833` (`IsCompilerLookupMethodRename`, a different question); `MethodKind.Ordinary` elsewhere (`NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase.cs:80`, `RH2002AsyncVoidShouldNotBeUsedAnalyzer.cs:52`) answers unrelated questions | One owner |
| Whether a rename meets an interface member in the type hierarchy | `HasInterfaceMemberOfNameInHierarchyAsync` only | One owner, unchanged |
| The rename cascade | `GetRenameCascadeAsync` only | One owner, unchanged |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH4002 `FixRenamesAClassToTheNameOfAMemberOfAnInterfaceItImplements` (t1), `…ThatADerivedClassImplements` (t2); RH4003 `FixRenamesAStructToTheNameOfAMemberOfAnInterfaceItImplements` (t5) | A type rename is not withheld by the interface check although its cascade holds its constructor; exact output, including the retargeted base-list reference in t2 | Withheld fix, or any other rewrite | `Verify(src, fixed)` (single and Fix All) | adequate; record and record struct are not tested but share the same constructor `MethodKind` and call site (fixtures t4, t7 fixed) |
| RH4120 `NoFixIsOfferedWhenTheParametersPropertyWouldImplementAnInterfaceProperty` | A positional parameter is withheld when its compiler-declared property would implement an interface member | A fix offered | `Verify(src, src)` | adequate; it fails if the allow-list ever drops properties reached through a parameter's cascade |
| RH4120 `FixRenamesTheParameterWhenTheInterfaceHasNoMemberOfTheNewName` | The control: no interface member of the new name, fix applied | Withheld fix or another rewrite | `Verify(src, fixed)` | adequate |

## Failure classes dismissed
The delta changed one kind filter and its documentation. The dismissals of `07-`, `13-`, `19-`, and `25-auditor.md` hold for everything it does not reach (FC-004/005/007–014/018–023/025–030/032–035/037–043). Re-checked for the delta:
- FC-001, FC-002: the filter's span now matches the spec's allow-list and the decision; every kind the deny-list let through is excluded, and every kind it dropped is covered by its property, event, or interface member (guard-scope table).
- FC-003: both sides of the type boundary (t1/t2/t5 fixed next to e1/e3/p1/p3/v1 withheld) and of the parameter boundary (RH4120 pair) are tested.
- FC-006, FC-016: the cascade remains the input; no member is checked against a different region.
- FC-015: one owner.
- FC-017: see the assertion table.
- FC-024: explicit implementations of a method are no longer checked themselves; x1 and x2 show the interface member's hierarchy covers their types and derived types.
- FC-031: the summaries and remarks of `RenameWithoutConflictAsync` and `HasInterfaceMemberOfNameInHierarchyAsync` and the call-site comment describe the allow-list; the spec Why has one wrong clause (#9).
- FC-036: the check only withholds.
- FC-042: no shape is declared unsupported in this run.

## Architecture findings
_None._ #7's structural cause, a deny-list over whatever the renamer cascades to, is replaced by an allow-list of the kinds that can implement an interface member by name; k1 (every other member kind a type declares) shows it no longer depends on the cascade's contents.

## Hints
- SHARED-CASING-009's interface paragraph opens with "a method, property, or event of the rename cascade" and narrows to ordinary methods only several sentences later ("The members of the cascade this paragraph checks are …"). The paragraph is consistent as written; saying "an ordinary method" in the opening sentence would read more directly. Not a finding.
- The `RenameWithoutConflictAsync` summary has a short orphan line (`/// ordinary method has, no`) left by the edit; cosmetic, the text is accurate.
- The `RH2001` renamer follow-up candidate in `00-lead.md` is unchanged.
