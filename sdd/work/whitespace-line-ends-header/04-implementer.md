Role: implementer
Commit: (this commit; base 90b6e4b)
Verdict: tests pass

The prompt carried no intended fix or hoped-for verdict. The issue's "Cause" paragraph and its prescribed `GetHeaderSplitIndex` change were not used as a specification; the change follows lead decisions 2 and 3.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Utilities/UsingLeadingTriviaBuilder.cs` | `GetHeaderSplitIndex` no longer keeps its private "two adjacent `EndOfLineTrivia`" test of a blank line. It walks the end-of-line trivia from the first significant trivia and asks the shared measure, `TokenGapAnalysis.OfTriviaRange(...).BlankLineCount`, whether the range through that line break holds one more blank line; the split lands behind the last line break that completes a blank line. A whitespace-only line and the line below a `///` comment (whose line break is inside the comment's own trivia) now count, as SHARED-BLANK-001 states; lines inside comment text do not. No dedicated code for `///` (decision 3). XML summary updated (FC-031) | FMT-USING-009 (via SHARED-BLANK-001), FMT-USING-010, FMT-USING-011 |

## Policy owners
- `rg -n "IsKind\(SyntaxKind.EndOfLineTrivia\)" -A1 src/Reihitsu.Formatter/Pipeline/UsingDirectives src/Reihitsu.Core/UsingDirective*.cs` for adjacent-line-break checks: no remaining adjacency test of a blank line in the using code after the change.
- Owners of "blank line" (SHARED-BLANK-001): `Reihitsu.Core/TokenGapAnalysis` (`OfTriviaRange`, `Between`, `IsBlankLine`) — now used by the header split; `UsingDirectiveOrderingUtilities.HasBlankLineAbove` (Core) counts line breaks in the layout trivia and ignores whitespace, so it agrees with the shared measure for its inputs (skeptic's finding) and was not changed.

## Production diff size
`git diff --shortstat -- src` (uncommitted, before this commit): 1 file changed, 17 insertions(+), 5 deletions(-). No new type or public member.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.UsingDirectives.UsingDirectiveHeaderBlankLineTests"` — Passed 12/12.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests"` — Passed 102/102.
- `scripts/test.sh --configuration Debug --project formatter` — Passed 5191/5191.
- `scripts/test.sh --configuration Debug --project analyzer` — Passed 9869/9869.
- `scripts/test.sh --configuration Debug --project cli` — Passed 281/281.
- `scripts/test.sh --configuration Debug --project architecture` — Passed 118/118.
- `scripts/build.sh --configuration Debug` — 0 warnings, 0 errors. `scripts/format.sh` on the changed file: no change.

## Disputed tests or scope
_None._ No test assertion was changed.

## Architecture findings
_None_ beyond the skeptic's FC-015 finding, which this change resolves for the header split by reusing `TokenGapAnalysis` (decision 2). The measure is evaluated once per end-of-line trivia over the growing range (quadratic in the length of one directive's leading trivia, which is a handful of trivia); a linear variant would need either a new public member on `TokenGapAnalysis` or a private copy of its line state, and was not chosen.

## Follow-up candidates
_None._
