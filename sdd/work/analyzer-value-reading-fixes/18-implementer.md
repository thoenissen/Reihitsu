Role: implementer
Commit: e48df40 (head the change was made on; production change and this handover are in the commit that adds this file)
Verdict: tests pass

Repair round 3, lead decision 13 (auditor 16 findings #3, #4; tests from 17), thoenissen/Reihitsu#1289 only. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 13. No test, assertion, input, or expected output was changed.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Design/RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs` | Removed `SkipCSharpBlock` (the C# lexing of `@code`/`@functions` blocks, with its `@*` and `@<` stops) and the now unused `using Microsoft.CodeAnalysis.CSharp`. The pattern matches only `@code` again (`\G@code\s*\{`, field renamed `_codeDirectivePattern`): `@functions` was matched only to skip its block. A line-starting `@code {` is recorded and the scan moves on by one character, so a block's content is scanned like the rest of the file, as on `main`. `FindCodeDirectives` summary rewritten: C# is not lexed, so a line-starting directive in a C# comment or literal is reported. Kept: line-start-only directives (Q3), line-starting `@*`/`<!--`, `@@` | RH2109-001 (as narrowed in effect by decision 13), decision 13 |

Nothing added beyond the removal; the remaining helpers (`StartsLine`, `StartsWith`, `SkipPast`) are all still used.

## Policy owners
- `rg -n "@code|<!--|ParseTokens" src --glob '!*Test*'` — Razor directive and comment scanning lives only in RH2109; RH2109 no longer calls `ParseTokens`. `StartsLine` matches elsewhere are unrelated same-named members (formatter/core layout helpers).

## Production diff size
- `git diff --shortstat c2866ea -- src ':(exclude,glob)src/*Test*/**'`: 5 files changed, 134 insertions(+), 15 deletions(-) (was 191/15 after round 2).
- This round: 1 file, +8/−65.
- RH2109 against `c2866ea`: +98/−8 gross; 56 added lines are neither blank nor `///` (was 95 after round 2). No new type.

## Deleted special cases
_N/A._ (No shape declared unsupported; decision 13 removes the block lexing as a refused class.)

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests"` — Passed 53/53 (16 failing before).
- `scripts/test.sh --configuration Debug --project analyzer` — Passed 8612/8612.
- `scripts/build.sh --configuration Debug` — 0 warnings, 0 errors. `scripts/format.sh` on the file — formatted 0 of 1.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None_ beyond the RH2109 entry already in `00-lead.md` (decision 13 items a–c), which this change matches: a line-starting `@*`/`<!--` inside a multi-line C# literal or comment now opens a comment in every host, `@code`/`@functions` blocks included.
