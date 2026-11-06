Role: implementer
Commit: see the commit that adds this file (production change and handover in one commit)
Verdict: tests pass

I was called for the code part of lead decision 19. My prompt named that structural change, which the lead has already decided, and carried no expected outputs. I changed no test. The 3 tests that `25-test-implementer.md` lists as failing now pass. Its 3 passing tests still pass.

## What decision 19 asked for, and what the code does

I first implemented decision 19 literally. The guard took its old names from the cascade before the rename: the name of every cascade symbol with `CanBeReferencedByName`. That made the 3 failing tests pass. It also turned the RH4121 guard test red: `FixIsOfferedWhenAnExcludedBranchNamesTheDifferentlySpelledTypeParameterOfTheOtherPartOfAPartialMethod` withheld the fix.

The cause is that `SymbolFinder.FindReferencesAsync`, the source of the cascade since decision 17, returns the other part's type parameter `TItem` as a definition. The renamer leaves `TItem` as it is. So the reference search reaches more than the renamer renames. The premise of decisions 17 and 19 is that the cascade is "what the rename renames", and that premise is not exact (see *Architecture findings*).

The shipped guard reads the renamer's own result, so it predicts nothing:

- **The old names** are the referable names that cascade symbols carried before the rename and no longer carry after it. The code compares, as counts of names, the cascade before the rename with the cascade of the renamed symbol after it. The second cascade was already computed for the native-entry-point check.
  - A symbol the renamer leaves alone, such as `TItem`, keeps its name in both cascades and adds no old name.
  - A differently spelled symbol the renamer renames, such as the other part's `other` or `position`, adds its spelling.
  - An accessor or a constructor adds nothing, because it cannot be referred to by name.
- **The documents** are unchanged: every document that can refer to a symbol of the cascade before the rename. A superset there only adds documents to scan, which is the withholding direction.
- **The order of checks changed.** The excluded-code check moved from `PrepareRenameAsync`, before the renamer runs, to `RenameWithoutConflictAsync`, after the renamed cascade is read. Every check still returns `null` for an unsafe rename, so the order changes no outcome. The renamer now also runs for a rename that excluded code withholds; this is a cost change only.

This matches SHARED-CASING-007 as restated in `24-spec-author.md`:
- the old names are those of the symbols of the cascade that source can refer to by name;
- a symbol outside the cascade "which the renamer leaves as it is" adds no old name.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | New private `GetOldNames(cascade, renamedCascade)`: the multiset difference of the referable names before and after the rename. `HasExcludedCodeOccurrenceAsync` takes `ImmutableHashSet<string> oldNames` in place of `string oldName`. The call moved from `PrepareRenameAsync` to `RenameWithoutConflictAsync`, after the renamed cascade and the native-entry-point check. | SHARED-CASING-007 (old-name bullet), SHARED-FIX-003; decision 19 |
| same file | I corrected the remarks of `GetRenameCascadeAsync`: the search can reach a symbol the renamer leaves, such as a partial method's other-part type parameter. That widens the region only, and `GetOldNames` takes no name from such a symbol. I updated the summary and remarks of `HasExcludedCodeOccurrenceAsync` and the remarks of `RenameWithoutConflictAsync`. I re-read `PrepareRenameAsync`, which now rejects only the namespace collision before the renamer runs; its summary still holds. | FC-031 |

## Policy owners
```shell
rg -n "FindReferencesAsync|CanBeReferencedByName|HasExcludedCodeOccurrence" src --glob '!*.Test*/**'
# only CasingCodeFixProviderBase{T}: GetRenameCascadeAsync (cascade), GetOldNames (old names), HasExcludedCodeOccurrenceAsync (one call site)
```
The excluded-code guard has one owner. It no longer assumes that every cascade symbol carries the reported spelling.

## Tests run
```shell
scripts/build.sh --configuration Debug                                       # Build succeeded, 0 errors
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4105|FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4121"   # Passed: 52, Failed: 0
scripts/test.sh --configuration Debug --project analyzer                     # Passed: 6721, Failed: 0
scripts/test.sh --configuration Debug --project architecture                 # Passed: 115, Failed: 0
```
`scripts/format.sh` ran over the changed file. Its formatting is included.

## Disputed tests or scope
- I dispute no test.
- **Scope.** The change is one production file: one new private method, and one private method's parameter changed from a name to a set. There is no new public member and no new dependency.
- **Deviation from decision 19's wording.** The old names are not "the names of every cascade symbol that can be referred to by name" taken from the reference search alone. That reading over-withholds and fails the RH4121 test that `24-spec-author.md` wrote as the guard against exactly this. The old names come from the cascade before and after the rename (see above). The lead should confirm this reading.

## Architecture findings
The rename cascade (`GetRenameCascadeAsync`, read from `SymbolFinder.FindReferencesAsync`, decision 17) is a superset of what `Renamer.RenameSymbolAsync` renames: the input above shows the reference search returning the other part's `TItem`, which the renamer leaves alone. The cascade's documents and native entry points are safe as a superset. Before and after the rename the entry-point set compares the same symbols, so that check is also unaffected. A check that needs exactly what the renamer renames cannot use the cascade alone. This change reads the renamer's result for the old names instead.

The remarks of `GetRenameCascadeAsync` and the spec's cascade paragraph describe the search as "the symbols the rename renames". The spec author may want to word that as "the symbols Roslyn's reference search cascades to", or as "at least" the renamed symbols. That is the lead's decision. I made no spec change.

## Follow-up candidates
_None_ beyond those in `00-lead.md`.
