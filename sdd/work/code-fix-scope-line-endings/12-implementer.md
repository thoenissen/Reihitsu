Role: implementer
Commit: (this commit; see `git log -1 -- sdd/work/code-fix-scope-line-endings/12-implementer.md`)
Verdict: tests pass

Repair round 2: auditor findings #1, #2, #3 (`09-auditor.md`), decisions 14, 15, 18 (`00-lead.md`), RH3001-009 (`10-spec-author.md`), tests from `11-test-implementer.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at the auditor's suggestions, which I re-derived rather than followed (see #2 below).

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3001NotOperatorShouldNotBeUsedCodeFixProvider.cs` | New `TouchesWord` / `IsWord`: the fix is not registered when the token in front of `!` is a keyword or identifier (`SyntaxFacts.IsKeywordKind`, contextual keywords included, or `IdentifierToken`) and neither that token's trailing trivia nor `!`'s leading trivia holds anything, or when the token behind the negation's last token is one and neither gap side holds trivia. Called from `RegisterCodeFixesAsync` next to the RH3001-008 check; the comment there names RH3001-009. | RH3001-009, RH3001-004 |
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | Finding #2: the private `FormatNodeAsync` becomes the one shared wrapper `FormatNodeInDocumentAsync` (public in the internal class). Finding #3: the line-separation description moved to the four-argument overload, which does the work; it now says that off a member line no gap is broken and the separators stay as written (RH3202-002, RH5417-003), instead of "only the member is formatted". The three-argument overload's summary now says it formats the whole member (RH5417-003), and points at the four-argument one. | FC-015, FC-031 |
| `src/Reihitsu.Analyzer.CodeFixes/Core/ExpressionBodyCodeFixUtilities.cs` | Its private `FormatNodeAsync` copy removed; the three-argument `ConvertBodyAsync` passes `BlankLineCodeFixUtilities.FormatNodeInDocumentAsync`. The `using Reihitsu.Formatter;` it no longer needs is removed. | FC-015 |

Notes on #2: the wrapper had to live in one of the two helpers (no new type, per the round's scope). `BlankLineCodeFixUtilities` keeps it because it is the older helper that already had the formatter call on `main`; the cost is a new dependency `ExpressionBodyCodeFixUtilities` → `BlankLineCodeFixUtilities` inside the same assembly. The opposite direction would work equally; the behavior is identical either way.

## Policy owners
- Word touching the negation (RH3001-009): `rg -n "IsKeywordKind" src --glob '*.cs' --glob '!*Test*'` — only the new RH3001 check. Related owner considered: `rg -n "WouldMergeIntoOneToken" src --glob '*.cs' --glob '!*Test*'` finds `Reihitsu.Core.TokenSeparationUtilities` (used by RH3002 and the formatter's `ExpressionBodyToBlockConverter`). Not reused, because RH3001-009 is keyed on the token kind and empty trivia (decision 18), not on whether the texts would lex differently: `return!(a)` is withheld by the requirement's letter, while `WouldMergeIntoOneToken("return", "(")` is false.
- Formatter call wrapper: `rg -n "private static .*FormatNodeAsync|FormatNodeInDocumentAsync\(document, (node|member), cancellationToken\)" src/Reihitsu.Analyzer.CodeFixes` — one owner left, `BlankLineCodeFixUtilities.FormatNodeInDocumentAsync`. The other providers' inline `(await ReihitsuFormatter.FormatNodeInDocumentAsync(...)).Output` calls predate the run and are not delegates handed to a helper.

## Production diff size
- `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this round: 8 files changed, 287 insertions(+), 38 deletions(-).
- This round: 3 files changed, 49 insertions(+), 30 deletions(-). No new type, no new public member outside the internal helper's renamed wrapper.

## Deleted special cases
_N/A._

## Tests run
- `scripts/build.sh --configuration Debug`: Build succeeded, 0 warnings.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3001NotOperatorShouldNotBeUsedAnalyzerTests"`: Passed 88/88 (the 14 red RH3001-009 tests and the Fix All test are green; no assertion changed).
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 8938/8938.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 117/117.
- `scripts/format.sh` over the three files: formatted 0 of 3.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
