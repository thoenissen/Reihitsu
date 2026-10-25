Role: implementer
Commit: c5df1c3
Verdict: tests pass

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It asked me to delete the helpers that lost their last caller once `24-test-implementer.md` removed their tests (8d8e9f8).

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Utilities/ReihitsuFormatterHelpers.cs` | Deleted `AdjustNodeIndentation` (internal) and `AdjustLeadingWhitespace` (private). Kept `IsAfterEndOfLine`, which `StartsOnNewLine` still uses. Kept `using System.Collections.Generic;`, which `IEnumerable<>` at line 54 needs. | FMT-PIPE-017 (no line moves after the last phase; the helper was that move) |

**Callers checked before deleting.** I ran `rg -n "AdjustNodeIndentation|AdjustLeadingWhitespace" src --glob '*.cs'` (outside `bin`/`obj`) on 8d8e9f8. Its only matches were the two declarations and the one call of `AdjustLeadingWhitespace` inside `AdjustNodeIndentation`. There was no production caller, no test caller, and no `cref`. After the deletion the same command finds nothing.

`scripts/format.sh --force` changed nothing in the file.

## Policy owners

- `rg -n "AdjustNodeIndentation|AdjustLeadingWhitespace" src --glob '*.cs'` finds nothing.
- The node-level column has two owners: `ReihitsuFormatterHelpers.ComputeDocumentColumn` counts it, and `LayoutComputer.ShiftToRootFirstTokenColumn` applies it (9081b4c).

## Tests run

All runs were on the tree of c5df1c3.
- `scripts/test.sh --configuration Debug --project formatter` → `Passed: 2981, Failed: 0`. That is six fewer than before, matching the six tests removed in 8d8e9f8.
- `scripts/test.sh --configuration Debug --project analyzer` → `Passed: 6226, Failed: 0`.
- The build output of both runs shows no RH, CS, S, or IDE warning for the changed file.

An intermediate state that also removed `using System.Collections.Generic;` failed to compile (CS0246 on `IEnumerable<>`). It was not committed.

## Disputed tests or scope

_None._

## Architecture findings

_None._

## Follow-up candidates

- `Reihitsu.Core.DocumentationCommentUtilities.ShiftContinuationExteriorMarkers` (public) no longer has a production caller. Its only caller was the deleted `AdjustLeadingWhitespace`. Only `DocumentationCommentUtilities.ShiftContinuationExteriorMarkersAppliesColumnOffset` in `src/Reihitsu.Core.Test/DocumentationCommentUtilitiesTests.cs` still uses it. Removing it changes a public member of the shared Core assembly and needs that test removed first, so I left it.
