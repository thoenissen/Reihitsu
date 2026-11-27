Role: implementer
Commit: 9281acb
Verdict: tests pass

Issue thoenissen/Reihitsu#1085, lane behavior-change. My prompt carried no intended fix or hoped-for verdict. No test was changed.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Naming/RH4001TypeNameShouldMatchFileNameHelper.cs` | `StripTimestampPrefix` replaced by `GetTimestampPrefixLength` (0 or 15), the one prefix predicate; `GetComparableFileName` strips that length, and `GetRenamedFileName` puts the same prefix, as written, in front of the expected stem. The collision check of the code fix reads `GetRenamedFileName` through `GetNewFileName` and follows unchanged | RH4001-008, RH4001-006 (lead decision 8: fix keeps exactly what the analyzer strips) |
| `src/Reihitsu.Analyzer/Rules/Naming/RH4009NamespaceNotAllowedAnalyzer.cs` | New `GetFullNamespaceName`: the `GetNamespaceName` values of every `BaseNamespaceDeclarationSyntax` in `AncestorsAndSelf`, outermost first, joined by `.`; the comparison uses it, the location stays `namespaceSyntax.Name` | RH4009-003 (RH4009-002 unchanged for a file-level declaration, whose full name is its own name) |

Summaries of every changed method were updated (FC-031). The mixed file-scoped/block-scoped shapes (CS8955, lead decision 4) also get the joined name; they do not compile and are unspecified.

## Policy owners
- `rg -n "IsDigit|TimestampPrefix" src --type cs`: the prefix predicate has one owner, `RH4001TypeNameShouldMatchFileNameHelper.GetTimestampPrefixLength`; the other `IsDigit` hits (`CasingUtilities`, RH4005 fix) are unrelated casing checks.
- `rg -n "Ancestors.*NamespaceDeclaration" src --type cs`: no other full-namespace helper; the name value stays with `GetNamespaceName` → `NamespaceCasingHelper.GetLocations`.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 2 files changed, 32 insertions(+), 19 deletions(-). No new type.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Naming.RH4001TypeNameShouldMatchFileNameAnalyzerTests|FullyQualifiedName~Naming.RH4009NamespaceNotAllowedAnalyzerTests"` — Passed: 96, Failed: 0 (the 19 red tests of 04 now green).
- `scripts/test.sh --configuration Debug --project analyzer` — Passed: 12303, Failed: 0.
- `scripts/test.sh --configuration Debug --project architecture` — Passed: 198, Failed: 0.
- `scripts/format.sh` on both files — formatted 0 of 2.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
