Role: implementer
Commit: 874cd54
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Before the change, the analyzer test project did not compile (missing `RH4012UnionNameCasingAnalyzer`, `RH4012UnionNameCasingCodeFixProvider`, `RH8035NonPrivateUnionsMustBeDocumentedAnalyzer`, `RH8036PrivateUnionsMustBeDocumentedAnalyzer`, and the `RH4012/RH8035/RH8036MessageFormat` resources), as `04-test-implementer.md` states.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Naming/RH4012UnionNameCasingAnalyzer.cs` (new) | `CasingAnalyzerBase` on `SyntaxKind.UnionDeclaration`, PascalCase, location = union identifier | RH4012-001 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Naming/RH4012UnionNameCasingCodeFixProvider.cs` (new) | `CasingCodeFixProviderBase<UnionDeclarationSyntax>` with `ToPascalCase`; collision, binding-change, and Fix All handling come from the shared base | RH4012-002, SHARED-CASING-006–010 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8035NonPrivateUnionsMustBeDocumentedAnalyzer.cs` (new) | `SplitElementDocumentationAnalyzerBase`, non-private group, `UnionDeclaration` | RH8035-001/002, SHARED-DOC-001/002 |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8036PrivateUnionsMustBeDocumentedAnalyzer.cs` (new) | Same, private group | RH8036-001/002, SHARED-DOC-001/002 |
| `src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs` | `UnionDeclaration` added to `DocumentableDeclarationKinds` (so also `SummaryDocumentationKinds`) and `TypeParameterOwnerKinds`; `ParameterOwnerKinds` deliberately unchanged, its summary now says why | RH8030-001, RH8203-001, RH8108–RH8111-001; RH8101–RH8104 non-goals |
| `src/Reihitsu.Analyzer/AnalyzerResources.resx`, `.cs` | `RH4012Title/MessageFormat`, `RH8035Title/MessageFormat`, `RH8036Title/MessageFormat` (texts = the rule titles) | — |
| `src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx`, `.cs` | `RH4012Title` = "Fix casing" | — |
| `src/Reihitsu.Analyzer.Package/README.MD` | Rows for RH4012 (analyzer, code fix), RH8035, RH8036 | — |
| `Release.ruleset` | `RH4012`, `RH8035`, `RH8036` as `Error`, like their siblings | — |
| `Reihitsu.sln` | Solution items for the three new rule pages | — |

The consumers of the two widened lists are exactly the intended rules: `rg -n "DocumentableDeclarationKinds|SummaryDocumentationKinds|TypeParameterOwnerKinds" src --glob '!*Test*/**'` → RH8030 (`SummaryDocumentationKinds`), RH8203 (`DocumentableDeclarationKinds`), RH8108, RH8109, RH8110, RH8111 (`TypeParameterOwnerKinds`). Type-parameter lookup (`TypeDeclarationSyntax { TypeParameterList: not null }`) and `CanInheritDocumentation` (`TypeDeclarationSyntax.BaseList`) already handle a union without change.

Rule pages RH4012/RH8035/RH8036 were already written by the spec author; not changed.

## Policy owners
- Type-name casing: `rg -n "CasingAnalyzerBase|CasingCodeFixProviderBase<" src/Reihitsu.Analyzer/Rules/Naming src/Reihitsu.Analyzer.CodeFixes/Rules/Naming` — every type-casing rule uses the shared `CasingAnalyzerBase` / `CasingCodeFixProviderBase<TNode>`; RH4012 reuses both, no private copy.
- Element documentation presence: `rg -n "SplitElementDocumentationAnalyzerBase" src/Reihitsu.Analyzer/Rules/Documentation` — RH8001–RH8016 and now RH8035/RH8036 all derive from it; accessibility grouping stays in `DocumentationAnalysisUtilities.HasImplicitPrivateAccessibility`.
- Documentable kinds: `rg -n "UnionDeclaration" src --glob '!*Test*/**'` — the only union kind lists are in `DocumentationAnalysisUtilities` (plus the new rules and the RH5404 comment, which is #1762's).

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (with this commit): 10 files changed, about 230 insertions, 1 deletion — four new rule types (two analyzers of ~35 lines, one casing analyzer and one fix of ~50 lines each, all on existing bases), resources, README rows, two list entries. No new base type, helper, or public member beyond the rule classes.

## Deleted special cases
_N/A._

## Tests run
- `scripts/build.sh --configuration Debug` → Build succeeded, 0 warnings, 0 errors.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Union|FullyQualifiedName~RH4012|FullyQualifiedName~RH8035|FullyQualifiedName~RH8036"` → Passed: 50, Failed: 0.
- `scripts/test.sh --configuration Debug --project analyzer` → Passed: 10186, Failed: 0.
- `scripts/test.sh --configuration Debug --project architecture` → Passed: 118, Failed: 0.
- `scripts/format.sh` over the seven changed/new `.cs` files → Formatted 0 of 7.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._ (The stale RH5404 comment about unions is already noted for #1762 in `00-lead.md`.)
