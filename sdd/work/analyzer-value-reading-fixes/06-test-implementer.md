Role: test implementer
Commit: e5a98ed
Verdict: tests written

Repair round for lead decision 10. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests.VerifyCodeBlockLineInsideInterpolatedRawStringIsNotReported` | RH2109-001, SHARED-DIAG-003 | `Verify(source, onConfigure, expected)` on `AnalyzerTestsBase<TAnalyzer>` | pass |

Change: fixture only. The configure callback now sets `test.TestState.MarkupHandling = MarkupMode.None` before adding `Counter.razor`, so the `$$` of the interpolated raw string is no longer consumed as a markup position marker and reaches the analyzer as written. Input, requirement tags, and the expected diagnostics ((1,1) and (7,1) only) are unchanged. Same mechanism as the existing `RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzerTests`. A one-line comment at the setting says why.

Fails for the right reason: with `RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs` temporarily restored to 87a472f (pre-fix, reverted afterwards, not committed), the repaired test fails with `expected "2" actual "3"`, the extra diagnostic at `Counter.razor(3,1)` — the `@code {` line inside the interpolated raw string. On this head it passes.

## Scenarios (bug fix)
_N/A — repair round; the reproductions of 04 stand._

## Other tests checked for the same markup problem
Scanned every added line of the run's test files (`git diff origin/main...HEAD` for RH0001, RH2003, RH2109, RH8402, RH4001, RH4009 tests) and the whole RH2109 test file for markup tokens (`$$`, `[|`, `|]`, `{|`, `|}`). Findings:
- RH2109: only the repaired test holds `$$`; no other additional-file fixture holds a markup token.
- RH4009 `VerifyWithAllowList`: `$$"""` is C# interpolation syntax in the test code; the resulting `reihitsu.json` text holds no `$$`. Not affected.
- RH2003, RH4001, RH4009: `{|RH####:…|}` spans are intended diagnostic markup in C# sources. Not affected.
- RH0001, RH8402: no markup token in any added JSON or source fixture.

## Not covered, and why
Nothing new; the matrix of 04 is unchanged.

## Commands
```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests"
```
Result on this head: Passed 33/33.
