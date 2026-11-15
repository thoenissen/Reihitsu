Role: implementer
Commit: 981c3eb
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Utilities/UsingLeadingTriviaBuilder.cs` | `SplitOriginalFirstHeaderTrivia`: when the header ends its last line (`SyntaxTriviaUtilities.EndsLine`, which also covers a `///` comment carrying its own line break) and only whitespace follows, that whitespace is left in the remainder instead of the header, so the original first directive keeps the whitespace in front of its own line when it moves away from the top. Summary and the `firstLeadingTriviaPrefix` parameter doc of `CreateLeadingTrivia` updated (FC-031) | FMT-USING-011 |
| `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Rewriter/UsingDirectiveOrderingRewriter.cs` | `OrganizeUsingDirectives`: when the split-off header ends its line, the first-position prefix gets the original first directive's line whitespace (`scopeIndentation`, read as written by `UsingDirectiveOrderingUtilities.GetLineIndentation`, tab included) behind the header, so the directive or comment taking the first position starts there on both sides of the FMT-USING-009 split | FMT-USING-011 |

A header that does not end its line (c3, `/* c */ using …;` on the directive's line) is unchanged: the split keeps it whole and nothing is appended, as before.

## Policy owners
- Line-end test of a trivia: `rg -n "static bool \w*(EndsLine|EndsWithLineBreak)" src/Reihitsu.Core src/Reihitsu.Formatter` → `SyntaxTriviaUtilities.EndsLine` / `EndsWithLineBreak` (Core); reused, no private copy written.
- Indentation of the original first directive's line: `rg -n "GetLineIndentation" src` → `UsingDirectiveOrderingUtilities.GetLineIndentation` (Core), already wrapped by the rewriter's `GetScopeIndentation`; reused.
- Header split: `rg -n "SplitOriginalFirstHeaderTrivia" src` → one owner, `UsingLeadingTriviaBuilder`, one production caller (the rewriter).

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (with the working changes): 2 files changed, 30 insertions(+), 2 deletions(-). No new type, no new public member.

## Deleted special cases
_N/A._

## Tests run
```
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ReihitsuFormatterUsingDirectivesTests|FullyQualifiedName~UsingDirectiveRequirementTests|FullyQualifiedName~UsingLeadingTriviaBuilderTests"
  Passed!  - Failed: 0, Passed: 54
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7201|…|FullyQualifiedName~RH7207"
  Passed!  - Failed: 0, Passed: 305
scripts/test.sh --configuration Debug --project formatter
  Passed!  - Failed: 0, Passed: 5059
scripts/test.sh --configuration Debug --project analyzer
  Passed!  - Failed: 0, Passed: 5951
scripts/test.sh --configuration Debug --project architecture
  Passed!  - Failed: 0, Passed: 118
```
`scripts/format.sh` over both changed files: 0 of 2 reformatted. A first attempt that tested only `EndOfLineTrivia` failed `SingleLineDocumentationHeaderChangesNoDirectiveIndentation` (c1, `///` carries its line break inside its structure); the shared `EndsLine` fixed it.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
