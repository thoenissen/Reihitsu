Role: implementer
Commit: (this commit; on top of db24264)
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

The handed-over tests failed on db24264 as 04-test-implementer.md lists (I did not re-run the Release baseline; the failing set is the one the handover records on 425afc1, and the test commit b1efb29 sits between). After the change every test project of the default run passes in Debug.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | New reason constant `ExpressionBodyArrowCommentReason`; `FindUnsupportedSyntax` visits every `ArrowExpressionClauseSyntax` and takes the first comment, directive, or disabled text of the `=>`'s trailing trivia or the expression's first token's leading trivia (`FindTriviaBehindExpressionBodyArrow`, reusing `FindFirstUnsupportedOperatorTrivia`). The candidate is listed last in both orderings, so FMT-UNSUP-010 keeps its reason where both start at the same directive; the ordering comment says so | FMT-UNSUP-012, FMT-UNSUP-001, FMT-UNSUP-003, FMT-UNSUP-010 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/PropertyLayoutLineBreakRewriter.cs` | `CollapseExpressionBody`: deleted the join refusal behind the arrow; the refusal in front of the arrow stays. Summary updated | FMT-BREAK-026, FMT-BREAK-001 (change doc *Deleted* 1) |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/ExpressionBodyToBlockConverter.cs` | Deleted every case for trivia behind the arrow (see below); `CreateBlock`, `ArrowLineHoldsComment`, `CreateStatement`, `CreateReturnStatement` summaries rewritten | FMT-STRUCT-004–006, FMT-STRUCT-026 (change doc *Deleted* 2) |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/GetOnlyMemberExpressionBodyTransform.cs` | `HasLayoutOnlySeams`: deleted the checks of the getter arrow's trailing trivia and of the expression's first token's leading trivia; summary states why the gap is no seam | FMT-STRUCT-015 (change doc *Deleted* 3) |

## Policy owners
- The shape's one owner is `PrecheckPhase.FindUnsupportedSyntax` (FMT-UNSUP-007); `ReihitsuFormatter.FindUnsupportedSyntax` and the code fixes (SHARED-FIX-010) already ask it, so no code-fix change was needed for RH3202–RH3208, RH5401, RH7101.
- `rg -n "ArrowToken.TrailingTrivia|ArrowToken.GetNextToken|Expression.GetFirstToken\(\).LeadingTrivia|Expression.GetLeadingTrivia" src/Reihitsu.Formatter src/Reihitsu.Analyzer.CodeFixes src/Reihitsu.Core` — after the change only the precheck's own check remains (plus an unrelated RH3003 literal hit).
- `rg -n "WouldJoinAcrossUnjoinableTrivia\(.*Arrow" src` — only the kept in-front refusal in `PropertyLayoutLineBreakRewriter`.
- The trivia predicate is the precheck's existing `FindFirstUnsupportedOperatorTrivia` (comment, or directive/disabled text via `SyntaxTriviaUtilities`); no new copy.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this commit: empty (no production change earlier in the run). This commit: 4 files changed, 70 insertions(+), 118 deletions(-). No new type, no new public member.

## Deleted special cases
- *Deleted* 1 — `PropertyLayoutLineBreakRewriter.CollapseExpressionBody`: `WouldJoinAcrossUnjoinableTrivia(expressionBody.ArrowToken, expressionBody.Expression.GetFirstToken())`.
- *Deleted* 2 — `ExpressionBodyToBlockConverter`:
  - `ArrowLineHoldsComment`'s check of the arrow's trailing trivia (the in-front part kept);
  - the move of own-line comments out of the expression's leading trivia (`TrySplitOwnLineComments(expression.GetLeadingTrivia(), …)` block);
  - the transfer of the arrow's trailing comments/directives onto the opening brace (`ExtractSignificantTrivia(arrowToken.TrailingTrivia)`; the brace now gets no trailing trivia, and the "unless its trailing trivia already ends the line" branch before the comment lines is gone with it);
  - in `CreateReturnStatement`: the move of a comment in front of the expression behind `return`, the line break written behind `return` in front of a comment leading the expression, `LeadsWithCommentOnItsLine`;
  - the `moveComments` and `lineBreak` parameters of `CreateStatement` and `CreateReturnStatement`, and the `lineBreak` computation.
  Kept, as *Checked and kept* says: own-line comments in front of the arrow leading the statement, the in-front part of `ArrowLineHoldsComment`, the semicolon's trivia (`ExtractSignificantTrivia` stays for it), and an expression leading trivia with a line break (blank line behind the arrow) kept as layout; leading whitespace alone is still dropped.
- *Deleted* 3 — `GetOnlyMemberExpressionBodyTransform.HasLayoutOnlySeams`: `IsLayoutOnly(arrowToken.TrailingTrivia)` and `IsLayoutOnly(getter.ExpressionBody.Expression.GetFirstToken().LeadingTrivia)`.
- *Deleted* 4 (spec text) — already done by the spec author.
- No further special case found.

## Tests run
All in Debug, after `scripts/build.sh --configuration Debug` (0 warnings, 0 errors) and `scripts/format.sh` over the four files (0 of 4 formatted):
- `scripts/test.sh --configuration Debug --project formatter` — Passed! Failed: 0, Passed: 5735
- `scripts/test.sh --configuration Debug --project cli` — Passed! Failed: 0, Passed: 385
- `scripts/test.sh --configuration Debug --project analyzer` — Passed! Failed: 0, Passed: 10635
- `scripts/test.sh --configuration Debug --project architecture` — Passed! Failed: 0, Passed: 197
- `scripts/test.sh --configuration Debug --project core` — Passed! Failed: 0, Passed: 738
- `scripts/test.sh --configuration Debug --project tooling` — Passed! Failed: 0, Passed: 133
- `scripts/test.sh --configuration Debug --project playground` — Passed! Failed: 0, Passed: 23

Self-hosting scans not run (outside the default run).

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
