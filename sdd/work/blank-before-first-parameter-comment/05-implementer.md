Role: implementer
Commit: (this commit; on top of e6c9bac)
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decision 3, which I carried out.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/SyntaxTriviaUtilities.cs` | New `IsContentOpeningToken(SyntaxToken)`: the single owner of "this token opens content": `{`, `(`, `[` of any construct, and `<` only under `TypeParameterList`, `TypeArgumentList`, or `FunctionPointerParameterList` | FMT-BLANK-011, RH5020-002, RH8303-002 (decision 3) |
| `src/Reihitsu.Formatter/Data/PrecedingTokenFacts.cs` | New fact `OpensContent`, captured from the Core owner in `From`. The parent kind for `<` is not among the other facts, and a node-level root reaches its preceding token only as facts | FMT-BLANK-011 |
| `src/Reihitsu.Formatter/Pipeline/Core/Utilities/BlockStartUtilities.cs` | New `IsFirstBehindOpeningToken` (missing token, `OpensContent`, or switch-label colon). `IsFirstInBlock` is unchanged and keeps the `/** */` move narrow. Class summary updated: it no longer claims that both phases read one predicate | FMT-BLANK-011, FMT-DOC-001 (change doc entry 4) |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTriviaBoundaryRewriter.cs` | `VisitToken` reads `IsFirstBehindOpeningToken` | FMT-BLANK-011 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTokenCleanupRewriter.cs` | In the FMT-BLANK-007/008/019 branch, the leading blank-line collapse is skipped when the preceding token opens content and the gap holds a comment. See the note below | FMT-BLANK-011, FMT-BLANK-019 |
| `src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs` | Summary of `IsMovableDelimitedDocumentation` only: the move stays on `IsFirstInBlock`, and the blank-line boundary now reads the wider predicate | FC-031 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5020CommentsShouldBePrecededByABlankLineAnalyzer.cs` | The private `IsFirstCommentInBlock` became `IsFirstCommentBehindOpeningToken`. It reads the Core owner, and its missing-token and switch-label-colon checks stay local | RH5020-002 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer.cs` | The private `IsFirstInBlock` became `IsFirstBehindOpeningToken` and reads the Core owner. Summary and inline comment updated. As before, it has no switch-label colon, which RH8303-002 does not name | RH8303-002 |

Note on the cleanup rewriter: after the predicate change, the one remaining failure was the existing `CollectionExpressionClosingBracketCommentTests.CommentBeforeClosingBracketOfEmptyCollectionExpressionIsIndented`. Its input is `[⏎⏎// none⏎]`, with the author's blank line behind `[`. `BlankLineTokenCleanupRewriter` collapsed the leading blank lines of `]` as if they stood directly before `]`, although a comment stands between them and `]`. Until now, FMT-BLANK-011 put the blank line back in a later subphase. Now it does not do so behind `[`, so the author's blank line was lost. That contradicts FMT-BLANK-019 and FMT-BLANK-011 ("behind an opening parenthesis, bracket, or `<` the author's blank line is kept"). The added condition is a cheap check on facts already captured. Behind `{` the earlier branch has already removed the blank lines (FMT-BLANK-007), so `{` is unaffected. A comment glued to `]` (FMT-BLANK-018) is still handled by `RemoveBlankLinesAboveGluedComment`. I did not change the test.

## Policy owners

- `rg -n "IsContentOpeningToken|OpensContent" src -g '*.cs'`: the owner is `SyntaxTriviaUtilities.IsContentOpeningToken`. Its readers are `PrecedingTokenFacts.From` (formatter), RH5020, and RH8303. `OpensContent` is read by `BlockStartUtilities.IsFirstBehindOpeningToken` and `BlankLineTokenCleanupRewriter.VisitToken`.
- `rg -n "IsFirstInBlock|IsFirstCommentInBlock" src -g '*.cs' -g '!**/obj/**' -g '!**/bin/**'`: only `BlockStartUtilities.IsFirstInBlock`, read by `DocumentationCommentFormattingPhase.IsMovableDelimitedDocumentation`. The two private analyzer copies are gone.
- The switch-label-colon check still exists twice, in `PrecedingTokenFacts.From`/`IsSwitchLabelColon` and in RH5020's private predicate, as before this run. It is a different decision (the first content of a switch section), and RH8303 deliberately omits it.

## Production diff size

`git diff --shortstat -- src` (working tree on e6c9bac, test projects untouched): `8 files changed, 90 insertions(+), 29 deletions(-)`. Most of it is XML documentation. About 25 lines are executable code. No new type, one new public member (`SyntaxTriviaUtilities.IsContentOpeningToken`, in Core as decision 3 asks), and one new property on the internal `PrecedingTokenFacts`.

## Deleted special cases

_N/A._

## Tests run

```
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~FirstCommentBehindOpeningTokenTests"
  Passed!  - Failed: 0, Passed: 14
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5020|FullyQualifiedName~RH8303|FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests.FixLeavesNoBlankLine|FullyQualifiedName~RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixLeavesNoBlankLine"
  Passed!  - Failed: 0, Passed: 109
scripts/test.sh --configuration Debug --project formatter      Passed!  - Failed: 0, Passed: 5279
scripts/test.sh --configuration Debug --project analyzer       Passed!  - Failed: 0, Passed: 10442
scripts/test.sh --configuration Debug --project core           Passed!  - Failed: 0, Passed: 738
scripts/test.sh --configuration Debug --project cli            Passed!  - Failed: 0, Passed: 303
scripts/test.sh --configuration Debug --project architecture   Passed!  - Failed: 0, Passed: 153
```

## Disputed tests or scope

- `BlankLineTokenCleanupRewriter` is a production path the handovers did not name (see the note under *Changes*). It is one condition and needs no new test: an existing test caught it.
- `SyntaxTriviaUtilities.IsContentOpeningToken` is a new public Core member with three consumers, as decision 3 asks. Core has no direct unit test for it. It is covered through the formatter, RH5020, and RH8303 opener matrices. The lead may route a small `Reihitsu.Core.Test` case (generic `<` against operator `<`) to the test implementer.

## Architecture findings

None beyond the one decision 3 resolves. That rule now has one owner.

## Follow-up candidates

_None._
