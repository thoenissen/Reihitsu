Role: test implementer
Commit: 935f59b
Verdict: tests written

Repair round 1: auditor finding 3 of `09-auditor.md`, plus a test for the `PreparationPhase.Execute` change that `10-implementer.md` made for finding 2. The test below is derived from FMT-PIPE-019's normative sentence. I did not copy the shape the implementer named: my test asserts an explicit expected output instead of only comparing two runners. No production code was touched, and neither was `AGENTS.md`.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Unit.Pipeline.FormattingPipelineTests.PreparationPhaseRunAloneReportsItsTwoSubPhasesInOrder` (finding 3: the reflection lookup and its stale "compiles before the type exists" comment are replaced by `new PreparationPhase()`; assertions unchanged) | FMT-PIPE-001; FC-031 | `FormattingPipeline.ExecutePhases` with an observer | passes |
| `Unit.Pipeline.FormattingPipelineTests.PreparationPhaseExecutedDirectlyReplacesTabsAndRewritesLineBreaks` (new) | FMT-PIPE-019 | `new PreparationPhase().Execute(…)` directly, context `\n` | passes on `935f59b`. Details below |
| `Unit.Pipeline.FormattingPipelineTests.PreparationPhaseExecutedDirectlyWithCancelledTokenThrowsOperationCanceled` (new) | none (the pipeline's cancellation behavior, which `Execute` now reaches through `ExecutePhases`) | `Execute` with a cancelled token | passes |

Details of `PreparationPhaseExecutedDirectlyReplacesTabsAndRewritesLineBreaks`:

- **Input:**
  - a tab in indentation and a tab between `#region` and its name;
  - `\r\n`, a lone `\r`, U+2028, and U+0085 as end-of-line trivia and as directive line breaks;
  - a lone `\r` and `\r\n` as documentation newlines.
- **Expected:** the explicit text with four spaces per tab and `\n` everywhere.
- **Second assertion:** the result equals `FormattingPipeline.ExecutePhases(root, context, [new PreparationPhase()], null, ct)`, so a direct `Execute` and the pipeline's runner cannot drift apart.
- **Why it can fail:** the expected output differs from the input in every carrier. An `Execute` that skipped either sub-phase, or returned its input, would fail the first assertion.

`scripts/test.sh --project formatter --filter "FullyQualifiedName~FormattingPipelineTests"` passes 14 of 14 on `935f59b` plus these changes.

## Not covered, and why

- The order of the two sub-phases inside `Execute` is not observable in its output: tab replacement and line-break rewriting touch disjoint characters. The order is asserted through the observer in `PreparationPhaseRunAloneReportsItsTwoSubPhasesInOrder`, and `Execute` now runs through that same runner.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~FormattingPipelineTests"
```
