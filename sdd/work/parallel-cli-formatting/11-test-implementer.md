Role: test implementer
Commit: 86f0420 (head the tests were written on; production unchanged by this round)
Verdict: tests written

Repair round 2. Worklist: decision 11 of `00-lead.md` — auditor findings #1, #2, #3 (test side) and hint 3 from `10-auditor.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the worklist; the auditor's suggested changes were read as suggestions, and the expected values come from CLI-022 and CLI-023.

## API needed from the implementer

The CLI test project does not compile until this exists (finding #3, the seam for the wiring test):

```csharp
// src/Reihitsu.Cli/Program.cs
internal static FormatCommandHandler CreateHandler(ParseResult result, string[] paths, FormatCommandDependencies dependencies)
```

It creates the handler from every parsed value, `result.MaxParallelism` included, and `Main` uses it instead of its inline `new FormatCommandHandler(...)`, so the tests exercise the code path `Main` runs. Every test call site of the `FormatCommandHandler` constructor now passes `suppressUnsupported:` and `maxParallelism:` by name, so the tests compile whether the constructor keeps its defaults or makes both parameters required (the auditor's suggestion for finding #3 is the implementer's call).

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Unit.FormatCommandHandlerAliasTests.FileReachedThroughTwoPathsHoldsFormattedTextWhenEmptyDuringWrite` (new) | CLI-022 | `SharedFileFileSystem` (new helper), formatter fake with a fixed text map, `--utf8-bom`, degree 2 | fails: the file ends empty (`""`, expected the 46-character formatted text with a BOM) — finding #1 reproduced |
| `Unit.FormatCommandHandlerAliasTests.FileReachedThroughTwoPathsHoldsFormattedTextWhenPrefixDuringWrite` (new) | CLI-022 | as above, without `--utf8-bom` | fails: the file ends as `"class A\n{\n    int x;\n}"` (the second class is lost) — finding #1, the prefix member of the auditor's class enumeration |
| `Unit.FormatCommandHandlerParallelTests.CancellationNamesFinishedFilesInSelectionOrderWhenLaterFileFinishedFirst` (new) | CLI-017, CLI-022 | NSubstitute file system; `C.cs` written, `B.cs`'s write started, `A.cs` read pending; precondition assert that C was written first | passes |
| `Unit.FormatCommandHandlerParallelTests.CommandLineCountIsTheDegreeOfTheHandler` (new) | CLI-023 | `Program.ParseArguments` → `Program.CreateHandler` → in-flight counter | does not compile until `Program.CreateHandler` exists; passes against a faithful stub |
| `Unit.FormatCommandHandlerParallelTests.WithoutCountTheHandlerProcessesAsManyFilesAsLogicalProcessors` (new) | CLI-023 | as above, no option, `Environment.ProcessorCount + 2` files | as above |
| `Unit.FormatCommandHandlerParallelTests.DegreeOneNeverProcessesTwoFilesAtTheSameTime`, `DegreeTwoProcessesTwoFilesAtTheSameTimeButNeverThree` (changed) | CLI-023 | in-flight counter now takes a handler factory and the expected degree | pass; assertions unchanged |
| `EndToEnd.CliMaxParallelismEndToEndTests.WriteModeFormatsFileReachedThroughSymbolicLink` (changed, hint 3) | CLI-022 | E2E | passes; the summary must now be `Formatted <n> of 2 file(s).` with `<n>` the number of `Formatted:` lines, instead of either count |
| `Integration.DefaultFileSystemTests.ExecuteAsyncLargeGeneratedRunWithBoundaryMarkerSkipsConfirmation` (changed) | — | constructor call | passes; now passes `suppressUnsupported: false, maxParallelism: 4` explicitly |
| `Integration.FormatCommandHandlerIntegrationTests` `CreateHandler` helper (changed) | — | constructor call | passes; now names `suppressUnsupported: false` next to `maxParallelism: 4` |

### How the alias test forces the overlap (finding #1)

`src/Reihitsu.Cli.Test/Helpers/SharedFileFileSystem.cs` gives `/work/real/A.cs` and `/work/link/A.cs` one content cell. A write is not atomic: while it is in progress the cell holds a given content without a BOM (empty, or a prefix of the formatted text that parses and that the formatter fake changes), as `File.WriteAllTextAsync` does after truncating. The read through the second path waits until a write has started; a write waits until that read has taken its snapshot; a read that saw a write in progress returns only after that write, so its own later write lands last. Every wait ends after 2 s, so an implementation that never lets a read overlap a write still finishes. Against the head, both tests fail deterministically (no timing dependency: the overlap is forced by events). Against a local stub that takes one `SemaphoreSlim(1)` around every `ReadFileAsync` and `WriteAllTextAsync` (reverted, not committed), both pass in about 1 s, three runs out of three. The tests assert content and encoding only (the CLI-022 sentence "The file MUST still hold the formatted text") and the exit code, not which path is named, which answer 5A leaves open.

### Watched failing for the right reason

- Finding #2: a local mutation of `ProcessFilesAsync` that flushes a canceled run in completion order (reverted) fails the new cancel test with `actual: ["Formatted: /work/C.cs", "Formatted: /work/B.cs"]`, while the existing `CancellationSpecExampleNamesEveryFinishedFileInSelectionOrder` still passes under it — which is what the auditor reported.
- Finding #3: with a stub `CreateHandler` that omits `result.MaxParallelism` (the handler's default 1), both wiring tests fail (`Expected values to be equal`, after the 10 s shared deadline). With the faithful stub they pass. The in-flight helper's wait deadline is now shared by every read of a run, so a wrong degree costs one 10 s timeout, not one per file.

## Scenarios (bug fix)
| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Alias read during a write, empty during write, `--utf8-bom`, degree 2 | LF | handler (unit) | reproduced | file `""` with BOM vs formatted text with BOM |
| Alias read during a write, parseable prefix during write, degree 2 | LF | handler (unit) | reproduced | first class only vs both classes |

## Not covered, and why
- `--check` and `--dry-run` with aliases: nothing is written, so no read can overlap a write; `CheckNamesBothPathsOfFileReachedThroughSymbolicLink` already covers CLI-022's one-after-another output for them.
- Degree 1 with aliases: one file at a time, no overlap possible (auditor's class table); not a new condition of the change.
- A real-file reproduction of finding #1 at the E2E level: the race hit once in 12 runs of 2,400 pairs (auditor), so it cannot be a deterministic test; the unit test forces the overlap instead.
- CRLF: the CLI handles content as opaque text here; no layout decision is involved.
- Hint 2 (unused token parameter of `IFileSystem.WriteAllTextAsync`): production-side cleanup in decision 11, routed to the implementer; `ExecuteAsyncStartedWriteCompletesWhenRunIsCanceled` keeps covering that a started write is not canceled. If the parameter is removed, the fakes in `FormatCommandHandlerParallelTests` and `FormatCommandHandlerTests` that read `ArgAt<CancellationToken>(3)` and the new `SharedFileFileSystem.WriteAllTextAsync` signature have to follow; the implementer may adjust those mechanically (signature only, assertions unchanged).
- Failure classes: as in `04-test-implementer.md`; nothing in this round touches syntax, trivia, a code fix, or the formatter.

## Commands
```shell
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.FormatCommandHandlerAliasTests"
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.FormatCommandHandlerParallelTests"
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.EndToEnd.CliMaxParallelismEndToEndTests.WriteModeFormatsFileReachedThroughSymbolicLink"
scripts/test.sh --project cli
```

With the faithful `CreateHandler` stub in place, the full CLI run was `Failed: 2, Passed: 358, Total: 360` — the two failures are the alias tests; the parallel and E2E parallelism classes passed five runs in a row.

## Follow-up candidates
_None._
