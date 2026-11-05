Role: implementer
Commit: see the commit that adds this file (parent 63e88917)
Verdict: tests pass

Structural change of `00-lead.md` row 24, the code part: `20-auditor.md` finding 6 and its architecture finding, with FMT-PIPE-029 as amended in 11179496 and the tests of `22-test-implementer.md`. My prompt named the work and carried no intended fix, suspected cause, or hoped-for verdict. No test was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/ReihitsuFormatter.cs` (`FormatSplitFieldInContext`) | "Another member shares the target's first / last line" is no longer predicted from the input gap with `TokenGapUtilities.HasLineBreakBetween`. It is read from the formatted context: the first field (front gap) or the member behind the last field (behind gap) shares the line exactly when, in the phases' layout, its first token or a documentation comment in its leading trivia starts a line. The question is asked only where the document's gap has no line break that separates the two tokens (`SyntaxTriviaUtilities.AreSeparatedByEndOfLine` == false, a line break inside comment text not counting) and the gap is between two members (`targetIndex` bounds, unchanged); otherwise the trivia stays as it stands | FMT-PIPE-029 (fifth gap bullet), FMT-PIPE-010, FMT-PIPE-018 |
| same | The target-position column (`targetFirstTokenColumn`) depends on the front-gap answer, which is now known only after a layout. Where the front-gap question arises, the context is laid out first without the column (the common shared case); when the layout says the first field keeps its line, it is laid out a second time with the document column. Only the indentation phase reads that column (`LayoutComputer`, `rg TargetFirstTokenColumn`), so the second layout starts the same lines as the first. Inputs without the question run the pipeline once, as before | FMT-PIPE-017, FMT-PIPE-029 |
| same | New private `TryGetSplitFields` (the existing "one single-declarator field per declarator" lookup, now used by both layouts) and `StartsLineInLayout` (the read of the phases' result, through `SyntaxTokenPositionUtilities.StartsLine` and `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`). Remarks of `FormatSplitFieldInContext` and the inline comments describe the new decision (FC-031) | FMT-PIPE-029 |

Reading of the spec's precondition: FMT-PIPE-029 says the question arises "only where the first token behind the gap does not start its line in the document". I implemented it as "nothing in the gap starts a line in the document" (no separating line break between the two tokens). The literal reading would also ask the question for `_p;⏎    /** x */ private int _x, _y;`, where `private` does not start its line but the documentation comment does; there the layout says "starts a line" and the write-back would add a second line break in a gap that already has one. The precondition I used keeps that trivia as it stands, which is what the spec's own reason (the token does not share a line with the member) requires. The spec author may want to align the wording; no test or output depends on the difference beyond this shape, which no test covers.

## Policy owners

- Whether a member starts a new line in document-level output: `rg -n "HasLineBreakBetween|AreSeparatedByEndOfLine|StartsLineAtLeadingTriviaIndex|StartsLine\(" src/Reihitsu.Formatter/ReihitsuFormatter.cs`. The entry point no longer calls `TokenGapUtilities.HasLineBreakBetween`; the line-break phase's rule has its owners in the phase only (`MemberLineStartRewriter:96` and the other line-break rewriters). The entry point reads the result; its one remaining input test (`AreSeparatedByEndOfLine`, also at `ReihitsuFormatter.cs:186`, pre-existing) asks whether the document already separates the tokens, not what a phase would do.
- Token / trivia "starts its line": `SyntaxTokenPositionUtilities.StartsLine` and `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex` (Core) are reused; no new copy.

## Tests run

Debug, on 63e88917 plus this change.

- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~ReihitsuFormatterFieldSplitTests"`: Passed 43, Failed 0 (12 failed before, as 22 says).
- `scripts/test.sh --configuration Debug --project formatter`: Passed 4513, Failed 0.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 6512, Failed 0 (includes the 14 new RH7101 tests).
- `scripts/test.sh --configuration Debug --project architecture`: Passed 115; `--project core`: Passed 607; `--project cli`: Passed 251.
- `scripts/build.sh --configuration Debug`: 0 errors; the S1168 warning a first draft introduced is gone (`TryGetSplitFields`).
- `scripts/format.sh src/Reihitsu.Formatter/ReihitsuFormatter.cs`: 0 of 1 changed.

## Disputed tests or scope

_None._ No public member or dependency added. The second pipeline run happens only for a target that another member precedes on its line and that the phases leave on that line.

## Architecture findings

_None new._ Row 24 is carried out: the entry point restates no phase's rule for the shared-line decision.

## Follow-up candidates

_None._
