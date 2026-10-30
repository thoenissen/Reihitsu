Role: implementer
Commit: a61f166
Verdict: test disputed

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Everything below comes from the code at `a61f166` and from Debug runs on it.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Preparation/PreparationPhase.cs` (new) | `IFormattingPhase` with parameterless constructor. `SubPhases` holds `TabReplacementPhase`, then `LineEndingNormalizationPhase`. `Execute` runs them in order. `GetSubPhaseName(phase)` returns `PreparationPhase/TabReplacement` or `PreparationPhase/LineEndings` for those two types, `null` for any other phase | FMT-PIPE-001, FMT-PIPE-019 |
| `src/Reihitsu.Formatter/Pipeline/FormattingPipeline.cs` | `CreatePhases` starts with `PreparationPhase`; `LineEndingNormalizationPhase` no longer runs last. `ExecutePhases` runs a `PreparationPhase` as its sub-phases, one observed step each. The new private `ExecutePhase` reports a phase as `PreparationPhase.GetSubPhaseName(phase) ?? phase.GetType().Name`, so a sub-phase run alone gets the same name. Summaries updated (FC-031) | FMT-PIPE-001 |
| `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/ControlFlowBraceTransform.cs` | The open-brace line break and the line break after each leading comment were `SyntaxFactory.ElasticCarriageReturnLineFeed`. They are now `SyntaxFactory.ElasticEndOfLine(endOfLine)`, the context's line ending; `NormalizeLeadingTrivia` takes `endOfLine` | FMT-PIPE-019 |
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTokenCleanupRewriter.cs` | `CollapseLeadingBlankLines`, `RemoveExcessBlankLinesAfterIndex`, `RemoveBlankLinesAfterTrailingEndRegion`, and `RemoveBlankLinesAfterTrailingDirective` take `endOfLine` (`_context.EndOfLine`) and write it. They no longer copy an input break with an `Environment.NewLine` default. Output is unchanged, because after the preparation phase every copied break already is the context value | FMT-PIPE-019 |
| `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | `ComputeDocumentColumn` no longer has its own tab rule, and `IsInOwnLeadingWhitespace` is removed. When the entry point does not keep the token's leading trivia, the column is measured on that trivia as `TabReplacementPhase.ReplaceWhitespaceTabs` rewrites it. It is the `SourceText` length of the token's last line, built from the text in front of the trivia plus the rewritten trivia. A byte order mark at text start still counts zero | FMT-PIPE-017 (decision `2026-10-30-preparation-phase-tab-column-copy.md`, 1B) |
| `src/Reihitsu.Formatter/Pipeline/TabReplacement/TabReplacementPhase.cs` | `ReplaceWhitespaceTabs` changed from private to internal and moved above the private methods; its body is unchanged. Class summary: "first sub-phase of the preparation phase" | FMT-TAB-001, FMT-PIPE-017 |
| `src/Reihitsu.Formatter/Pipeline/LineEndings/LineEndingNormalizationPhase.cs` | Class summary only: second sub-phase, runs before every layout phase, returns its input node when it has nothing to rewrite (FC-031). Body unchanged | FMT-EOL, FMT-PIPE-019 |
| `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs` | Class summary only: the trace reports the preparation phase as its two sub-phases | FMT-PIPE-001 |

The 2026-10-30 tab-column decision (1B) required the existing FMT-PIPE-017 tests to pass unchanged. They do: `ReihitsuFormatterNodeColumnTests` and `ByteOrderMarkTests` pass in the formatter run below.

## Policy owners

- Line-ending writers: `rg -n "ElasticCarriageReturnLineFeed|CarriageReturnLineFeed|Environment\.NewLine|SyntaxFactory\.(Elastic)?LineFeed" src/Reihitsu.Formatter src/Reihitsu.Core --type cs`. Remaining hits:
  - `LineEndingUtilities.cs:64` and the doc comment at `ReihitsuFormatterHelpers.cs:65`: the detection fallback, FMT-PIPE-007, the owner.
  - `DocCommentElementNormalizer.cs:106`: not changed. See *Disputed tests or scope*.
- Tab replacement rule: `rg -n "'\\t'|\"\\t\"" src/Reihitsu.Formatter src/Reihitsu.Core --type cs`. The only code that replaces tabs is `TabReplacementPhase` (`:64`, `:66`, `:99`). The other hits detect whitespace characters and do not count columns: `AdjacentTokenSpacingUtilities`, `FormattingTextAnalysisUtilities`, `DocumentationCommentUtilities`, and `DocCommentElementNormalizer.cs:159`. `ComputeDocumentColumn` now calls `TabReplacementPhase.ReplaceWhitespaceTabs` and has no copy.
- Reported phase names: one owner, `PreparationPhase.GetSubPhaseName`, used by `FormattingPipeline.ExecutePhase`.

## Tests run

```shell
scripts/build.sh --configuration Debug                                  # Build succeeded.
scripts/test.sh --configuration Debug --project formatter               # Failed: 1, Passed: 3146, Total: 3147
scripts/test.sh --configuration Debug --project cli                     # Failed: 0, Passed: 251, Total: 251
scripts/test.sh --configuration Debug --project architecture            # Failed: 0, Passed: 89, Total: 89
scripts/test.sh --configuration Debug --project analyzer                # Failed: 0, Passed: 6242, Total: 6242
```

Every test listed in `03-test-implementer.md` passes. That includes `PhaseLineEndingOwnershipTests`, `ReihitsuFormatterLineEndingIndependenceTests`, the FMT-PIPE-001 tests in `FormattingPipelineTests`, the FMT-PIPE-019 tests in `PhaseColumnOwnershipTests`, `TabReplacementPhaseTests`, the node-level FC-028 tests, and `PipelineTraceCommandTests`. `PhaseOwnershipDocumentationTests` and `SpecConsistencyTests` pass too. The one failure is the disputed test below. I did not run `scripts/format.sh`; that is the cleaner's step.

## Disputed tests or scope

### 1. `Unit.Pipeline.PhaseStabilityCheckTests.CheckRunsTheProductionPhasesInPipelineOrder` (`src/Reihitsu.Formatter.Test/Unit/Pipeline/PhaseStabilityCheckTests.cs:85-96`), not changed by the test implementer

- What it asserts: the names `ObservedPipeline.Run` observes equal `PhaseStabilityCheck.CreateProductionPhases().Select(phase => phase.GetType().Name)`.
- What the spec says: FMT-PIPE-001 and the change doc (*Sub-phases are phases of their own*) report the sub-phases as `PreparationPhase/TabReplacement` and `PreparationPhase/LineEndings`. Those are not type names; a type name cannot contain `/`. So no production design makes both sides equal. If `CreatePhases` listed the two sub-phase types instead of `PreparationPhase`, `GetType().Name` would give `TabReplacementPhase` and `LineEndingNormalizationPhase`, which still differ.
- Failure at `a61f166`: expected (observed) `["PreparationPhase/TabReplacement", "PreparationPhase/LineEndings", "StructuralTransformPhase", …, "CleanupPhase"]` (12); actual (checked) `["PreparationPhase", "StructuralTransformPhase", …, "CleanupPhase"]` (11).
- Same assumption elsewhere in test code, not failing today:
  - `PhaseStabilityCheck.FindChangingPhases` (`Helpers/PhaseStabilityCheck.cs:212`) reports a change as `phase.GetType().Name`. A change made by the preparation phase is therefore reported as `PreparationPhase`, a name the observer never reports.
  - `EveryCauseNamesAProductionPhase` (`PhaseStabilityCheckTests.cs:806-815`) compares causes with type names. No cause names a preparation sub-phase, so it passes.
- Facts for the test implementer: `CreatePhases` returns `PreparationPhase` as its first element. `PreparationPhase.SubPhases` lists the sub-phases. `PreparationPhase.GetSubPhaseName` maps a sub-phase to its reported name, and `FormattingPipeline.ExecutePhases` reports by that name.

### 2. A run defect without a test: the documentation-comment phase writes `Environment.NewLine` (FMT-PIPE-019, and FMT-PIPE-004 after the reorder)

- Input: a CRLF file whose last line is an expandable documentation element with no final line break. Its bytes: `class C\r\n{\r\n}\r\n/// <summary>a</summary>`.
- `bash scripts/trace.sh <file> --no-install` on `a61f166`:
  - pass 1: `DocumentationCommentFormattingPhase` expands the summary;
  - pass 2: `PreparationPhase/LineEndings` reports `Line endings: CRLF=2, LF=2, CR=0 -> CRLF=4, LF=0, CR=0`.
  So pass 1 wrote two `\n` into the CRLF file, and the second pass changes the output. Before the reorder, the final `LineEndingNormalizationPhase` repaired them in pass 1, so this second-pass change is new with this run.
