Role: implementer
Commit: see the commit that adds this file (on top of 4dc74a2)
Verdict: test disputed

Repair round 2. Worklist: decision 11 of `00-lead.md`, which covers auditor finding #1, finding #3 on the production side with `Program.CreateHandler`, and hint 2. My prompt said an earlier implementer for this repair was interrupted before it wrote a handover. I reviewed its uncommitted edits. Its work on hint 2 and finding #3 is correct, and I kept it. It had not started finding #1, so I did that myself. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the worklist.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Cli/Program.cs` | New `internal static FormatCommandHandler CreateHandler(ParseResult, string[], FormatCommandDependencies)`, which passes every parsed value including `MaxParallelism`. `Main` now calls it instead of an inline constructor call (finding #3, the seam named in 11-test-implementer.md). | CLI-023 |
| `src/Reihitsu.Cli/FormatCommandHandler.cs` constructor | `suppressUnsupported` and `maxParallelism` are now required, with no defaults. This removes the second owner of the default degree, which was 1; the parser's `Environment.ProcessorCount` is now the only one (finding #3). | CLI-023 |
| `src/Reihitsu.Cli/Abstractions/IFileSystem.cs`, `DefaultFileSystem.cs`, `FormatCommandHandler.HandleChangedFileAsync` | `WriteAllTextAsync` takes no cancellation token. `DefaultFileSystem` passes `CancellationToken.None` to `File.WriteAllTextAsync`, so no caller can make a started write cancelable again (hint 2). | CLI-022 |
| `src/Reihitsu.Cli/FormatCommandHandler.cs` (`ProcessFilesAsync`, `ProcessSelectedFileAsync`, `ProcessFileAsync`, new private `ReadFileAsync`, `HandleChangedFileAsync`) | Finding #1: in write mode, the run creates one `SemaphoreSlim(1, 1)` "file access" gate. Every file read and every write takes it, so no read overlaps a write. Formatting stays parallel. In `--check` and `--dry-run` the gate is `null` and reads are not serialized. A write waits for the gate with the run's token and checks the token again once it holds the gate, so a canceled run starts no write, and a started write is never interrupted. Summaries and remarks updated (FC-031). | CLI-022 |
| `src/Reihitsu.Cli.Test/Helpers/SharedFileFileSystem.cs`, `Unit/FormatCommandHandlerParallelTests.cs`, `Unit/FormatCommandHandlerTests.cs` | Hint-2 follow-through, signature only, as 11-test-implementer.md allows. The fourth `Arg.Any<CancellationToken>()` is dropped from every `WriteAllTextAsync` arrangement and assertion. In the four write fakes that read `ArgAt<CancellationToken>(3)`, the `writeToken.ThrowIfCancellationRequested()` line is removed, because no token reaches the write any more. The XML summary of `ExecuteAsyncStartedWriteCompletesWhenRunIsCanceled` no longer mentions the token. No assertion, input, or expected value changed. | — |

## Policy owners
- Default degree of parallelism: `rg -n "maxParallelism = |MaxParallelism = |ProcessorCount" src/Reihitsu.Cli --glob '*.cs'`. The only owner is now `Program.ParseArguments` (`Environment.ProcessorCount`); the handler has no default.
- Whether a write may be canceled: `rg -n "WriteAllTextAsync" src/Reihitsu.Cli --glob '*.cs'`. The interface has no token. `DefaultFileSystem` passes `CancellationToken.None`. The only caller is `HandleChangedFileAsync`, which checks the run's token before the write starts.
- When a file may be read in write mode: `rg -n "SemaphoreSlim" src/Reihitsu.Cli --glob '*.cs'`. The only owner is the `fileAccess` gate in `ProcessFilesAsync`, taken in `FormatCommandHandler.ReadFileAsync` and `HandleChangedFileAsync`.

## Production diff size
- `git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` measured before this commit: 5 files changed, 281 insertions(+), 45 deletions(-).
- This round's production change: 4 files changed, 84 insertions(+), 20 deletions(-).
- No new type.

## Deleted special cases
_N/A._

