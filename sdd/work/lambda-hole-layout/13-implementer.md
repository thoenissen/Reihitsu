Role: implementer
Commit: <see git log: "Leave the lambda return-type gap to one owner">
Verdict: tests pass

Repair round 1, finding 2 of `11-auditor.md` (thoenissen/Reihitsu#1567). My prompt carried a suggested direction ("leave the gap with a single owner if cheap, else reword the comment"); I re-derived it from the code below rather than taking it as given.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ListElementInteriorJoinRewriter.cs` | `IsOwnedGap`: the parameter-list `(` is now treated as owned elsewhere for every owner except a `ParenthesizedLambdaExpressionSyntax { ReturnType: null }` (before: except every parenthesized lambda). The gap between a lambda's explicit return type and its `(` inside a list element is therefore left to `LineBreakListRewriter.VisitParenthesizedLambdaExpression`, its one owner. Comment rewritten to state that ownership | FMT-BREAK-015, FMT-BREAK-045 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs` | Inline comment of `VisitParenthesizedLambdaExpression` names the list-element case and adds FMT-BREAK-015 to the owners of the gap without a return type. No code change | FMT-BREAK-045 |

Why the output cannot change: both rewriters run in the same phase with nothing between them (`LineBreakPhase.cs:47-48`), both refuse through `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`, and both write `CreateJoinedTrailingTrivia` + `RemoveLeadingEndOfLineAndWhitespace`. Their line-break tests differ (`AreSeparatedByEndOfLine` vs `TokenGapUtilities.HasLineBreakBetween`) only for trivia the shared guard refuses anyway. `LineBreakListRewriter`'s 18 overrides all call `base`, so it visits every lambda the other rewriter reached. Checked: a fixture with return-type lambdas inside arguments, a named argument, a tuple element, a conditional branch inside an argument, `static` and `async` with return type, a blank line in the gap, a delimited and a line comment in the gap, `#if/#else` in the gap, and an attribute list in front — formatted with the change and with the change reverted (`git apply -R`, `scripts/format.sh` rebuilds): byte-identical output.

## Policy owners
`rg -n "CollapseOwnedOpenParenToDeclarationLine" src` — one implementation in `LineBreakListRewriter`, reused there and by `DeclarationBraceLineBreakRewriter`. The return type → `(` gap of a lambda now has that one owner everywhere; `ListElementInteriorJoinRewriter` joins only the `(` gap of a parenthesized lambda without a return type (behind a modifier or as an element's interior).

## Production diff size
`git diff --shortstat origin/main...HEAD -- src` before this round: 4 files changed, 39 insertions(+), 67 deletions(-). Round delta: 2 files changed, 10 insertions(+), 6 deletions(-) (one condition split into three lines, comments). No new type or member.

## Deleted special cases
_N/A._

## Tests run
Debug configuration.
- `scripts/test.sh --configuration Debug --project formatter`: Passed 7541/7541 (includes every formatter filter of 05/12: `LambdaReturnTypeJoinTests`, `ListElementInteriorJoinTests`, `ContinuationJoinTests`, `ParenthesizedLambdaLineStartTests`, ...).
- `scripts/test.sh --configuration Debug --project analyzer --filter "<05's analyzer filter>|FullyQualifiedName~RH5105"`: Passed 454/454.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 198/198.
- `scripts/format.sh` over both changed files: 0 of 2 reformatted.

## Disputed tests or scope
- Spec wording to follow the single owner (for the spec author; I touch only `src/`): `sdd/specs/formatter/line-breaks.md` FMT-BREAK-015 (line ~1083) lists as owned elsewhere "the opening parenthesis of a parameter list other than a lambda's" — it is now "other than that of a parenthesized lambda without an explicit return type" (FMT-BREAK-045 owns the one behind a return type); FMT-BREAK-045 (line ~4574) says "as a list element (where FMT-BREAK-015 joins the same gap)" and its Why (line ~4578) "and inside a list element (FMT-BREAK-015)" — FMT-BREAK-045 now joins it inside a list element too. Output is identical either way. If the lead prefers not to amend the spec, the alternative is to revert the condition and keep only the reworded comment (two owners, as the spec states now).

## Architecture findings
_None._

## Follow-up candidates
_None._
