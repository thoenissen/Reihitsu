Role: implementer
Commit: 5a6d3e1
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The change follows lead decision 2: `UnionDeclaration` is added to the kind lists this run's requirements need, nothing is generalized to `TypeDeclarationSyntax`, and extension blocks and `Reihitsu.Core/AttributeTargetUtilities` are untouched.

All handed-over tests were failing before the change as `04-test-implementer.md` lists (30 formatter, 5 analyzer, 1 CLI); no test file was changed.

## thoenissen/Reihitsu#1762

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/DeclarationBraceLineBreakRewriter.cs` | `UnionDeclarationSyntax` added to the type-declaration case of `Visit` | FMT-BREAK-002 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/MemberLineStartRewriter.cs` | new `VisitUnionDeclaration`, same body as the other type kinds | FMT-BREAK-007 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs` | new `VisitUnionDeclaration`: the case-type list's `(` joins the name like a primary constructor's | FMT-BREAK-011 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/GenericConstraintLineBreakRewriter.cs` | union arm in `GetConstraintClauseAdapter` | FMT-BREAK-023 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/EmptyTypeDeclarationSemicolonTransform.cs` | new `VisitUnionDeclaration`, gated at C# 15 through the shared `CanConvertSafely` (which keeps braces holding a comment or directive, FMT-STRUCT-018) | FMT-STRUCT-017, FMT-STRUCT-018 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/FieldDeclarationSplitTransform.cs` | `Splits` admits a union parent; new `VisitUnionDeclaration`; summary updated (FC-031) | FMT-STRUCT-021, FMT-PIPE-031 |
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` | comments only: `SplitsFieldDeclaration` summary and the `FormatSplitFieldInContext` cast comment name the union (FC-031) | FMT-PIPE-031 |

The RH7101 fix is now offered on a union field with no change in the code fix: it already asks `ReihitsuFormatter.SplitsFieldDeclaration`. The RH5404 result changes only through the formatter.

### Policy owners
- `rg -ln "VisitRecordDeclaration|RecordDeclarationSyntax or InterfaceDeclarationSyntax|case RecordDeclarationSyntax" src --type cs -g '!*Test*'` → the six formatter sites above plus `src/Reihitsu.Core/AttributeTargetUtilities.cs` (left as is: follow-up candidate (a), lead decision 2). The seven sites match the skeptic's table; no private copy was added, each site stays the single owner of its decision.
- Empty-body eligibility: owned by `Reihitsu.Core/EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely` (kind-agnostic), reused.

## thoenissen/Reihitsu#1763

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/HorizontalSpacing/Rules/WithSpacingRule.cs` | new `IsWithElementKeyword` (`WithKeyword` whose parent is `WithElementSyntax`); `DesiredSpacesAfter` returns 0 for it as left token; class summary names FMT-SPACE-028 | FMT-SPACE-028 |

The rule sits in the existing `with` rule, at the place FMT-SPACE-002's order lists FMT-SPACE-025 and FMT-SPACE-028 together. A comment in the gap is handled by the phase's existing two-sided gap logic (FMT-SPACE-013/014), confirmed by `CommentInTheGapStaysAndTheSpaceBehindItIsRemoved`.

### Policy owners
- `rg -n "WithKeyword" src --type cs -g '!*Test*'` → `WithSpacingRule` is the only spacing owner of the `with` token; the other hits are `AccessorExpressionBodyTransform` (Roslyn `WithKeyword(...)` method, unrelated), `ObjectInitializerContributor` (indentation of a `with` expression's initializer), and RH3002 (an analyzer). No other spacing rule decides the pair.

## Whole run

### Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` → 8 files changed, 77 insertions(+), 7 deletions(-). No new type, no new public member, no new dependency.

### Deleted special cases
_N/A._ (no shape declared, lead decision 5)

### Tests run
All in Debug with `--no-install`:
- `scripts/test.sh --project formatter --filter "FullyQualifiedName~UnionDeclarationLayoutTests|FullyQualifiedName~UnionDeclarationStructuralTests|FullyQualifiedName~WithElementSpacingTests|FullyQualifiedName~ReihitsuFormatterSplitsFieldDeclarationTests|FullyQualifiedName~WithExpressionSpacingTests"` → Passed 110/110
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~VerifyCombinedStaticFieldInAUnion|FullyQualifiedName~VerifyCombinedFieldsInAUnionAreSplitTogether|FullyQualifiedName~VerifyFixLaysOutAUnionInsideTheFixedType|FullyQualifiedName~VerifyFixRemovesTheSpaceInFrontOfTheArgumentListOfAWithElement"` → Passed 5/5
- `scripts/test.sh --project cli --filter "FullyQualifiedName~CliLanguageVersionEndToEndTests"` → Passed 16/16
- Whole projects: formatter 5238/5238, analyzer 10141/10141, cli 303/303, core 718/718, architecture 118/118.
- `scripts/format.sh` over the eight changed files: formatted 0 of 8.

### Disputed tests or scope
_None._

### Architecture findings
_None new._ The kind-list duplication the skeptic reported stands (now seven sites, six of them listing unions; extension blocks still differ between rewriters); per lead decision 2 it was fixed narrowly and the consolidation stays a follow-up.

### Follow-up candidates
_None_ beyond those already in `00-lead.md`.
