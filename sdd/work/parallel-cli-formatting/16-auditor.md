Role: auditor
Commit: b99b1f4679c1128c64944841402d025037c074b6
Verdict: pass

## Scope

Repair delta, repair round 2: `git diff 00262afec8bba7e58a8780b79235b0e7652cf7da..b99b1f4679c1128c64944841402d025037c074b6`, audited in the context of the whole branch. Base `origin/main` is `eb89003591b8371d91e4a14b8590d764bdf6d605` (the SHA in the prompt; `git fetch` gives the same SHA, and `git merge-base --is-ancestor` confirms it is merged). The checkout is the head SHA, and the working tree is clean. `scripts/lane-check.sh behavior-change` reports `confirmed`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

The diff is what handovers 11–15 and decisions 11 and 12 of `00-lead.md` describe:
- `src/Reihitsu.Cli/FormatCommandHandler.cs`: a write-mode file-access gate (`SemaphoreSlim(1, 1)`, `null` in `--check` and `--dry-run`) that every `ReadFileAsync` and `WriteAllTextAsync` takes. Its constructor parameters `suppressUnsupported` and `maxParallelism` are now required.
- `src/Reihitsu.Cli/Program.cs`: `CreateHandler`.
- `IFileSystem`/`DefaultFileSystem`: `WriteAllTextAsync` takes no token.
- Tests: `SharedFileFileSystem`, `FormatCommandHandlerAliasTests`, three cancel/order tests reworked, two wiring tests, the E2E summary-count assertion, and mechanical signature follow-ups.

No spec, change doc, or user document changed, and the scope did not grow.

Previous findings:
| # | Status | Evidence |
|---|---|---|
| 1 | closed | Every read and every write of a write-mode run holds `fileAccess` (`FormatCommandHandler.cs:282`, `:508-525`, `:578-590`), so no read can see a file that another selected path's write has truncated. The gate does not depend on how the alias is made. It covers every member the class table of `10-auditor.md` names: symbolic-link directory, file symbolic link, hard link, and case-insensitive spelling; an empty file and a parseable prefix; with and without `--utf8-bom`; degree 2 and more. `ReadSourceHeaderAsync`, the only other read, runs in `ConfirmLargeRunIfRequiredAsync` before `ProcessFilesAsync` starts (`:180`). Writes to one file are now serialized as well, and both write the same text. A read after the other path's write sees the formatted text and is named `Unchanged:`, which answer 5A admits. Two deterministic unit tests reproduce the empty and prefix members. Without the gate they fail (12-implementer.md, control run). |
| 2 | closed | `CancellationNamesFinishedFilesInSelectionOrderWhenLaterFileFinishedFirst` makes `C.cs` finish first, with a precondition assert, and cancels inside `B.cs`'s write while `A.cs` is unfinished. Traced: `C.cs`'s result is set but not flushed, because `nextToWrite` stays 0 behind `A.cs`. The post-`WhenAll` loop then flushes `B`, `C` in index order. A completion-order flush gives `[C, B]` (13-test-implementer.md, mutation). |
| 3 | closed | The constructor has no default for `maxParallelism`, and the only owner of the default is `Program.ParseArguments` (`Environment.ProcessorCount`, `Program.cs:182`). `Main` builds the handler through `CreateHandler` (`Program.cs:116`, `:146-158`). `CommandLineCountIsTheDegreeOfTheHandler` and `WithoutCountTheHandlerProcessesAsManyFilesAsLogicalProcessors` run `ParseArguments` → `CreateHandler` → the in-flight counter. |

Previous hints: 2 closed (no token reaches a write), 3 closed (the summary count equals the number of `Formatted:` lines). The lead dropped hints 1 and 4 under decision 11. Hint 1's window is unchanged in this diff, and `main` has the same window.

Check 10, visible output: no expected value of a visible surface changed in this range except the E2E summary assertion. That assertion now accepts a subset of what it accepted before, and CLI-014/CLI-022 with answer 5A cover it. The reworked tests changed fixture mechanics only, and their assertions and expected lines are unchanged.

