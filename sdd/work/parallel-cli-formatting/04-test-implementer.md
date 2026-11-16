Role: test implementer
Commit: 588b97d
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: thoenissen/Reihitsu#1795, `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, `03-spec-author.md`, `sdd/changes/2026-11/parallel-cli-formatting.md`, `sdd/specs/cli/reihitsu-format.md` (CLI-001, CLI-002, CLI-006, CLI-017, CLI-022, CLI-023), `squad/failure-classes.md`. Expected values come from the spec examples and requirement sentences only.

## API the tests compile against

The tests do not compile on this head; they need two additions and nothing else:

- `ParseResult.MaxParallelism` (`int`) — the parsed degree; `Environment.ProcessorCount` without the option.
- A `FormatCommandHandler` constructor parameter named `maxParallelism` (`int`), passed by name after `suppressUnsupported`; whether it is optional is free.

With exactly those two additions as a stub (option parsed nowhere, value ignored), `scripts/build.sh` (Release) builds warning-free, and the tests below fail for the reasons in *Expected now*. With a throwaway feasibility stub (semaphore-bounded, per-file buffered output flushed in selection order, write with a non-cancelable token, `OperationCanceledException` after the flush; parser and usage lines), every new and changed test passed, six repeated runs of the new classes in a row; both stubs were reverted and are not committed.

## Tests

