Role: implementer
Commit: e676781
Verdict: tests pass

My prompt named the issue, the slug, the lane, the branch, and the handovers to read. It carried no intended fix or suspected cause.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | New reason constant `BinaryOperatorCommentReason`. New `case BinaryExpressionSyntax` in `FindUnsupportedSyntax`'s descendant walk. New `FindTriviaBehindBinaryOperator`: the first comment, directive, or disabled text in the operator's trailing trivia, then in the right operand's first token's leading trivia, using the existing `FindFirstUnsupportedOperatorTrivia`. The candidate is listed last in both tie orders, so FMT-UNSUP-004 and FMT-UNSUP-010 keep their reason when they start at the same trivia. The tie comment is extended to say so. | FMT-UNSUP-011, FMT-UNSUP-001, FMT-UNSUP-003 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/BinaryOperatorLineBreakRewriter.cs` | `NormalizeBinaryOperatorPosition`: the `WouldJoinAcrossUnjoinableTrivia(operatorToken, node.Right.GetFirstToken())` refusal is deleted. The XML summary never mentioned it and still describes the code. | FMT-BREAK-017, change doc *Special cases* |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider.cs` | Deleted from `RegisterCodeFixesCoreAsync`: the `IsTrailingOperator(...) && SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(...)` guard. Deleted: `IsTrailingOperator`, which only that guard used. Deleted: the now-unused `using Reihitsu.Core;`. The existing `ReihitsuFormatter.FindUnsupportedSyntax(formattingNode)` check now withholds the fix. | RH5302-005, SHARED-FIX-010 |

## Policy owners
- The shape check: `rg -n "FindFirstUnsupportedOperatorTrivia|BinaryExpressionSyntax" src/Reihitsu.Formatter/Pipeline/Precheck`. One owner, `PrecheckPhase`. `ReihitsuFormatter.FindUnsupportedSyntax` delegates to it (FMT-UNSUP-007), and the code fixes ask it.
- Remaining join guards: `rg -n "WouldJoinAcrossUnjoinableTrivia\(" src --glob '!*Test*'`. Each of these is outside the shape: `FluentChain.cs:115` and `ChainLineBreakRewriter.cs:174` (member-access chains), and `LineBreakAssignmentRewriter.cs:77/101/132/187` (assignment and equals-value operators). The guard deleted from `BinaryOperatorLineBreakRewriter` was the only binary-operator caller.
- `rg -ln "BinaryExpressionSyntax" src --glob '!*Test*' | xargs rg -l "IsCommentTrivia|IsDirectiveOrDisabledTextTrivia"`. It finds `PrecheckPhase.cs` and `src/Reihitsu.Analyzer/Core/CommentPositionUtilities.cs`. The second is the analyzer side (RH5049/SHARED-COMMENT-007), which the change doc keeps: analyzers do not consult the precheck.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 3 files changed, 32 insertions(+), 29 deletions(-). No new type.

## Deleted special cases
- `BinaryOperatorLineBreakRewriter.NormalizeBinaryOperatorPosition`: the `LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(operatorToken, node.Right.GetFirstToken())` refusal. This fulfils change doc *Special cases / Deleted*, entry 1.
- `RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider.RegisterCodeFixesCoreAsync`: the `IsTrailingOperator(binaryExpression) && SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(operatorToken, binaryExpression.Right.GetFirstToken())` guard, plus `IsTrailingOperator`. This fulfils entry 2.
- Entry 3 (spec text) was the spec author's and is already done.
- No further special case found. The *Checked and kept* entries are kept as the change doc lists them: `EndLineInFrontOf` in the same method, `NormalizePatternOperatorPosition`, `ListElementInteriorJoinRewriter.IsOperatorWrap`, and `BinaryExpressionContributor`.
- No throw site added.

## Tests run
Debug build: `scripts/build.sh --configuration Debug`. Build succeeded, 0 warnings, 0 errors.

- `scripts/test.sh --configuration Debug --project formatter`: Passed. Failed 0, Passed 5603, Total 5603. That is the test implementer's 86 red plus 5517 green.
- `scripts/test.sh --configuration Debug --project cli --filter "FullyQualifiedName~CliHiddenCodeEndToEndTests"`: Passed. Failed 0, Passed 18.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5302|FullyQualifiedName~RH2001|FullyQualifiedName~RH7101|FullyQualifiedName~RH3202"`: Passed. Failed 0, Passed 550.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed. Failed 0, Passed 10516.
- `scripts/test.sh --configuration Debug --project cli`: Passed. Failed 0, Passed 377.
- `scripts/test.sh --configuration Debug --project architecture`: Passed. Failed 0, Passed 185.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None_ new.