Measured on the Release build of this head (built after the last production commit), on 558 files from `src/Reihitsu.Formatter` and `src/Reihitsu.Analyzer` with `private` turned into `private  ` (4 cores): write mode took 17.9 s at `--max-parallelism 1` and 9.1 s at 4. Both formatted 325 of 550 files, and the resulting trees are byte-identical (`diff -r`). So the gate keeps the issue's goal in write mode, and the output does not depend on the degree.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| | | | | | | None. | |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether the run takes the file-access gate | `_checkOnly \|\| _dryRun ? null : new(1, 1)` (`FormatCommandHandler.cs:282`) | Whether a file is written: `HandleChangedFileAsync` returns before the write for `_checkOnly` and for `_dryRun` (`:557-572`) | Agree: the same two flags with the same meaning, so the gate is non-null exactly when `WriteAllTextAsync` can be reached | ok |
| A read in write mode does not overlap a write | `fileAccess` held from before `_fileSystem.ReadFileAsync` until it returns (`:515-524`) | Write held from before `WriteAllTextAsync` until it completes (`:578-590`) | Agree: one gate, each held for the whole call. Neither holder waits on anything else while it holds the gate, so there is no deadlock with `slots` | ok |
| A canceled run starts no write; a started write is not interrupted | `fileAccess.WaitAsync(cancellationToken)`, then `ThrowIfCancellationRequested()` under the gate, then `WriteAllTextAsync` with no token | CLI-022 "MUST NOT interrupt a write … that has started"; "its write … not started — is left as it was" | Agree. A cancel while the file waits for the gate leaves the file unwritten and unnamed. Once the write has started, nothing can cancel it (`IFileSystem` has no token, and `DefaultFileSystem` passes `CancellationToken.None`) | ok |
| Gate lifetime | `using` around the loop and `await Task.WhenAll(tasks)` (`:282-337`) | Every `fileAccess.Release()` runs inside a task that `WhenAll` awaits | Agree: no release after disposal, also on the `break` paths | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Default degree of parallelism | `rg -n "ProcessorCount\|maxParallelism =" src/Reihitsu.Cli`: `Program.ParseArguments` only. The handler has no default | one owner (finding 3 closed) |
| Construction of the handler from parsed options | `rg -n "new FormatCommandHandler\(" src` outside tests: `Program.CreateHandler` only | one owner |
| When a file may be read or written in write mode | `rg -n "SemaphoreSlim" src/Reihitsu.Cli`: `fileAccess` in `ProcessFilesAsync`, taken in `FormatCommandHandler.ReadFileAsync` and `HandleChangedFileAsync` | one owner |
| Whether a write may be canceled | `rg -n "WriteAllTextAsync" src` outside tests: `IFileSystem` (no token), `DefaultFileSystem` (`CancellationToken.None`), one caller | one owner (hint 2 closed) |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FormatCommandHandlerAliasTests.FileReachedThroughTwoPathsHoldsFormattedTextWhenEmptyDuringWrite` | CLI-022: the file holds the formatted text and the BOM when a write shows an empty file while it is in progress | Final content `""`, or no preamble. Without the gate the fake forces the overlap by events, not by timing | `SharedFileFileSystem`: one content cell for two full paths, a non-atomic write, and event-driven overlap with 2 s fallbacks so that a gated handler finishes | adequate |
| `…WhenPrefixDuringWrite` | Same, when the write shows a parseable prefix that the formatter changes, without `--utf8-bom` | Final content is the formatted prefix (second class lost) | as above | adequate |
| `FormatCommandHandlerParallelTests.CancellationNamesFinishedFilesInSelectionOrderWhenLaterFileFinishedFirst` (new) | CLI-022: finished files named in selection order behind an unfinished file when a later file finished first; a started write completes | `[C, B]` (completion order); `[]` (no flush on cancel); missing write | `HoldUntilCanceled` on `A.cs` in formatting; `B.cs`'s formatting waits for `C.cs`'s write; precondition assert | adequate (finding 2 closed) |
| `CancellationSpecExampleNamesEveryFinishedFileInSelectionOrder` (reworked) | The spec example: `B`, `C` named, `C`'s started write completes, `A.cs` untouched and unnamed, no summary | A missing line or write, a third line, a summary | `A.cs` held in formatting, so it holds no gate. Expected lines unchanged | adequate |
| `WriteModeWritesEveryChangedFileAndKeepsSelectionOrder` (reworked) | Selection order in write mode when a later file is written first; formatting overlaps a write | Completion-order lines; precondition false if write mode were fully serialized | `HoldUntil` in formatting | adequate; it also shows that the gate leaves formatting parallel |
| `CommandLineCountIsTheDegreeOfTheHandler`, `WithoutCountTheHandlerProcessesAsManyFilesAsLogicalProcessors` | CLI-023: the parsed count or the processor count is the handler's degree, through the code path `Main` uses | A maximum in flight other than the expected degree (a dropped or constant argument) | in-flight counter with one shared deadline | adequate. `Main`'s one-line call of `CreateHandler` is not under test, but the required parameter makes a construction without a degree fail to compile. On a one-processor machine the default test cannot tell 1 from the processor count, and CI runners have more |
| `DegreeOneNeverProcessesTwoFilesAtTheSameTime`, `DegreeTwoProcessesTwoFilesAtTheSameTimeButNeverThree` (factory change) | unchanged | unchanged | `--check`: the gate is `null` there, so reads overlapping in this counter also show that check-mode reads are not gated | adequate |
| `CliMaxParallelismEndToEndTests.WriteModeFormatsFileReachedThroughSymbolicLink` (changed) | The summary count equals the number of `Formatted:` lines | `Formatted 2 of 2` with one `Formatted:` line, or the reverse | exact string | adequate (hint 3 closed) |
| `FormatCommandHandlerTests.ExecuteAsyncStartedWriteCompletesWhenRunIsCanceled` (token removed) | A started write completes and is named | `writtenContent` null; the `Formatted:` line missing | exact | adequate: the token side of the invariant is now structural, because no token reaches a write |

## Failure classes dismissed
The dismissals of `10-auditor.md` hold unchanged: the repair touches no syntax, trivia, formatter phase, code fix, precheck, or analyzer. That covers FC-001, 002, 004–014, 018–030, 036–044 and the tab-width matrix. For the classes the repair can reach:
- FC-003: the degree boundaries are unchanged. Degree 1 runs exactly as before, because the gate is uncontended there.
- FC-015: see *Policy ownership*, one owner each.
- FC-016: the gate covers every file read during processing. `ReadSourceHeaderAsync` finishes before processing starts, and no other `_fileSystem` read exists in the handler (`rg -n "_fileSystem\." src/Reihitsu.Cli/FormatCommandHandler.cs`).
- FC-017: see *Assertion adequacy*.
- FC-031: the changed summaries describe the new code: `ProcessFilesAsync` remarks, `ProcessSelectedFileAsync`, `ProcessFileAsync`, `ReadFileAsync`, `HandleChangedFileAsync`, `IFileSystem.WriteAllTextAsync`, `CreateHandler`, `SharedFileFileSystem`, the alias tests, and the new holds. No `TODO`, and no tracker numbers.
- FC-032: the measurement used the Release build from 23:48, which is newer than the last production commit at 23:36.
- FC-033, FC-034: the alias fake forces the reported shape itself (one file, two full paths, truncated or prefix content during a write) at the handler boundary. The real-file race cannot be made deterministic.
- FC-035: the expected values come from CLI-022 and the fixture's own formatter map, not from a patched copy of the code.
- FC-037: no text-only claim.

## Architecture findings
_None._

## Hints
1. The follow-up candidate "Confirm Windows behavior of concurrent writes to one physical file via aliases" in `00-lead.md` no longer has a subject. The gate serializes every read and write of a write-mode run, so two opens of one file never overlap within a run, and `--check` and `--dry-run` only read.
2. Each alias test runs for about 2 s, because a gated handler finishes through the fake's hold-timeout fallbacks. That is by design (`SharedFileFileSystem` summary), not a flake.

## Follow-up candidates
_None_ beyond those in `00-lead.md` (see hint 1).
