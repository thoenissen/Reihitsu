Role: test implementer
Commit: 9e743e9
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples of FMT-PIPE-001, FMT-PIPE-019, and FMT-PIPE-020, the change doc, and the skeptic's candidate inputs. Where a test's subject is line endings and its fixture also exercises unrelated layout rules (blank lines around `#endif`/`#pragma`), that unrelated layout was taken from the unchanged code at `9e743e9` through `scripts/format.sh`, not from a patched copy. No production code was touched.

All results below are from runs on `9e743e9` plus these test changes.

## Tests

New and changed tests. "Fails now" means it fails on the unchanged code for the reason in the last column; "passes now" means it already holds and must keep holding.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Unit.Pipeline.FormattingPipelineTests.ExecuteObserverReportsAllPhasesInOrderWithoutChangingOutput` (expected list changed) | FMT-PIPE-001 | observer through `FormattingPipeline.Execute` | fails now: observed names are the old twelve, starting `TabReplacementPhase`, ending `LineEndingNormalizationPhase` |
| `…FormattingPipelineTests.TabReplacementSubPhaseRunAloneIsReportedUnderItsSubPhaseName` | FMT-PIPE-001 | `FormattingPipeline.ExecutePhases` | fails now: reported as `TabReplacementPhase` |
| `…FormattingPipelineTests.LineEndingsSubPhaseRunAloneIsReportedUnderItsSubPhaseName` | FMT-PIPE-001 | `ExecutePhases` | fails now: reported as `LineEndingNormalizationPhase` |
| `…FormattingPipelineTests.PreparationPhaseRunAloneReportsItsTwoSubPhasesInOrder` | FMT-PIPE-001 | `ExecutePhases`; locates the type named `PreparationPhase` in the formatter assembly by reflection and creates it with its parameterless constructor, so the test project compiles before the type exists | fails now: no type `PreparationPhase` |
| `Unit.Pipeline.PhaseColumnOwnershipTests.FirstPhaseReplacesEveryIndentationTabAndNoLaterPhaseSeesOne`, `…NoPhaseAfterTheFirstSeesATabInTheWhitespaceOfTheCodeOrBetweenDirectiveTokens`, `…TabInFrontOfDocumentationContinuationLineIsReplacedByTheIndentationPhase` (retagged FMT-PIPE-016 → FMT-PIPE-019; first-phase name assertion now `PreparationPhase/TabReplacement`) | FMT-PIPE-019 | `ObservedPipeline.Run` | fails now: first observed name is `TabReplacementPhase` |
| `Unit.TabReplacement.TabReplacementPhaseTests` (11 tests, unchanged; they use `ObservedPipeline.RunTabReplacementPhase`, whose name assertion now expects `PreparationPhase/TabReplacement`) | existing tags unchanged (FMT-TAB) | `ObservedPipeline.RunTabReplacementPhase` | fails now: first observed name is `TabReplacementPhase` |
| `Unit.Pipeline.PhaseLineEndingOwnershipTests.BracesAroundAStatementWithALeadingCommentAreWrittenWithTheDetectedLineEnding` (spec example) | FMT-PIPE-019 | `ObservedPipeline.Run` + `FindForeignLineBreaks`, LF and CRLF, second pass, `PhaseStabilityCheck.Verify` | fails now: under LF, `RegionFormattingPhase` receives two `\r\n` end-of-line trivia (written by the structural transform phase) |
| `…PhaseLineEndingOwnershipTests.BracesAroundBranchesWithLeadingCommentsAreWrittenWithTheDetectedLineEnding` (4 rows: two `//` comments; a `/* */` comment; an `else` branch with a comment; an `else if` chain with comments) | FMT-PIPE-019; FC-026, FC-027 | same | fails now: under LF, `RegionFormattingPhase` receives 2 to 4 `\r\n` end-of-line trivia |
| `…PhaseLineEndingOwnershipTests.LineBreaksAroundDirectivesAndCommentsAreTheDetectedLineEndingInEveryPhase` (`#region`, `#if false`/`#else`/`#endif`, `#pragma`, doc comment, trailing, block and leading comments) | FMT-PIPE-019; FC-020, FC-021 | same | fails now: under LF, `RegionFormattingPhase` receives two `\r\n` |
| `…PhaseLineEndingOwnershipTests.ForeignLineBreaksOfTheInputAreRewrittenBeforeEveryLaterPhase` (5 rows: lone `\r`, U+0085, U+2028, U+2029, and the other of LF/CRLF; each placed at the end of a `#region` directive, between tokens, and inside a documentation comment) | FMT-PIPE-019 | same | fails now: `RegionFormattingPhase` receives the five foreign breaks (directive trivia, end-of-line trivia, documentation newlines) |
| `…PhaseLineEndingOwnershipTests.LineBreaksInsideLiteralsCommentsAndDisabledTextKeepTheirCharacters` (2 rows: lone `\r`, U+2028 inside a verbatim string, a multi-line raw string, a block comment behind code, and `#if false` disabled text) | FMT-PIPE-019, FMT-EOL-004; FC-022 | same | fails now only on the first-phase name assertion; the line-ending scan of every phase from the third on holds, and `scripts/format.sh` on the LF input returns it unchanged for both rows |
| `Unit.ReihitsuFormatterLineEndingIndependenceTests.LoneCarriageReturnFileGetsTheLayoutOfItsLineFeedCopy` (spec example) | FMT-PIPE-020; FC-034 | `FormatSyntaxTree` and `FormatDocumentAsync`, output vs the copy's output vs the spec's output, second pass | fails now: `#region A` in column 0 |
| `…LineEndingIndependenceTests.UnicodeTerminatorFileGetsTheLayoutOfItsLineFeedCopy` (U+0085, U+2028, U+2029) | FMT-PIPE-020 | same | fails now: two blank lines between `#endregion // A` and `#region B` |
| `…LineEndingIndependenceTests.MixedLineBreaksGetTheLayoutOfTheirLineFeedCopy` (`\r\n`, `\n`, lone `\r` in turn; 4 `\r\n` against 6 one-character breaks, so `\n` is detected) | FMT-PIPE-020 | same | fails now: `#region A` in column 0 |
| `…LineEndingIndependenceTests.TiedLineBreaksGetTheLayoutOfTheirCarriageReturnLineFeedCopy` (5 `\r\n`, 5 lone `\r`, so `\r\n` is detected) | FMT-PIPE-020 | same | passes now |
| `…LineEndingIndependenceTests.ForeignNewlinesInsideADocumentationCommentGiveTheLayoutOfTheLineFeedCopy` (lone `\r`, U+0085, U+2028, U+2029 only inside a documentation comment of an LF file; comment, doc comment and member start at column 0) | FMT-PIPE-020 | same | fails now: `// c` stays at column 0 where the copy puts it at column 4 |
| `…LineEndingIndependenceTests.KeptLineBreaksInsideLiteralsCommentsAndDisabledTextAreTheSameInTheCopy` (10 rows: verbatim string, multi-line raw string, block comment behind code, block comment starting its line, `// c` above `#if false` disabled text, each with a lone `\r` and with U+2028 kept inside; the file's other breaks are lone `\r`, the copy's are `\n`, and the breaks inside the raw string and at the end of the disabled text stay lone `\r` in both) | FMT-PIPE-020; FC-022 | same, plus the kept characters must be in the output | passes now |
| `Unit.ReihitsuFormatterNodeColumnTests.StatementTargetIndentedWithTabsHoldingAForeignLineBreakCountsEachOfItsTabsAsFourColumns` | FMT-PIPE-017; FC-015, FC-028 | `FormatNodeInDocumentAsync`; under LF with `\r\n`, lone `\r`, U+2028 and under CRLF with `\n`, lone `\r`, U+2029 as the foreign break inside the target; second pass | passes now |
| `…NodeColumnTests.MisindentedStatementTargetHoldingAForeignLineBreakKeepsItsFirstTokenAndAlignsTheWrappedArgument` | FMT-PIPE-017; FC-028 | same | passes now |
| `…NodeColumnTests.ChainTargetWithContextHoldingAForeignLineBreakAlignsTheWrappedLink` | FMT-PIPE-017; FC-028 | `FormatNodeInDocumentWithContextAsync`, same foreign breaks | passes now |
| `Unit.ReihitsuFormatterNodeBlankLineTests.CollapsesTwoBlankLinesAboveRewrittenPropertyHoldingAForeignLineBreakToOne`, `…KeepsBlankLineAboveRewrittenPropertyHoldingAForeignLineBreak` | FC-028 (no requirement tag, like their siblings) | `FormatNodeInDocumentAsync`, same foreign breaks, `AssertUsesLineEnding` | passes now |
| `Unit.ReihitsuFormatterTests.FormatNodeFormatsDetachedNodeHoldingAForeignLineBreak` | FC-028 | `FormatNode` on a detached method, same foreign breaks, second pass | passes now |
| `Reihitsu.Cli.Test.Unit.PipelineTraceCommandTests.ExecuteAsyncMixedLineEndingsReportsCountFallback` (header now `== Pass 1: PreparationPhase/LineEndings ==`) | none (repository tool) | `PipelineTraceCommand.ExecuteAsync` | fails now: header is `LineEndingNormalizationPhase` |
| `…PipelineTraceCommandTests.ExecuteAsyncPreservesDetectedLineEndings` (asserts no `PreparationPhase/LineEndings` in the trace instead of no `LineEndingNormalizationPhase`) | none | same | passes now, trivially; it holds its meaning after the rename |
| `…PipelineTraceCommandTests.ExecuteAsyncReportsTabReplacementUnderItsSubPhaseName` | none | same | fails now: no `PreparationPhase/TabReplacement` header |

