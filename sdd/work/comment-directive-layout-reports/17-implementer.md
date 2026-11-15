Role: implementer
Commit: 5b8fbaf
Verdict: tests pass

Repair round 3, for audit 14 findings #1–#3 as decisions 17–19 decide them. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond those recorded decisions.

Per issue: #1147: RH5201 back on `main`'s line-start measure (finding #1), and only line-starting earlier-line comments are checked in `IsAlignedAt` (finding #3). #1386: one diagnostic per `///` line in every documentation mode (finding #2). #1152, #1387, #1594, #1655, #1736: nothing to do.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Layout/RH5201MethodChainsShouldBeAlignedAnalyzer.cs` | Restored to `main` (3b3639e) byte for byte: the private `GetLineStartColumn` again measures the line's first non-whitespace character, and `IsMisplaced` calls it instead of `CommentPositionUtilities.GetLineStartColumn` | RH5201-002, decision 17 |
| `src/Reihitsu.Analyzer/Core/CommentPositionUtilities.cs` | New `GetCommentLocations(SyntaxTrivia)`: for a `SingleLineDocumentationCommentTrivia`, one location per line, from the line's `///` to the end of its text (line break excluded); any other comment yields `GetCommentLocation` | SHARED-COMMENT-003, decision 18 |
| `src/Reihitsu.Analyzer/Base/CommentPositionAnalyzerBase.cs` | `OnSyntaxTree` reports every location of `GetCommentLocations`; class summary names the per-line `///` count | SHARED-COMMENT-003 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5048CommentsMustNotBePlacedBeforeFirstCallOfWrappedChainAnalyzer.cs` | `AnalyzeChain` reports every location of `GetCommentLocations` (`SelectMany`) | SHARED-COMMENT-003, RH5048-001 |
| `src/Reihitsu.Analyzer/Core/InitializerLayoutAnalysisUtilities.cs` | `IsAlignedAt`: the earlier-line loop skips a comment that does not start its line (`TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`); summary rewritten to say so | SHARED-INIT-001, decision 19 |

## Policy owners
- Column of a chain link's line start (RH5201-002): `rg "GetLineStartColumn" src/Reihitsu.Analyzer` → `CommentPositionUtilities.GetLineStartColumn` (SHARED-COMMENT-011, trivia-based: the token's column behind a comment tail) and RH5201's private text-based copy (first non-whitespace character, which counts a `*/` tail as the line start). These are two measures that differ only for a tail-led line. Decision 17 restores RH5201's own measure and drops the class, so the second owner is intended. It is not a copy of the shared decision.
- Location of a comment diagnostic: `rg "GetCommentLocation" src/Reihitsu.Analyzer` → `CommentPositionUtilities.GetCommentLocation` (RH5204 keeps it for the whole documentation block it re-indents) and the new `GetCommentLocations`, which builds on it (callers: `CommentPositionAnalyzerBase`, RH5048). No other owner.
- Which earlier-line comments anchor an initializer line: `rg "IsAlignedAt|StartsLineAtLeadingTriviaIndex" src/Reihitsu.Analyzer` → `InitializerLayoutAnalysisUtilities.IsAlignedAt` (callers RH5301/RH5303/RH5304 through it) and `CommentPositionUtilities.TryGetLineStartComment`, which uses the same Core predicate `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`. One predicate owns "starts its line".

## Production diff size
- Whole run: `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` → 17 files changed, 323 insertions(+), 111 deletions(-). That is one file fewer than before this round: RH5201 is back to `main`.
- This round (`git diff --shortstat HEAD~1 -- src`): 5 files changed, 89 insertions(+), 11 deletions(-). Roughly 50 lines are production code and the rest are XML docs. The RH5201 revert accounts for +22/−3 of these: it adds back the private method and removes the round-1 using and call.
- No new type.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "Name~EveryLineOfTripleSlashRun|Name~AnchorColumnBehindMisalignedCommentTail|Name~LineOfTwoComments|Name~FurtherCommentEndingOnItsLine|Name~FurtherCommentWhoseLineStartsOutside|Name~LineOfTwoMisplacedComments|Name~FurtherCommentsBehindCommentsThatStartEarlierLines"` → Passed: 38, Failed: 0. These were red on 98aeef9, as 16 states.
- `scripts/test.sh --configuration Debug --project analyzer` → Passed: 9742, Failed: 0.
- `scripts/test.sh --configuration Debug --project architecture` → Passed: 118, Failed: 0.
- `scripts/format.sh` over the five files: formatted 0 of 5.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._ RH5201's reverse tail case and the SHARED-COMMENT-012 tail class stay dropped (decisions 17 and 20).
