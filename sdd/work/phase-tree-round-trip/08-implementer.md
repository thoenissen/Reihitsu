Role: implementer
Commit: 83c236e
Verdict: tests pass

Repair round. Inputs:
- `05-implementer.md`, `06-spec-author.md`, and `07-test-implementer.md`;
- FMT-STRUCT-026 and FMT-BREAK-029 as amended at c4d4b21, and the amended change doc;
- the decisions `squad/decisions/2026-10-31-phase-tree-round-trip-{accessor-arrow,elastic-trivia,break-residue-reach}.md`.

The prompt carried no intended fix or hoped-for verdict to ignore. I changed no test.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/AccessorExpressionBodyTransform.cs` | The created `=>` has one explicit space on each side. The created `=>` and `;` are built with explicit trivia (`SyntaxFactory.Token(leading, kind, trailing)`), so they carry no elastic trivia. | FMT-STRUCT-026 (created `=>`, no elastic trivia) |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/GetOnlyMemberExpressionBodyTransform.cs` | Same for the member-level `=>` and `;`. | FMT-STRUCT-026 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/ExpressionBodyToBlockConverter.cs` | The created `{`, `}`, `;`, and `return` are built with explicit trivia. The fusion check reads `returnKeyword.TrailingTrivia.Count == 0` again, because no zero-width marker is left to discount. | FMT-STRUCT-026 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/ControlFlowBraceTransform.cs` | The inserted `{` and `}` are built with explicit trivia, which is the separator line break where one is required and none otherwise. They were previously built by `SyntaxFactory.Block(list)` and then edited. | FMT-STRUCT-026 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/EmptyTypeDeclarationSemicolonTransform.cs` | The created `;` is built with explicit trivia. Its trailing trivia is unchanged, the removed `}`'s. | FMT-STRUCT-026 |

The field split is unchanged, because it lies outside FMT-STRUCT-026.

FMT-BREAK-029's amended reach needs no code change. `TokenGapNormalizer` and `LineBreakInitializerRewriter` stay as they are under the break-residue-reach decision.

No public API, dependency, new type, or code-owner change. The production paths are the structural transforms already named in the change doc.

## Policy owners

- `rg -n "SyntaxFactory\.(Token|Block)\(" src/Reihitsu.Formatter/Pipeline/StructuralTransforms`: every token a structural transform creates is now built through `SyntaxFactory.Token(leading, kind, trailing)`. The calls are in the five files above, plus `FieldDeclarationSplitTransform`, which is outside the requirement.
- The space around a created `=>` is written in two places, one for each transform the spec names: `AccessorExpressionBodyTransform.ConvertToExpressionBody` and `GetOnlyMemberExpressionBodyTransform.TryCreateExpressionBody`. The get-only path builds its own arrow and does not reuse the accessor's.

## Tests run

```shell
scripts/test.sh --configuration Debug --project formatter     # Passed!  - Failed: 0, Passed: 3332   (07 reported 40 failing at 3cfdde8)
scripts/test.sh --configuration Debug --project analyzer      # Passed!  - Failed: 0, Passed: 6307
scripts/test.sh --configuration Debug --project architecture  # Passed!  - Failed: 0, Passed: 89
scripts/test.sh --configuration Debug --project cli           # Passed!  - Failed: 0, Passed: 251
scripts/test.sh --configuration Debug --project core          # Passed!  - Failed: 0, Passed: 535
scripts/test.sh --configuration Debug --project tooling       # Passed!  - Failed: 0, Passed: 125
scripts/test.sh --configuration Debug --project playground    # Passed!  - Failed: 0, Passed: 17
scripts/build.sh --configuration Debug                        # 0 Error(s); no warning in a formatter file
scripts/format.sh <the five changed files>                    # Formatted 0 of 5 file(s)
```

## Disputed tests or scope

_None._

## Architecture findings

_None._ The elastic-trivia finding of `05-implementer.md` is fixed for the structural transforms, as the decision chose. The pipeline-wide guarantee stays a follow-up candidate (`06-spec-author.md`).

## Follow-up candidates

Unchanged from `05-implementer.md`, minus the items the decisions settled:

- `WouldMergeWhenTight` counts `@` as merging, so the created `return` gets a space in front of `@x` that the parser does not need.
- `ExtractSignificantTrivia` still strips the whitespace in front of the comments it moves behind `{` and in front of the created `;`.
- In a nested unbraced `if`, the inner inserted `}` can get a line break even though the outer inserted `}` follows it.
- A moved UTF-8 raw literal token carries its value as a string.
- Property and indexer accessor-list braces are still placed by two paths.