New files: `src/Reihitsu.Cli.Test/Unit/FormatCommandHandlerParallelTests.cs`, `src/Reihitsu.Cli.Test/EndToEnd/CliMaxParallelismEndToEndTests.cs`. Changed: `Unit/ProgramTests.cs` (parser tests appended), `Unit/FormatCommandHandlerTests.cs`, `Integration/FormatCommandHandlerIntegrationTests.cs`, `Helpers/CapturedConsoleOutput.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `FormatCommandHandlerParallelTests.CheckSpecExampleKeepsSelectionOrderWhenLastFileFinishesFirst` | CLI-022; FC-033 | NSubstitute fakes; `A.cs` read held until `C.cs` is formatted; `InterleavedOutput` | fails (does not compile; with the sequential stub the precondition fails: A is never overtaken; with a completion-order stub the sequence fails) |
| `…CheckSpecExampleWithDegreeOneGivesTheSameOutput` | CLI-022, CLI-023 | fakes | fails (compile) |
| `…GeneratedFileSkippedByNameKeepsSelectionOrder` | CLI-022; FC-016 | fakes, held read | fails |
| `…UnsupportedFileErrorLineStaysBetweenItsNeighborsAcrossStreams` | CLI-022 | fakes, refusing formatter for `u.cs`, interleaved capture | fails |
| `…FailingFileDoesNotAffectOtherFilesAndKeepsSelectionOrder` | CLI-022 | fakes, formatter throws for `B.cs` | fails |
| `…UndecodableFileDoesNotAffectOtherFilesAndKeepsSelectionOrder` | CLI-022 | fakes, read of `B.cs` faults with `DecoderFallbackException` | fails |
| `…DryRunKeepsEveryDiffWithItsFileInSelectionOrder` | CLI-022 | fakes, diff generator returns `diff of <path>` | fails |
| `…WriteModeWritesEveryChangedFileAndKeepsSelectionOrder` | CLI-022 | fakes, `A.cs` held until `C.cs` is written | fails |
| `…DegreeOneNeverProcessesTwoFilesAtTheSameTime` | CLI-023; FC-003 | in-flight counter on `ReadFileAsync` | fails (compile; passes on a sequential stub, fails on an unbounded one: 4) |
| `…DegreeTwoProcessesTwoFilesAtTheSameTimeButNeverThree` | CLI-023; FC-003 | in-flight counter, reads wait until two are in flight or all started | fails (sequential: 1; unbounded: 6) |
| `…DegreeAboveFileCountProcessesEveryFile` | CLI-023 | fakes, degree 1000 | fails (compile) |
| `…LargeRunConfirmationIsAnsweredBeforeAnyFileIsReadForFormatting` | CLI-022 | event queue: `confirm` before every `read` | fails (compile; passes on the unchanged algorithm) |
| `…CancellationSpecExampleNamesEveryFinishedFileInSelectionOrder` | CLI-017, CLI-022 | `A.cs` read waits on its token; `C.cs` write cancels the run after `B.cs` is written, then checks its token | fails |
| `…CancellationDuringWriteCompletesItAndStartsNoFurtherFile` | CLI-017, CLI-022 | degree 1; the first write cancels the run; asserts one file read, written, named, whichever it is | fails |
| `FormatCommandHandlerTests.ExecuteAsyncStartedWriteCompletesWhenRunIsCanceled` (replaces `ExecuteAsyncPassesCancellationTokenToFileWrite`) | CLI-017, CLI-022 | write cancels the run then checks its own token | fails (on `main` the write receives the run's token and throws; content `null`) |
| `ProgramTests.ParseArgumentsWithoutMaxParallelismUsesProcessorCount` | CLI-023 | `ParseArguments` | fails (compile) |
| `…ParseArgumentsMaxParallelismMixesWithPathsAndOptions` | CLI-001, CLI-023 | | fails |
| `…ParseArgumentsMaxParallelismAcceptsDecimalCount` (`1`, `2`, `1000`, `2147483647`, `007`) | CLI-023 | DataRow | fails |
| `…ParseArgumentsMaxParallelismInvalidValueReturnsArgumentError` (`0`, `0000`, `-1`, `+2`, ` 2`, `2 `, `two`, `1.5`, `1,000`, `1e3`, `0x10`, `99999999999`, `2147483648`, `٣`, empty) | CLI-002, CLI-023; FC-003 | DataRow, exact message | fails |
| `…ParseArgumentsMaxParallelismWithoutValueReturnsArgumentError` | CLI-002 | exact message | fails |
| `…ParseArgumentsMaxParallelismConsumesFollowingOptionAsValue` | CLI-002, CLI-023 | `--max-parallelism --check` | fails |
| `…ParseArgumentsRepeatedMaxParallelismUsesLastValue` | CLI-023 | `2` then `8` | fails |
| `…ParseArgumentsInvalidMaxParallelismFollowedByValidCountReturnsArgumentError` | CLI-002 | `0` then `4` | fails |
| `…ParseArgumentsMaxParallelismAfterSeparatorIsPath` | CLI-001 | | fails |
| `CliMaxParallelismEndToEndTests.HelpListsMaxParallelismWithItsDescription` | CLI-023 | `Program.Main`, `ConsoleCapture` | fails (option line absent) |
| `…InvalidCountFailsBeforeAnyFileIsRead` | CLI-002, CLI-023 | real temp file stays unchanged | fails (`Unknown option`) |
| `…MissingCountFailsWithMissingValueError` | CLI-002 | | fails |
| `…InvalidCountTakesPrecedenceOverHelp` | CLI-002 | | fails |
| `…CheckSpecExampleWritesSelectionOrderForEveryDegree` (`4`, `1`) | CLI-022 | real files, exact stdout | fails |
| `…CheckSpecExampleWithUnsupportedFileReportsEveryFile` | CLI-022 | real CLI-019 `u.cs` | fails |
| `…CheckOfDirectoryWritesTheSameOutputForEveryDegree` | CLI-022, CLI-023 | mixed tree; default vs `2` vs `1` | fails |
| `…WriteModeWritesTheSameFilesForEveryDegree` | CLI-022 | two identical mixed trees (BOM, syntax error, generated by name and header); bytes compared | fails |
| `…WriteModeFormatsFileReachedThroughSymbolicLink` | CLI-022 (alias exemption) | `Directory.CreateSymbolicLink`; set-valued assertions | fails |
| `…CheckNamesBothPathsOfFileReachedThroughSymbolicLink` | CLI-006, CLI-022 | symlink; degree 4 equals degree 1, `2 of 2` | fails |

Shared changes:

- `CapturedConsoleOutput` gains `InterleavedOutput` (both streams in write order, prefixed `stdout: ` / `stderr: `) and a lock; the per-stream properties now return snapshots. Needed for CLI-022's cross-stream order, which the two separate lists cannot show (02-skeptic.md).
- `FormatCommandHandlerTests.CreateHandler` and the integration `CreateHandler` pass `maxParallelism: 4`, so every existing handler unit test and integration test also run with a degree above one (02-skeptic.md: existing tests must hold when the default degree is above 1). Their assertions are unchanged.

## Existing tests whose asserted output the change touches

`rg` over `src/Reihitsu.Cli.Test` for each changed output — the usage text (`Show this help`, `--lang-version <version>`), `Unknown option`, `Operation canceled`, the write token, `MaxParallelism`:

- `FormatCommandHandlerTests.ExecuteAsyncPassesCancellationTokenToFileWrite` — **replaced** by `ExecuteAsyncStartedWriteCompletesWhenRunIsCanceled`. It asserted that the write receives the run's token, which CLI-022's "MUST NOT interrupt a write to a file that has started" contradicts for a file system that honors its token (`DefaultFileSystem` forwards it to `File.WriteAllTextAsync`). The new test asserts behavior, not a token identity.
- `ExecuteAsyncPassesCancellationTokenToFileRead` and `…ToFormatter` — **left**: they assert the read and the formatter receive the run's token itself (`cts.Token`, not a linked one). That stays compatible with CLI-022 but constrains the implementation: a body token from `Parallel.ForEachAsync` would fail them. Reported so the lead knows before the implementer starts.
- `ExecuteAsyncCancellationIsNotSwallowedAsError` — **left**: an `OperationCanceledException` from a read propagates out of `ExecuteAsync` even when the run's token is not canceled. The new cancellation tests rely on the same contract (`ExecuteAsync` throws; `Program.Main` writes `Operation canceled.`).
- Help tests in `CliEndToEndTests` and `CliLanguageVersionEndToEndTests` use `Contains`; unaffected.

## Not covered, and why

- **Program passes `MaxParallelism` to the handler.** No seam observes the degree through `Program.Main` with the real file system; the end-to-end tests cover the parsed option's effect on output only. The wiring is one argument; the auditor checks it by reading.
- **Default degree in the handler.** Tested at the parser (`Environment.ProcessorCount`), which is where CLI-023 places it; the handler always receives a value.
- **Hard links and case-insensitive aliases** (02-skeptic.md class 1). The symbolic-link directory member stands for the class: CLI-022's exemption names the class by "two selected paths name one physical file", and the handler sees only two full paths in every member; a hard link needs P/Invoke. `--check` equality is asserted; write mode is asserted set-valued, never to one value (03-spec-author.md).
- **A file whose lines reach both streams.** No file writes to both streams today (02-skeptic.md); stated in the spec, no input exercises it.
- **Windows concurrent writes through aliases.** Not runnable here; already a follow-up candidate.
- **Cancellation with more files than the degree** (which later files start). Timing-dependent which files have started; the degree-1 test asserts "no further file" deterministically, and the spec example covers several in progress.
- **Failure classes.** Applicable and covered: FC-003 (both sides of the degree bound, of digit/non-digit, of 1/0 and 2147483647/2147483648), FC-016 (the generated-by-name line written outside the per-file path), FC-017 (exact sequences, not containment; set-valued only where the spec allows variance), FC-033 (fakes that force out-of-order completion, with a precondition assert that the overtaking happened), FC-035 (values from the spec only). Not reachable: FC-001/002/005/015/044 (no guard or predicate over syntax), FC-004/019–021 (no trivia or token gap), FC-006/022–029/036 (no rewrite), FC-007–009/018/030 (no code fix), FC-010–014 (the formatter is not changed; the skeptic found no shared mutable state), FC-038–043 (no shape declared unsupported), FC-031/032/034/037 (concern the implementation or a text-only proof, not tests).
- **Line endings.** Not a layout change; the formatter is not touched.

## Commands

```shell
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.FormatCommandHandlerParallelTests"
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.EndToEnd.CliMaxParallelismEndToEndTests"
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.ProgramTests.ParseArguments&FullyQualifiedName~MaxParallelism"
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.FormatCommandHandlerTests.ExecuteAsyncStartedWriteCompletesWhenRunIsCanceled"
scripts/test.sh --project cli
scripts/test.sh --project architecture
```

`scripts/test.sh --project architecture` builds the solution, so it fails to compile until the two API additions exist; `EveryRequirementOfAnEnforcedSpecIsCovered` (CLI-022, CLI-023 uncovered in 01 and 03) was not run on this head for that reason.

## Notes for the lead

- `DegreeTwoProcessesTwoFilesAtTheSameTimeButNeverThree` and the precondition asserts of the ordering tests require files to actually be processed at the same time at a degree above one. CLI-022 says the CLI MAY; the issue (no `agent-filed` label, goals binding) and the change doc's *What* say it does. A purely sequential implementation fails these tests. The ordering assertions themselves hold for any implementation.
- `CancellationDuringWriteCompletesItAndStartsNoFurtherFile` deliberately does not assert that the first started file is `A.cs`: no requirement says files start in selection order.

## Follow-up candidates

_None._
