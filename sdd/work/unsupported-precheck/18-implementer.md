Role: implementer
Commit: (this commit; see `git log -1` on the branch)
Verdict: tests pass

Repair round 2, Decision 18, finding 1 of `16-auditor.md`. The prompt named the finding and SHARED-FIX-010 and carried no suspected cause or hoped-for verdict beyond the auditor's suggested change; the change below was re-derived from SHARED-FIX-010's third bullet ("the node it builds, as it would be formatted") and its "MUST NOT decide the question by a check of its own".

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7104PartialElementsMustDeclareAccessModifierCodeFixProvider.cs` | New private `CreateUpdatedTypeDeclaration(typeDeclaration, semanticModel, cancellationToken)` builds the type with the accessibility keywords in front of its modifiers (taking over the first modifier's leading trivia). `ApplyCodeFixAsync` uses it (then annotates, replaces, formats as before; output unchanged). `RegisterCodeFixesAsync` now gets the semantic model and asks `ReihitsuFormatter.FindUnsupportedSyntax` about that built type, not the type as it stands; it registers nothing when root or semantic model is null (the action would return the document unchanged then). The comment restating which shapes the precheck refuses is removed. | SHARED-FIX-010 |

## Policy owners
- `rg -n "no variable declaration|answer it gives" src --glob '!*Test*/**'`: no hit in production code after the change (test-only hits describe formatter inputs). The only owner of which shapes are refused stays `PrecheckPhase`, reached through `ReihitsuFormatter.FindUnsupportedSyntax`.
- Builder of the updated type: one owner, `CreateUpdatedTypeDeclaration`, shared by registration and action — the same pattern as `TypeMemberOrderingCodeFixProviderBase` (`MoveMemberBefore`) and RH7107 (`TryCreateUpdatedMemberDeclaration`).

## Production diff size
- `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (before this commit): 64 files changed, 1230 insertions(+), 523 deletions(-).
- Delta of this round: 1 file changed, 24 insertions(+), 10 deletions(-). No new type, no new public member.

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "TestCategory=SHARED-FIX-010|FullyQualifiedName~RH7104"`: `Passed!  - Failed: 0, Passed: 43, Skipped: 0, Total: 43`.
- `scripts/test.sh --configuration Debug --project analyzer`: `Passed!  - Failed: 0, Passed: 7748, Skipped: 0, Total: 7748`.
- `scripts/format.sh` on the changed file: formatted 0 of 1.

## Disputed tests or scope
_None._ As `17-test-implementer.md` states, no test can tell the old query from the new one today (the type as it stands and the built type give the same answer for every input); `NoFixIsOfferedWhenTheTypeHoldsACommentTheFormatterRefuses` pins the outcome and stays green.

## Architecture findings
_None._

## Follow-up candidates
_None._
