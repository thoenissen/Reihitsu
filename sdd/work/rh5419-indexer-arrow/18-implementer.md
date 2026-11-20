Role: implementer
Commit: 37635a0
Verdict: tests pass

Repair round 3: auditor finding #3 (`16-auditor.md`, FC-015). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the finding. Finding #4 is the spec author's and test implementer's; the spec and change doc were not touched here.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/AttributeListCodeFixUtilities.cs` | New `StandsOnOneLineKeepingBlankLines(MemberDeclarationSyntax member, MemberDeclarationSyntax formatted)`: the start token behind the attribute lists, the line-break test through `formatted.Span.End`, and `KeepsBlankLinesBehindAttributeLists`; class summary names the one-line check | RH5401-004, RH5419-004, FC-015 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider.cs` | `FormatsOnOneLineKeepingBlankLines` formats the property and calls the shared check; private start-token and line-break code removed; unused `using System.Linq` removed; summary points at the shared method | RH5401-004 |
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Layout/RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedCodeFixProvider.cs` | `FormatsOnOneLineKeepingBlankLines` joins and formats the indexer, then calls the shared check; private start-token and line-break code removed; summary points at the shared method | RH5419-004 |

The span end is now `formatted.Span.End` for both rules. For an expression-bodied indexer that is the end of its `;`, the same end the RH5419 provider measured before (`SemicolonToken.Span.End`), so neither rule's behavior changes.

## Policy owners
- One-line span of a single-line fix for an expression-bodied member: `rg -n "AttributeLists\.Last\(\)\.GetLastToken\(\)\.GetNextToken\(\)" src --glob '!*Test*'` now finds `AttributeListCodeFixUtilities.cs:46` (the one owner, called by both providers) and `ConditionalGroupUnitUtilities.cs:233`. The latter matches a token against the first token behind a member's attribute lists to find the unit lists of a type body, a different decision, and lives in Core; not merged. `DeclarationModifierUtilities.GetFirstTokenBehindAttributeLists` (private, Core) returns the first modifier first and serves modifier insertion; also a different decision, not merged. Making either public to share a one-line ternary would widen Core's surface for no shared policy.
- `rg -n "IndexOf\('\\\\n'" src/Reihitsu.Analyzer.CodeFixes`: only the shared method among the two providers.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` after this commit: `8 files changed, 461 insertions(+), 108 deletions(-)` (before this round: `8 files changed, 440 insertions(+), 89 deletions(-)`). Round delta: 3 files, 38 insertions, 36 deletions. No new type; one new internal method on an existing internal class.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5419|FullyQualifiedName~RH5401"`: `Passed! - Failed: 0, Passed: 218, Skipped: 0, Total: 218`.
- `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198`.
- `scripts/build.sh` (Release): `0 Warning(s)`, `0 Error(s)`.
- `scripts/format.sh` over the three files: `Formatted 0 of 3 file(s).`

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
