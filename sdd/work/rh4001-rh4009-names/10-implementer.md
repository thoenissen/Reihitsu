Role: implementer
Commit: 038379c
Verdict: tests pass

Issue thoenissen/Reihitsu#1085, lane behavior-change, repair round 1 (auditor 08 findings #1 and #2; lead decisions 9 and 10). My prompt carried no intended fix or hoped-for verdict beyond the findings; the auditor's suggestions were re-derived, not copied. No test was changed.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/NamespaceCasingHelper.cs` | New `GetFullNameParts(SyntaxNode)`: the `GetLocations` values of the names of every `BaseNamespaceDeclarationSyntax` that is or encloses the node, outermost first. The one owner of "the full namespace a node is declared in" | RH4009-003, RH7002-004 (finding #1) |
| `src/Reihitsu.Analyzer/Rules/Naming/RH4009NamespaceNotAllowedAnalyzer.cs` | `GetNamespaceName` and `GetFullNamespaceName` deleted; the comparison joins `GetFullNameParts(namespaceSyntax)` with `.` | RH4009-002/003 unchanged |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer.cs` | `GetTypeKey` takes its namespace names from `GetFullNameParts(typeDeclaration)` instead of its own ancestor chain | RH7002-004 unchanged |
| `src/Reihitsu.Analyzer/Rules/Naming/RH4001TypeNameShouldMatchFileNameHelper.cs` | New private `GetStem` (part before the first dot); `GetComparableFileName` and `GetRenamedFileName` both use it; the suffix is now `fileNameWithoutExtension.Substring(stem.Length)` | RH4001-002/008 unchanged (finding #2) |

Parity:
- RH7002: the old chain was `typeDeclaration.Ancestors()…SelectMany(GetLocations)…Select(Name)`; the new one uses `AncestorsAndSelf()`, and a type declaration is never itself a namespace declaration, so the sequence is identical for every input.
- RH4009: the previous head joined per declaration, then across declarations; the new code joins the flat sequence. Both give the same string whenever every declaration's name yields at least one identifier value. They differ only for a name `GetLocations` yields nothing for (an alias-qualified namespace name such as `namespace global::A`, a compile error) inside or around a nested declaration — not compiling code, and against `main` the flat join is exactly what a file-level declaration already did.
- RH4001: the stem and suffix are the same substrings as before (stem = up to the first dot; suffix = from the first dot, or empty).

Summaries of every changed method re-read (FC-031): `GetTypeKey`'s and `OnNamespaceDeclarationSyntax`'s still describe the code; new methods have their own.

## Policy owners
- `rg -n "GetNamespaceName|NamespaceCasingHelper|BaseNamespaceDeclarationSyntax>\(\)" src --type cs`: full namespace name now only in `NamespaceCasingHelper.GetFullNameParts` (callers RH4009, RH7002); RH4007/RH4008 use only `GetLocations` per identifier.
- `rg -n "IndexOf\('\.'\)|GetFileNameWithoutExtension" src/Reihitsu.Analyzer* --type cs`: stem only in `RH4001TypeNameShouldMatchFileNameHelper.GetStem`; `PartialMemberUtilities` uses `GetFileNameWithoutExtension` for generated-file suffixes, an unrelated policy.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 4 files changed, 46 insertions(+), 39 deletions(-) (was 2 files, +32/-19). Round delta: 4 files, +32/-38. No new type; one new internal member on a shared helper (`NamespaceCasingHelper.GetFullNameParts`, decided by the lead in decision 9).

## Deleted special cases
_N/A._

## Tests run
- `scripts/format.sh` on the four files — formatted 0 of 4.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Naming.RH4001TypeNameShouldMatchFileName|FullyQualifiedName~Naming.RH4009NamespaceNotAllowed"` — Passed: 96, Failed: 0.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7002FileMayOnlyContainASingleTopLevelType"` — Passed: 13, Failed: 0 (the class lives under `Reihitsu.Analyzer.Test.Design`).
- `scripts/test.sh --configuration Debug --project analyzer` — Passed: 12303, Failed: 0.
- `scripts/test.sh --configuration Debug --project architecture` — Passed: 198, Failed: 0.

## Disputed tests or scope
- RH7002's production file is now part of the diff (decision 9). Its test class sits in the `Design` folder/namespace although the rule is in the Organization range — pre-existing, not touched.

## Architecture findings
_None._ (Finding #1's structural cause is removed.)

## Follow-up candidates
_None._
