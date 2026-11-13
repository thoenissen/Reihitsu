Role: implementer
Commit: <filled by the commit that adds this file; parent 35bab1d>
Verdict: tests pass

The prompt named only the change asked (make the red tests of `23-test-implementer.md` pass); it carried no intended fix, suspected cause, or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributeListShapeCodeFixProviderBase.cs` | `StartsOnTheLineOfTheHoldingBrace` (owner-kind switch: accessor list, base type, namespace, local-function block) replaced by `StartsBehindAnOpenBraceOnItsLine`: walks the tokens in front of the list's `[` backwards while they start on the `[`'s line, counts `}` tokens still waiting for their `{`, and withholds the split fix on a `{` found while none is waiting. Missing tokens are skipped; braces in literals, comments, and disabled text are not tokens. XML summary and the guard comment in `TryGetFixableAttributeList` rewritten to the new condition (FC-031) | SHARED-ATTR-022 (as restated by spec author 22), SHARED-ATTR-017 pointer |

## Policy owners
- `rg -n "ATTR-022" src --glob '!*Test*'` → `Base/TargetAttributeListShapeCodeFixProviderBase.cs` only (the guard and its call-site comment).
- `rg -n "OpenBraceToken\)" src/Reihitsu.Analyzer.CodeFixes src/Reihitsu.Core --glob '!*Test*'` → no other scan of a line's tokens for an open `{` (`TrailingCommaCodeFixHelper`, `RegionDirectiveBlankLineCodeFixProviderBase` look up a node's own brace for other decisions). One owner.

## Production diff size
- Run: `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` → 21 files changed, 338 insertions(+), 189 deletions(-) before this round.
- This round: 1 file changed, 37 insertions(+), 23 deletions(-). No new type, member visibility, or dependency.

## Deleted special cases
_N/A._ (No shape is declared unsupported; the owner-kind enumeration is replaced by the one condition decision 23 states.)

## Tests run
```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5514MethodAttributeListsMustFollowShapeRulesAnalyzerTests|FullyQualifiedName~RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzerTests"
```
Passed! - Failed: 0, Passed: 84 (the 4 red tests now pass).

```shell
scripts/test.sh --configuration Debug --project analyzer
scripts/test.sh --configuration Debug --project architecture
```
Passed! - Failed: 0, Passed: 8804; Passed! - Failed: 0, Passed: 117.

## Disputed tests or scope
_None._

## Architecture findings
_None._ The change is the structural refusal of decision 23: one token scan replaces the enumeration of owner kinds.

## Follow-up candidates
_None._
