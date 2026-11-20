Role: implementer
Commit: 92678eb
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/LineEndingUtilities.cs` | New `GetLineBreakTrivia(SyntaxNode)` (end-of-line trivia, `descendIntoTrivia: true`) and `GetLineBreakTokens(SyntaxNode)` (documentation newline tokens, `descendIntoTrivia: true`): the one statement of which trivia and tokens carry a line break, kind and traversal together. `DetectEndOfLine` and `NormalizeLineEndings` now walk through them; their bucket rule and two-step replacement are unchanged. Summaries of the class and both methods updated (FC-031) | FMT-PIPE-007, FMT-EOL-001..004 (unchanged); FC-015 |
| `src/Reihitsu.Analyzer/Rules/Layout/RH5604CodeMustNotContainMixedLineEndingsAnalyzer.cs` | Both walks use the shared enumerations; the "mixed" rule (distinct texts), the call to `DetectEndOfLine` for the predominant ending, and the per-line location stay its own (decision 2, skeptic internal choice 2). Unused `Microsoft.CodeAnalysis.CSharp` using removed; summary describes the method | RH5604-001..005 (unchanged) |

Internal choice 1 (shape): option (a)-like — two shared enumerations, one per carrier sort, so the normalization keeps `ReplaceTokens` then `ReplaceTrivia` on the token-normalized node. The traversal flag is part of the shared definition, not only the kind.

## Policy owners
- `rg -n "XmlTextLiteralNewLineToken" src --glob '!*Test*/**'`: `LineEndingUtilities.GetLineBreakTokens` (the only line-ending carrier statement left); `RH8308...Analyzer.cs:90` (reads doc-comment line structure, a different decision, skeptic *Surfaces*); `ArchitectureTests/.../PhaseOwnershipDocumentationTests.cs:330` (guard-name fixture string); `Formatter.Test/Helpers/ObservedPipeline.cs:174` (test helper).
- `rg -n "DescendantTrivia\(descendIntoTrivia: true\)" src --glob '!*Test*/**' | rg EndOfLine`: only `LineEndingUtilities.GetLineBreakTrivia`.
- Other `IsKind(SyntaxKind.EndOfLineTrivia)` uses in production (about 80) test single trivia in a gap for layout decisions, not the file's line-ending carrier set; the skeptic checked them as not statements of this rule.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 2 files changed, 53 insertions(+), 31 deletions(-). No new type; two new public static methods on the existing public `LineEndingUtilities` (Core is referenced by analyzer and formatter, so the analyzer can use them).

## Deleted special cases
_N/A._

## Tests run
Debug configuration, after `scripts/format.sh` on both files (0 of 2 changed):
- `scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5604CodeMustNotContainMixedLineEndingsAnalyzerTests"`: Passed 79/79
- `scripts/test.sh --project formatter --filter "FullyQualifiedName~LineEnding"`: Passed 95/95
- `scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterHelpersTests"`: Passed 18/18
- `scripts/test.sh --project core --filter "FullyQualifiedName~LineEndingUtilitiesTests"`: Passed 28/28
- Full projects: core 775/775, architecture 198/198, analyzer 11452/11452, formatter 6441/6441

The handed-over tests are characterization tests that passed before the change (decision 1); they still pass.

## Disputed tests or scope
_None._ The two new public members are within the change the skeptic scoped (shared definition in `Reihitsu.Core`).

## Architecture findings
_None_ beyond the issue's own FC-015 finding, which this change resolves: the carrier set is now stated once.

## Follow-up candidates
_None_ new. The skeptic's FC-040 candidate (RH5604 under `DocumentationMode.None`) stands; this change does not affect it.
