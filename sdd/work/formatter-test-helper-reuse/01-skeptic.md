Role: skeptic
Commit: c91da5c583fd6f30f9499ac7674f71126457425f
Verdict: #1847: proceed narrower; #1848: proceed

My prompt carried no intended fix or verdict. It did ask me to say which lane-check category each design option needs, which I do under *Surfaces* and *Internal choices*.

Experiments ran in temporary `git worktree`s under the session scratchpad, at the commit above. I deleted them afterwards, and the shared tree was never edited. Every result below was observed with `scripts/test.sh`.

## thoenissen/Reihitsu#1847

### Premise

Real, but narrower than the issue states, and one of its "Done when" items is already met.

- No production caller is involved. The "caller" here is the test helpers in `src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs`. `ApplyRule` (line 368 onwards) runs `FormattingPipeline.Execute` through `PhaseTreeRoundTrip.ExecuteAndCompare`. That call discards `FormattingResult.Unsupported` (`PhaseTreeRoundTrip.cs`, the `.Output` at the end of the `Execute` call). The syntax-error branch of `ApplyRule` (`FormattingPipeline.Execute(...).Output`) also drops `Unsupported`. A refusal (FMT-UNSUP-001) returns the input unchanged, so a helper whose expectation is "unchanged" cannot tell a refusal from a kept input.
- **The issue's proposed demonstration fixture does not discriminate.** The fixed-size-buffer fixture `public fixed /* d */  int _d[4];` (the fixture of `HorizontalSpacingRequirementTests.CommentBetweenFixedAndTheTypeOfAFixedSizeBufferIsUnsupported`) already makes every `ApplyRule` helper except `FormatWithPipeline` fail on `main`. The refusal is not what trips them. A refused input is never formatted, so the phase-level stability check (FMT-PIPE-027) sees `HorizontalSpacingPhase`, run alone, collapse the double space. A test built on that fixture passes on `main` as well, so it proves nothing about the new check.
- **What does pass silently on `main` is a refused input that is otherwise a fixed point of every phase run alone.** Probe fixture (LF, written byte-exact; CRLF via the helpers' own loop):

  ```csharp
  internal class TestClass
  {
      private int /* comment */ _value;
  }
  ```

  The probe was a temporary test class deriving `FormatterTestsBase` that calls each helper on that input. Command: `scripts/test.sh --project formatter --filter "FullyQualifiedName~ZzProbeTests"`.
  - Unmodified helpers (`main` at the commit above): `AssertRuleResult(input)`, `AssertRuleResult(input, input)`, `AssertSecondPassIsStable(input)`, `AssertBlankLinesAbove(input, ("{", false))` and `FormatWithPipeline(input, "\n")` all **return without failing**.
  - With a non-refusal check added to both `ApplyRule` branches, all five fail. So does the syntax-error-branch probe (below).
- **Cost today: none.** With the check in both branches of `ApplyRule`, the whole formatter test project passes. `scripts/test.sh --project formatter` gave `Passed! - Failed: 0, Passed: 5738, Total: 5738`. This is after #1839 merged (06a2f5d, FMT-UNSUP-012). The issue's coordination paragraph (fixtures with a comment behind `=>`) is stale: #1839 already moved or re-asserted them, and no `ApplyRule` fixture is refused today.

### Mechanism and candidates

Mechanism: every `FormatterTestsBase` helper that reaches `ApplyRule` derives its verdict from the output text alone. `ApplyRule` drops `FormattingResult.Unsupported`, and a refusal's output equals its input. So any expectation the unchanged input satisfies passes, unless an unrelated check (FMT-PIPE-027 stability, a differing expected text) fails first.

Dispatch: the callers of the private `ApplyRule` in `FormatterTestsBase` (`rg "ApplyRule\(" src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs`):

| Candidate | Silent on a canonical refused input on `main`? |
|---|---|
| `AssertRuleResult(input)` (expected `null`) | yes (observed) |
| `AssertRuleResult(input, expected == input)` | yes (observed) |
| `AssertRuleResult(input, expected != input)`, first pass refused | no: the output equals the input, not the expected text, so it already fails |
| `AssertRuleResult(input, expected != input)`, **second pass refused** (the first pass's output holds a shape the precheck refuses, which FMT-UNSUP-005 forbids) | yes, by reasoning: the second pass returns `actual` unchanged, which equals the expected text. No input shows it today, since it would take a production defect. The same `ApplyRule` check covers it |
| `AssertSecondPassIsStable` (both passes) | yes (observed) |
| `AssertBlankLinesAbove` | yes (observed) |
| `FormatWithPipeline` (**not named in the issue**) | yes (observed). Its callers assert a property of the output, which a refused input can satisfy |
| `AssertSupported` | input: already checked through `ReihitsuFormatter.FormatSyntaxTree`; its passes then go through `AssertSecondPassIsStable`, covered above |
| Syntax-error branch of `ApplyRule` (bypasses `ExecuteAndCompare`) | not observed as silent: a refused input with a syntax error failed on `main` through FMT-PIPE-027 (`HorizontalSpacingPhase` re-indents `}` after the error). It is a separate code path, so a check placed only in `ExecuteAndCompare` would not cover it |

Other test helpers that run the pipeline, checked and holding:
- `ObservedPipeline.Run` already fails on a refusal. The precheck throws before the observer is called, so `Assert.IsNotEmpty(phases, ...)` fails.
- `PhaseStabilityCheck.FindChangingPhases` runs single phases without the precheck, so it never sees a refusal.
- `PhaseTreeRoundTripTests` calls `ExecuteAndCompare` directly with no refused input. It stayed green under the check.

### Breaking inputs

- FC catalog: no failure class of `squad/failure-classes.md` reaches test-helper code; the defect is a test-oracle gap, not formatter behavior.
- Class matrix: axis 1 is the helper (the rows above). Axis 2 is the pass that refuses (first or second). Axis 3 is whether the refused input is otherwise a fixed point of every phase run alone. Only cells where axis 3 is "fixed point" are silent, because a non-canonical refused input is already caught by FMT-PIPE-027.
- The test implementer must cover:
  - a refused input that is a fixed point of every phase (the probe fixture above, or any canonical file holding one FMT-UNSUP shape), reaching at least `AssertRuleResult` without an expected text, `AssertSecondPassIsStable`, `AssertBlankLinesAbove`, and `FormatWithPipeline`;
  - a refused input on the syntax-error branch, if that branch is checked.

  The issue's fixed-buffer fixture must not be the demonstration: it fails on `main` already.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `src/Reihitsu.Formatter.Test/Helpers/FormatterTestsBase.cs` (`ApplyRule`, both branches) | must change |
| `src/Reihitsu.Formatter.Test/Helpers/PhaseTreeRoundTrip.cs` | may change, if `Unsupported` is surfaced there; its tests (`PhaseTreeRoundTripTests`) must stay green |
| A test of the helper in `Reihitsu.Formatter.Test` | must be added |
| Formatter production code, analyzers, code fixes, CLI, rule pages, specs | not involved |
| Lane-check category | every option for this issue touches only `.cs` files under `src/Reihitsu.Formatter.Test/**`, which is the **test** category. No `*.csproj` change is needed |

### Refuse option

There is nothing to refuse, because no production input class is involved. The alternative is no change. Then the gap stays: a fixture whose shape becomes refused by a future FMT-UNSUP declaration passes silently in five helpers. That is exactly the drift the issue describes. The change costs a few test-helper lines, and the full suite shows it refuses no fixture today.

Declaring a shape unsupported: not an option here. No formatter phase or code fix is touched.

### Output questions

_None._

### Internal choices

1. **Where the check sits.** One option is in `ApplyRule`, covering both branches. The other is inside `PhaseTreeRoundTrip.ExecuteAndCompare`, which misses the syntax-error branch unless that branch gets it too. Both were green on the full suite when applied together. Recommendation: whichever placement covers both branches, so that one place decides it. If #1848 makes a shared parse-and-run owner, the check belongs there (see *Whole run*).
2. **Requirement tag of the helper test.** The helper test checks a test oracle, not formatter behavior. FMT-UNSUP-005 is the requirement the second-pass cell protects. Recommendation: tag it only if the spec checker's coverage rules ask for it.

### Issue goals

- "`ApplyRule`-based assertions fail when the formatter refuses the fixture": **keep, widen.** Include `FormatWithPipeline`, which the issue does not list but which is silent today, and the syntax-error branch. No visible output.
- "a test shows that": **keep, change the fixture.** The issue's fixed-buffer fixture already fails on `main` through FMT-PIPE-027. Use a refused input that is otherwise canonical. No visible output.
- "fixtures that refuse on purpose use `AssertUnsupportedFor`": **drop as already met.** No `ApplyRule` fixture is refused (5738/5738 green with the check), so nothing has to move. No visible output.
- The "Coordination" paragraph: stale since #1839 merged. No action.

### Architecture findings

_None._ This is a missing check in one helper, not two owners of one decision. The two-owner question belongs to #1848.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1848

### Premise

Real. The issue's statement about today's code holds, and unifying finds no failure today.

- `VerifyFormatterFixCore` in `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs` (from line 207) parses with `CSharpParseOptions.Default` and builds `new FormattingContext(endOfLine, languageVersion: LanguageVersionResolver.Resolve(tree.Options))`. That is the body of `SecondPassInput.Create` (`src/Reihitsu.Formatter.Test/Helpers/SecondPassInput.cs`) restated: `null` parse options mean the defaults. It then calls `FormattingPipeline.Execute` without the FMT-PIPE-021/FMT-PIPE-034 checks of `PhaseTreeRoundTrip.ExecuteAndCompare`.
- The same file already uses the formatter test project's `SecondPassInput` rule for its last step. `PhaseStabilityCheck.AssertNoPhaseChanges` (public) parses through `SecondPassInput.Create` (`PhaseStabilityCheck.cs:224`). So the analyzer-side helper formats with a private copy and checks with the shared original: one decision, two owners.
- **Visibility facts, compiled:**
  - `Reihitsu.Analyzer.Test.csproj` already has `<ProjectReference Include="..\Reihitsu.Formatter.Test\Reihitsu.Formatter.Test.csproj" />`. No project-reference change is needed for any option.
  - Making `SecondPassInput` and `PhaseTreeRoundTrip` `public` as they stand **does not compile**: `error CS0050` (return type `(SyntaxNode Root, FormattingContext Context)`) and `error CS0051` (parameter `FormattingContext`). `FormattingContext` is `internal` in `Reihitsu.Formatter` (`src/Reihitsu.Formatter/Data/FormattingContext.cs:14`). Observed with `scripts/test.sh --project formatter --filter "FullyQualifiedName~PhaseTreeRoundTripTests"`.
  - Adding `[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Reihitsu.Analyzer.Test")]` to `src/Reihitsu.Formatter.Test/Properties/AssemblyInfo.cs` compiles. Then I added to `VerifyFormatterFixCore` a second run through `SecondPassInput.Create` and `PhaseTreeRoundTrip.ExecuteAndCompare`, asserting no failures and output equal to the existing `formatted`. Command: `scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatter|FullyQualifiedName~Reihitsu.Analyzer.Test.Base.FormatterTestsBaseTests"`. Result: `Passed! - Failed: 0, Passed: 488, Total: 488`, which is the 485 parity tests plus the 3 `FormatterTestsBaseTests`. So the restated parse gives the same output as `SecondPassInput`, and no parity fixture fails FMT-PIPE-021 or FMT-PIPE-034.
  - One side note from those builds: the self-hosted analyzers (RH5603, RH7305, MSTEST0037) apply to test code. Any new file there follows the house style.

### Mechanism and candidates

Mechanism: the analyzer-side formatter-parity helper restates the parse-and-context step that `SecondPassInput` owns and omits the round-trip checks `ApplyRule` runs. The two owners can drift, and analyzer parity fixtures never get the FMT-PIPE-021/034 checks.

Candidates (the callers of `VerifyFormatterFixCore`, `rg "VerifyFormatterFixCore" src/Reihitsu.Analyzer.Test`):
- `VerifyFormatter`, both passes (lines 131 and 135);
- `VerifyFormatterKeepsReportedShape` (line 154).

All 160 files deriving `FormatterTestsBase<` are under `Reihitsu.Analyzer.Test/Formatter/` or `Base/`, so the filter above covers every caller.

### Breaking inputs

- FC catalog: none reaches test-helper code.
- No failing member today (488/488 with the unified path). The test implementer's evidence is the existing parity suite staying green. The existing `FormatterTestsBaseTests` (refusal) must keep failing as they do now. If the non-refusal check moves into the shared owner, their expected message (the reason text) must still appear.
- `VerifyFormatterFixCore`'s parameter `transformParseOptions` is passed `null` at every call site (lines 131, 135, 154). It is dead, and a shared owner makes it moot.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs` (`VerifyFormatterFixCore`) | must change |
| `src/Reihitsu.Formatter.Test/Helpers/` (`SecondPassInput`, `PhaseTreeRoundTrip`, possibly a new shared entry) | must change (visibility or a new shared entry) |
| `src/Reihitsu.Formatter.Test/Properties/AssemblyInfo.cs` | changes only under option A below |
| `Reihitsu.Analyzer.Test.csproj`, `Reihitsu.Formatter.Test.csproj`, `Reihitsu.sln` | must stay: the reference already exists |
| Production code, analyzers, code fixes, CLI, rule pages, specs | not involved |

### Refuse option

There is nothing to refuse (no production input class). The no-change option keeps the restatement on purpose. It costs nothing now, but it leaves two owners of the parse step and 485 fixtures without the FMT-PIPE-021/034 checks. Declaring a shape unsupported: not an option.

Design options and their lane-check category (`scripts/README.md`, *The lane check*: a test project's `*.csproj` is tooling, every other file of `src/*.Test/**` is test):

| Option | Paths | Category | Status |
|---|---|---|---|
| A. `InternalsVisibleTo("Reihitsu.Analyzer.Test")` in `src/Reihitsu.Formatter.Test/Properties/AssemblyInfo.cs` | `.cs` in a test project | **test** | compiled, 488/488 green |
| B. Make `SecondPassInput` and `PhaseTreeRoundTrip` `public` as they stand | `.cs` in a test project | test | **does not compile** (CS0050, CS0051) |
| B'. A `public` entry in `Reihitsu.Formatter.Test` whose signature names no internal formatter type (text, line ending and parse options in; text out). Precedent: `PhaseStabilityCheck.AssertNoPhaseChanges(string, string, CSharpParseOptions)` and `ObservedPipeline.Run`, already public and used across projects | `.cs` in a test project | **test** | not built by me |
| C. A shared test-helper project | new `*.csproj`, `Reihitsu.sln`, and `ProjectReference`s | **tooling**. Its `.cs` files are **production** unless the project name matches `src/*.Test/**`, and production next to tooling without a change doc is admitted by no lane (exit 3) | redundant: `Reihitsu.Formatter.Test` already is the shared project the analyzer tests reference |
| D. Linked source files (`<Compile Include=... Link=...>`) | `Reihitsu.Analyzer.Test.csproj` | **tooling** | compiles a second copy of the internal types into another assembly, although a project reference exists |
| E. Keep the restatement | none | n/a | leaves the drift |

### Output questions

_None._ No end-user surface is touched.

### Internal choices

- **A vs B'.** Both stay in the tests lane, and neither changes visible output.
  - A is one line and was verified. It opens every internal of `Reihitsu.Formatter.Test` to the analyzer tests.
  - B' adds a narrow public entry, as the two public helpers already used across projects do.

  Recommendation: either holds. Pick B' if the shared owner is a single "format once and check" entry that both `ApplyRule` and `VerifyFormatterFixCore` call. Pick A if `VerifyFormatterFixCore` is to call `SecondPassInput` and `PhaseTreeRoundTrip` directly. C and D widen the lane to tooling and gain nothing, so I recommend against them.

### Issue goals

- "One owner holds the parse-and-run steps and the round-trip checks for both test projects": **keep.** It serves the purpose, and option A or B' meets it in the tests lane. No visible output.
- "`VerifyFormatterFixCore` runs them": **keep.** Verified green. No visible output.
- The issue's option list ("an `InternalsVisibleTo` grant, a shared test-helper project, or linked source files"): proposals. The last two widen the lane to tooling, and the shared project is redundant because the reference exists. Drop them. No visible output.

### Architecture findings

Test infrastructure, not production: `VerifyFormatterFixCore` restates `SecondPassInput.Create`, a shared helper and a private copy of it, while the same file's stability step uses the original. This is the issue's own subject, so the run fixes it. It is not an extra finding.

### Follow-up candidates

_None._

## Whole run

- **Shared mechanism.** Both issues concern the formatter test helpers' parse-run-check path. #1847 adds the non-refusal check to `ApplyRule`. `VerifyFormatterFixCore` already asserts non-refusal (`Assert.IsNull(result.Unsupported, ...)`). Once #1848 gives both projects one owner, a single non-refusal check in that owner serves both, and the analyzer-side copy of the check becomes redundant. Order the work so the check is written once.
- **Lane.** Options A and B' for #1848, and every option for #1847, touch only `.cs` files under `src/Reihitsu.Formatter.Test/**` and `src/Reihitsu.Analyzer.Test/**`, which is the **test** category. The claimed lane, tests, holds. Only options C and D need a `*.csproj` or solution change, which widens the lane to tooling, and neither is needed.
- **Combined experiment.** I applied both checks together, at the commit above: the `ApplyRule` non-refusal check in both branches, plus the analyzer-side round-trip under an `InternalsVisibleTo` grant.
  - `scripts/test.sh --project formatter`: 5738/5738 passed.
  - The analyzer parity filter above: 488/488 passed.
