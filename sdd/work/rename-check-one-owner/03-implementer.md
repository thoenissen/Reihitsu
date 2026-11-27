Role: implementer
Commit: fb2c45f
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict; the fix follows decision 2 of `00-lead.md` and the skeptic's mechanism (FC-044: the guard reconstructed the Renamer's edits from `SyntaxTree.GetChanges`, a heuristic diff).

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingRenameSafetyUtilities.cs` | `RewritesOtherCodeAsync` no longer reads `renamedTree.GetChanges(originalTree)`. It walks the tokens of each changed document (structured trivia included) before and after the rename side by side (`OnlyRenamesOrQualifies`): a token without an old name has to match kind and text; an identifier with an old name matches the first renamed identifier spelled with the new name or its own name, behind an optional qualifier (`FindQualifiedName`, `IsQualifierToken`: names, `this`, `base`, `global`, type arguments with predefined types, ending in `.` or `::`). New parameter `identifier` (the new name). Unused `Microsoft.CodeAnalysis.Text` using removed. Remarks rewritten for the new code (FC-031) | SHARED-CASING-009 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingCodeFixProviderBase{T}.cs` | The one caller passes `identifier` | SHARED-CASING-009 |

The check is a linear walk over the two token lists with no lookahead beyond the qualifier, so it is a cheap syntactic check of what the renamer produced, not a model of the differ. It stays strict where the old check was: a cast inserted in front of an argument (`box.Write((object)1)`) or a static-call rewrite (`Extensions.ToList(values)`) leaves a token without an old name changed, so those renames stay withheld (their pinned tests pass). It is slightly stricter than before in one respect: an insertion touching an old-name token that is not a qualifier ending in `.`/`::` (for example parentheses or a cast directly in front of a cascade reference) used to pass the diff check and is now refused; no test reaches such an input. A qualifier holding a nullable or tuple type argument is also refused (safe direction, rare).

Trivia: the walk compares tokens, not comment or whitespace trivia. The renamer runs with default `SymbolRenameOptions` (no renaming in comments or strings), so it changes no comment; whitespace the renamer writes around a qualifier is not code. Excluded code stays covered by `ExcludedCodeUtilities.HasExcludedCodeOccurrenceAsync`, unchanged.

## Policy owners
`rg -n "RewritesOtherCodeAsync|RenameChangesBindingAsync|OnlyRenamesOrQualifies" src --glob '*.cs'`:
- `CasingRenameSafetyUtilities.RewritesOtherCodeAsync` / `OnlyRenamesOrQualifies` — casing single fix and Fix All, one caller in `CasingCodeFixProviderBase<T>.RenameWithoutConflictAsync`.
- `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.RenameChangesBindingAsync` — the second owner, kept separate by decision 1 (unchanged).
No other copy of the text-diff check exists (`rg -n "GetChanges\(originalTree\)" src` finds nothing now).

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 2 files changed, 140 insertions(+), 24 deletions(-) (mostly XML documentation). No new type; three private helper methods.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~FixQualifies|FullyQualifiedName~FixAllQualifies|FullyQualifiedName~NoFixIsOfferedWhenAnAnonymousObjectMemberTakesItsNameFromTheField"` — Passed! Failed: 0, Passed: 40.
- `scripts/test.sh --configuration Debug --project analyzer` — Passed! Failed: 0, Passed: 12087 (includes every casing, RH4103 name-binding cast/static-call, and RH2001 test).
- `scripts/test.sh --configuration Debug --project architecture` — Passed! Failed: 0, Passed: 198.

## Disputed tests or scope
_None._ Before my change, the focused filter would fail the 14 rows the test implementer lists; I ran the build and focused tests after the change only (the handover's observed failures are recorded in `02-test-implementer.md`).

## Architecture findings
_None new._ The FC-044 dependence on the differ is removed; the two-owner finding is handled by decisions 1 and 2.

## Follow-up candidates
_None._