- Cause in the code: `DocCommentElementNormalizer.GetLineBreak` (`:102-107`) copies the line break of the start tag's line. When that line is the last line of the text, it falls back to `Environment.NewLine`, which is `\n` on Linux.
- Why I did not fix it: the fix needs the context's line ending inside `DocCommentElementNormalizer.BuildReplacement`. `DocCommentElementNormalizerTests` (`:152`, `:177`, `:213`, `:249`) call `BuildReplacement(element, sourceText)` directly, and their expected outputs take their line breaks from the test file's checkout. Changing the signature breaks those four calls, and the role forbids changing them. Any variant that keeps the two-argument form compiling leaves a path that does not use the context's line ending: an overload, an optional argument, or a fallback inferred from the text.
- None of the per-phase inputs in `PhaseLineEndingOwnershipTests` reaches the fallback. Their documentation comments are followed by a line break. A CRLF case on Linux, or an LF case on Windows, with a documentation element on the last line and no final line break, would reach it.

## Architecture findings

_None._ The duplicate tab rule (`02-skeptic.md`) is resolved as the 2026-10-30 tab-column decision (1B) requires.

## Follow-up candidates

- `PhaseStabilityCheck.FindChangingPhases` names changes by type name. This is test-helper scope, already covered under dispute 1.
- `BlankLineTokenCleanupRewriter` and `ControlFlowBraceTransform` now write the context value. No other `Environment.NewLine` or fixed CRLF/LF writer remains in the formatter except the one under dispute 2.
