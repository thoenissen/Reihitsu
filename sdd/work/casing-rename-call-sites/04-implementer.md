Role: implementer
Commit: 4e56156
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | `RenameWithoutConflictAsync` returns `null` (fix withheld, Fix All skips the candidate) when the rename rewrites code other than cascade references (R1) or when a reference outside the cascade binds to the renamed symbol afterwards (R2). New private helpers `RewritesOtherCodeAsync` and `CountReferenceLocationsAsync`. The summary of `RenameWithoutConflictAsync` names the new check | SHARED-CASING-009 (second paragraph), SHARED-FIX-003 |

How the two checks work:

- **R1 (`RewritesOtherCodeAsync`).** For every document the renamer changed, `renamedTree.GetChanges(originalTree)` gives the syntax differ's changes in original coordinates. Each change must lie on identifiers that carry an old cascade name: a replaced span may only overlap such identifiers, and an insertion (empty span) must touch one. Qualifying a cascade reference (the existing RH4002 test `FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture`, `order` → `N.Order`) therefore stays allowed, while `this.name`, `(object)1`, `Enumerable.ToList(values)`, `Outer.Log(...)`, `nameof(Guard.x)` are refused. A first version that compared token streams refused that existing RH4002 test, which SHARED-CASING-009 keeps ("a reference to a symbol of the rename cascade ... is not such a reference").
- **R2 (`CountReferenceLocationsAsync`).** The number of distinct reference locations (`SymbolFinder.FindReferencesAsync`, deduplicated by document and span) of the cascade before the rename must equal the number after it. A reference left as written that now binds to the renamed symbol adds a location; this covers the dependent-project test as well, because the search is solution-wide.

Both are cheap checks on data the renamer and the reference search already provide; no new type, no new mechanism beyond one extra reference search per rename.

## Policy owners
`rg -n "Renamer\.Rename|SyntaxTree.*GetChanges\(|FindReferencesAsync" src --glob '!*Test*'`: the casing rename safety decision has one owner, `RenameWithoutConflictAsync` (single fix through `HasRenameConflictAsync`, Fix All through `ApplyFixAllAsync` and `CandidatesConflictAsync`). `ApplyCodeFixAsync` (line 141) calls the renamer only after `HasRenameConflictAsync` approved the same rename. The other renamer user, `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider`, is a different rule (see Follow-up candidates).

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 1 file changed, 87 insertions(+), 2 deletions(-). No new type.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"` → `Passed! - Failed: 0, Passed: 729` (the 36 previously failing tests pass; no assertion changed).
- `scripts/build.sh` (Release) → 0 warnings, 0 errors.
- `scripts/test.sh --project analyzer` → `Passed! - Failed: 0, Passed: 9546`.
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"` (Release, after final edits) → `Passed! - Failed: 0, Passed: 729`.

## Disputed tests or scope
_None._ The R1 check relies on Roslyn's syntax differ (`SyntaxTree.GetChanges`) giving changes local to the edited tokens; should it ever return one whole-document change, the check refuses (withholds), never accepts, so the failure mode is a withheld fix, not an unsafe one.

## Architecture findings
_None._

## Follow-up candidates
- `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider` (`src/Reihitsu.Analyzer.CodeFixes/Rules/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs:100`) also calls `Renamer.RenameSymbolAsync`; whether it can rewrite or rebind references to other symbols was not checked. Unverified; the lead decides whether it is worth a skeptic look.