Helper changes, in `src/Reihitsu.Formatter.Test/Helpers/ObservedPipeline.cs`: constants `TabReplacementPhaseName` (`PreparationPhase/TabReplacement`) and `LineEndingsPhaseName` (`PreparationPhase/LineEndings`); `RunTabReplacementPhase` asserts the new first name; new `FindForeignLineBreaks(root, endOfLine)` reports every `EndOfLineTrivia` (descending into directive and documentation trivia) and every `XmlTextLiteralNewLineToken` whose text differs from the line ending, with all five terminators distinguished. Line breaks inside literal tokens, comment trivia, and disabled-text trivia are not separate trivia or tokens, so they are not scanned (the FMT-PIPE-019 exemption).

Comment-only change (FC-031): the summary of `DocumentationCommentFormattingPhaseTests.NormalizesEveryDocumentationLineTerminator` no longer says the separators are "normalized later by `LineEndingNormalizationPhase`".

## Not covered, and why

- `Reihitsu.ArchitectureTests` `PhaseOwnershipDocumentationTests` and `SpecConsistencyTests` already fail against the spec author's text (12 observed names; `PreparationPhase` as a code owner); they need no new test.
- `PhaseStabilityCheck` runs whatever `CreatePhases` returns and reports changes by `phase.GetType().Name`. With a composite `PreparationPhase` it will name `PreparationPhase`, not a sub-phase; no `PhaseReversalExceptionList` entry names a preparation phase, so nothing is affected. Not a test of mine to change.
- The `Environment.NewLine` fallbacks in `DocCommentElementNormalizer.GetLineBreak` and `BlankLineTokenCleanupRewriter`: the skeptic found no input that reaches a foreign write from them (the doc fallback needs the start tag on the root text's last line). The CRLF rows of every `PhaseLineEndingOwnershipTests` test would show such a write on Linux. No dedicated input exists.
- Defect 2 of `02-skeptic.md` (a kept terminator inside a comment or disabled text decides layout) is out of the run. The FMT-PIPE-020 kept rows only compare file and copy, which agree by construction. They do not assert the layout of those cells.
- Failure classes not reachable: FC-007, FC-008, FC-009, FC-018, FC-030, FC-036 (no code fix and no rename changes); FC-013, FC-014 (no analyzer rule changes); FC-023, FC-024, FC-025, FC-029 (no syntax-shape rule changes); FC-037 (no text-only proof claimed). FC-032: analyzer results gathered before the implementation are not evidence after it; the validator's full run covers that.
- The analyzer test project was not run; no analyzer test was changed.

## Observations for the next roles

- `PreparationPhaseRunAloneReportsItsTwoSubPhasesInOrder` requires a type named exactly `PreparationPhase` in the formatter assembly, implementing `IFormattingPhase`, with a parameterless constructor (public or not).
- `PhaseLineEndingOwnershipTests` expects the observer to report the sub-phases as phases 1 and 2 of `Execute`, with `PreparationPhase/LineEndings` receiving the output of `PreparationPhase/TabReplacement`.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~PhaseLineEndingOwnershipTests|FullyQualifiedName~ReihitsuFormatterLineEndingIndependenceTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~FormattingPipelineTests|FullyQualifiedName~PhaseColumnOwnershipTests|FullyQualifiedName~TabReplacementPhaseTests|FullyQualifiedName~DocumentationCommentFormattingPhaseTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterNodeColumnTests|FullyQualifiedName~ReihitsuFormatterNodeBlankLineTests|FullyQualifiedName~Unit.ReihitsuFormatterTests"
scripts/test.sh --project cli --filter "FullyQualifiedName~PipelineTraceCommandTests"
```

Result on `9e743e9` plus these tests: formatter, 22 of 33 in the first command fail, 18 of 221 in the second and third together fail (all on the phase names), the node-level tests all pass; CLI, 2 of 13 fail.
