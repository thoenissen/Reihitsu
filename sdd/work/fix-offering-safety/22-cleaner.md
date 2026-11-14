Role: cleaner
Commit: see git log (subject ends `[skip ci]`)
Verdict: clean

## Formatted
_None._ Comment-only change; no code touched.

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| Finding #4 (comment) | `src/Reihitsu.Analyzer.CodeFixes/Base/ReihitsuCodeFixProviderBase.cs` class summary; `src/Reihitsu.ArchitectureTests/Analyzer/FixAllProviderCreationTests.cs` class summary | Both summaries now name the casing Fix All path: it asks `HoldsHidingCarrierAsync` for each changed document instead of using `CreateFixAllProvider`. |

## Unresolved
_None._ Note: `--strict-docs` reports BEHAVIORAL for both files because the base class is public and its summary changed. The auditor argued that this is not public API documentation, since no XML file is packed and no assembly is a public API (AGENTS.md, Supported inputs). The strict verdict is therefore not a compiled-behavior finding. The lead decides whether it is acceptable.

## Checks
- `scripts/verify-text-only.sh --strict-docs --base 970f447c5db57c04654f770b3a8ec389282772ec --head worktree`: `TEXT-ONLY PROOF: FAIL — ... 2 of 4 changed file(s) carry compiled behavior; xml-doc impact: 2 file(s), including public API documentation`
- Same without `--strict-docs`: `TEXT-ONLY PROOF: PASS — base=970f447c head=worktree — 4 changed file(s) (2 non-compiled, 2 C# comment/layout-only); xml-doc impact: 2 file(s), including public API documentation`. Both C# files show "changed trivia: documentation" with identical tokens.
