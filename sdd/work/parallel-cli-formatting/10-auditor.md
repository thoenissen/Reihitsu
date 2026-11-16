Role: auditor
Commit: 00262afec8bba7e58a8780b79235b0e7652cf7da
Verdict: findings

## Scope

Full audit of head `00262afec8bba7e58a8780b79235b0e7652cf7da` against base `eb89003591b8371d91e4a14b8590d764bdf6d605` (`git ls-remote origin refs/heads/main` returned that SHA; `git merge-base --is-ancestor` confirms it is merged). Checkout is the head SHA, working tree clean. The diff (`git diff eb89003…HEAD`) is what handovers 01–09 describe: change doc, CLI spec (CLI-001, 002, 006, 017, 022, 023), spec index row, requirement IDs, `src/Reihitsu.Cli/{BufferedConsoleOutput.cs, FormatCommandHandler.cs, ParseResult.cs, Program.cs, README.MD}`, and the CLI tests. `scripts/lane-check.sh behavior-change`: confirmed. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Inputs: thoenissen/Reihitsu#1795, `00-lead.md`, handovers 01–09, `sdd/changes/2026-11/parallel-cli-formatting.md`, `sdd/specs/cli/reihitsu-format.md`, the diff, and the counterparts outside it (`Abstractions/DefaultFileSystem.cs`, `IFileSystem.cs`, `DefaultConsoleOutput.cs`, `DefaultSourceFormatter.cs`, `FileProcessResult.cs`, static state in `Reihitsu.Formatter` and `Reihitsu.Core`).

