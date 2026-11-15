Role: implementer
Commit: 8847090 (production change; this handover in the next commit)
Verdict: tests pass

Repair round 3: decisions 14 and 15 of `00-lead.md`, auditor findings 7 and 8 in `25-auditor.md`, SHARED-CASING-009 as clarified in `26-spec-author.md`, tests of `27-test-implementer.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming decision 14's allow-list, which the spec now states and this change follows.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | `HasInterfaceMemberOfNameInHierarchyAsync`: the deny-list kind filter (`SymbolKind.Method/Property/Event`, minus local functions) is replaced by the allow-list the spec names: `symbol is not (IPropertySymbol or IEventSymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary })`. Constructors, static constructors, finalizers, operators, conversions, accessors, explicit method implementations, and local functions now add no hierarchy, so type renames (t1, t2, t5) are fixed again. A positional record parameter is still refused through the property in its cascade (decision 15; no code change needed). | SHARED-CASING-009 (interface paragraph), SHARED-FIX-003 |
| same | FC-031: the method's summary and remarks describe the allow-list, the kinds not checked, the type-rename and positional-parameter consequences; the summary and remarks of `RenameWithoutConflictAsync` and the call-site comment say "cascade property, event, or ordinary method" instead of "cascade member" / "method, property, and event". | — |

## Policy owners
- "Kinds that can implement an interface member by name": `rg -n "MethodKind.Ordinary" src --glob '!*Test*'` → the new condition; `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase.cs:80` and `RH2002AsyncVoidShouldNotBeUsedAnalyzer.cs:52` filter ordinary methods for unrelated decisions (region placement, async void). No shared owner; the condition is a one-line pattern.
- Interface lookup in the hierarchy: unchanged, sole owner as in `22-implementer.md`.

## Production diff size
- Run: `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` → 1 file changed, 155 insertions(+), 6 deletions(-) (152/6 before this round). No new type, no new member.
- Round delta (8847090): 16 insertions(+), 13 deletions(-); code is one condition (3 lines → 2), the rest XML documentation and the call-site comment.

## Deleted special cases
_N/A._ (The local-function condition of round 2 is subsumed by the allow-list and removed.)

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"` → `Passed! - Failed: 0, Passed: 762`. The 3 failing tests of `27-test-implementer.md` (RH4002 t1, t2; RH4003 t5) pass; the RH4120 pair, RH4104 local-function tests, RH4103 e1/e3, RH4114 p1/p3, RH4102 v1 and the other controls stay green. No assertion changed.
- `scripts/build.sh` (Release) → 0 warnings, 0 errors.
- `scripts/test.sh --project analyzer` (Release) → `Passed! - Failed: 0, Passed: 9610` (a first run reported 9072 after the kernel killed a process for memory; the rerun completed in full).
- `scripts/format.sh` on the changed file → no change.

## Disputed tests or scope
_None._

## Architecture findings
_None._ Finding 7's structural cause (a deny-list that depends on what the renamer cascades to) is removed by the allow-list of decision 14.

## Follow-up candidates
_None_ new. The RH2001 renamer candidate stays in `00-lead.md`.
