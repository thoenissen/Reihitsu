Role: implementer
Commit: a4fc274 (production change; this handover in the next commit)
Verdict: tests pass

Repair round 2: decisions 12 and 13 of `00-lead.md`, auditor findings 5 and 6 in `19-auditor.md`, SHARED-CASING-009 as clarified in `20-spec-author.md`, tests of `21-test-implementer.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the decisions, which this change follows.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | `HasInterfaceMemberOfNameInHierarchyAsync` takes the rename cascade (`prepared.Value.Cascade`) instead of the reported symbol and runs the unchanged per-symbol name check for every method, property, and event of the cascade, returning true on the first hit (decision 12, finding 5: e1, e3, p1, p3, v1). The kind filter now also skips `IMethodSymbol { MethodKind: MethodKind.LocalFunction }` (decision 13, finding 6: l1, l2). The call in `RenameWithoutConflictAsync` passes the cascade; the check still runs before `Renamer.RenameSymbolAsync`. | SHARED-CASING-009 (interface paragraph), SHARED-CASING-007, SHARED-FIX-003 |
| same | FC-031: the summary of `RenameWithoutConflictAsync` now says "a member of an interface in the type hierarchy of a cascade member", and its remarks list the interface check among the checks that take their region from `GetRenameCascadeAsync` ("the interface members in the type hierarchy of every cascade method, property, and event"), so the remark the auditor called overstated holds again. The call-site comment and the summary, `cascade` parameter, and remarks of `HasInterfaceMemberOfNameInHierarchyAsync` describe the cascade-wide check and the local-function exemption. | — |

The cascade can also hold accessors, parameters, and type parameters: parameters and type parameters fail the kind filter; an accessor's containing type is that of its property, which the property already contributes, so it only repeats a hierarchy.

## Policy owners
- Interface member lookup by name in a type hierarchy: `rg -n "FindDerivedClassesAsync|FindImplementationsAsync|FindDerivedInterfacesAsync|FindImplementationForInterfaceMember" src --glob '!*Test*'` → only `HasInterfaceMemberOfNameInHierarchyAsync`; `InterfaceImplementationUtilities.GetImplementedInterfaceNames` answers a different question (SHARED-REGGROUP-004), as in `16-implementer.md`.
- Local-function exemption: `rg -n "MethodKind.LocalFunction" src --glob '!*Test*'` → the new condition; `IsCompilerLookupMethodRename` (same file, `:832`, compiler-lookup names, a different decision); `ExcludedCodeUtilities.cs:156`; RH4122/RH4123 analyzers. No shared owner of "can this symbol implement an interface member"; the condition is a one-line pattern.

## Production diff size
- Run: `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` → 1 file changed, 152 insertions(+), 6 deletions(-). No new type, no new member (one private method's parameter changed from `ISymbol` to `ImmutableArray<ISymbol>`).
- Round delta (a4fc274): 39 insertions(+), 27 deletions(-), net +12; ignoring whitespace (`git diff -w`) 27/15. Most of the raw delta is the existing body re-indented into the `foreach`; the new code is the loop, the local-function condition, and the `return true`/`return false`; the rest is XML documentation.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"` → `Passed! - Failed: 0, Passed: 757`. The 7 tests of `21-test-implementer.md` pass; their controls (b1–b6, a3, the interface-as-declaring-type tests, `FixRenamesAMethodToTheNameOfAMethodOfAnInterfaceOutsideItsTypesHierarchy`, `FixRenamesAMethodThatADerivedTypeUsesToImplementASourceInterface`, the existing RH4104 fix tests) stay green. No assertion changed.
- `scripts/build.sh` (Release) → 0 warnings, 0 errors.
- `scripts/test.sh --project analyzer` (Release) → `Passed! - Failed: 0, Passed: 9605`.
- `scripts/format.sh` on the changed file → no change.

## Disputed tests or scope
_None._ Size: the round adds net +12 lines over decision 11's +138/−4 (now +152/−6), all in the method decision 12 names; the check stays a name lookup per cascade member, with one `SymbolFinder` hierarchy search per cascade method, property, or event of a class or interface.

## Architecture findings
_None._ Finding 5's structural cause (a check keyed on the reported symbol while every other check takes the cascade) is removed: the interface check now takes its region from the cascade like the others.

## Follow-up candidates
_None_ new. The RH2001 renamer candidate stays in `00-lead.md`.
