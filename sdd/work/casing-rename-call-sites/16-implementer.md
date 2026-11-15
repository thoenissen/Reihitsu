Role: implementer
Commit: 11d3dd8 (production change; this handover in the next commit)
Verdict: tests pass

Architecture-finding repair (decision 10 of `00-lead.md`, auditor finding #4 in `13-auditor.md`, SHARED-CASING-009 as clarified in `14-spec-author.md`). The prompt said decision 10 is a refusal by a cheap name check that replaces the interface-implementation count; that is what decision 10 and the spec state, so I followed it. It carried no suspected cause or hoped-for verdict beyond that.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | `CountImplementedInterfaceMembers` and its before/after comparison removed. New private `HasInterfaceMemberOfNameInHierarchyAsync(solution, symbol, identifier, ct)`: for a renamed method, property, or event, it collects the containing type plus, for a class, `SymbolFinder.FindDerivedClassesAsync`, and, for an interface, `FindImplementationsAsync` (transitive: derived classes of implementers and implementers of derived interfaces included) and `FindDerivedInterfacesAsync`; it returns true when any of their `AllInterfaces` has a member named the new name (`GetMembers(identifier)`, any member kind). `RenameWithoutConflictAsync` calls it before `Renamer.RenameSymbolAsync`, so a refused rename costs no rename. Fields, parameters, locals, types, and type parameters return false (spec: outside the paragraph). Structs have no derived types, so only their own interfaces count. | SHARED-CASING-009 (interface paragraph), SHARED-FIX-003 |
| same | Summary of `RenameWithoutConflictAsync`, the inline comment at the reference check (no longer mentions interfaces), and the new method's summary and remarks describe the new code (FC-031). | — |

## Policy owners
- Interface member lookup by name in a type hierarchy: `rg -n "FindImplementationForInterfaceMember|FindDerivedClassesAsync|FindImplementationsAsync|FindDerivedInterfacesAsync" src --glob '!*Test*'` → only the new method, plus `InterfaceImplementationUtilities.GetImplementedInterfaceNames` (`src/Reihitsu.Analyzer/Core/InterfaceImplementationUtilities.cs:61`), which answers a different question (region grouping, SHARED-REGGROUP-004), as in `10-implementer.md`. No second owner of the rename decision.

## Production diff size
- Run: `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` → 1 file changed, 138 insertions(+), 4 deletions(-). No new type.
- Round delta (11d3dd8): 1 file changed, 45 insertions(+), 20 deletions(-), net +25 lines. About 12 of the added lines are the new method's XML documentation; the rest is the call site (5 lines) and the hierarchy collection (three `SymbolFinder` calls, which the count did not need). See *Disputed tests or scope*.

## Deleted special cases
_N/A._ (No shape is declared unsupported. The count `CountImplementedInterfaceMembers` is removed and replaced, as decision 10 asks.)

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"` → `Passed! - Failed: 0, Passed: 750`. The 9 failing tests of `15-test-implementer.md` pass; its controls (`FixRenamesAMethodToTheNameOfAMethodOfAnInterfaceOutsideItsTypesHierarchy`, `FixRenamesAFieldToTheNameOfAMethodOfAnInterfaceThatADerivedTypeImplements`, `FixRenamesAMethodThatADerivedTypeUsesToImplementASourceInterface`, the a3 tests for RH4103/RH4114/RH4102) and the controls of `09-test-implementer.md` pass. No assertion changed.
- `scripts/build.sh` (Release) → 0 warnings, 0 errors.
- `scripts/test.sh --project analyzer` (Release) → `Passed! - Failed: 0, Passed: 9598`.

## Disputed tests or scope
- **Size.** The prompt asked that the production diff not grow past where decision 9 left it (+114/−5). It is now +138/−4: net +24 over decision 9. The count was 8 lines because it read one type's mapping; the name check has to find the derived, implementing, and inheriting types of the solution, which takes three `SymbolFinder` calls and a type switch. Still one file, no new type, no new public member; every added line serves the refused class. Narrowing the interface-as-declaring-type arm (the spec author's *Internal choices* offered it) would save about 4 lines and drop two passing tests' basis in the spec, so I did not take it. Lead decides.

## Architecture findings
_None._ Decision 10 is resolved: the refusal reads no interface mapping, so the derived-type mapping class (b1, b2, b4–b6) has no counting to get wrong.

## Follow-up candidates
_None_ new. The RH2001 renamer candidate stays in `00-lead.md`.
