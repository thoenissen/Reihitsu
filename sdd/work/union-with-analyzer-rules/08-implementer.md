Role: implementer
Commit: 0fb675a (base; the commit carrying this handover follows it)
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Layout/RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzer.cs` (new) | Empty-type base with `SyntaxKind.UnionDeclaration`, `LanguageVersion.CSharp15` | RH5418-001 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5418EmptyUnionsShouldUseSemicolonDeclarationsCodeFixProvider.cs` (new) | Empty-type fix base, same kind and version | RH5418-001, SHARED-EMPTY-003/004 |
| `src/Reihitsu.Analyzer/Rules/Design/RH2110NestedUnionsShouldNotBeUsedAnalyzer.cs` (new) | Reports a nested `UnionDeclarationSyntax` through `NestedTypeAnalyzerHelper`, as RH2104 does for structs | RH2110-001 |
| `AnalyzerResources.resx/.cs`, `CodeFixResources.resx/.cs` | `RH2110Title`/`MessageFormat`, `RH5418Title`/`MessageFormat`, code-fix `RH5418Title` | — |
| `Release.ruleset`, `Reihitsu.sln` | RH2110 and RH5418 entries; rule pages as solution items | — |
| RH2004, RH5404, RH7102, RH7103, RH7104, RH7105, RH7106, RH7109, RH7110, RH7308, RH7309, RH7311 analyzers; `TypesOrganizedWithRegionsAnalyzerBase` (RH7305/RH7305A), `MembersShouldBeGroupedByRegionsAnalyzerBase`, `NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase` (RH7401–RH7412) | `SyntaxKind.UnionDeclaration` added to the registered kinds; RH5404's comment that unions are deliberately absent removed | RH2004-001/006, RH5404-001, RH7102-001, RH7103-001, RH7104-001, RH7109-001, RH7110-001, SHARED-MODORDER-001, SHARED-TYPEREGION-001, RH7308-001, RH7309-002, RH7311-001, SHARED-REGGROUP-001 |
| `src/Reihitsu.Analyzer/Core/ParameterListParentPolicy.cs` | `UnionDeclarationSyntax` covered (RH5105, RH5106) | RH5105-001, RH5106-001 |
| `RH5108ParameterListMustFollowDeclarationAnalyzer.cs` | New `OnUnionDeclaration` for the case-type list | RH5108-001 |
| `RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.cs` | New `OnUnionDeclaration` for constraint clauses | RH5110-001 |
| `src/Reihitsu.Core/AttributeTargetUtilities.cs` | `TryResolveImplicitTarget`: a union resolves to `AttributeTargets.Struct`; this also drives the formatter split (FMT-STRUCT-028) and RH5507/RH5508 | SHARED-ATTR-001, FMT-STRUCT-028 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributePlacementCodeFixProviderBase.cs` | `HasUnusableEnclosingScope` skips a type without braces (open-brace token of kind `None`); the union `[A] union U(int, string);` after other code crashed with a `NullReferenceException` (its default brace token has no syntax tree and position 0). Summary updated (FC-031) | RH5507-003 |

## Policy owners

- Type-kind lists: `rg -n "SyntaxKind.RecordStructDeclaration" src/Reihitsu.Analyzer src/Reihitsu.Analyzer.CodeFixes src/Reihitsu.Core` — each registration is its own owner (the duplication is the architecture finding already in `00-lead.md`, decision 8); every list of a rule the change doc names now carries the union. Left as is by spec: RH2103, RH2104, RH2106, RH5413, RH5416, RH4003/RH4011, RH8003/RH8004/RH8009/RH8010 (unions have their own rules or are non-goals).
- Attribute target: `rg -n "AttributeTargets.Struct" src/Reihitsu.Core src/Reihitsu.Analyzer src/Reihitsu.Formatter` — `AttributeTargetUtilities.TryResolveImplicitTarget` is the single owner; the formatter split reads it.
- Parameter-list parents: `ParameterListParentPolicy.IsClosingParenthesisCovered` is the single owner for RH5105/RH5106.
- Empty-type decision: `EmptyTypeDeclarationSemicolonAnalysisUtilities` (Core), reused unchanged by RH5418 and the formatter.

## Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this commit: 1 file, +2. This commit: 27 files changed, 241 insertions(+), 16 deletions(-) (about 120 lines are the three new rule files, the rest resources and one-line kind additions). New types: `RH2110NestedUnionsShouldNotBeUsedAnalyzer`, `RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzer`, `RH5418EmptyUnionsShouldUseSemicolonDeclarationsCodeFixProvider` (the rules the maintainer asked for, Q3/Q4).

## Deleted special cases

_N/A._

## Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "<07-test-implementer.md filter>|FullyQualifiedName~VerifyFixLaysOutAUnionInsideTheFixedType"`: Passed 216/216 (54 failed before the change).
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~UnionDeclarationStructuralTests|FullyQualifiedName~ListOfEachDeclarationOwnerIsSplitWithNothingBetweenTheCreatedLists"`: Passed 35/35.
- `scripts/test.sh --configuration Debug` (all local projects): Analyzer 10394, Formatter 5243, Core 738, Cli 303, Architecture 118 (Sdd checks included), Tooling 126, Playground 23 — all passed, none failed.
- `scripts/format.sh` over the changed `.cs` files: 0 of 25 reformatted. `scripts/lane-check.sh behavior-change`: confirmed.

## Disputed tests or scope

- `TargetAttributePlacementCodeFixProviderBase.HasUnusableEnclosingScope` is outside the files the handovers name. It was needed: the RH5507 union test crashed with a `NullReferenceException` there, because a union declared with a semicolon has a default open-brace token. The change is a two-line skip of a brace pair whose open token has kind `None`; for a class, struct, or record declared with a semicolon the brace pair never enclosed the attribute list, so their behavior is unchanged (all tests green).

## Architecture findings

_None_ new; the duplicated type-kind lists remain as recorded in `00-lead.md` (decision 8).

## Follow-up candidates

_None_ beyond those in `00-lead.md`.
