Role: implementer
Commit: 28bce46
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md`, `03-spec-author.md`, `04-test-implementer.md`, `sdd/changes/2026-11/parallel-cli-formatting.md`, `sdd/specs/cli/reihitsu-format.md` (CLI-002, CLI-006, CLI-017, CLI-022, CLI-023). No test was changed.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Cli/BufferedConsoleOutput.cs` (new) | `IConsoleOutput` that records one file's lines with their stream in write order; `FlushTo(IConsoleOutput)` replays them | CLI-022 |
| `src/Reihitsu.Cli/FormatCommandHandler.cs` | Constructor parameter `int maxParallelism = 1` (`ArgumentOutOfRangeException` below 1). `ProcessFilesAsync` starts each selected file on `Task.Run`, bounded by a `SemaphoreSlim` of the degree; each file writes into its own `BufferedConsoleOutput`; under a lock, the finished prefix of the selection is flushed to the console as soon as it is complete. No further file starts once the run's token is canceled or a file throws `OperationCanceledException`; after the started files end, every finished file's lines are flushed in selection order (also behind an unfinished one) and the cancellation is rethrown (the file's own exception, or `ThrowIfCancellationRequested`), so no summary is written. The generated-by-name skip moved into the per-file path (`ProcessSelectedFileAsync`) so its line is ordered like any other. `ProcessFileAsync`, `ReportUnsupported`, `HandleChangedFileAsync` take the per-file `IConsoleOutput`. `HandleChangedFileAsync` checks the token before the write and passes `CancellationToken.None` to `WriteAllTextAsync`, so a started write completes and the file counts as finished | CLI-017, CLI-022, CLI-023 |
| `src/Reihitsu.Cli/ParseResult.cs` | `int MaxParallelism` appended | CLI-023 |
| `src/Reihitsu.Cli/Program.cs` | `--max-parallelism <count>` parsed like `--lang-version` (next argument is the value, first error wins via `??=`, last valid value wins, missing-value error at the end); `TryParseMaxParallelism` uses `int.TryParse(..., NumberStyles.None, InvariantCulture)` (ASCII digits only, overflow rejected) and `>= 1`; default `Environment.ProcessorCount`; passed to the handler; two usage lines between `--lang-version` and `--help` | CLI-001, CLI-002, CLI-023 |
| `sdd/specs/cli/reihitsu-format.md` | `BufferedConsoleOutput` added to the code-owners row | — |

`DefaultFileSystem` is unchanged: it still forwards the token it receives; the handler no longer passes the run's token to the write. `IFileSystem.WriteAllTextAsync` keeps its token parameter.

## Policy owners
- Output ordering / per-file output: `rg -n "IConsoleOutput" src/Reihitsu.Cli` — `DefaultConsoleOutput` (real console), `BufferedConsoleOutput` (new, per-file buffer), test `CapturedConsoleOutput`. The handler is the only place that decides when a file's lines reach the console.
- Option value parsing: `rg -n "expectsLanguageVersion|expectsMaxParallelism|ArgumentError" src/Reihitsu.Cli` — `Program.ParseArguments` only; the new option follows the `--lang-version` pattern there.
- Write cancellation: `rg -n "WriteAllTextAsync" src/Reihitsu.Cli` — `FormatCommandHandler.HandleChangedFileAsync` (only caller), `DefaultFileSystem` (implementation).

## Production diff size
`git diff --shortstat` (working tree, production, before commit): 3 files changed, 212 insertions(+), 45 deletions(-), plus the new 62-line `BufferedConsoleOutput.cs`. New type: `BufferedConsoleOutput` (internal sealed). `origin/main...HEAD` production before this commit: 1 file, 7 insertions (README.MD by the spec author).

## Deleted special cases
_N/A._

## Tests run
- `scripts/build.sh --configuration Debug` — 0 errors; warnings in the CLI project are the pre-existing S1125 in `ConfirmLargeRunIfRequiredAsync`/`CollectFiles` (unchanged lines).
- `scripts/test.sh --configuration Debug --project cli` — `Passed! - Failed: 0, Passed: 355, Skipped: 0, Total: 355`.
- `scripts/test.sh --configuration Debug --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.FormatCommandHandlerParallelTests|FullyQualifiedName~CliMaxParallelismEndToEndTests"` — five runs in a row, each `Passed: 25, Failed: 0`.
- `scripts/test.sh --configuration Debug --project cli --filter "FullyQualifiedName~MaxParallelism|FullyQualifiedName~ExecuteAsyncStartedWriteCompletesWhenRunIsCanceled"` — `Passed: 39, Failed: 0`.
- `scripts/test.sh --configuration Debug --project architecture` — `Passed: 153, Failed: 0` (includes `EveryRequirementOfAnEnforcedSpecIsCovered`).
- `scripts/format.sh` over the four changed production files — `Formatted 0 of 4 file(s).`

## Disputed tests or scope
- New type `BufferedConsoleOutput` (62 lines). It is part of the issue's own fix (per-file output kept in selection order), not a structural change; reported per the role file's *Scope*.
- Behavior to note: a run canceled after its last file finished but before the summary now ends with `Operation canceled.` and no summary (`ProcessFilesAsync` rethrows whenever the token is canceled once processing ends). On `main` the sequential loop printed the summary in that window. CLI-022 ("MUST NOT write the summary") demands it, and `ExecuteAsyncStartedWriteCompletesWhenRunIsCanceled` (single file, canceled during the write, expects the exception) requires it.
- The handler's `maxParallelism` defaults to 1 for callers that omit it (`DefaultFileSystemTests` does); `Program` always passes the parsed value.

## Architecture findings
_None._

## Follow-up candidates
_None_ beyond those in `00-lead.md`.
