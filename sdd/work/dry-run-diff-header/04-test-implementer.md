Role: test implementer
Commit: 7c47253 (tests written on this head; the commit that adds this handover follows it)
Verdict: tests written

Inputs: thoenissen/Reihitsu#975, `00-lead.md`, `02-skeptic.md`, `03-spec-author.md`, CLI-013 and CLI-024 in `sdd/specs/cli/reihitsu-format.md`, `sdd/changes/2026-11/dry-run-diff-header.md`. The prompt carried no intended fix, cause, or hoped-for verdict.

Lane is behavior-change, so these are spec-example tests, not a bug reproduction; the first test is still the issue's reported scenario. All tests are end to end through `Program.Main()` in one new class, `src/Reihitsu.Cli.Test/EndToEnd/CliDryRunDiffHeaderEndToEndTests.cs`, because only that path sees the process's real current directory (FC-033; unit tests substitute `IFileSystem`/`IDiffGenerator`). Each test sets the current directory, reads it back with `Directory.GetCurrentDirectory()`, and builds every expected path from that value (skeptic's macOS `/var` hazard). Assertions compare whole lines (FC-017): the `Would format:` line plus both header lines, and for the CLI-013 example the whole standard output.

Every test was run on this head: 13 fail, each only on the `---`/`+++` lines, which read `a//<full path>` today; the Windows test is skipped on Linux. The rest of the CLI project (425 tests) and the architecture project (198, including requirement coverage of CLI-024) pass.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SpecExampleRelativePathInsideTheCurrentDirectoryPrintsTheRelativeHeader` — CLI-013 example, whole output (`--- a/src/A.cs`, hunk, `\ No newline…`, summary) | CLI-013, CLI-024 | `Program.Main` in a set current directory | fails (header `a//…`) |
| `FullPathInsideTheCurrentDirectoryPrintsTheRelativeHeader` | CLI-024 | same | fails |
| `DirectoryInsideTheCurrentDirectoryPrintsTheRelativeHeader` (`src/`) | CLI-024 | same | fails |
| `FileDirectlyInTheCurrentDirectoryPrintsItsFileName` (`a/A.cs`) | CLI-024 | same | fails |
| `FileNameStartingWithTwoDotsInTheCurrentDirectoryIsInside` (`a/..A.cs`; FC-002) | CLI-024 | same | fails |
| `SpecExampleFileOutsideTheCurrentDirectoryPrintsItsFullPath` (`../other/B.cs` from `work/src`) | CLI-024 | same | fails |
| `FileInSiblingDirectoryWithTheSameNamePrefixIsOutside` (`work-old` for `work`; FC-002, FC-003) | CLI-024 | same | fails |
| `InsideAndOutsideFilesInOneRunGetTheirOwnHeaders` (one run, both kinds, per-file decision) | CLI-024 | same | fails |
| `FileBelowTheRootDirectoryIsInsideWhenTheRootIsTheCurrentDirectory` (current directory `/`, root ends with a separator) | CLI-024 | same | fails |
| `OnLinuxADirectoryDifferingOnlyInCaseIsAnotherDirectory` (`Work` vs `work`; Linux only) | CLI-024 | same, `[OSCondition(Linux)]` | fails |
| `OnWindowsADirectoryDifferingOnlyInCaseIsTheSameDirectory` (`C:\…\Work` vs `work`, relative path with `/`; Windows only) | CLI-024 | same, `[OSCondition(Windows)]` | skipped on Linux/CI |
| `OnLinuxABackslashInAFileNameIsKeptInTheRelativeHeader` (`a/src/x\y.cs`; Linux only) | CLI-024 | same, `[OSCondition(Linux)]` | fails |
| `FullPathThroughASymbolicLinkIntoTheCurrentDirectoryIsOutside` (change doc example 5; also asserts the process reports the physical directory) | CLI-024 | same, `[OSCondition(Linux \| OSX)]` | fails |
| `RelativePathInACurrentDirectoryReachedThroughASymbolicLinkIsInside` | CLI-024 | same, `[OSCondition(Linux \| OSX)]` | fails |

Both sides of every condition CLI-024 states (FC-003): inside/outside (`src/A.cs` vs `../other/B.cs`), ancestry vs name prefix (`..A.cs` inside vs `work-old` outside), case (Linux outside vs Windows inside), separator (Windows `\` → `/` inside, Linux `\` in a name kept), links (full path through link outside vs relative path inside), and given-as (relative, full, directory).

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue scenario: run in `work`, `--dry-run src/A.cs` (`SpecExampleRelativePathInside…`) | LF | CLI end to end | reproduced | observed `--- a//tmp/ReihitsuCliTest/<guid>/work/src/A.cs`, expected `--- a/src/A.cs`; every other output line already matches the CLI-013 example |

## Existing tests the change touches

`rg` over `src/Reihitsu.Cli.Test` for `--- a/`, `+++ b/`, `Generate(`, and `diff of`:

- Left, internal contracts, not visible output; they change only if the implementer moves the decision into these seams: `FormatCommandHandlerTests.ExecuteAsyncDryRunCallsDiffGenerator` (pins `Generate(filePath, …)` with the full path), `FormatCommandHandlerParallelTests.DryRunKeepsEveryDiffWithItsFileInSelectionOrder` (substituted generator, `diff of /work/A.cs`), `FormatCommandHandlerTests.ExecuteAsyncDryRunShowsDiff` (substituted diff text), `DiffGeneratorTests` (lines 172, 311 and following; `--- a/<given>`). The implementer updates them if the seam's contract changes, without weakening them.
- Left, must stay: `PipelineTraceCommandTests.ExecuteAsyncReportsChangedPhasesAndLeavesInputUntouched` passes a full temp path outside the test's current directory and asserts `--- a/` and `+++ b/`; it fails if the trace's header picks up the CLI-024 rule, so it guards the trace non-goal. Line 191's `DoesNotContain("--- a/")` is unaffected.
- `FormatCommandHandlerIntegrationTests.ExecuteAsyncDryRunProducesUnifiedDiffOutput` and `CliEndToEndTests.MainDryRunShowsDiffAndReturnsFormattingNeeded` assert only `@@`; unaffected, not changed.

## Not covered, and why

- Windows other drive (`D:\other\B.cs`): no portable way to create a second drive; the outside rule is the same code path as `SpecExampleFileOutsideTheCurrentDirectoryPrintsItsFullPath`, whose expected value is the full path exactly as `Would format:` prints it, so on Windows that test already checks `\` is kept for an outside file. The Windows-only tests never run in CI (ubuntu-only).
- Line endings (CRLF source): the header is written from the path alone, before any hunk, in `DiffGenerator.Generate` (`builder.AppendLine($"--- a/{filePath}")`), independent of the content's line endings; line separators in the output are `Environment.NewLine`, which `Lines` splits on.
- macOS case rule: CLI-024 says case-sensitive on macOS, but the default file system is case-insensitive and whether `getcwd` reports the typed case is not verified; no test asserts macOS case behavior.
- FC-004/005/019-029, FC-006-014, FC-018, FC-036, FC-038-044: formatter, analyzer, code-fix, rename, and precheck classes; the change touches only how the CLI names a file in the diff header.
- FC-015 (one case-rule owner) is a review concern for the implementer's code; the case tests above fail if a second rule disagrees with CLI-006 on Linux or Windows.
- FC-035: expected values come from CLI-024 and the change doc examples only.

## Commands

```shell
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.EndToEnd.CliDryRunDiffHeaderEndToEndTests"
scripts/test.sh --project cli --filter "TestCategory=CLI-024"
scripts/test.sh --project architecture
```

## Follow-up candidates

_None_ beyond the `scripts/trace.sh` header already in `00-lead.md`.
