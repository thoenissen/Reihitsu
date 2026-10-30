Role: implementer
Commit: aa5621b
Verdict: tests pass

Repair round 1, auditor finding 2 of `09-auditor.md`. My prompt carried no intended fix or suspected cause. It passed on the auditor's suggestion, labeled as only a suggestion; the repair below I worked out myself. I did not touch `AGENTS.md` or the `ControlFlowBraceTransform` comment (findings 1 and 4, other owners).

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Preparation/PreparationPhase.cs` | `Execute` no longer has its own loop over `SubPhases`. It returns `FormattingPipeline.ExecutePhases(root, context, [this], phaseObserver: null, cancellationToken)`. That method expands a `PreparationPhase` into its sub-phases and runs each through `ExecutePhase`, which handles per-sub-phase cancellation and reporting. A `<remarks>` says so. | FMT-PIPE-001 (sub-phases are phases of their own) |

Production now holds one statement of the order (`PreparationPhase.SubPhases`) and one production runner of it (`FormattingPipeline.ExecutePhases`). `PreparationPhase.Execute` reaches that runner through the `phase is PreparationPhase` branch. It does not recurse, because `SubPhases` holds no `PreparationPhase`. The output of `PreparationPhase.Execute` is the output of `ExecutePhases` for the same input by construction, with no copy of the loop that could drift. Without an observer, a sub-phase failure propagates unwrapped, as it does for every other phase run without one.

I considered two other repairs and rejected both:

- Removing `Execute`: `PreparationPhase` must stay an `IFormattingPhase`. It is an element of `CreatePhases`, and the existing test creates it as one.
- Moving the loop into `PreparationPhase` and having `ExecutePhases` call it: this would lose per-sub-phase observation, which FMT-PIPE-001 requires.

## Policy owners

`rg -n "foreach \(var subPhase|SubPhases" src --type cs`:

- `PreparationPhase.cs:38` declares the list.
- `FormattingPipeline.cs:78` is the production runner.
- `PhaseStabilityCheck.cs:182` (test helper) expands the list into the phases the stability check runs one at a time. It does not run them in sequence. Test code, not mine.

## Tests run

```shell
scripts/build.sh --configuration Debug                       # Build succeeded.
scripts/test.sh --configuration Debug --project formatter    # Failed: 0, Passed: 3148, Total: 3148
scripts/test.sh --configuration Debug --project cli          # Failed: 0, Passed: 251, Total: 251
```

The analyzer and architecture projects were not rerun. This change touches only a method that no production path calls, and that no test calls before the new test below.

## For the test implementer

No existing test runs `PreparationPhase.Execute`, which finding 2 notes. A new test to name:

- `FormattingPipelineTests` (FMT-PIPE-001): `new PreparationPhase().Execute(root, context, ct)` gives the same text as `FormattingPipeline.ExecutePhases(root, context, [new PreparationPhase()], null, ct)`. The input needs a tab in indentation and a foreign line break, for example a lone `\r` in an LF file, so that both sub-phases change something. The output must also differ from the input.

Finding 3 (the test's reflection lookup of `PreparationPhase`) is test code and stays with the test implementer.

## Disputed tests or scope

_None._

## Architecture findings

_None._

## Follow-up candidates

_None._
