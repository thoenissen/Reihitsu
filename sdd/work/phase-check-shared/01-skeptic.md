Role: skeptic
Commit: 33da21cb7cb4aa1507e651b5ae7536076bcdbd4f
Verdict: proceed narrower

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I read thoenissen/Reihitsu#1805 (labeled `agent-filed`, no comments) and the code it names.

## Premise

The premise holds for the analyzer base. For the CLI test it is true but adds no coverage.

- **The gap exists.** `PhaseStabilityCheck` (`src/Reihitsu.Formatter.Test/Helpers/PhaseStabilityCheck.cs`) is called only from `Reihitsu.Formatter.Test` and from `src/Reihitsu.SelfHosting.Test/Formatting/FormatterSelfHostingTests.cs:136` (`DescribeChangingPhases(SyntaxTree)`). Neither `Reihitsu.Analyzer.Test` nor `Reihitsu.Cli.Test` references it (`rg PhaseStabilityCheck src`).
- **Analyzer base.** `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs` formats each fixture twice under the test's line ending and under the other one, and asserts that both passes equal `fixedSource`. It never runs a phase alone. Derived classes: 158, not the issue's 159 (`rg -l "FormatterTestsBase<" src/Reihitsu.Analyzer.Test` returns 159 files, and one of them is the base). `VerifyFormatter(` is called 460 times outside the base, so about 920 outputs (two line endings each) are idempotency-checked without the per-phase check. These fixtures are separate from the ones in `Reihitsu.Formatter.Test`, so checking them adds real coverage of FMT-PIPE-027.
- **The analyzer base does not format through an FMT-PIPE-027 entry point.** `VerifyFormatterFixCore` calls `FormattingPipeline.Execute(root, new FormattingContext(endOfLine, languageVersion: ...), ...)` directly, with `CSharpParseOptions.Default` and the line ending the test passes. It does not call `FormatSyntaxTree` or `FormatDocumentAsync`. So it skips their syntax-error, generated-code, and hiding-carrier (FMT-PIPE-033) returns, and it never reads `FormattingResult.Unsupported`. FMT-PIPE-027 covers only the output of a run that formatted, from `FormatDocumentAsync`/`FormatSyntaxTree`. On the base's output, the check is therefore the formatter's phases run alone on what the full pipeline produced. It is FMT-PIPE-027 only where the input formats and the forced line ending equals the one detected in the output. See *Breaking inputs* for the members where these differ.
- **CLI test.** `src/Reihitsu.Cli.Test/Integration/FormatCommandHandlerIntegrationTests.cs:601` `ExecuteAsyncFormattingIsIdempotent` formats one 17-line fixture (`ValidInputTestData`, line 25) twice through `FormatCommandHandler`, under one line ending. The handler (`src/Reihitsu.Cli/FormatCommandHandler.cs:420-456`) parses with `CSharpParseOptions.Default.WithLanguageVersion(languageVersion)`, calls `FormatSyntaxTree` through `DefaultSourceFormatter`, and writes `formattingResult.Output.GetTextAsync()` unchanged, apart from the encoding. A per-phase check on the written file therefore checks only `FormatSyntaxTree`'s output for one trivial fixture. It can find nothing about the CLI, and `Reihitsu.Formatter.Test` already checks that formatter path on far richer inputs. The test has no `[Requirement]` tag.
- **Reachability.** This is a tests-only issue. The check runs on outputs of `FormatSyntaxTree`/`FormatDocumentAsync`, which the CLI and whole-document formatting call, and of the full pipeline, which these entry points run. No caller-reachability concern applies.
- **Referenced test projects do not run their tests in the referencing project (checked).** `scripts/test.sh --project selfhosting --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Indentation.LayoutComputerTests.ComputeReturnsNonEmptyModelForSimpleClass"` built `Reihitsu.Formatter.Test` as a dependency and reported `No test matches the given testcase filter ... in .../Reihitsu.SelfHosting.Test.dll`. A `ProjectReference` from `Reihitsu.Analyzer.Test` to `Reihitsu.Formatter.Test` therefore adds no `Reihitsu.Formatter.Test` tests to the analyzer run. It only adds build time.

## Mechanism and candidates

_N/A._ This is not a bug. The defect is a missing assertion: the check has one owner, and two test projects do not call it. No rule is stated twice. `DescribeFailure` holds the failure text once, so this is not an architecture finding.

## Breaking inputs

Failure classes that reach the change: FC-010 and FC-012 (what the check finds), FC-017 (assertion weaker than the guarantee), FC-033 (the harness normalizes the scenario away), and FC-042 (a phase needs no guard for a declared shape).

Candidate inputs the test implementer must settle:

