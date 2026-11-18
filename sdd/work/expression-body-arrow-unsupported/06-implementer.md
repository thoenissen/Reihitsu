Role: implementer
Commit: 51ffef7
Verdict: tests pass

Inputs: `00-lead.md`, `05-test-implementer.md`, the accepted change doc `sdd/changes/2026-11/expression-body-arrow-leading-comment-unsupported.md`, and the spec diff (`git diff origin/main -- sdd AGENTS.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Before the change, the handed-over tests failed as `05-test-implementer.md` states (formatter 126, analyzer 33, CLI 11; reproduced by the test implementer on 752469f, production unchanged since).

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | New reason constant `ExpressionBodyArrowLeadingCommentReason`; `FindUnsupportedSyntax` collects, per `ArrowExpressionClauseSyntax`, the first comment, directive, or disabled text in the trailing trivia of the token in front of `=>` or in the `=>`'s leading trivia (`FindTriviaInFrontOfExpressionBodyArrow`); skipped when the token in front of `=>` lies outside the input node. Listed last among the candidates, so FMT-UNSUP-010 keeps its reason for the same directive (strict `<` tie-break) | FMT-UNSUP-013, FMT-UNSUP-003, FMT-UNSUP-001 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/PropertyLayoutLineBreakRewriter.cs` | Join refusal deleted from `CollapseExpressionBody`; summary and `<returns>` updated | FMT-UNSUP-013 (*Special cases*) |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/ExpressionBodyToBlockConverter.cs` | Comment move deleted from `CreateBlock` (condition and inserted line break), `ArrowLineHoldsComment` and `TrySplitOwnLineComments` deleted; the arrow's leading trivia always goes to the open brace; summary and param doc updated | FMT-STRUCT-006 (changed), FMT-UNSUP-013 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/GetOnlyMemberExpressionBodyTransform.cs` | `HasLayoutOnlySeams` no longer checks `getter.Keyword.TrailingTrivia` and `getter.ExpressionBody.ArrowToken.LeadingTrivia`; summary updated | FMT-STRUCT-015 (changed), FMT-UNSUP-013 |

## Policy owners

- `rg -n "ArrowToken\.LeadingTrivia|ArrowToken\.GetPreviousToken\(\)|ArrowToken\.HasLeadingTrivia" src --glob '!**/*Test*/**' --glob '*.cs'` — after the change only `ExpressionBodyCodeFixUtilities.ConvertBodyAsync` (copies formatted trivia of the last signature token; change doc *Checked and kept*). The precheck is the one owner of the shape (FMT-UNSUP-007).
- `rg -n "WouldJoinAcrossUnjoinableTrivia" src/Reihitsu.Formatter` — remaining users: `LineBreakTriviaUtilities.CollapseTokenToSameLine` (generic guard of every join), `AccessorListLayout`, `GenericConstraintLineBreakRewriter`, `ListElementInteriorJoinRewriter`, `ArgumentListLineStartRewriter`, `ChainLineBreakRewriter`. Kept (change doc *Checked and kept*).
- `ExpressionBodyRewriteUtilities.BlocksRewrite` (via `ExpressionBodiedTransformUtilities.RequiresExpressionBodyPreservation`) unchanged: the analyzers of RH3202–RH3208 decide by it (SHARED-BODY-001).

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 4 files changed, 52 insertions(+), 136 deletions(-). No new type.

## Deleted special cases

- `PropertyLayoutLineBreakRewriter.CollapseExpressionBody`: `if (LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(expressionBody.ArrowToken.GetPreviousToken(), expressionBody.ArrowToken)) return node;` — change doc *Deleted*, entry 1.
- `ExpressionBodyToBlockConverter.CreateBlock`: the `ArrowLineHoldsComment(...) == false && TrySplitOwnLineComments(...)` branch, the inserted line break in front of the moved comment lines, and the methods `ArrowLineHoldsComment` and `TrySplitOwnLineComments` — entry 2.
- `GetOnlyMemberExpressionBodyTransform.HasLayoutOnlySeams`: `IsLayoutOnly(getter.Keyword.TrailingTrivia)` and `IsLayoutOnly(getter.ExpressionBody.ArrowToken.LeadingTrivia)` — entry 3.
- No further special case found: the other files that touch expression bodies (`rg -l "ExpressionBody|ArrowToken|EqualsGreaterThan" src/Reihitsu.Formatter`) hold no check of the gap in front of the arrow. `CollapseTokenToSameLine` keeps its own generic unjoinable-trivia guard, which serves every join.

## Tests run

```shell
scripts/build.sh --configuration Debug                                  # 0 Warning(s), 0 Error(s)
scripts/test.sh --configuration Debug --project formatter               # Passed! Failed: 0, Passed: 5873
scripts/test.sh --configuration Debug --project analyzer                # Passed! Failed: 0, Passed: 10673
scripts/test.sh --configuration Debug --project cli                     # Passed! Failed: 0, Passed: 396
scripts/test.sh --configuration Debug --project architecture            # Passed! Failed: 0, Passed: 198
scripts/test.sh --configuration Debug --project core                    # Passed! Failed: 0, Passed: 738
scripts/format.sh <the four changed files>                              # Formatted 0 of 4 file(s)
```

## Disputed tests or scope

_None._

## Architecture findings

_None._

## Follow-up candidates

_None._
