Role: implementer
Commit: 68697a9
Verdict: tests pass

Repair round 1, worklist: auditor findings 1 and 2 (12-auditor.md), against RH5401-004 as amended in babcc50 (13-spec-author.md) and the tests of 870dcd7/8eb6bbb (14-test-implementer.md). My prompt carried no intended fix, suspected cause, or hoped-for verdict. I did not take the auditor's suggestion as given: its "whole-region count only" variant would offer the fix for example `S`; the change implements the spec's two conditions together.

Before the change: `RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests` 10 failed / 82 passed, the ten failures exactly the four `N`, `O`, `P`, `R` tests and the combined `N`/`O`/`P`/`R`/`S` test on both line endings, each `expected count: 0, actual count: 1`, as 14 states.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider.cs` | New `CountBlankLinesInAttributeRegion`: sums `TokenGapAnalysis.Between(token, next).BlankLineCount` over every token gap from the first attribute list's `[` through the gap in front of the first token behind the attribute lists. The gate compares that sum for the property as written and as formatted, in addition to the existing per-gap `KeepsBlankLinesBehindAttributeLists` (both required; a property without attribute lists skips both). | RH5401-004 (whole-region condition, examples `N`, `O`, `P`, `R`; `S` still by the per-gap condition), RH5401-003 (`L`, `Q` stay offered) |
| same file | Finding 2: `CanCollapse` renamed to `FormatsOnOneLineKeepingBlankLines`; its summary and return doc name both conditions (own code on one line; no blank line inserted or removed among and behind the attribute lists, per gap and in all); the call-site comment in `RegisterCodeFixesCoreAsync` now names both reasons to withhold. The early-return split of the one-line check keeps its existing comment directly on that check, and a new comment states why both blank-line conditions are needed. | RH5401-004; FC-031 |

`KeepsBlankLinesBehindAttributeLists` is unchanged; its summary still describes exactly what it checks.

## Policy owners
- Blank-line count of a token gap: `rg -n "BlankLineCount" src --glob '*.cs'` (non-test, non-bin) — `TokenGapAnalysis.BlankLineCount` (Reihitsu.Core) is the one owner; used by `BlankLineSpacingPolicy` and the formatter's blank-line policies, and by the RH5401 fix through `KeepsBlankLinesBehindAttributeLists` and the new `CountBlankLinesInAttributeRegion`. No private counter of blank lines was written; the new method only walks tokens and sums the owner's per-gap count.
- "First token behind the attribute lists": `AttributeLists.Last().GetLastToken().GetNextToken()`, the same expression already used in the method and in `OrderingDeclarationUtilities` (one expression, not a policy).

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 3 files changed, 129 insertions(+), 53 deletions(-). Round delta (68697a9 alone): 1 file changed, 45 insertions(+), 12 deletions(-). No new type, no new public member.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5401"` — Passed! Failed: 0, Passed: 94, Total: 94.
- `scripts/test.sh --configuration Debug --project analyzer` — Passed! Failed: 0, Passed: 11035, Total: 11035.
- `scripts/test.sh --configuration Debug --project architecture` — Passed! Failed: 0, Passed: 198, Total: 198.
- `scripts/format.sh` on the changed file: formatted 0 of 1.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None_ beyond 13's formatter candidate (a blank line in front of the `]` of a list the formatter splits); with this change that input keeps both counts and still gets the fix, as the spec states.
