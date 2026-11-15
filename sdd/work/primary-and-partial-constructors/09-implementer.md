Role: implementer
Commit: see the commit that adds this handover (production change and handover in one commit on top of d9c7e7b)
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Repair round 1 for #1680: lead decisions 9 (finding 1, adjacent fix) and 11 (hint on `IsRepeatedRun`'s summary). No new type.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Base/AnalysisContextExtensions.cs` | `IsRepeatedRun` first returns `false` for a node that is not part of structured trivia (`IsPartOfStructuredTrivia()`), so the existing guard is a cheap no-op for ordinary expression nodes and applies to every node inside structured trivia, directive-condition expressions included. Summaries of the class, both `RegisterStructuredTriviaAction*` methods, `IsRepeatedRun` (now names both one-run groups: parameter list / base argument list for the constructor run; base type name and `[method: ...]` lists for the type run — the auditor's hint), and `WrapWithRepeatedRunCheck` describe the new scope | RH3002-007, RH3005-003, RH3006-002 (one diagnostic per element); hint (FC-031) |
| `src/Reihitsu.Analyzer/Rules/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.cs` | `ParenthesizedExpression` registered through `RegisterStructuredTriviaAction` | RH3002-007 |
| `src/Reihitsu.Analyzer/Rules/Clarity/RH3005UseReadableConditionsAnalyzer.cs` | comparison kinds registered through `RegisterStructuredTriviaAction` | RH3005-003 |
| `src/Reihitsu.Analyzer/Rules/Clarity/RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer.cs` | `LogicalOrExpression` registered through `RegisterStructuredTriviaAction`; `OrPattern` stays a plain registration (a pattern cannot occur in a directive condition) | RH3006-002 |

The precheck is needed for correctness, not only speed: without it, an ordinary expression whose `ContainingSymbol` is the primary constructor and that lies outside the parameter list and base argument list (for example in a `[method: ...]` attribute argument) could be dropped; this was reasoned, not probed. Inside structured trivia the position test is unchanged and was already correct for any node (07-auditor.md, guard scope). For the ten existing trivia registrations the precheck is always true (the registered node is itself structured trivia or inside it), so their behavior is unchanged.

## Policy owners
- Skip the repeated primary-constructor run: `rg -n "IsRepeatedRun|WrapWithRepeatedRunCheck|IsPrimaryConstructorRun" src/Reihitsu.Analyzer src/Reihitsu.Core` — `AnalysisContextExtensions` only; now reached by 13 registrations (the ten from round 0 plus RH3002, RH3005, RH3006).
- `rg -n "IsPartOfStructuredTrivia" src/Reihitsu.Analyzer` — RH6011, RH6012, RH6015, RH6024 use it to skip trivia nodes altogether (a different decision: they do not report inside trivia), and the new precheck.

## Production diff size
- `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (after this commit): 15 files changed, 220 insertions(+), 122 deletions(-).
- Delta of this round: 4 files changed, 28 insertions(+), 22 deletions(-). No new type, no new public member (the registration methods are internal and existed).

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~ReportedOnce&(FullyQualifiedName~RH3002|FullyQualifiedName~RH3005|FullyQualifiedName~RH3006|FullyQualifiedName~RH7302)"` — Passed: 15, Failed: 0 (the seven finding-1 tests failed with "expected 1 actual 2" before, per 08-test-implementer.md).
- `scripts/test.sh --configuration Debug --project analyzer` — Passed: 9388, Failed: 0.
- `scripts/test.sh --configuration Debug --project architecture` — Passed: 118, Failed: 0.
- `scripts/format.sh` over the four files: reformatted the RH3005 registration's argument alignment only.

## Disputed tests or scope
_None._ The method name `RegisterStructuredTriviaAction` now also takes expression kinds that occur inside structured trivia; its summary says so. A rename was not made, to keep the round small.

## Architecture findings
_None_ new; the per-registration opt-in remains the structural fact the skeptic and auditor recorded.

## Follow-up candidates
_None._
