Role: implementer
Commit: 0f58dc2
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. No test, input, or expected value was changed.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/UsingDirectiveOrderingSafety.cs` | `CanSafelyReorder` checks the leading trivia of each token of a directive that `ContainsDirectives`. In the first token of the block's first directive, only a conditional directive makes the block unsafe. In any other token, any directive does. The XML summary is rewritten to match. | SHARED-USING-009 |
| `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Utilities/UsingLeadingTriviaBuilder.cs` | `GetHeaderSplitIndex` returns the end of the list when the leading trivia holds a directive (`trivia.IsDirective`). The whole significant trivia then becomes the header that stays at the top. The trailing whitespace on the directive's own line still goes to the remainder through the existing line-start logic in `SplitOriginalFirstHeaderTrivia` (FMT-USING-011). The summaries of both methods are updated. | FMT-USING-012, FMT-USING-009 (its directive exception) |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.cs` | Comment-only change to the `IsOrganized` summary. It said "a block that holds a preprocessor directive" and now says "a block that may not be reordered (SHARED-USING-009)", citing FMT-USING-004. The code is unchanged. | SHARED-USING-009, RH7207-001 |

The analyzers, the code-fix providers (`UsingDirectiveOrderingCodeFixProviderBase`, the RH7207 provider, `UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync`), and the formatter rewriter all reach the narrowed guard through `CanSafelyReorder`. None of them needed a change.

## Policy owners
- `rg -n "IsConditionalDirective" src` finds `SyntaxTriviaUtilities.IsConditionalDirective` in Core. The predicate reuses it; I removed a private copy I had first written.
- `rg -n "CanSafelyReorder" src` finds one owner, `UsingDirectiveOrderingSafety`. Its consumers: the RH7207 analyzer, `UsingDirectiveCodeFixUtilities`, `UsingDirectiveOrderingCodeFixProviderBase`, the RH7207 code-fix provider, and `UsingDirectiveOrderingRewriter`.
- `rg -n "SplitOriginalFirstHeaderTrivia|GetHeaderSplitIndex" src` finds one owner, `UsingLeadingTriviaBuilder`. Its one caller is `UsingDirectiveOrderingRewriter.DetachHeaderOfOriginalFirst`.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src` (working tree included): 3 files changed, 48 insertions(+), 8 deletions(-). No new type and no new public member.

## Deleted special cases
_N/A._

## Tests run
All in Debug:
- `scripts/test.sh --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.UsingDirectiveOrderingSafetyTests"`: Passed 64/64
- `scripts/test.sh --project formatter --filter "FullyQualifiedName~UsingDirectiveHeaderDirectiveTests|FullyQualifiedName~ReihitsuFormatterUsingDirectivesTests|FullyQualifiedName~UsingDirectiveRequirementTests"`: Passed 72/72
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH720|FullyQualifiedName~RH7207"`: Passed 431/431
- Full projects:
  - core: Passed 935
  - formatter: Passed 7572
  - analyzer: Passed 12482
  - cli: Passed 467, Skipped 1 (`OnWindowsADirectoryDifferingOnlyInCaseIsTheSameDirectory`, platform-conditional)
  - architecture: Passed 221
- After the final edit, which swapped in the shared helper and does not change behavior, I re-ran:
  - core in full: 935 passed
  - the analyzer RH720x/RH7207 filter: 431 passed
  - the formatter `UsingDirective` filter: 250 passed
- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors
- `scripts/format.sh` over the 3 changed files: 0 files changed

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