1. **Fixtures the precheck refuses (FC-042 direction).** The base ignores `FormattingResult.Unsupported`. A refused fixture passes the base today, because its output equals its input and is analyzer-clean. FMT-PIPE-027 does not cover such output, and per `AGENTS.md` (*Formatter phase ownership*, `PrecheckPhase` row) a later phase needs no guard for a declared shape. A phase run alone on a refused fixture may therefore change it, which is a false failure. I found no analyzer formatter fixture that is refused. Two tests say they deliberately avoid the FMT-UNSUP-004 shape (`RH6003SemicolonsMustBeSpacedCorrectlyFormatterTests.cs:74`, `RH5113DeclarationSemicolonMustStayOnDeclarationLineFormatterTests.cs:145`). I did not run all 460 fixtures through the precheck. Whether any is refused is unverified, and it is the first fact to establish.
2. **Generated-code, hiding-carrier, and syntax-error inputs.** The base formats these through `FormattingPipeline.Execute`, while `FormatSyntaxTree` would return them unchanged. No derived class holds an `<auto-generated>` marker (`rg` found none). Syntax errors fail the analyzer `Verify` step first. Ten derived classes hold `#if` fixtures. Whether any holds a hiding carrier is unchecked, and the case is rare. Running the check on such an output is stricter than FMT-PIPE-027. That is the stance `PhaseStabilityCheck`'s own summary takes for its session helpers, and an internal choice (below).
3. **Mixed-line-ending fixtures (FC-033).** `RH5604CodeMustNotContainMixedLineEndingsFormatterTests.VerifyFormatterFixesViolation` (line 30) passes mixed input through the base with a forced `Environment.NewLine`. Whether the check's line ending is the forced one or the one detected in the output decides whether the check sees FMT-PIPE-027's input. For a fully normalized output they agree. Which line ending the check uses is an internal choice (below).
4. **Both line endings.** The base already runs every fixture under the test's line ending and under the other one. The check must run on both outputs, not on the first ending only (FC-017; FC-010 says this class often appears only under CRLF).
5. **The output the check runs on.** The check must run on the output the base asserts idempotent, not on the input. Both outputs equal `fixedSource` once the assertions have passed.
6. **The wiring must be falsifiable (FC-017, FC-033).** `Reihitsu.Formatter.Test` proves its session check fires by injecting phases through `PhaseStabilityCheck.OverrideForCurrentTest`. That method is `internal` to `Reihitsu.Formatter.Test` and session-bound, and `Reihitsu.Formatter.Test` declares no `InternalsVisibleTo`. From `Reihitsu.Analyzer.Test`, only the public `AssertNoPhaseChanges` (text and tree overloads), `DescribeChangingPhases(SyntaxTree)`, and `FindChangingPhases` are reachable. The test implementer has to show that the base actually reaches the check. A call that is never exercised on a failing input is an assertion nobody saw fail.
7. **Existing fixtures may already fail.** This is a different corpus from `Reihitsu.Formatter.Test` and the repository's own source. Any analyzer fixture whose output a single phase changes is a pre-existing formatter defect that this change exposes. It is in the run's reach (`squad/README.md`, *Architecture findings*), and the lead routes it. I did not run the corpus, because doing so needs the change itself.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer (`Reihitsu.Analyzer`) | not involved |
| Formatter phases and entry points | must stay. No production change; the check only reads the pipeline |
| Code fixes, Fix All | not involved |
| `Reihitsu.Core` | not involved |
| `src/Reihitsu.Formatter/Properties/AssemblyInfo.cs` | must stay under the reference option. The shared-project option would need a new `InternalsVisibleTo` here, a production path (below) |
| `src/Reihitsu.Analyzer.Test/Base/FormatterTestsBase{TAnalyzer}.cs` | must change |
| `src/Reihitsu.Analyzer.Test/Reihitsu.Analyzer.Test.csproj` | must change (one `ProjectReference`). The lane check classifies this as tooling, so the lead's expected widening applies |
| `src/Reihitsu.Formatter.Test/Helpers/PhaseStabilityCheck.cs` | must stay, or change only in its summary if the lead wants it to name the analyzer base as a caller. Its public surface already covers a session-free caller |
| `src/Reihitsu.Cli.Test/...` and its `.csproj` | not involved if the CLI goal is dropped (recommended, *Issue goals*) |
| Rule pages, specs | not involved. FMT-PIPE-027 already states the requirement, and the run adds no requirement |
| `scripts/test.sh`, `TestScriptProjectSelectionTests` | not involved under the reference option. A new shared project that is a test project would touch them |

## Refuse option

Refusing an input class does not apply here: no fix or formatter construct is involved. The lighter option is **no change at all**. `Reihitsu.SelfHosting.Test` and `Reihitsu.Formatter.Test` already run the check, but the analyzer fixtures are a distinct corpus, so no change leaves real coverage unused. I do not recommend it.

Structure, as the issue asks:

