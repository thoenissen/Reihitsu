Role: implementer
Commit: ca67223 (the change and this file are in the commit that adds this file)
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | `FindTriviaInFrontOfComma` walks `node.DescendantTokens()` once, carrying the last token with text and the tokens with an empty span behind it; at a comma it inspects that gap. `comma.GetPreviousToken()` and the `GetNextToken(includeZeroWidth: true)` walk are gone. `EnumerateGapTrivia` is replaced by `FindTriviaInGap`, which reads the same trivia in the same order (trailing trivia of the token in front, leading and trailing trivia of each token without text, leading trivia of the comma). A comma with no token with text in front of it inside the node is skipped, which is what the former `previousToken.SpanStart < node.FullSpan.Start` check decided. XML summaries rewritten for the new code. | FMT-UNSUP-009 (answers unchanged; cost only) |

How the answers stay identical, against the skeptic's facts 1 to 5:
1. Gap start: "token with text" is `Span.IsEmpty == false`, the same predicate Roslyn's default `GetPreviousToken` uses (non-zero width); tokens with an empty span between it and the comma contribute leading and trailing trivia, as before.
2. Line break: the same `ToFullString()` test over every gap trivia.
3. First comma in text order wins; reason still keyed on `comma.Parent is VariableDeclarationSyntax`.
4. `DescendantTokens()` does not descend into structured trivia, as before; the former `GetPreviousToken`/`GetNextToken` calls did not enter trivia either.
5. Node-level boundary: a comma before the node's first token with text has no carried token and is skipped.

## Policy owners
`rg -n "EnumerateGapTrivia|FindTriviaInFrontOfComma|GetPreviousToken|GetNextToken" src/Reihitsu.Formatter/Pipeline/Precheck/` — only `PrecheckPhase` owned the gap walk; no other copy. After the change, no `GetPreviousToken`/`GetNextToken` call remains in the precheck.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 1 file changed, 46 insertions(+), 42 deletions(-). No new type.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.Precheck|FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck"` — Passed: 469, Failed: 0 (the 464 existing precheck tests plus the 5 `LongListPrecheckCostTests`), 6 s in total.
- `scripts/test.sh --configuration Debug --project formatter` — Passed: 5608, Failed: 0.
- `scripts/format.sh src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` — formatted 0 of 1 file.
- No test assertion, input, or expected output changed.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None_ beyond those already in `00-lead.md`.
