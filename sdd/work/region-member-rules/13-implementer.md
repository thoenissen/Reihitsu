Role: implementer (repair round 1, audit findings #1 and #2)
Commit: 7f6d88e (changes on top of it)
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/AccessorOrderingUtilities.cs` | `ContainsConditionalCompilationBoundaryOutsideBodies` excludes each body through the new private `GetBodySpan`: a block's span, or an expression body from the arrow through the accessor's `;` (arrow-clause end when the `;` is missing). A directive or excluded code in the `;`'s leading trivia now lies inside the body. Summary updated (FC-031) | RH7107-001, RH7108-001, SHARED-DIAG-008 (finding #1) |
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | New `HasConditionalDirectiveBetweenModifiers`: the RH7105/RH7106 judged span `[first modifier start, last modifier start)`, two or more modifiers; its summary names the difference from `HasModifiersSplitByConditionalDirective` (which stays unchanged, decision 13) | SHARED-MODORDER-003, SHARED-DIAG-008 (finding #2) |
| `src/Reihitsu.Analyzer/Rules/Organization/RH7105DeclarationKeywordsMustFollowOrderAnalyzer.cs`, `RH7106ProtectedMustComeBeforeInternalAnalyzer.cs` | Inline span copies replaced by the helper; unused `Microsoft.CodeAnalysis.Text` using removed | RH7105-001, RH7106-001 |

## Policy owners
- RH7105/RH7106 judged modifier span: `rg -n "modifiers.First\(\).SpanStart" src --glob '!*Test*'` → `OrderingDeclarationUtilities.HasModifiersSplitByConditionalDirective` (RH7102/RH7103 span, through the token behind the last modifier) and `OrderingDeclarationUtilities.HasConditionalDirectiveBetweenModifiers` (RH7105/RH7106 span). Two spans by decision 13, one owner each, side by side.
- Accessor body span: `rg -n "Body \?\? .*ExpressionBody" src --glob '!*Test*'` → no other owner; `AccessorOrderingUtilities.GetBodySpan` is the only one.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (before this commit): 37 files changed, 529 insertions(+), 387 deletions(-). Round delta: 4 files, +54/-10. No new type; one new public member (`HasConditionalDirectiveBetweenModifiers`, in Core, consumers RH7105 and RH7106 only).

## Deleted special cases
_N/A._

## Tests run
- `scripts/build.sh --configuration Debug` — 0 warnings, 0 errors.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~ExpressionBodyOfThe|FullyQualifiedName~ConditionalDirectiveInFrontOfTheFirstModifier"` — Passed 15/15 (the five formerly failing RH7107/RH7108 tests included).
- `scripts/test.sh --configuration Debug --project analyzer` — Passed 9278/9278.
- `scripts/test.sh --configuration Debug --project core` — Passed 652/652.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
