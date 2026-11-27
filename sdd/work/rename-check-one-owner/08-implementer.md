Role: implementer
Commit: b0b7bcd
Verdict: tests pass

Repair round 1, auditor finding #1 (`06-auditor.md`), lead decision 5. The prompt carried no intended fix, suspected cause, or hoped-for verdict; the repair follows decision 5 and the auditor's suggested change.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingRenameSafetyUtilities.cs` | `FindQualifiedName` no longer skips tokens from a token-kind list. The token at the reference's start is either the renamed identifier itself, or the first token of the left side of the innermost member access (simple), qualified name, alias-qualified name, or qualified cref that starts at that token and whose name is an identifier spelled with the old or new name; the left side has to be `this`, `base`, or a `TypeSyntax` (any type arguments: arrays, nullable, tuples, nested generics). The identifier's index is then looked up in the token list by token identity. New private `GetQualifierAndName`; `IsQualifierToken` deleted. Remarks of `OnlyRenamesOrQualifies` and `FindQualifiedName` describe the new code (FC-031) | SHARED-CASING-009 |

The check stays cheap and syntactic: one ancestor walk bounded by the nodes that start at the token, and a forward scan to the identifier. It reads the renamer's result instead of predicting which tokens a qualifier holds (FC-044). Cast-argument and static-call rewrites stay withheld: a cast or a changed call shape leaves a token that carries no old name changed, which the unchanged token walk refuses (pinned tests pass). Behavior on the auditor's "qualifier contains the name" member (`N.Order` around class `order`) is unchanged: the innermost node whose name is spelled like the old or new name wins, so it stays refused, as on `main`.

## Policy owners
`rg -n "IsQualifier|GetQualifierAndName|FindQualifiedName" src --glob '*.cs'`: only `CasingRenameSafetyUtilities` (one caller each). `rg -n "RenameSymbolAsync|RewritesOtherCodeAsync|RenameChangesBindingAsync" src`: the two owners kept separate by decision 1; both now read the qualifier from syntax.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 2 files changed, 171 insertions(+), 24 deletions(-). Round delta: 1 file changed, 64 insertions(+), 33 deletions(-). No new type; one private helper added, one deleted.

## Deleted special cases
_N/A._

## Tests run
- Before the change (recorded by the test implementer, `07-test-implementer.md`): 11 failing rows, 4 holding.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~FixQualifiesTheCallWithTheGenericBaseType|FullyQualifiedName~FixQualifiesAReferenceWithTheGenericBaseType"` — Passed! Failed: 0, Passed: 15.
- `scripts/test.sh --configuration Debug --project analyzer` — Passed! Failed: 0, Passed: 12102 (every casing, RH4103 cast/static-call, and RH2001 test).
- `scripts/test.sh --configuration Debug --project architecture` — Passed! Failed: 0, Passed: 198.

## Disputed tests or scope
_None._

## Architecture findings
_None new._ The token-kind prediction behind finding #1 is removed.

## Follow-up candidates
_None._
