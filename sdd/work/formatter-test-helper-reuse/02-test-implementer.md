Role: test implementer
Commit: 05b1ccf (base); the commit holding this handover carries the changes below
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. It named scope and lane only, which match `00-lead.md`.

## thoenissen/Reihitsu#1847

### Change

- New `src/Reihitsu.Formatter.Test/Helpers/FormatterPass.cs`: the public `FormatterPass.Format(string text, string endOfLine, CSharpParseOptions parseOptions)` (decision 3, option B'). Its signature names no internal formatter type. It parses through `SecondPassInput.Create` and runs the production pipeline. Text without syntax errors goes through `PhaseTreeRoundTrip.ExecuteAndCompare`, with its FMT-PIPE-021 and FMT-PIPE-034 failures. Text with syntax errors goes through `FormattingPipeline.Execute`. In **both** branches it asserts `result.Unsupported == null`, with the message `The formatter must not refuse the input as unsupported: <reason>` (decision 1).
- `PhaseTreeRoundTrip.ExecuteAndCompare` now returns the `FormattingResult<SyntaxNode>` instead of its `Output`, so the caller sees `Unsupported`. Its one other caller, `PhaseTreeRoundTripTests.AssertEveryPhaseParsesBack`, discards that element and is unchanged.
- `FormatterTestsBase`: the private `ApplyRule` is deleted, and every former caller calls `FormatterPass.Format`. Those callers are both branches of `AssertRuleResult`, `AssertSecondPassIsStable`, `AssertBlankLinesAbove` and `FormatWithPipeline`. Usings that became unused were removed. The `FormatWithPipeline` documentation now names the refusal check.
- `SecondPassInput` documentation names `FormatterPass` and its two users.
- Decision 2: the demonstration uses the skeptic's canonical refused input, `private int /* comment */ _value;`, not the fixed-buffer fixture. No fixture was moved to `AssertUnsupportedFor`, because none is refused (formatter suite green, below).

## Tests

New file `src/Reihitsu.Formatter.Test/Unit/Pipeline/FormatterTestsBaseRefusalTests.cs`, a test class deriving `FormatterTestsBase`. The tests carry no requirement tag. They check a test oracle, not formatter behavior (skeptic, internal choice 2).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `AssertRuleResultWithoutExpectedTextFailsWhenTheFormatterRefusesTheInput` | none (helper oracle) | `AssertRuleResult(input)` | pass (failed on base: no exception thrown) |
| `AssertRuleResultWithTheInputAsExpectedTextFailsWhenTheFormatterRefusesTheInput` | none | `AssertRuleResult(input, input)` | pass (failed on base: no exception thrown) |
| `AssertSecondPassIsStableFailsWhenTheFormatterRefusesTheInput` | none | `AssertSecondPassIsStable` | pass (failed on base: no exception thrown) |
| `AssertBlankLinesAboveFailsWhenTheFormatterRefusesTheInput` | none | `AssertBlankLinesAbove` | pass (failed on base: no exception thrown) |
| `FormatWithPipelineFailsWhenTheFormatterRefusesTheInput` (LF and CRLF) | none | `FormatWithPipeline` | pass (failed on base: no exception thrown) |
| `FormatWithPipelineFailsWhenTheFormatterRefusesAnInputWithASyntaxError` (LF and CRLF, missing `;`) | none | `FormatWithPipeline`, syntax-error branch | pass (failed on base: no exception thrown) |
| `HelpersPassWhenTheFormatterKeepsTheInput` (other side of the condition) | none | all five | pass (passed on base) |
| `FormatWithPipelinePassesWhenTheFormatterDoesNotRefuseAnInputWithASyntaxError` (other side, syntax-error branch) | none | `FormatWithPipeline` | pass (passed on base) |

Every failing test asserts that the exception message contains `VariableDeclarationCommentReason`. A failure for an unrelated reason, such as FMT-PIPE-027, would not satisfy it.

I watched the six refusal tests fail against the unchanged helpers. I stashed the helper changes and ran the focused command below: `Failed: 6, Passed: 2`, and each of the six failed with `Expected exception of exact type AssertFailedException but no exception was thrown.`

## Not covered, and why

- **Second-pass refusal of `AssertRuleResult(input, expected != input)`.** No input reaches it without a production defect (FMT-UNSUP-005), as the skeptic reasoned. Both passes call the same `FormatterPass.Format`, which `AssertRuleResultWithTheInputAsExpectedTextFailsWhenTheFormatterRefusesTheInput` shows checks every pass.
- **`AssertSupported`.** It already checks `Unsupported` through `ReihitsuFormatter.FormatSyntaxTree`. Its passes run through `AssertSecondPassIsStable`, which is tested above.
- **Failure classes.** None of `squad/failure-classes.md` reaches test-helper code; the skeptic's walk applies. The line-ending dimension is covered all the same: every helper test runs under LF and CRLF.

## thoenissen/Reihitsu#1848

### Change

- `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs`: `VerifyFormatterFixCore` is now synchronous and calls `FormatterPass.Format(StripMarkup(source), endOfLine, null)`. It then asserts the output equals the expected fixed text.
  - The restated parse and context are removed.
  - The direct `FormattingPipeline.Execute` call is removed.
  - The helper's own `Assert.IsNull(result.Unsupported, ...)` is removed. The shared owner checks it once (decision 5).
  - The dead `transformParseOptions` parameter is removed (decision 4).
  - The returned parse options are removed, and `PhaseStabilityCheck.AssertNoPhaseChanges` receives `null`. `CSharpSyntaxTree.ParseText` treats `null` as the defaults, the same `CSharpParseOptions.Default` as before.
  - Parity fixtures now also get the FMT-PIPE-021 and FMT-PIPE-034 checks.
  - Unused usings were removed. The class summary states the shared pass.
- No `InternalsVisibleTo` was needed (option A not used), and no `*.csproj` or solution file changed.

### Tests

No new test. Neither side of the new round-trip check can be driven without a production defect: a phase tree that does not parse back, or that adds elastic trivia. The evidence is:

- The existing `FormatterTestsBaseTests` still pass. These are the refusal tests for `VerifyFormatter` and `VerifyFormatterKeepsReportedShape`, plus the kept-source control. Their expected reason text still appears, because the shared message keeps the format `The formatter must not refuse the input as unsupported: <reason>`.
- All 485 parity fixtures stay green under the shared pass.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FormatterTestsBaseTests.VerifyFormatterFailsWhenTheFormatterRefusesTheSource` (existing, unchanged) | none | `VerifyFormatter` | pass |
| `FormatterTestsBaseTests.VerifyFormatterKeepsReportedShapeFailsWhenTheFormatterRefusesTheSource` (existing, unchanged) | none | `VerifyFormatterKeepsReportedShape` | pass |
| `FormatterTestsBaseTests.VerifyFormatterPassesWhenTheFormatterKeepsTheSource` (existing, unchanged) | none | `VerifyFormatter` | pass |

## Whole run

- Changed paths, all in the test category:
  - `src/Reihitsu.Formatter.Test/Helpers/FormatterPass.cs` (new)
  - `src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs`
  - `src/Reihitsu.Formatter.Test/Helpers/PhaseTreeRoundTrip.cs`
  - `src/Reihitsu.Formatter.Test/Helpers/SecondPassInput.cs`
  - `src/Reihitsu.Formatter.Test/Unit/Pipeline/FormatterTestsBaseRefusalTests.cs` (new)
  - `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs`
  - this handover

  No `*.csproj`, solution file, or production file changed.
- One owner: `FormatterPass.Format` holds the parse (via `SecondPassInput`), the run, the round-trip checks and the non-refusal check for both test projects.
- `scripts/format.sh` was run over the changed files and changed nothing.
- Results:
  - `scripts/test.sh --project formatter`: `Passed: 5746, Failed: 0`. That is the skeptic's 5738 plus the 8 new tests.
  - Analyzer parity filter: `Passed: 488, Failed: 0`.
  - Default run (`scripts/test.sh`): every project green: Analyzer 10639, Formatter 5746, Core 738, Cli 385, Architecture 198, Tooling 133, Playground 23, all `Failed: 0`.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~FormatterTestsBaseRefusalTests"
scripts/test.sh --project formatter
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatter|FullyQualifiedName~Reihitsu.Analyzer.Test.Base.FormatterTestsBaseTests"
scripts/test.sh
```

## Follow-up candidates

_None._ One observation for the lead to weigh, not a defect: `PhaseTreeRoundTripTests.AssertEveryPhaseParsesBack` (`src/Reihitsu.Formatter.Test/Unit/Pipeline/PhaseTreeRoundTripTests.cs`) still builds its parse and context by hand instead of through `SecondPassInput`. It is a test of the round-trip helper itself, passes `CSharpParseOptions.Default` explicitly, and is outside both issues' surfaces, so I left it.
