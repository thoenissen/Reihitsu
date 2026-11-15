Role: implementer
Commit: 5e6cbda
Verdict: #1680: tests pass; #1742: tests pass (no production change)

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1680

### Changes

All paths under `src/Reihitsu.Analyzer/`.

| File | What changed | Requirement |
|---|---|---|
| `Base/StructuredTriviaAnalysisContextExtensions.cs` (new) | `RegisterStructuredTriviaAction` and `RegisterStructuredTriviaActionWithDocumentationModeCheck` register a syntax-node action on structured-trivia kinds, wrapped in one guard: a callback whose `ContainingSymbol` is a constructor declared by a `TypeDeclarationSyntax` in the node's tree is skipped, unless the node lies inside the full span of the type's `ParameterList` or of its `PrimaryConstructorBaseTypeSyntax.ArgumentList` (the nodes only that run sees, which stay reported once) | RH8204-002, RH8304-003, RH8305-003, RH8307-003, RH8308-003, RH8309-003, RH7302-001/-003, RH7310-003, SHARED-REGDESC-002; lead decision 4 |
| `Rules/Documentation/RH8204…`, `RH8304…`, `RH8305…`, `RH8307…`, `RH8308…`, `RH8309…Analyzer.cs` | Registration on the trivia kind switched to `RegisterStructuredTriviaActionWithDocumentationModeCheck` | as above |
| `Rules/Organization/RH7302…`, `RH7310…Analyzer.cs`, `Base/RegionDescriptionAnalyzerBase.cs` (RH7306, RH7307) | Registration on `RegionDirectiveTrivia`/`EndRegionDirectiveTrivia` switched to `RegisterStructuredTriviaAction` | as above |
| `RH8305…Analyzer.cs`, `RH8307…Analyzer.cs` | Private `IsPrimaryConstructorRun` calls removed (FC-015) | lead decision 4 |
| `Core/DocumentationAnalysisUtilities.cs` | `IsPrimaryConstructorRun` deleted (no caller left); its too-wide condition dropped the parameter-list nodes | RH8305-003, RH8307-003 |

### Policy owners

- `rg -n "IsPrimaryConstructorRun" src sdd/specs` — none left.
- `rg -n "DeclaringSyntaxReferences.*TypeDeclarationSyntax|ContainingSymbol is IMethodSymbol" --glob '!*Test*' src` — only `SingleLetterIdentifierUtilities.cs:31` and `RH4122…Analyzer.cs:48`, which inspect a parameter symbol's owner, a different decision.
- Every registration on a structured-trivia kind in `src/Reihitsu.Analyzer` (`rg -n "Register\w*Action" …`, skeptic's table) now goes through the new extension: RH8204, RH8304, RH8305, RH8307, RH8308, RH8309, RH7302, RH7310, `RegionDescriptionAnalyzerBase` (RH7306, RH7307). `rg -n "RegisterSyntaxNodeAction\w*\([^)]*(Trivia|SyntaxKind\.Xml)"` finds only RH8201, whose action is named `OnDocumentationCommentTrivia` but is registered on declaration kinds.

### Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 11 files changed, 132 insertions(+), 26 deletions(-). New type: `StructuredTriviaAnalysisContextExtensions` (internal static extension class, ~115 lines with documentation; the one owner of the guard).

### Deleted special cases

_N/A._

### Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "<the twelve classes of 03-test-implementer.md>"` — `Passed! - Failed: 0, Passed: 419, Total: 419` (the 25 reproductions now pass, including the two RH8305/RH8307 parameter-list cases that were 0 vs 1).
- `scripts/test.sh --configuration Debug --project analyzer` — `Passed! - Failed: 0, Passed: 9379, Total: 9379`.
- `scripts/test.sh --configuration Debug --project architecture` — `Passed! - Failed: 0, Passed: 118, Total: 118`.
- `scripts/build.sh --configuration Debug` — 0 errors; the one warning (RH5603 on the new file's final newline) is fixed. `scripts/format.sh --force` over the changed files formatted nothing.

### Disputed tests or scope

- New type `StructuredTriviaAnalysisContextExtensions` (scope report, `implementer.md` *Scope*). Chosen because the region rules do not use `DocumentationModeAnalysisContextExtensions`, and putting the guard in the registration rather than in a predicate called by each analyzer leaves one owner (lead decision 4). The alternative without a new type — keep a predicate in `DocumentationAnalysisUtilities` and call it in ten analyzers — repeats the private-call pattern the skeptic reported.
- The guard's edge is the full span of the parameter list and of the base-type argument list (including the leading trivia of their `(`). Covered by tests for the parameter list (RH8305, RH8307, RH8308, RH7302); the base-type argument list is not tested (03-test-implementer.md, *Not covered*), and its exact driver behavior for the trivia in front of its `(` was not probed.

### Architecture findings

_None_ beyond the skeptic's, which this change resolves for #1680.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1742

No production change: maintainer answer 1 (B) keeps today's output and SHARED-DIAG-013 writes it down. The five characterization tests of 03-test-implementer.md pass on the changed tree (part of the 419 above). The new guard does not touch a partial constructor's parts: its condition requires a constructor declared by a type declaration.

## Whole run

Commit 5e6cbda carries the production change; this handover is committed after it.