## Tests run
- `scripts/build.sh --configuration Debug`: Build succeeded, 0 errors. The only warnings are the Debug-only S1125 on unchanged lines (decision 9).
- `scripts/test.sh --configuration Debug --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.FormatCommandHandlerAliasTests"`: `Passed! Failed: 0, Passed: 2`.
- `scripts/test.sh --configuration Debug --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.FormatCommandHandlerParallelTests"`: `Failed! Failed: 3, Passed: 14, Total: 17`. The three failures are listed under *Disputed tests or scope*. The new wiring tests `CommandLineCountIsTheDegreeOfTheHandler` and `WithoutCountTheHandlerProcessesAsManyFilesAsLogicalProcessors` pass.
- `scripts/test.sh --configuration Debug --project cli`: `Failed! Failed: 3, Passed: 357, Total: 360`. The failures are the same three.
- `scripts/test.sh --configuration Debug --project architecture`: `Passed! Failed: 0, Passed: 153`.
- Control run with the gate disabled locally (reverted): `Failed: 2, Passed: 358`. The two failures are the alias tests. So the three failures come from the gate alone, and the alias tests need it.

## Disputed tests or scope

**The alias tests and three output-order tests cannot both pass under any implementation that fits the scope.**

The alias tests are `FormatCommandHandlerAliasTests.FileReachedThroughTwoPathsHoldsFormattedTextWhenEmptyDuringWrite` and `…WhenPrefixDuringWrite`. They require that a read through one path never overlaps a write through another path that names the same file. The handler cannot tell those two paths from two different files: `SharedFileFileSystem` deliberately gives it two distinct full paths and no identity. So the only safe handling is to keep every read in write mode from overlapping every write.

The following tests fix their order by holding one file's read open until another file's write has completed or has started:

| Test | What the fixture holds | What it then asserts or needs |
|---|---|---|
| `FormatCommandHandlerParallelTests.WriteModeWritesEveryChangedFileAndKeepsSelectionOrder` | `HoldRead` keeps `A.cs`'s read open until `C.cs` is written | It asserts this as a precondition: "C.cs must have been written while A.cs was still being read." |
| `FormatCommandHandlerParallelTests.CancellationSpecExampleNamesEveryFinishedFileInSelectionOrder` | `A.cs`'s read stays pending until the run is canceled, and the cancel is raised inside `B.cs`'s write | With the gate, `B.cs`'s write cannot start while `A.cs`'s read holds the gate. The fake gives up after its timeout with `A.cs was never canceled.`, and the output has a third line. |
| `FormatCommandHandlerParallelTests.CancellationNamesFinishedFilesInSelectionOrderWhenLaterFileFinishedFirst` | `A.cs`'s read stays pending until the cancel, which is raised during `B.cs`'s and `C.cs`'s writes | Same mechanism and the same failure as the row above. |

What these three tests check is selection order behind an unfinished file, and the cancel path. CLI-022 requires both, and the gate leaves both intact. Only the way the fixtures stall `A.cs` conflicts with the gate: in reality a read finishes on its own, and it never waits for another file's write.

Other approaches I considered and rejected:
- **Redo a read that overlapped a write.** `FormatCommandHandlerTests` (around line 1311) asserts exactly `fileCount` reads at degree 4 in write mode, so a redone read would make that test fail intermittently. It would also add reads in large runs and needs a hand-built async wait.
- **Gate per physical file.** It needs a file-identity query on `IFileSystem`. On .NET that query is incomplete: there is no portable inode API for hard links, and a resolved path does not cover two spellings on a case-insensitive file system. `SharedFileFileSystem` would also have to supply the identity. That is a new public member, and the alias fake would no longer model what the handler actually sees.
- **Gate keyed by file name.** It misses hard links in one directory, which is a member of the auditor's class.

**Suggested route (the lead decides).** Keep the gate, which is committed. The test implementer re-stages the three fixtures so that `A.cs` stalls somewhere other than its read, for example in the formatter fake's `FormatSyntaxTree` or `IsAutoGeneratedSource`, observing the token on the cancel path. The assertions and expected output stay as they are. The precondition text in `WriteModeWritesEveryChangedFileAndKeepsSelectionOrder` would then say "while A.cs was still being formatted". If the lead rejects the gate instead, finding #1 needs another mechanism, and the alias tests decide what that must be.

**Scope note.** The gate serializes the reads of a write-mode run among themselves, not only against writes. A reader/writer split would need a hand-built async primitive, and reads are short next to formatting. `--check` and `--dry-run` are not gated.

## Architecture findings
_None._ Finding #1 is one decision inside one component: when a file may be read in write mode.

## Follow-up candidates
_None_ beyond those in `00-lead.md`. The candidate "Dedupe one physical file reached by several paths" would let the gate become per-file later.
