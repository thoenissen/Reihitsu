Role: test implementer
Commit: see the commit that adds this file (parent cd90461)
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the disputed test; I did not take the implementer's proposed change on trust and re-derived it below.

## Repair

`RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests.FixAllOrganizesOnlyTheBlockWithoutPreprocessorDirective`
(`tests/Reihitsu.Analyzer.Test/Ordering/RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests.cs`).

- Defect (mine, in the setup): the expected fixed text marks the diagnostic that stays, `using {|RH7203:System|};`, but the fixed and batch-fixed states of `Microsoft.CodeAnalysis.Testing` drop markup for a fixable diagnostic ID unless told otherwise. The failure the implementer reported, `Diagnostics of fixed state — expected "0" actual "1"` with the actual one at exactly the marked span (5,7)-(5,13), is that markup being ignored, not a wrong output.
- Repair: in the `onConfigure` lambda, `test.FixedState.MarkupHandling = MarkupMode.Allow;` and `test.BatchFixedState.MarkupHandling = MarkupMode.Allow;`, with the same comment as the repository's existing precedent `RH8204DoNotUsePlaceholderElementsAnalyzerTests` (the only other test whose expected fixed source carries a withheld-fix diagnostic of the fixed rule).
- Unchanged: both expected texts, the requirement tags, `NumberOfIncrementalIterations = 1`, and `NumberOfFixAllIterations = 1`. With the markup honored, the test now asserts the stronger thing it was meant to: the compilation-unit diagnostic remains after the fix (FC-017), and a missing report there would fail it.
- The iteration counts my earlier handover could not confirm are confirmed: the test passes with 1 and 1.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH7203 `FixAllOrganizesOnlyTheBlockWithoutPreprocessorDirective` (setup repaired) | RH7203-004, SHARED-USING-009 | `Verify(source, fixed, cfg)` with fixed/batch-fixed `MarkupMode.Allow` | passes |

## Scenarios (bug fix)

_N/A_ (behavior change).

## Not covered, and why

Unchanged from `04-test-implementer.md`.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Ordering.RH720|FullyQualifiedName~RH7207UsingDirectives"
```

Result on this head (implementer's production change c8d4e52 + this repair): Failed 0, Passed 356, Total 356.

## Follow-up candidates

_None._
