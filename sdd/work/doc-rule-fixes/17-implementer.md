Role: implementer
Commit: the commit that adds this handover (subject "Share one reader of a declaration's documentation comments")
Verdict: tests pass

Worklist: auditor 15 finding #6 (decision 24). Refactor only; no behavior change, no test or assertion changed. The inline comment at RH8203 analyzer :83–85 (finding #5) is the cleaner's and was left untouched.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Core/DirectDocumentationSyntaxChecker.cs` | New `GetDocumentationComments(SyntaxNode)`: every `DocumentationCommentTriviaSyntax` structure in the declaration's leading trivia, in source order, as `ImmutableArray`. `GetDocumentationComment` is now its first element. | FC-015 (one owner); SHARED-DOC-004 selection unchanged |
| `src/Reihitsu.Analyzer/Core/XmlDocumentationExpander.cs` | `GetExpandedDocumentationOfPart` builds the placeholder text from `GetDocumentationComments` instead of filtering the trivia by kind. Equivalent: only the two documentation-comment trivia kinds have a `DocumentationCommentTriviaSyntax` structure, and a structured trivia's full text is its structure's full text. | RH8203-001 (unchanged) |
| `src/Reihitsu.Analyzer/Rules/Documentation/RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.cs` | The partial-part branch reads its comments through `GetDocumentationComments` instead of its own `Select/OfType/ToList`. | RH8203-001 (unchanged) |

## Policy owners
- `rg -n "OfType<DocumentationCommentTriviaSyntax>\(\)" src/Reihitsu.Analyzer src/Reihitsu.Analyzer.CodeFixes`: only `DirectDocumentationSyntaxChecker.GetDocumentationComments` selects from a declaration's leading trivia now; the other hit (RH8401 code fix :110) walks ancestors of a token, a different question.
- `rg -n "SingleLineDocumentationCommentTrivia" ...`: `DocumentationAnalysisUtilities.GetDocumentationCommentsBehindCode` (behind-code comments over a whole root), RH8303, RH8401, RH5203 code fix: each a different policy, none selects a declaration's documentation comments.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this commit: 19 files changed, 400 insertions(+), 103 deletions(-). Round delta: 3 files changed, 14 insertions(+), 9 deletions(-). One new internal method, no new type.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH8203"`: Passed! Failed 0, Passed 34.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed! Failed 0, Passed 8939.
- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors. `scripts/format.sh` on the three files: 0 of 3 changed.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
