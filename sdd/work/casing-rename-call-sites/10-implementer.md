Role: implementer
Commit: 8c7e443 (production change; this handover amended in the next commit)
Verdict: tests pass

Repair round 1. The prompt carried no intended fix, suspected cause, or hoped-for verdict; it named decisions 6 and 7 of `00-lead.md`, which this round implements as stated there.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | **F1 (decision 6).** `RenameWithoutConflictAsync` computes the renamed cascade and `GetOldNames(cascade, renamedCascade)` before the R1/R2 checks and passes those old names to `RewritesOtherCodeAsync`, which no longer builds its own name set (parameter `ImmutableArray<ISymbol> cascade` replaced by `ImmutableHashSet<string> oldNames`). The excluded-code and caller-argument checks reuse the same `oldNames` value (`oldNames.Add(identifier)`), so the cascade's old-name policy has one statement and one computation per rename. | SHARED-CASING-009 (cascade reference matched by symbol, not spelling) |
| same | **F2 (decision 7).** New private `CountImplementedInterfaceMembers(ISymbol)`: the number of members of the containing type's `AllInterfaces` whose `FindImplementationForInterfaceMember` is the symbol. The fix is withheld when the count for the renamed declaration differs from the count before the rename. The compiler's own re-implementation rule decides, so a type that does not list the interface itself keeps the inherited implementation and the count stays (controls pass). Cheap: one lookup per interface member of the renamed member's type, no new type. | SHARED-CASING-009 (interface paragraph), SHARED-FIX-003 |
| same | XML summary/remarks of `RenameWithoutConflictAsync` and `RewritesOtherCodeAsync`, and the inline comment at the check, updated to the new code (FC-031). | — |

Both repairs are cheap checks; neither needed more.

## Policy owners
- Old names of the cascade: `rg -n "CanBeReferencedByName\)" src/Reihitsu.Analyzer.CodeFixes` → only `GetOldNames` (`CasingCodeFixProviderBase{T}.cs:405`, `:410`). The private copy in `RewritesOtherCodeAsync` is gone.
- Which member implements an interface member: `rg -n "FindImplementationForInterfaceMember" src --glob '!*Test*'` → `CountImplementedInterfaceMembers` (new) and `InterfaceImplementationUtilities.GetImplementedInterfaceNames` (`src/Reihitsu.Analyzer/Core/InterfaceImplementationUtilities.cs:61`, private). The latter answers a different question (the names of the interfaces whose region an implementation belongs in, SHARED-REGGROUP-004; it short-circuits on explicit implementations and returns distinct interface names), so it is not a second owner of the rename decision; both read the compiler's mapping directly rather than modelling it.

## Production diff size
- Run: `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` → 1 file changed, 114 insertions(+), 5 deletions(-) (round 0: +87/−2). No new type. This is slightly past the ~100 production lines the lead named; about 30 of the 114 are XML documentation and comments, and nothing goes beyond the two repairs.
- Round delta: 1 file changed, 41 insertions(+), 17 deletions(-) (of which ~20 lines are XML documentation).

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"` → `Passed! - Failed: 0, Passed: 741` (the 7 failing tests of `09-test-implementer.md` pass; the must-stay-green controls, including RH4002 `FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture`, RH4105 `FixRenamesTheDifferentlySpelledParameterInBothPartsOfAPartialMethodWhenNoExcludedBranchNamesIt`, RH4121 `FixIsOfferedWhenAnExcludedBranchNamesTheDifferentlySpelledTypeParameterOfTheOtherPartOfAPartialMethod`, and the hiding/interface controls, pass). No assertion changed.
- `scripts/build.sh` (Release) → 0 warnings, 0 errors.
- `scripts/test.sh --project analyzer` (Release) → `Passed! - Failed: 0, Passed: 9558`.

## Disputed tests or scope
_None._ Note: the R1 verdict for a non-cascade reference adjacent to a cascade token (c1) still depends on how Roslyn's syntax differ merges spans, as the auditor's hint says; it fails safe (withholds), and c1 is now pinned by a test.

## Architecture findings
_None_ beyond decision 6, which this round resolves (the R1 guard now takes its names from `GetOldNames`).

## Follow-up candidates
_None_ new. The RH2001 renamer candidate stays in `00-lead.md`.
