Role: implementer
Commit: see the commit that adds this file (production change and handover in one commit)
Verdict: tests pass

Called for the code part of lead decision 17: the rename cascade that every check uses is read from Roslyn, not modeled by relation kind. My prompt named that structural change, which is the lead's recorded decision, and carried no expected outputs. I changed no test. The 3 tests that `19-test-implementer.md` lists as failing now pass, and so does its passing test.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | `GetRenameCascadeAsync` no longer walks overrides and interface implementations by hand (`OverriddenMethod`/`FindOverridesAsync`/`FindImplementationsAsync`/`FindImplementedInterfaceMembersAsync`). It now takes the definitions of `SymbolFinder.FindReferencesAsync(symbol, solution)`, the cascading reference search the renamer uses to find what it renames, keeps those with a source location, and puts the declared symbol first. The other part of a partial method and its type parameters and parameters now enter the cascade, so the excluded-code check scans the other part's document. The native-entry-point check takes the same set, before and after the rename, unchanged in shape. Callers (`PrepareRenameAsync`, `RenameWithoutConflictAsync`) are unchanged. | SHARED-CASING-007 (the one cascade statement), SHARED-CASING-009, SHARED-FIX-003; decision 17 |
| same file | The summary and remark of `GetRenameCascadeAsync` describe the new determination; relation kinds appear only as examples, as in SHARED-CASING-007. Metadata members are now left out of the set (the spec's "not part of the cascade"); before they were in it with no documents, so the excluded-code region is unchanged by that, and the native-entry-point comparison drops them on both sides symmetrically. The other summaries that mention the cascade (`HasExcludedCodeOccurrenceAsync`, `GetNativeEntryPoints`, `HasAddedCompileErrorsAsync`, `RenameWithoutConflictAsync`, `PrepareRenameAsync`) were re-read and still describe the code (FC-031). | FC-031 |

## Policy owners
```shell
rg -n "FindReferencesAsync|FindOverridesAsync|FindImplementationsAsync|FindImplementedInterfaceMembersAsync" src --glob '!*.Test*/**'
# before: CasingCodeFixProviderBase{T}.GetRenameCascadeAsync only (the three hand-modeled relations)
# after:  CasingCodeFixProviderBase{T}.GetRenameCascadeAsync only (FindReferencesAsync); no hand-written model of the renamer's reach remains
```
The renamer (truth) and the reference engine the cascade now reads are the same Roslyn search; SHARED-CASING-007 states the cascade once (`18-spec-author.md`).

## Tests run
```shell
scripts/build.sh --configuration Debug                                                                    # Build succeeded, no warnings
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4121"   # Passed: 26, Failed: 0
scripts/test.sh --configuration Debug --project analyzer                                                  # Passed: 6715, Failed: 0
scripts/test.sh --configuration Debug --project architecture                                              # Passed: 115, Failed: 0
```
`scripts/format.sh` over the changed file: no change.

## Disputed tests or scope
- No test is disputed.
- Scope: one production file, one private method body (net 37 lines removed). No new public member, no new dependency.
- The reference search cascades further than the old model (for example a type to its constructors, a property to its accessors, a record positional parameter to its property). Each extra symbol can only add documents to the excluded-code scan, which is the withholding direction; every existing offered-fix test stays green.
- Cost: `FindReferencesAsync` searches every reference in the solution, twice per rename evaluation (before and after), where the old model ran only the hierarchy searches. This adds to the cost hint of `17-auditor.md` (decision 18 records it in the PR's review notes). Code fixes run on demand.

## Architecture findings
_None_ new. Decision 17's finding (the base modeled the renamer's reach) is addressed: the cascade is read from Roslyn's own cascading search, so a relation the renamer applies cannot be missing from a hand-written list.

## Follow-up candidates
_None_ beyond those in `00-lead.md`.
