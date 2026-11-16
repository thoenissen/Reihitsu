Role: test implementer
Commit: 074c079
Verdict: tests written

My prompt carried the scope of decisions 1–4 in `00-lead.md` and no intended fix, suspected cause, or hoped-for verdict beyond them; nothing to ignore.

In the tests lane I own the harness change as well as the tests. No production code is touched; no spec covers the harness, so no test carries a `[Requirement]` tag.

## Harness change

- `src/Reihitsu.Analyzer.Test/Verifiers/WarningWaveDiagnostics.cs` (new): compiles every project of a solution at warning level **9999** (decision 3) and keeps the warnings whose `Diagnostic.WarningLevel` is above 4, not suppressed. That is the wave set read from the compiler itself, so no ID list drifts with a Roslyn upgrade. `GetIntroducedAsync(source, fixed)` returns the fixed-side diagnostics of every ID the fixed solution holds **more often** than the source (decisions 1 and 2: only what the fix adds counts; the source state is not checked on its own).
- `CSharpCodeFixVerifierTest{TAnalyzer,TCodeFix}.cs`: overrides `RunImplAsync`. After the testing library's own checks pass, it evaluates the test, fixed, and Fix All fixed states the way the library does (`WithInheritedValuesApplied` + `WithProcessedMarkup`), builds each with `CreateProjectAsync` plus the test's `SolutionTransforms`, and fails through `Verify.True` with a message naming the state ("fixed state" / "Fix All fixed state") and every introduced diagnostic. It skips a state whose sources equal the source state, skips the Fix All state when it equals the fixed state, and honors `CodeFixTestBehaviors.SkipFixAllCheck`. `VerifyFixAsync` is not virtual in 1.1.4, and `GetSolutionAsync` is not accessible, hence this shape. Because the check lives in the verifier class, every construction reaches it: `AnalyzerTestsBase<,>.Verify` (and through it `VerifyCodeFixWithEndOfLine` and `BatchCodeFixTestsBase.FixAllCorrectsEveryDiagnostic`), and the direct constructions in `Naming/RH4102EventNameCasingAnalyzerTests.cs` and `Shared/CodeFixInteractionTests.cs` (decision 4). Proof: `rg -n "new CSharpCodeFixVerifierTest" src/Reihitsu.Analyzer.Test` lists exactly those three construction sites and the new test class, and all of them share `CSharpCodeFixVerifierTest<,>.RunImplAsync`.
- `AnalyzerTestsBase{TAnalyzer,TCodeFix}.cs`: the two ad-hoc paths that apply a fix and return its text, `TryApplyCodeFixAsync` and `TryApplyCodeFixAtAsync`, compare the source and the changed document through the same `WarningWaveDiagnostics.GetIntroducedAsync` and fail with `Assert.IsTrue`. This covers `ApplyCodeFixAsync`, `VerifyCodeFixIsWithheldOrProducesOneOf` (whose fixed text the analyzer verifier otherwise checks for errors only), `VerifyCodeFixMatchesFormatter`, and every test class that calls these helpers directly. The paths that only register actions (`GetCodeFixActionsAsync`, `RegisterActionsForEveryDiagnosticAsync`, `VerifyNoCodeFixIsOffered…`) produce no fixed text and are not checked. The analyzer verifier is unchanged; it only compiles inputs.
- `ParenthesisRemovalProbeAnalyzer.cs` / `ParenthesisRemovalProbeCodeFixProvider.cs` (new, test-only): a probe that reports every parenthesized expression under RH3002's ID (derived from `DiagnosticAnalyzerBase`, like `DiagnosticAnalyzerBaseProbe`) and a fix that replaces its text with the inner expression's text without checking need, with `BatchFixer` for Fix All. No production fix writes CS8848 today (01-skeptic.md), so the proof needs it.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Verifiers.CodeFixWarningWaveCheckTests.FixedStateThatIntroducesAWarningWaveDiagnosticFailsVerification` | FC-033 (issue's proof: CS8848 in the fixed state, direct verifier construction) | `CSharpCodeFixVerifierTest` directly | pass (red before the harness change: no exception thrown) |
| `…CodeFixWarningWaveCheckTests.FixAllFixedStateThatIntroducesAWarningWaveDiagnosticFailsVerification` | FC-033, FC-003 (Fix All state reached on its own: `FixOne` keeps the fixed state clean, Fix All writes CS8848; asserts the message names "Fix All") | direct | pass (red before) |
| `…CodeFixWarningWaveCheckTests.OneStepFixedStateThatHoldsNoNewWarningWaveDiagnosticPassesVerification` | FC-003 (the one-step state of the Fix All test passes alone, so that test fails on Fix All only) | direct | pass (pass before) |
| `…CodeFixWarningWaveCheckTests.FixedStateThatHoldsAWarningWaveDiagnosticMoreOftenThanTheSourceStateFailsVerification` | FC-003 (counting per ID: source holds CS8848 once, fix writes it a second time) | direct | pass (red before) |
| `…CodeFixWarningWaveCheckTests.WarningWaveDiagnosticTheSourceStateAlreadyHoldsPassesVerification` | FC-003 (skeptic input 2: CS8848 in both states equally often; both states declare it at level 9999 with warnings compared, proving it is there) | direct | pass (pass before) |
| `…CodeFixWarningWaveCheckTests.LowerCaseTypeNameTheFixLeavesInPlacePassesVerification` | FC-003 (skeptic input 2: CS8981, wave 7, left in place; declared in both states) | direct | pass (pass before) |
| `…CodeFixWarningWaveCheckTests.OrdinaryWarningTheFixIntroducesPassesVerification` | FC-003 (skeptic input 3: the fix writes CS0184, level 1; declared in the fixed state) | direct | pass (pass before) |
| `Verifiers.CodeFixWarningWaveCheckBaseTests.VerifyFailsWhenTheFixWritesAWarningWaveDiagnostic` | FC-033 (base-class `Verify` path) | `Verify(source, fixedSource, …)` | pass (red before) |
| `…CodeFixWarningWaveCheckBaseTests.ApplyCodeFixFailsWhenTheFixWritesAWarningWaveDiagnostic` | FC-033 (ad-hoc path) | `ApplyCodeFixAsync` | pass (red before) |
| `…CodeFixWarningWaveCheckBaseTests.TryApplyCodeFixAtFailsWhenTheFixWritesAWarningWaveDiagnostic` | FC-033 (ad-hoc path at a chosen location) | `TryApplyCodeFixAtAsync` | pass (red before) |
| `…CodeFixWarningWaveCheckBaseTests.VerifyCodeFixIsWithheldOrProducesOneOfFailsWhenTheFixWritesAWarningWaveDiagnostic` | FC-033 | `VerifyCodeFixIsWithheldOrProducesOneOf` | pass (red before) |
| `…CodeFixWarningWaveCheckBaseTests.ApplyCodeFixPassesWhenTheSourceAlreadyHoldsTheWarningWaveDiagnostic` | FC-003 (ad-hoc side of skeptic input 2) | `ApplyCodeFixAsync` | pass (pass before) |
| `…CodeFixWarningWaveCheckBaseTests.ApplyCodeFixPassesWhenTheFixWritesAnOrdinaryWarning` | FC-003 (ad-hoc side of skeptic input 3, CS0184) | `ApplyCodeFixAsync` | pass (pass before) |
| `…CodeFixWarningWaveCheckBaseTests.FixAllCorrectsEveryDiagnostic`, `CodeFixProvidesFixAllProvider` (inherited) | — (required by `BatchCodeFixTestsBase`; harmless two-diagnostic scenario) | base | pass |

Red run (both harness files stashed, new tests in place): 7 failed — exactly the seven `…Fails…` tests, each with "Expected exception … but no exception was thrown" — and 8 passed. Green run: 15 of 15 passed.

## Existing tests

Full analyzer suite with the harness change: **10418 passed, 0 failed** (the 10404 of the baseline plus 14 new tests; the 15th new test, `FixedStateThatHoldsAWarningWaveDiagnosticMoreOftenThanTheSourceStateFailsVerification`, was added afterwards with no harness change and passes in the focused run). Baseline on `90da109`, run in a separate worktree: **10404 passed, 0 failed**. **No existing test changes outcome**, matching the skeptic's measurement; no fixture was edited.

## Not covered, and why

- **Line endings (LF/CRLF)**: the check compares compiler diagnostics by ID and count, not layout or positions; no line-ending variant can change a count. Not a formatter change.
- **Additional projects**: the check builds and compares every project of the state's solution; no current test uses an additional project together with a fix that changes it, and the probe has none. Left untested.
- **FC-004/019–021 (trivia), FC-010–014 (formatter), FC-038–043 (unsupported shapes), FC-018/036 (naming)**: no formatter phase, rewrite of user code, or rename is involved; the change is in the test harness only.
- **FC-007/008 (code-fix convergence, Fix All on overlapping spans)**: the probe fix is a test fixture, not a shipped fix; the library's own checks still run on it unchanged.
- **FC-015 (one owner)**: the verifier path and the ad-hoc path share `WarningWaveDiagnostics`; no private copy.
- **`CompilerDiagnostics.None` tests**: the check runs whatever `CompilerDiagnostics` a test sets, because it compares only what the fix adds; the full-suite run covers the existing `None` users (RH8036, RH8109, …) and they pass.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Verifiers.CodeFixWarningWave"
scripts/test.sh --project analyzer
```

## Cost

The check compiles two to three extra small solutions per code-fix test whose fixed text differs from its source. Suite duration (test host): 5 m 14 s on the baseline versus 5 m 40 s with the check, about 8 % more; the baseline overlapped with some focused runs, so the number is approximate.

## Follow-up candidates

_None._