Checks without a finding: specs vs. code for CLI-001, 002, 006, 017, 023 and every CLI-022 bullet other than the alias content guarantee (finding 1); visible output decided (check 10): the usage lines and argument errors are covered by answers 3A and 6A, ordered output by the maintainer's decision on the issue, the cancellation lines and the missing summary (decision 7) by answer 4A, the alias variance by answer 5A, and the started write that completes and is named by decision 1 — a safety-rail repair whose only alternative loses user code, named by CLI-011 once the write completes; README row and example are answer 3A verbatim. Scope: nothing outside the run. Comments: no `TODO`, no tracker numbers, every changed summary describes the new code (FC-031). Release build (`src/Reihitsu.Cli/bin/Release/net11.0`, built 22:26 after the last production commit) was used for the experiments; I trust the cleaner's build and the roles' test results and re-ran no repository test.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Cli/FormatCommandHandler.cs:284-335` (`ProcessFilesAsync` starts files concurrently), `:414` (`ReadFileAsync`), `:547` (`WriteAllTextAsync`) | — (no catalog class: concurrent read of a file another task is writing) | – | PR-introduced | In write mode, when two selected paths name one physical file (CLI-006), one path's read can run while the other path's write is in progress. `File.WriteAllTextAsync` truncates on open and writes in chunks, so the read sees an empty file or a prefix, and the handler then formats and writes that. CLI-022 says "The file MUST still hold the formatted text"; the change doc says "the file holds the formatted text either way". Both are broken: user code is lost. See *Counterexample* below. | Make sure no read in write mode overlaps a write, for example with one async gate that every `ReadFileAsync` and every `WriteAllTextAsync` of the run takes; formatting stays parallel and no file-identity check is needed. Add a deterministic unit test whose fake file system gives two paths one shared content cell, shows that cell as empty (and as a prefix) while a write is in progress, and asserts the final content for each alias member. Answer 5A's output carve-out stays as it is. |
| 2 | medium | `src/Reihitsu.Cli.Test/Unit/FormatCommandHandlerParallelTests.cs:716` `CancellationSpecExampleNamesEveryFinishedFileInSelectionOrder` | FC-017, FC-033 | – | PR-introduced | CLI-022 requires a canceled run to write the finished files "in the order it selected them — also when a file selected before it is unfinished". In this test `B.cs` finishes before `C.cs` (C's write waits for `bWritten`), so completion order and selection order are the same. An implementation that flushes the buffered files in completion order after a cancel passes it. No other test cancels with more than one finished file behind an unfinished one. | Make the later file finish first on the cancel path. For example, `B.cs`'s write waits until `C.cs` is written and the cancel arrives inside `B.cs`'s started write, while `A.cs` is unfinished. Then assert `Formatted: B.cs` before `Formatted: C.cs`, with a precondition assert that C finished first. |
| 3 | medium | `src/Reihitsu.Cli/FormatCommandHandler.cs:124` (`int maxParallelism = 1`) vs. `src/Reihitsu.Cli/Program.cs:161` (`Environment.ProcessorCount`) and `:116` (wiring) | FC-015, FC-017 | – | PR-introduced | Two different defaults state the default degree: CLI-023 and the parser use the number of logical processors, and the handler uses 1. The wiring at `Program.cs:116` has no test (04-test-implementer.md, *Not covered*). If that argument is dropped, every run is one after another, CLI-023's default and the issue's goal are lost, and every test stays green: `Program.Main` E2E output is identical at degree 1. Correct by reading today. | Make `maxParallelism` a required constructor parameter so that omitting it does not compile. `DefaultFileSystemTests` (the only caller that relies on the default) passes it explicitly. |

### Counterexample for finding 1

The mechanism, measured on this machine (Linux, 4 cores): a loop of `File.ReadAllBytesAsync` reads run against `File.WriteAllTextAsync` of a 54,890-byte text, 2,000 rounds. Of 25,678 reads made during a write, 18,308 returned 0 bytes, 6,521 returned a strict prefix, 849 returned the whole text, and none returned NUL padding (`/tmp/claude-0/-home-user-Reihitsu/aa4057d5-31e9-53a6-a160-6cb46b2233ae/scratchpad/race/Program.cs`, a session scratchpad, not committed).

End to end, with the Release CLI of this head (`/tmp/claude-0/-home-user-Reihitsu/aa4057d5-31e9-53a6-a160-6cb46b2233ae/scratchpad/alias-race2.sh <dir> 200 40 16 16 --utf8-bom`; hard-link variant `/tmp/claude-0/-home-user-Reihitsu/aa4057d5-31e9-53a6-a160-6cb46b2233ae/scratchpad/hardlink-race.sh`):
- `real/F<i>.cs` holds 40 classes of `class C\n{\n    int  x;\n}\n\n`, has no BOM, and is about 1.1 KB; `link` is a symbolic link to `real`.
- The paths are passed explicitly, in groups of 16 `real/` paths followed by the same 16 `link/` paths.
- The command is `reihitsu-format --force --verbose --utf8-bom --max-parallelism 16 <paths>`.
- The expected content comes from the same files formatted without aliases at `--max-parallelism 1`.

The run printed

```text
Formatted: …/wc/real/F16.cs
Formatted: …/wc/link/F16.cs
```

and exited with 0. `real/F16.cs` then held 3 bytes, `357 273 277` (the UTF-8 BOM only), where 1,151 bytes were expected. The file's code was lost. `link/F16.cs` read the file while the `real/F16.cs` write had truncated it but not yet written to it. The empty text parses without errors and formats to itself, and it has no BOM, so `--utf8-bom` writes an empty file with a BOM after the first write. The race is rare: it hit once in 12 such runs (2,400 alias pairs). Six runs of 300 hard-link pairs in one directory at degree 16, five at degree 4, and six without `--utf8-bom` did not hit.

Class enumeration (two full paths, one physical file, write mode):

| Axis | Member | Result |
|---|---|---|
| Alias kind | symbolic-link directory (reproduced), file symlink, hard link, two spellings on a case-insensitive file system with the `Ordinal` comparer (macOS) | all fail the same way: the handler sees two full paths in every member, and the read/write overlap does not depend on how the alias is made |
| Mode | write mode | fails |
| Mode | `--check`, `--dry-run` | hold: nothing is written, so no read can overlap a write |
| What the read sees × options | empty file with `--utf8-bom` | fails, reproduced (BOM-only file) |
| What the read sees × options | empty file without `--utf8-bom` | holds: formats to itself, `Unchanged:`, no write |
| What the read sees × options | prefix of a file longer than one write chunk that ends between top-level members, so it parses without errors, which the formatter changes (for example by removing the final line break, as `FormattedSource` in the E2E tests shows) | traced, not reproduced: the prefix is written back and the file is truncated, also without `--utf8-bom` |
| What the read sees × options | prefix that does not parse | holds: skipped with syntax errors, no write |
| Degree | 1 | holds |
| Degree | 2 or more | fails |
| Platform | Windows | not run; there, the concurrent open may also fail with a sharing violation (already a follow-up candidate in `00-lead.md`) |

The skeptic's statement that content is not at risk on Linux (02-skeptic.md, class 1) rests on concurrent writes only, not on a read during a write. Answer 5A was asked on that statement ("the file's content is the formatted text either way"). The repair suggested above restores that statement, so the outputs 5A decided stay unchanged.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Start no further file (CLI-022) | `slots.WaitAsync(cancellationToken)` (run token, only while waiting for a slot) and `Volatile.Read(ref cancellation)` (a file's own `OperationCanceledException`), both before `Task.Run` | CLI-022 "MUST start no further file" once Ctrl+C is pressed | Partial. When a slot is released just before the cancel and the loop's continuation runs just after it, the file starts anyway. Its read honors the token, so the only effect is a `Skipped (generated): <path>` line in `--verbose` for a file generated by its name (`ProcessSelectedFileAsync` checks no token). `main` has the same window between files | hint (see *Hints*) |
| A finished file's lines are written | `results[nextToWrite] != null` under `outputGate`; after `WhenAll`, `results[index] != null` | "every file it finished" = `ProcessSelectedFileAsync` returned a result | Agree. A canceled file never sets its result, and a finished file always does before it flushes | ok |
| No summary on cancellation | `cancellation != null \|\| cancellationToken.IsCancellationRequested` after `WhenAll` | CLI-022 "MUST NOT write the summary"; `ExecuteAsync` reaches `PrintSummary` only when `ProcessFilesAsync` returns | Agree; decision 7 covers the window after the last file | ok |
| A write starts only while the run is not canceled | `cancellationToken.ThrowIfCancellationRequested()` directly before `WriteAllTextAsync(…, CancellationToken.None)` | CLI-022 "a file not finished … its write not started — is left as it was"; CLI-017 | Agree | ok |
| Accepted count | `int.TryParse(value, NumberStyles.None, InvariantCulture)` and `count >= 1` | CLI-023: ASCII digits only, 1..2147483647; constructor `ThrowIfLessThan(maxParallelism, 1)` | Agree. .NET also accepts trailing NUL characters, which a process argument cannot hold | ok |
| Name-based generated skip | `GeneratedFileUtilities.IsGeneratedFile(filePath)`, moved into `ProcessSelectedFileAsync` | `ConfirmLargeRunIfRequiredAsync` (the same helper); counting in `ProcessFilesAsync` (`SkippedGeneratedCount == 0` for the total) | Agree; one helper, and the total is unchanged from `main` | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Default degree of parallelism | `Program.ParseArguments` (`Environment.ProcessorCount`); `FormatCommandHandler` constructor default `maxParallelism = 1` (`rg -n "maxParallelism\|MaxParallelism" src/Reihitsu.Cli`) | two owners with different values — finding 3 |
| When a file's lines reach the console | `FormatCommandHandler.ProcessFilesAsync` only (`rg -n "FlushTo\|_console\." src/Reihitsu.Cli`); `BufferedConsoleOutput` only records | one owner |
| `--max-parallelism` value parsing and errors | `Program.ParseArguments`/`TryParseMaxParallelism` only | one owner |
| Whether a write may be canceled | `FormatCommandHandler.HandleChangedFileAsync` (passes `CancellationToken.None`); `DefaultFileSystem.WriteAllTextAsync` still forwards whatever token it gets (`rg -n "WriteAllTextAsync" src/Reihitsu.Cli`) | one decision owner; see hint 2 |
| Generated by name | `GeneratedFileUtilities.IsGeneratedFile`, called from `ProcessSelectedFileAsync` and `ConfirmLargeRunIfRequiredAsync` | one owner |
| Thread safety of the formatter | Static state in `Reihitsu.Formatter`/`Reihitsu.Core` (`rg "static readonly .*(Dictionary\|ConditionalWeakTable\|Lazy\|HashSet\|List<\|\[\])"`): `LineStartIndex._indexes` (`ConditionalWeakTable`, thread-safe) and `SpacingPolicy._rules` (immutable); no mutable static field found | confirms 02-skeptic.md |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FormatCommandHandlerParallelTests.CheckSpecExampleKeepsSelectionOrderWhenLastFileFinishesFirst`, `GeneratedFileSkippedByNameKeepsSelectionOrder`, `UnsupportedFileErrorLineStaysBetweenItsNeighborsAcrossStreams`, `FailingFile…`, `UndecodableFile…`, `DryRunKeepsEveryDiff…`, `WriteModeWritesEveryChangedFile…` | Selection order across both streams when a later file finishes first | Lines in completion order; a cross-stream line out of place | `HoldRead` with a precondition assert that the overtaking happened; exact `InterleavedOutput` sequence | adequate |
| `CheckSpecExampleWithDegreeOneGivesTheSameOutput`, `DegreeAboveFileCountProcessesEveryFile` | Output at degree 1 and 1000 | Any differing line | exact sequence | adequate |
| `DegreeOneNeverProcessesTwoFilesAtTheSameTime`, `DegreeTwoProcessesTwoFilesAtTheSameTimeButNeverThree` | At most `<count>` in progress, and the bound is reached (FC-003) | A maximum other than 1 or 2 | in-flight counter; reads wait until the degree is reached | adequate |
| `LargeRunConfirmationIsAnsweredBeforeAnyFileIsReadForFormatting` | Confirmation before any formatting read | A `read` event before `confirm` | event queue | adequate |
| `CancellationSpecExampleNamesEveryFinishedFileInSelectionOrder` | Finished files named in selection order behind an unfinished one; a started write completes | Completion-order flush | completion order equals selection order in the fixture | weak — finding 2 |
| `CancellationDuringWriteCompletesItAndStartsNoFurtherFile` | No further file starts; the started write completes and is named | A second read; missing write or line | degree 1, `ReceivedCalls` | adequate |
| `FormatCommandHandlerTests.ExecuteAsyncStartedWriteCompletesWhenRunIsCanceled` | A started write gets a token the run's cancel does not cancel | The run's token reaches the write, which then throws: `writtenContent` stays null | exact content and output | adequate |
| `ProgramTests.ParseArguments…MaxParallelism…` (9 tests, 21 data rows) | CLI-001/002/023 parsing, both sides of 0/1, 2147483647/2147483648, digit/non-digit, `٣` | Any accepted invalid value or wrong message | exact message | adequate |
| `CliMaxParallelismEndToEndTests` help, invalid, missing, precedence | Usage lines and their position; error text; exit code; file untouched | Wrong or missing line | exact lines | adequate |
| `CliMaxParallelismEndToEndTests.CheckSpecExample…`, `CheckOfDirectory…`, `WriteModeWritesTheSameFilesForEveryDegree` | Same output and bytes for every degree on real files | A differing line or byte | exact text, byte comparison | adequate for what they cover; they cannot show order under out-of-order completion, which the unit tests do |
| `CliMaxParallelismEndToEndTests.WriteModeFormatsFileReachedThroughSymbolicLink` | Alias carve-out: content formatted; allowed name and count sets | Unformatted content; a name or count outside the sets | set-valued assertions | adequate for 5A's sets, but cannot show finding 1: both aliases start together and read the original. Hint 3 |
| `CliMaxParallelismEndToEndTests.CheckNamesBothPathsOfFileReachedThroughSymbolicLink` | `--check` equal at degrees 1 and 4 for aliases | Differing output | exact text | adequate |
| Program → handler wiring of the degree | CLI-023 default reaches the handler | — (no test) | — | missing — finding 3 |
| `CapturedConsoleOutput` (helper) | Thread-safe capture, one interleaved sequence | — | lock; snapshots | adequate |

## Failure classes dismissed
- FC-001, FC-002, FC-005, FC-044: no guard over syntax or trivia. The CLI guards are traced under *Guard scope*.
- FC-003: covered at the degree bound and in parsing (see *Assertion adequacy*).
- FC-004, FC-019–FC-021: no token gap, trivia, or line join is edited.
- FC-006, FC-022–FC-029, FC-036: no rewrite; the formatter is unchanged and each file is formatted whole by one worker.
- FC-007–FC-009, FC-018, FC-030, FC-043: no code fix touched.
- FC-010–FC-014: the formatter is unchanged. Idempotency of output across degrees is asserted byte-exact in `WriteModeWritesTheSameFilesForEveryDegree`. No analyzer is involved.
- FC-015: finding 3; otherwise one owner per policy (see *Policy ownership*).
- FC-016: the generated-by-name sibling of the per-file lines was moved into the ordered path and is tested. Sibling shapes of finding 1 are enumerated there.
- FC-031: every changed summary and comment checked (`ProcessFilesAsync`, `ProcessSelectedFileAsync`, `ProcessFileAsync` catch comment, `HandleChangedFileAsync`, `ReportUnsupported`, `TryParseMaxParallelism`, `ParseResult`, `BufferedConsoleOutput`, the test helpers). All describe the new code.
- FC-032: no project result from before the compiled change is relied on. The experiments used the Release build made after the last production commit.
- FC-033: the unit ordering tests force out-of-order completion with precondition asserts. The cancel path is the exception (finding 2).
- FC-034: the tests reproduce the spec examples themselves.
- FC-035: expected values come from the spec examples. The feasibility stub in 04 was reverted, and its values match the spec text.
- FC-037: no text-only claim.
- FC-038–FC-042: no shape declared unsupported, and no precheck change.
- Tab-width matrix: does not reach the CLI.

## Architecture findings
_None._ Finding 1 is a missing coordination of one decision (when a file may be read in write mode) inside one component, not two owners or a prediction.

## Hints
1. Start after cancel (`FormatCommandHandler.cs:286-305`): after `slots.WaitAsync` succeeds, the run token is not checked again before `Task.Run`, and `ProcessSelectedFileAsync` checks no token for a file generated by its name. In a window of microseconds, a file starts after Ctrl+C and, with `--verbose`, prints `Skipped (generated): <path>`. Nothing is written. `main` has the same window between files. A check of `cancellationToken.IsCancellationRequested` next to the `Volatile.Read` would close it.
2. `IFileSystem.WriteAllTextAsync` keeps a token parameter that its only production caller now always fills with `CancellationToken.None`, while `DefaultFileSystem` forwards it to `File.WriteAllTextAsync`. A future caller that passes the run's token brings the truncation back without any test noticing beyond `ExecuteAsyncStartedWriteCompletesWhenRunIsCanceled`.
3. `WriteModeFormatsFileReachedThroughSymbolicLink` accepts any combination of a name set and a summary count. It does not check that the count equals the number of `Formatted:` lines.
4. The two symbolic-link E2E tests need the symbolic-link privilege on Windows. CI (`build.yml`) and the scripts run on Linux and macOS only.

## Follow-up candidates
_None_ beyond those in `00-lead.md`.
