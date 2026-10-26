Role: implementer
Commit: (this commit; see `git log -1 -- sdd/work/phase-prediction-removal/40-implementer.md`)
Verdict: tests pass

My prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It named the revert of the context entry point, which is the recorded decision "Audit 3", option B (`00-lead.md`, last section), not a hint. This is the final round under that decision.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs`, `FormatNodeInDocumentWithContextAsync` | Back to its code at 504310f, the state before the #8 repair. The target's original leading trivia is written back in every case. The context's first-token column counts every character in front of it as one column (`countsOwnWhitespaceTabsAsReplaced: false`). The `keepsLeadingTrivia` condition and the `RestoreBlankLines` write-back are gone. The comments now state FMT-PIPE-018 and the gap FMT-PIPE-017 records. The `<remarks>` gain one sentence: the leading trivia is written back as it stands, so a target that starts a later line of the context keeps that line's column. | FMT-PIPE-018, FMT-PIPE-017 (recorded gap), FMT-PIPE-010; finding #11 |
| `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | Removed `RestoreBlankLines` and its private helpers `SplitIntoGaps`, `CountLineBreaks`, `EndsWithLineBreak`, and `RestoreGapLineBreaks`. After the revert they have no caller. The file is byte for byte as at 504310f. | — |

**The #9 repair stays.** `IndentationPhase.cs` and `IndentationRewriter.cs` are unchanged in this round. The `lineOffset` mapping is still gone, and the clone of an attached root is still the one mechanism. `RawStringLiteralTargetMovesContentUnderTheOpeningQuotes` and `MisalignedRawStringInAttachedMethodBelowTheFirstLineIsCorrected` pass.

**Kept, still in use.**
- `ComputeDocumentColumn(token, countsOwnWhitespaceTabsAsReplaced)` keeps its parameter: `FormatNodeInDocumentAsync` passes `keepsLeadingTrivia == false`.
- `StartsOnNewLineIncludingDocumentation` is still called by `FormatNodeInDocumentAsync` and `RootListPosition`.

## Policy owners

- Restoring a node-level target's leading trivia after the pipeline: `rg -n "WithLeadingTrivia\(originalFirstToken|RestoreBlankLines" src/Reihitsu.Formatter` finds two places:
  - `FormatNodeInDocumentAsync` (`ReihitsuFormatter.cs:199`), under FMT-PIPE-010's condition;
  - `FormatNodeInDocumentWithContextAsync` (`ReihitsuFormatter.cs:306`), unconditionally, per FMT-PIPE-018.

  Neither decides any more whether part of the trivia starts its line, so finding #11's second owner of that decision is gone.
- "Does this trivia start its line": `rg -n "StartsOnNewLineIncludingDocumentation" src/Reihitsu.Formatter` gives the `FormatNodeInDocumentAsync` condition and `RootListPosition`, both as on `main`. The private `EndsWithLineBreak` copy that 8cb740c added beside `BlankLineTriviaUtilities.EndsWithLineBreak` is removed.

## Tests run

All runs are Debug, on the working tree of this commit.
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ReihitsuFormatterNodeColumnTests|FullyQualifiedName~IndentationPhaseRawStringMoveTests"` gives `Passed: 44, Failed: 0`. The test implementer saw 18 failures in `ReihitsuFormatterNodeColumnTests`.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5111|FullyQualifiedName~RH5201"` gives `Passed: 149, Failed: 0`. The test implementer saw 16 failures.

| Project | Result |
|---|---|
| formatter | `Passed: 3002, Failed: 0` |
| analyzer | `Passed: 6242, Failed: 0` |
| architecture | `Passed: 89, Failed: 0` |
| core | `Passed: 533, Failed: 0` |
| cli | `Passed: 250, Failed: 0` |
| tooling | `Passed: 125, Failed: 0` |
| playground | `Passed: 17, Failed: 0` |
| selfhosting | `Passed: 3607, Failed: 0` |

`scripts/format.sh` over the two changed files reformatted nothing (`Formatted 0 of 2 file(s)`). The build logs show no RH, CS, S, or IDE warning for them.

## Disputed tests or scope

_None._

## Architecture findings

_None new._ Finding #11's structural cause was two copies of "does this part of a context target's leading trivia start its line". The revert removes the entry point's copy. The cost is the recorded gap in FMT-PIPE-017: a target that starts a later line of its context keeps its first line's column. The decision names the restructuring of the context entry point as a follow-up candidate.

## Follow-up candidates

- The restructuring of the context entry point, so that a context target that starts a later line of its context takes the phases' column (decision audit 3; FMT-PIPE-017's recorded gap).
- Still open from earlier rounds:
  - the blank lines in front of a `FormatNodeInDocumentAsync` target (Q1 of `33-spec-author.md`);
  - removing `DocumentationCommentUtilities.ShiftContinuationExteriorMarkers` (decision audit 2, #10);
  - RH5201 against change doc row 10;
  - the RH5304 statement and expression-body targets;
  - `FormatNode` on an attached node.
- From the test implementer's spec observations, for the spec checker: SHARED-FIX-007 and RH5111-004 are not narrowed for the outputs that FMT-PIPE-018 and the FMT-PIPE-017 gap now require (`39-test-implementer.md`, *Spec observations*).