- **A: `ProjectReference` from `Reihitsu.Analyzer.Test` to `Reihitsu.Formatter.Test` (recommended).** One line in one `.csproj`. It follows the precedent of `Reihitsu.SelfHosting.Test` (run #1794, decision 6). It needs no `InternalsVisibleTo` change, because `PhaseStabilityCheck` is compiled into `Reihitsu.Formatter.Test`, which already sees the internals of `Reihitsu.Formatter`. It runs no extra tests (checked above). Cost: `scripts/test.sh --project analyzer` now also builds `Reihitsu.Formatter.Test`. Both test projects declare a public `RequirementAttribute` (`Reihitsu.Formatter.Test.Helpers` and `Reihitsu.Analyzer.Test.Base`). The base file lives in `Reihitsu.Analyzer.Test.Base`, so its own attribute is in the enclosing namespace. I did not compile either variant.
- **B: a shared test-support project.** It would move `PhaseStabilityCheck`, `PhaseChange`, `SecondPassInput`, and the session-free path out of `Reihitsu.Formatter.Test`. These use internal formatter types (`FormattingPipeline`, `IFormattingPhase`, `PreparationPhase`, and `FormattingContext` are all `internal`), so the new assembly needs a new `InternalsVisibleTo` in `src/Reihitsu.Formatter/Properties/AssemblyInfo.cs`. That is a production path, and together with the tooling paths, no lane admits the diff without a change doc (`squad/README.md`, *Lanes*). B also adds a project to the solution and splits `PhaseStabilitySession` from the `FormatterTestsBase` lifecycle it serves. It costs a new project and a production edit, and gains nothing A lacks.

Declaring a shape unsupported is not an option here: the run changes no phase, fix, or spec clause.

## Output questions

_None._ No visible surface changes. The check only adds assertions to tests whose expected outputs stay as they are.

## Internal choices

1. **Which overload the analyzer base calls.**
   - (a) Text overload `AssertNoPhaseChanges(finalOutput, endOfLine, CSharpParseOptions.Default)`, with the line ending and parse options the base formatted with. This mirrors `Reihitsu.Formatter.Test`'s session, which also passes its explicit line ending.
   - (b) Tree overload, which detects the line ending in the output, as FMT-PIPE-027 states. The base has only a root from `FormattingPipeline.Execute`, not the tree it parsed, so (b) needs a reparse with the base's options.

   They differ only for an output whose detected line ending differs from the forced one, and visible output is unaffected either way. Recommendation: **(a)**. It checks exactly the input the base's own second pass formats, and FMT-PIPE-027's input is the same text whenever the output is normalized to the forced ending, as the base asserts it is.
2. **Whether the base checks an output the precheck refused.**
   - (a) Skip it when `FormattingResult.Unsupported` is set. This is what FMT-PIPE-027 covers.
   - (b) Check it anyway.

   Recommendation: **(a)**, if any fixture is refused at all (*Breaking inputs* 1). A phase run alone on a declared shape has no guard to keep and is not wrong to change it. If no fixture is refused, the choice is moot.
3. **Whether the base checks generated-code or hiding-carrier output** that it formats only because it bypasses `FormatSyntaxTree`. Recommendation: check it, like the `Reihitsu.Formatter.Test` session helpers, which are documented as stricter than the requirement. No such fixture is known. Visible output is unaffected either way.

## Issue goals

The issue is `agent-filed`.

- **Drop the CLI goal** ("the CLI idempotency test run[s] the per-phase check"). Doing so changes no visible output. The CLI writes `FormatSyntaxTree`'s output verbatim, so the check there tests the formatter on one trivial 17-line fixture under one line ending. It can never find a CLI defect, and it would add a test-to-test `ProjectReference` to `Reihitsu.Cli.Test` for no coverage. With it dropped, the issue's purpose (FMT-PIPE-027 checked wherever a meaningful full-run idempotency corpus exists, with one owner of the failure text) is still met, and the PR can say `Closes #1805` with the difference stated.
- **Correct the stale count.** The issue says 159 derived classes; there are 158. This has no visible effect.
- **"Check's failure text defined once"** holds already (`PhaseStabilityCheck.DescribeFailure`) and stays true under option A. No action beyond not copying the check.
- **The issue's "structural cause"** (check in one project, idempotency asserted in three) is not an architecture finding in the sense of `squad/README.md`. No decision has two owners and no rule is stated twice. It is a coverage gap.

## Architecture findings

_None._

## Follow-up candidates

- **Adjacent candidate for this run, the lead's call:** `src/Reihitsu.Analyzer.Test/Formatter/Formatting/RH5604CodeMustNotContainMixedLineEndingsFormatterTests.cs:182` `VerifyFormatterEntryPointsAndIdempotency` asserts full-run idempotency of `FormatSyntaxTree` on mixed-line-ending input. That output is exactly FMT-PIPE-027's input, with the line ending detected. Once option A is in place, a call to `PhaseStabilityCheck.AssertNoPhaseChanges(firstTree)` there is a single line. It is in the component the run changes and puts nothing visible in question. Other derived classes also call `FormatSyntaxTree` themselves (RH5021, RH5029, RH5030, RH5104, RH5111, RH5201, RH5412 to RH5416, RH6001). I did not check which of them assert idempotency; they are candidates of the same kind.
- **Unverified fixture failures.** Any analyzer fixture that the new check fails on is a pre-existing formatter defect (FC-010/FC-012) that this change exposes. If the run does not fix it, it is a follow-up (*Breaking inputs* 7).
