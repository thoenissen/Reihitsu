Role: implementer
Commit: (see the commit that adds this handover)
Verdict: tests pass

My prompt carried no intended fix or verdict beyond the run's inputs.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/TrailingCommaCodeFixHelper.cs` | `CommaPrecedesConditionalCompilationBoundary` renamed to `CommaPrecedesIfDirective`; it withholds only when an `IfDirectiveTrivia` (active, or parsed inside an excluded branch) starts in the gap between the comma and the next token, instead of any conditional directive or disabled text. Still `true` when the span holds no comma. Summary rewritten (FC-031). | SHARED-TRAIL-002, SHARED-TRAIL-004, SHARED-FIX-009 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/TrailingCommaRemovalCodeFixProviderBase.cs` | Calls the renamed guard; inline comment rewritten to the new rule (FC-031). | SHARED-TRAIL-002, SHARED-TRAIL-004 |

The formatter is untouched (answer C): `EnumTrailingCommaRemovalTransform` and `InitializerTrailingCommaRemovalTransform` keep calling `SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary`, whose summary still describes those callers correctly, so it was not changed.

## Policy owners
`rg -n "IfDirectiveTrivia" src --type cs` (excluding `obj/`): no existing helper answers "does this span hold an `#if` directive". `SyntaxTriviaUtilities.IsConditionalDirective` (private, all four kinds), `ContainsConditionalCompilationBoundary` (all four kinds plus disabled text, formatter policy kept by answer C), `RH2001…CodeFixProvider:500` (all four kinds), `RH7004…:199` and `RH8201…:53` (related or first directive, different questions), `ReihitsuFormatterHelpers:238` (depth tracking). The trailing-comma fix's own guard stays its single owner.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src` before this commit: empty; this commit: 2 files changed, 22 insertions(+), 10 deletions(-). No new type, no new public member (the helper is `internal`; one method renamed).

## Deleted special cases
_N/A._

## Tests run
- `scripts/build.sh --configuration Debug` — no warnings, no errors.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5409|FullyQualifiedName~RH5410|FullyQualifiedName~RH5411|FullyQualifiedName~RH5303"` — Passed 186, Failed 0 (17 red before).
- `scripts/test.sh --configuration Debug --project analyzer` — Passed 11934, Failed 0.
- `scripts/test.sh --configuration Debug --project formatter` — Passed 7287, Failed 0.
- `scripts/test.sh --configuration Debug --project core` — Passed 887, Failed 0.
- `scripts/test.sh --configuration Debug --project architecture` — Passed 198, Failed 0.

## Disputed tests or scope
_None._

## Architecture findings
_None._ The fix and the formatter now decide this comma differently, which the maintainer chose (answer C) and the change doc records as a decision.

## Follow-up candidates
_None._
