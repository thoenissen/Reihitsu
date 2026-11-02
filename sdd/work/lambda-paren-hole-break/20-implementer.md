Role: implementer
Commit: (this commit; see `git log`)
Verdict: tests pass

Repair round 2, for lead decisions 12 and 13 (auditor finding #2 and the architecture finding in 18-auditor.md). I am a fresh agent. The prompt named the change through the lead's recorded decisions and carried no suspected cause or hoped-for verdict beyond them. I worked from 00-lead.md, 15, 18 and 19.

On f4622a3, the 16 tests in `HoleAnchorOnUnmovedLineAlignmentTests` give 6 failed and 10 passed, as 19-test-implementer.md says. All 16 pass now. No test was changed.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutModel.cs` | The 0 floor now applies only to the column after the move, and it is written once, in `FloorColumn`. New public `GetColumnAfterRawStringShift(line, columnBeforeShift)` returns `FloorColumn(before + shift)`. `Set`, `SetUnlessPlaced` and `ShiftRange` all store `FloorColumn(columnAfterShift) - shift` through `ToLayoutBeforeRawStringShift`, so a moved line can keep a column below 0 before the move. The old `ShiftRange` clamp of the column before the move (`newColumn < 0 → 0`) is gone. `TryGetPlacedColumn` and `TryGetLayoutColumn` read through `GetColumnAfterRawStringShift`. The class, `Set`, `ShiftRange`, `TryGetLayout` and `TryGetPlacedColumnBeforeRawStringShift` summaries are updated. | FMT-INDENT-031, FMT-INDENT-028, FMT-INDENT-030, FMT-RAW-007 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/IndentationRewriter.cs` | `WriteLineStart` records each placed line's layout column, which is the column before the move and may be below 0. It still writes whitespace only for a positive column, because it cannot write a negative one. That is no longer a floor of the result: the move rewrites the line. A placed `///` comment records its first line's column for its later lines too. `Apply` passes `MoveLines` a delegate that turns the recorded column into the final column with `model.GetColumnAfterRawStringShift`. A line the rewriter did not place uses the column it has in the indented tree. The summaries of `Apply`, `ApplyIndentation` and `WriteLineStart` are updated. | FMT-INDENT-031, FMT-RAW-007, FMT-RAW-001 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/RawStringLineShifts.cs` | `MoveLines`/`CollectEdits` take `getColumnAfterMove(line, writtenColumn)` instead of `getShift(line)`. The edit's new length is that column, so the local `Math.Max(0, indentation + shift)` is gone. A line whose column does not change is skipped, as a line with shift 0 was before. The summaries are updated. | FMT-RAW-007, FMT-RAW-009 |

**Decision 13.**
- Before: three places floored a hole line's column. `ShiftRange` and the rewriter did it before the move, and `CollectEdits` did it after.
- Now: one statement, `LayoutModel.FloorColumn`, floors only the column after the move. Every path reaches it through `GetColumnAfterRawStringShift`, or `ToLayoutBeforeRawStringShift` for stores.
- In finding #2's members, the block's brace line is stored at -1 before the move and lands in column 12 after it. The body is stored at 3 and lands in 16.
- `LayoutModelTests.ShiftRangeWithNegativeDeltaClampsToZero` still holds. On a line that does not move, the floor after the move is the old floor.

## Policy owners

- `rg -n "Math\.Max\(0|< 0\)|Column > 0|FloorColumn" src/Reihitsu.Formatter/Pipeline/Indentation src/Reihitsu.Core/DocumentationCommentUtilities.cs`
  - `LayoutModel.FloorColumn`: the one floor, applied after the move. It is used by `GetColumnAfterRawStringShift` and by `ToLayoutBeforeRawStringShift`.
  - `IndentationRewriter.WriteLineStart` (`layout.Column > 0`): writes no whitespace for a column at or below 0. It is not a floor of the result: the column is recorded, and the move writes the floored final column.
  - `DocumentationCommentUtilities.AlignContinuationExteriorMarkers` / line 361 (`Math.Max(0, …)`): limits where the `///` markers are written before the move. The move then sets the later lines from the recorded column. This is `Reihitsu.Core` and unchanged.
- `rg -n "MoveLines|GetRawStringShift" src --glob '!**/obj/**'`: `MoveLines` has one caller, `IndentationRewriter.Apply`. `GetRawStringShift` stays the one statement of a line's move.

## Tests run

All runs are Debug, on the working tree of this commit.

```text
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~HoleAnchorOnUnmovedLineAlignmentTests"
  before (f4622a3): Failed: 6, Passed: 10, Total: 16
scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~HoleAnchorOnUnmovedLineAlignmentTests|FullyQualifiedName~LayoutModelTests|FullyQualifiedName~AnonymousMethodHoleAlignmentTests|FullyQualifiedName~RawString"
  after: Failed: 0, Passed: 206, Total: 206
scripts/test.sh --configuration Debug --project formatter   Failed: 0, Passed: 4179, Total: 4179
scripts/test.sh --configuration Debug --project analyzer    Failed: 0, Passed: 6397, Total: 6397
scripts/test.sh --configuration Debug --project cli         Failed: 0, Passed: 251, Total: 251
scripts/build.sh --configuration Debug                      0 Warning(s), 0 Error(s)
```

`scripts/format.sh` over the three changed files changed nothing.

## Disputed tests or scope

_None._ The change adds one public member to an internal type, `LayoutModel.GetColumnAfterRawStringShift`, which decision 13 calls for. It changes the signature of the internal `RawStringLineShifts.MoveLines`, which has one caller. There is no new dependency. Three production files changed, all of them components decision 13 names.

The `///`-comment handling in the rewriter keeps a doc comment's later lines on its first line's column when that column is below 0 before the move. For every other column the result is the same as before. No test covers a `///` comment on a hole line whose anchor stays left of the move. The handling exists so that the one floor also holds for those lines.

## Architecture findings

_None_ beyond the one decision 13 repairs, which this change resolves.

## Follow-up candidates

_None._
