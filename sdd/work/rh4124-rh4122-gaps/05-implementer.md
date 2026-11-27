Role: implementer
Commit: 2cab105
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing to ignore.

Before the change, the handed-over positive tests failed only on missing diagnostics, as 04-test-implementer.md states. All 41 tests of the two classes pass now.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Naming/RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer.cs` | New `OnRangeVariableDeclaration`, registered for `FromClause`, `LetClause`, `JoinClause`, `JoinIntoClause`, `QueryContinuation`; reports the clause's own `Identifier` when `SingleLetterIdentifierUtilities.HasSingleLetterName(identifier.ValueText)` holds (so `@b` counts as `b`). Uses of range variables are identifier names, not these clauses, so they are never reported. Class `<summary>` widened to query range variables (spec author's note in 03). | RH4124-003 |
| `src/Reihitsu.Analyzer/Rules/Naming/RH4122SingleLetterParameterNamesAnalyzer.cs` | `IsSupportedParameter` also accepts a parameter whose containing symbol is an extension block (`INamedTypeSymbol { IsExtension: true }`). The existing per-`ParameterSyntax` action fires once per receiver declaration, so one diagnostic per block; an unnamed receiver has an empty name and is not reported. The framework-mandated and record-primary-constructor exclusions return false for a receiver. Class `<summary>` widened. | RH4122-004 |

## Policy owners
- Single-letter decision: `rg -ln "HasSingleLetterName" src` — owner `src/Reihitsu.Analyzer/Core/SingleLetterIdentifierUtilities.cs`; consumers RH4122, RH4123, RH4124, RH4125, RH4126, RH4127. `rg -n "Length == 1" src/Reihitsu.Analyzer` finds no private copy of the name check. Both changes call the shared owner.

## Production diff size
Against the previous HEAD: `2 files changed, 32 insertions(+), 3 deletions(-)` (src). No new type, no new public member.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH4122SingleLetterParameterNamesAnalyzerTests|FullyQualifiedName~RH4124SingleLetterLambda"` — Passed: 41, Failed: 0.
- `scripts/test.sh --configuration Debug --project analyzer` — Passed: 12158, Failed: 0 (self-hosting scan included).
- `scripts/test.sh --configuration Debug --project architecture` — Passed: 198, Failed: 0.
- `scripts/format.sh` over both files — 0 of 2 changed.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
