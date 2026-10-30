Role: test implementer
Commit: cd2f8fa
Verdict: tests written

This repair round follows `04-implementer.md` (disputes 1 and 2) and the maintainer's decision `squad/decisions/2026-10-30-preparation-phase-disputed-tests.md` (1A, 2A). The message that started the round carried no intended fix, suspected cause, or hoped-for verdict beyond that decision. No production code was touched.

**The test project does not compile until production follows the new signature.** The four `DocCommentElementNormalizerTests` calls now pass a third argument, so production needs `DocCommentElementNormalizer.BuildReplacement(XmlElementSyntax element, SourceText sourceText, string endOfLine)`. Until then the build fails with CS1501 at `DocCommentElementNormalizerTests.cs:153`, `:178`, `:214`, `:250`, and with no other error.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Unit.Pipeline.PhaseStabilityCheckTests.CheckRunsTheProductionPhasesInPipelineOrder` (assertion kept; the checked names are now `PhaseStabilityCheck.GetReportedName` of each production phase) | none (test infrastructure) | `PhaseStabilityCheck.CreateProductionPhases`, `ObservedPipeline.Run` | passes on `cd2f8fa`: both sides are the same 12 names, starting `PreparationPhase/TabReplacement`, `PreparationPhase/LineEndings` |
| `Unit.Pipeline.PhaseStabilityCheckTests.EveryCauseNamesAProductionPhase` (compares with reported names instead of type names) | none | same | passes |
| `Unit.Pipeline.PhaseLineEndingOwnershipTests.ExpandedDocumentationElementOnTheLastLineIsWrittenWithTheDetectedLineEnding` (input of dispute 2, exactly: `class C` / `{` / `}` / `/// <summary>a</summary>` with no final line break, under LF and CRLF) | FMT-PIPE-019, FMT-PIPE-004; FC-010, FC-033, FC-034 | `AssertEveryPhaseUsesTheDetectedLineEnding` (per-phase scan, exact output, second pass, `PhaseStabilityCheck.Verify`) | fails on `cd2f8fa` under CRLF: `DocumentationCommentFormattingPhase` returned two `\n` documentation newlines (lines 2 and 3); the LF row holds |
| `Unit.DocumentationComments.DocCommentElementNormalizerTests.BuildReplacementExpandsSingleLineSummary`, `…CollapsesNonSummaryElement`, `…DoesNotDuplicateLeadingSentenceText`, `…PreservesCodeIndentation` (call changed to pass the detected line ending; assertions unchanged) | existing tags (FMT-DOC-008, FMT-DOC-009, FMT-DOC-011) | `BuildReplacement(element, sourceText, DetectEndOfLine(element))`, where the new private `DetectEndOfLine` calls `ReihitsuFormatterHelpers.DetectEndOfLine` on the element's root | does not compile until the signature exists |

The regression test's expected output `class C;` + blank line + the expanded summary was taken from `scripts/format.sh` on the LF input at `cd2f8fa`, where LF is the right line ending on Linux. Only the CRLF line endings differ from what the code produces today.

Helper changes (`src/Reihitsu.Formatter.Test/Helpers/`):

- `PhaseStabilityCheck.CreateProductionPhases` replaces the `PreparationPhase` element of `FormattingPipeline.CreatePhases` with its `SubPhases`. The check therefore runs each sub-phase alone on the final output, as decision 1A states.
- `PhaseStabilityCheck.GetReportedName(phase)` is new. It runs the phase alone through `FormattingPipeline.ExecutePhases` on an empty compilation unit and returns the single name the observer reports. The naming rule therefore stays in production (`FormattingPipeline`/`PreparationPhase.GetSubPhaseName`) and is not copied into the test helper.
- `FindChangingPhases` names each change by `GetReportedName`.
- `PhaseChange`, `PhaseReversalCause`, and `PhaseReversalEntry` doc comments now say "the name under which the pipeline observer reports the phase" instead of "the type name" (FC-031). No exception-list entry names a preparation sub-phase, so no entry changes.

Run with the four `DocCommentElementNormalizerTests` calls temporarily restored to `HEAD` (and put back afterwards): `PhaseStabilityCheckTests` plus `PhaseLineEndingOwnershipTests`, 56 passed and 1 failed. The failure is the new regression test, under CRLF, for the reason above.

## Not covered, and why

- No `DocCommentElementNormalizerTests` case passes a line ending that differs from the source's. The regression test covers the fallback path through the pipeline, which is where FMT-PIPE-019 states the guarantee.
- The LF row of the regression test cannot fail on Linux (`Environment.NewLine` is `\n`). The CRLF row is the one that reaches the fallback.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~PhaseStabilityCheckTests|FullyQualifiedName~PhaseLineEndingOwnershipTests|FullyQualifiedName~DocCommentElementNormalizerTests"
```
