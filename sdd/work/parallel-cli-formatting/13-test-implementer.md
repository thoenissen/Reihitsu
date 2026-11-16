Role: test implementer
Commit: see the commit that adds this file (on top of 0edf08e; production unchanged by this round)
Verdict: tests written

Repair round 2, continued. Worklist: lead decision 12 — rework the three tests that `12-implementer.md` disputes, so that no file's read stays open until another file's write. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond decision 12. I followed the implementer's suggested route (stall `A.cs` in formatting) because it is the place the gate allows, not because it was suggested.

## What changed

Only `src/Reihitsu.Cli.Test/Unit/FormatCommandHandlerParallelTests.cs`. No assertion, input, expected output, or requirement tag changed.

- `CreateFormatter` takes an optional `Action<string, CancellationToken> holdFormatting`, run with the file path and the run's token before each file is formatted. A held file has completed its read and released the write-mode gate, so it is unfinished without holding any file access.
- New helpers `HoldUntil(path, event, releasedByEvent)` (blocks the formatting of one file until an event, at most `_holdTimeout`, then 100 ms) and `HoldUntilCanceled(token)` (blocks until the token is canceled and observes it; throws `TimeoutException("A.cs was never canceled.")` otherwise).
- The two cancel tests also had a second cross-file wait inside a write: one file's write waited for another file's write. The gate serializes writes too, so that wait could deadlock until the timeout. `CancellationNamesFinishedFilesInSelectionOrderWhenLaterFileFinishedFirst` passed against 3b0a3b6 only when the race went its way. That wait now sits in the waiting file's formatting, before its write starts. The write itself still raises Ctrl+C while it is in progress.
- Formatting is synchronous in the handler (`ISourceFormatter.FormatSyntaxTree`, called on the file's `Task.Run` worker), so the holds block a thread-pool thread. There are at most two such threads per test.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Unit.FormatCommandHandlerParallelTests.WriteModeWritesEveryChangedFileAndKeepsSelectionOrder` (reworked) | CLI-022 | NSubstitute; `A.cs` held in formatting until `C.cs` is written; precondition now reads "C.cs must have been written while A.cs was still being formatted." | passes |
| `Unit.FormatCommandHandlerParallelTests.CancellationSpecExampleNamesEveryFinishedFileInSelectionOrder` (reworked) | CLI-017, CLI-022 | `A.cs` held in formatting until canceled; `C.cs`'s formatting waits for `B.cs` to be written; Ctrl+C during `C.cs`'s write; precondition "B.cs must have been written before the write of C.cs started." | passes |
| `Unit.FormatCommandHandlerParallelTests.CancellationNamesFinishedFilesInSelectionOrderWhenLaterFileFinishedFirst` (reworked) | CLI-017, CLI-022 | `A.cs` held in formatting until canceled; `B.cs`'s formatting waits for `C.cs` to be written, plus 100 ms; Ctrl+C during `B.cs`'s write; precondition "C.cs must have been written before the write of B.cs started." | passes |

### Watched failing for the right reason (each mutation local, reverted)
- **Completion-order flush:** each file flushes its own lines when it finishes, in normal and canceled runs alike. `WriteModeWritesEveryChangedFileAndKeepsSelectionOrder` fails with actual `[Unchanged: B, Formatted: C, Formatted: A, …]`. `CancellationNamesFinishedFilesInSelectionOrderWhenLaterFileFinishedFirst` fails with actual `[Formatted: C, Formatted: B]`. The spec-example cancel test passes under this mutation, as before: in its scenario completion order equals selection order. I checked this on the final text in Release as well.
- **No cancel-path flush:** the loop that names finished files behind an unfinished one is removed. Both cancel tests fail with actual `[]`.
- **Gate independence:** with the gate widened to `SemaphoreSlim(16, 16)`, all 17 parallel tests pass and only the two alias tests fail. So the reworked tests neither need the gate nor are blocked by it, and the alias tests still detect its absence.
- Stability: the parallel class passed 5 runs out of 5, in about 0.7 s each (no timeout taken).

## Not covered, and why
- Other tests that use `HoldRead` (`CheckSpecExample…`, `GeneratedFileSkippedByName…`, `UnsupportedFileErrorLine…`, `FailingFile…`, `UndecodableFile…`, `DryRunKeepsEveryDiff…`): they run in `--check` or `--dry-run`, where the gate is `null`, so a read waiting for another file is allowed there. They are unchanged.
- Failure classes: as in `04-test-implementer.md`. This round touches only fixture mechanics.

## Commands
```shell
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.FormatCommandHandlerParallelTests"
scripts/test.sh --project cli
scripts/test.sh --project architecture
```
Results on this commit (Release): CLI `Passed: 360, Failed: 0`. Architecture `Passed: 153, Failed: 0`.

## Follow-up candidates
_None._
