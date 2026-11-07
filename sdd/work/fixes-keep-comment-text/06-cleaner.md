Role: cleaner
Commit: (this commit; see `git log -1 -- sdd/work/fixes-keep-comment-text/06-cleaner.md`)
Verdict: clean

## Formatted
_None._ `scripts/format.sh` over the 14 changed C# files: 0 changed (and 0 of 1 after the edit below).

## Warnings resolved
| Warning | Location | Form-only change |
|---|---|---|
| S3267 (twice, lines 39 and 47) | `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider.cs`, `FindLineEndingTrivia` | The two loops became `SemicolonToken.TrailingTrivia.Concat(nextToken.LeadingTrivia).FirstOrDefault(SyntaxTriviaUtilities.EndsLine)`; added `using System.Linq;`. Same first-match order, same `default` result |

## Unresolved
_None._

## Checks
- `scripts/build.sh`: 0 Warning(s), 0 Error(s).
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5011|FullyQualifiedName~RH5030"`: Passed 77, Failed 0.
