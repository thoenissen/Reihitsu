Role: test implementer
Commit: (this commit)
Verdict: tests written

The prompt pointed at `07-implementer.md` and asked for a configuration-only repair. It carried no intended fix or hoped-for verdict beyond that, and I checked the implementer's claim myself.

## Claim checked
- The test passes explicit `expected` diagnostics, and its fixed source marks the withheld diagnostic as `{|RH5107:,|}`. Without `MarkupMode.Allow` on the fixed states, the test framework does not read that markup as an expected diagnostic. That produces the reported `Diagnostics of fixed state: expected "0" actual "1"`. The repository's other tests with a withheld diagnostic in the fixed source set both states (for example `RH4105MethodParameterCasingAnalyzerTests` around line 1166).
- Fix: `FixedState.MarkupHandling` and `BatchFixedState.MarkupHandling` are set to `MarkupMode.Allow` through the `onConfigure` overload of `Verify`, and `using Microsoft.CodeAnalysis.Testing;` is added. No iteration override is needed. The expected texts are unchanged and correct under RH5107-004: `, int b` moves behind `int a`, and `, /* c */ int c` keeps its comma and its diagnostic.
- The test still guards the behavior. If the fix were not withheld for the commented comma, the fixed state would differ from the expected text, and the test would fail.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzerTests.VerifyFixAllMovesOnlyTheCommaWithoutACommentBehindIt` (configuration only) | RH5107-004 | `Verify(source, fixedSource, onConfigure, expected)` | passes (the whole class passes: 38/38) |

## Not covered, and why
No new tests. The task was limited to this test's configuration.

## Commands
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzerTests"
