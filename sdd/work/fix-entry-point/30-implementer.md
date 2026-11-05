Role: implementer
Commit: 1d215751
Verdict: tests pass

Repair round 2, finding #8 of `28-auditor.md` (decision 26 of `00-lead.md`). My prompt carried no intended fix beyond naming the finding.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | `StartsLineInLayout` calls `SyntaxTriviaUtilities.IsDocumentationCommentTrivia` instead of restating its two `IsKind` checks. Same kinds, so no output changes. The summary still describes the code (FC-031), so it is unchanged. | FC-015 (FMT-PIPE-029 unchanged) |

## Policy owners
- `rg -n "IsKind\(SyntaxKind\.(SingleLine|MultiLine)DocumentationCommentTrivia\)" src/Reihitsu.Formatter/ReihitsuFormatter.cs`: no match after the change.
- Owner: `SyntaxTriviaUtilities.IsDocumentationCommentTrivia` in `src/Reihitsu.Core/SyntaxTriviaUtilities.cs:82`. `Reihitsu.Core` was already imported by `ReihitsuFormatter.cs`.

## Tests run
- `scripts/test.sh --configuration Debug --project formatter`: `Passed! - Failed: 0, Passed: 4517`
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7101"`: `Passed! - Failed: 0, Passed: 83`
- `scripts/test.sh --configuration Debug --project analyzer`: `Passed! - Failed: 0, Passed: 6516`

One early RH7101 run failed `FixCountsEachTabInFrontOfAFirstFieldBehindACommentThatSpansLinesAsOneColumn` while the parallel test implementer was still editing that test. The same filter passed against HEAD's production code, and it passed again with this change once 15725b91 was committed. The failure came from the test edit in progress, not from this change. The full analyzer run above includes 15725b91's tests.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
